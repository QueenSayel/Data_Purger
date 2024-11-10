using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.WindowsAPICodePack.Taskbar;

namespace Data_Purger
{
    public class HeadlessPurger
    {
        private const int bufferSizeInKB = 64;
        private const int fillPercentage = 100;

        public async Task PurgeDrive(string driveLetter, int passes, bool quickFormat)
        {
            string drivePath = driveLetter.ToUpper() + ":\\";

            if (!IsDriveAvailable(drivePath))
            {
                Console.WriteLine($"Error: Drive {drivePath} is not available.");
                return;
            }

            for (int i = 1; i <= passes; i++)
            {
                Console.WriteLine($"Pass {i} of {passes} - Starting format...");
                await FormatDriveAsync(drivePath, quickFormat);

                Console.WriteLine($"Pass {i} of {passes} - Writing random files...");
                await FillDriveWithRandomFilesAsync(drivePath);

                Console.WriteLine($"Pass {i} of {passes} - Post-write format...");
                await FormatDriveAsync(drivePath, quickFormat);
            }
        }

        private bool IsDriveAvailable(string drive)
        {
            try
            {
                var driveInfo = new DriveInfo(drive);
                return driveInfo.IsReady;
            }
            catch
            {
                return false;
            }
        }

        private async Task FormatDriveAsync(string drive, bool quickFormat)
        {
            try
            {
                using (Process process = new Process())
                {
                    process.StartInfo.FileName = "diskpart";
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardInput = true;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.CreateNoWindow = true;
                    process.Start();

                    process.StandardInput.WriteLine("select volume " + drive.Substring(0, 1));
                    process.StandardInput.WriteLine(quickFormat ? "format fs=ntfs quick" : "format fs=ntfs");
                    process.StandardInput.WriteLine("exit");

                    Task outputTask = Task.Run(() =>
                    {
                        string output;
                        while ((output = process.StandardOutput.ReadLine()) != null)
                        {
                            if (output.Contains("percent complete"))
                            {
                                int percent = ExtractPercentage(output);

                                Console.SetCursorPosition(0, Console.CursorTop);
                                Console.Write(new string(' ', Console.WindowWidth));
                                Console.SetCursorPosition(0, Console.CursorTop);
                                Console.Write($"Progress: {percent}%");
                            }
                            else
                            {
                                Console.WriteLine(output);
                            }
                        }
                    });

                    await process.WaitForExitAsync();
                    await outputTask;

                    if (process.ExitCode != 0)
                    {
                        throw new Exception("Diskpart encountered an error during formatting.");
                    }

                    await WaitForDriveToBeReadyAsync(drive);
                    Console.WriteLine($"\nDrive {drive} formatted successfully.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error formatting drive {drive}: {ex.Message}");
            }
        }

        private int ExtractPercentage(string output)
        {
            var match = System.Text.RegularExpressions.Regex.Match(output, @"(\d+)\s*percent complete");
            if (match.Success)
            {
                return int.Parse(match.Groups[1].Value);
            }
            return 0;
        }

        private async Task WaitForDriveToBeReadyAsync(string drive)
        {
            DriveInfo driveInfo = new DriveInfo(drive);

            while (!driveInfo.IsReady)
            {
                await Task.Delay(5000);
                driveInfo = new DriveInfo(drive);
            }
        }

        private async Task FillDriveWithRandomFilesAsync(string drive)
        {
            try
            {
                Console.WriteLine($"Filling drive {drive} with random files...");
                DriveInfo driveInfo = new DriveInfo(drive);
                long availableSpace = driveInfo.AvailableFreeSpace;
                long targetSpace = (long)(availableSpace * (fillPercentage / 100.0));

                Random rand = new Random();
                string[] fileExtensions = { ".mp3", ".docx", ".pdf", ".txt", ".jpg", ".png" };

                long totalBytesWritten = 0;

                while (totalBytesWritten < targetSpace)
                {
                    string fileName = Path.Combine(drive, $"{GenerateRandomFileName(rand)}{fileExtensions[rand.Next(fileExtensions.Length)]}");
                    long fileSize = rand.Next(1024 * 10, 1024 * 1024 * 50);

                    if (totalBytesWritten + fileSize > targetSpace)
                    {
                        fileSize = targetSpace - totalBytesWritten;
                    }

                    await WriteRandomFileAsync(fileName, fileSize);
                    totalBytesWritten += fileSize;

                    int progressPercentage = (int)((totalBytesWritten * 100) / targetSpace);
                    Console.WriteLine($"Progress: {progressPercentage}% - Created {fileName}");
                }

                Console.WriteLine("Drive filled with random files successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error filling drive {drive}: {ex.Message}");
            }
        }

        private async Task WriteRandomFileAsync(string fileName, long fileSize)
        {
            byte[] buffer = new byte[bufferSizeInKB * 1024];
            Random rand = new Random();

            using (FileStream fs = new FileStream(fileName, FileMode.Create))
            {
                long bytesWritten = 0;
                while (bytesWritten < fileSize)
                {
                    rand.NextBytes(buffer);
                    int bytesToWrite = (int)Math.Min(buffer.Length, fileSize - bytesWritten);
                    await fs.WriteAsync(buffer, 0, bytesToWrite);
                    bytesWritten += bytesToWrite;
                }
            }
        }

        private string GenerateRandomFileName(Random rand)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, 8).Select(s => s[rand.Next(s.Length)]).ToArray());
        }
    }
}
