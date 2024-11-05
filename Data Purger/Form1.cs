using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Taskbar;

namespace Data_Purger
{
    public partial class Form1 : Form
    {
        private int bufferSizeInKB = 64;
        private int fillPercentage = 100; // Example: Fill 80% of available space, can be controlled by a slider

        public Form1()
        {
            InitializeComponent();
            PopulateDriveComboBox();
            numericPasses.Value = 1; // Set default value to 1
            trackBarBufferSize.Scroll += new EventHandler(trackBarBufferSize_Scroll);
        }

        private void PopulateDriveComboBox()
        {
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady && drive.DriveType == DriveType.Removable)
                {
                    comboBoxDrive.Items.Add(drive.Name.Substring(0, 1)); // Add only the drive letter
                }
            }
        }

        private async void btnWipeDrive_Click(object sender, EventArgs e)
        {
            DisableControls();

            progressBar.Value = 0;
            Log("Starting drive wipe operation...");

            string driveLetter = comboBoxDrive.Text.ToUpper() + ":\\";

            int passes = (int)numericPasses.Value;

            try
            {
                for (int i = 1; i <= passes; i++)
                {
                    // Update title for formatting stage
                    this.Text = $"Formatting {driveLetter} - Pass {i} of {passes}";
                    Log($"Pass {i} of {passes} - Formatting...");
                    await FormatDriveAsync(driveLetter);

                    // Update title for writing stage
                    this.Text = $"Writing to {driveLetter} - Pass {i} of {passes}";
                    Log($"Pass {i} of {passes} - Writing random files...");
                    await FillDriveWithRandomFilesAsync(driveLetter);

                    // Update title for post-write formatting stage
                    this.Text = $"Post-write format {driveLetter} - Pass {i} of {passes}";
                    Log($"Pass {i} of {passes} - Post-write formatting...");
                    await FormatDriveAsync(driveLetter);
                }

                Log("Drive wipe operation completed.");
                this.Text = "Drive wipe complete"; // Final title after the operation
            }
            catch (Exception ex)
            {
                Log($"An error occurred during the process: {ex.Message}");
            }
            finally
            {
                EnableControls();
            }
        }

        private void DisableControls()
        {
            comboBoxDrive.Enabled = false;
            trackBarBufferSize.Enabled = false;
            numericPasses.Enabled = false;
            btnWipeDrive.Enabled = false; // Disable the wipe button
            checkBoxQuick.Enabled = false;
        }

        private void EnableControls()
        {
            comboBoxDrive.Enabled = true;
            trackBarBufferSize.Enabled = true;
            numericPasses.Enabled = true;
            btnWipeDrive.Enabled = true; // Enable the wipe button
            checkBoxQuick.Enabled = true;
        }

        private async Task FormatDriveAsync(string drive)
        {
            try
            {
                Log($"Formatting drive {drive}... (This may take a while for a full format)");

                using (Process process = new Process())
                {
                    process.StartInfo.FileName = "diskpart";
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardInput = true;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.CreateNoWindow = true;
                    process.Start();

                    process.StandardInput.WriteLine("select volume " + drive.Substring(0, 1));

                    // Use quick format if checkBoxQuick is checked
                    if (checkBoxQuick.Checked)
                    {
                        process.StandardInput.WriteLine("format fs=ntfs quick label=MyDrive");
                    }
                    else
                    {
                        // Full format
                        process.StandardInput.WriteLine("format fs=ntfs label=MyDrive");
                    }

                    process.StandardInput.WriteLine("exit");

                    Task outputTask = Task.Run(() =>
                    {
                        string output;
                        while ((output = process.StandardOutput.ReadLine()) != null)
                        {
                            Log($"[Diskpart] {output}");

                            // Extract percentage from the output
                            if (output.Contains("percent complete"))
                            {
                                int percent = ExtractPercentage(output);
                                UpdateProgressBar(percent);
                            }
                        }
                    });

                    // Wait for the process to complete
                    await process.WaitForExitAsync();
                    await outputTask;

                    if (process.ExitCode != 0)
                    {
                        throw new Exception("Diskpart encountered an error during formatting.");
                    }
                }

                await WaitForDriveToBeReadyAsync(drive);
                Log($"Drive {drive} formatted successfully.");
            }
            catch (Exception ex)
            {
                Log($"Error formatting drive {drive}: {ex.Message}");
            }
        }


        // Helper method to extract the percentage from the diskpart output
        private int ExtractPercentage(string output)
        {
            var match = System.Text.RegularExpressions.Regex.Match(output, @"(\d+)\s*percent complete");
            if (match.Success)
            {
                return int.Parse(match.Groups[1].Value);
            }
            return 0;
        }

        // Helper method to update the progress bar
        private void UpdateProgressBar(int percent)
        {
            if (progressBar.InvokeRequired)
            {
                progressBar.Invoke(new Action(() => { progressBar.Value = percent; }));
            }
            else
            {
                progressBar.Value = percent;
            }

            // Update taskbar progress as well
            TaskbarManager.Instance.SetProgressValue(percent, 100);
        }

        private async Task WaitForDriveToBeReadyAsync(string drive)
        {
            Log($"Waiting for drive {drive} to become ready...");

            DriveInfo driveInfo = new DriveInfo(drive);

            // Continuously check until the drive becomes available
            while (true)
            {
                if (driveInfo.IsReady)
                {
                    Log($"Drive {drive} is now ready.");
                    break;
                }

                // Wait a bit before rechecking
                await Task.Delay(5000); // 5 seconds delay
                driveInfo = new DriveInfo(drive); // Recreate the DriveInfo object to refresh its state
            }
        }

        private async Task FillDriveWithRandomFilesAsync(string drive)
        {
            try
            {
                Log($"Filling drive {drive} with random files...");
                DriveInfo driveInfo = new DriveInfo(drive);
                long availableSpace = driveInfo.AvailableFreeSpace;
                long targetSpace = (long)(availableSpace * (fillPercentage / 100.0));

                Random rand = new Random();
                string[] fileExtensions = { ".mp3", ".docx", ".pdf", ".txt", ".jpg", ".png" };

                long totalBytesWritten = 0;

                while (totalBytesWritten < targetSpace)
                {
                    string fileName = Path.Combine(drive, $"{GenerateRandomFileName(rand)}{fileExtensions[rand.Next(fileExtensions.Length)]}");
                    long fileSize = rand.Next(1024 * 10, 1024 * 1024 * 50); // Random file size between 10KB and 50MB

                    if (totalBytesWritten + fileSize > targetSpace)
                    {
                        fileSize = targetSpace - totalBytesWritten; // Adjust the size of the last file if necessary
                    }

                    await WriteRandomFileAsync(fileName, fileSize);

                    totalBytesWritten += fileSize;

                    int progressPercentage = (int)((totalBytesWritten * 100) / targetSpace);
                    progressBar.Value = progressPercentage;

                    // Update taskbar progress
                    TaskbarManager.Instance.SetProgressValue(progressPercentage, 100);

                    Log($"Created {fileName} ({fileSize / 1024} KB)");
                }

                Log($"Drive {drive} filled with random files successfully.");
                TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.NoProgress);
            }
            catch (Exception ex)
            {
                Log($"Error filling drive {drive}: {ex.Message}");
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

        private void Log(string message)
        {
            if (logTextBox.InvokeRequired)
            {
                logTextBox.Invoke(new Action(() =>
                {
                    logTextBox.AppendText($"[{DateTime.Now}] {message}{Environment.NewLine}");
                }));
            }
            else
            {
                logTextBox.AppendText($"[{DateTime.Now}] {message}{Environment.NewLine}");
            }
        }

        private void trackBarBufferSize_Scroll(object sender, EventArgs e)
        {
            bufferSizeInKB = trackBarBufferSize.Value;
            labelBufferSize.Text = $"{bufferSizeInKB} KB";
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}