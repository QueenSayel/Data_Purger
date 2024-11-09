using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Taskbar;
using System.Management;

namespace Data_Purger
{
    public partial class Form1 : Form
    {
        private int bufferSizeInKB = 64;
        private int fillPercentage = 100;
        private ManagementEventWatcher usbWatcher;
        private CancellationTokenSource cancellationTokenSource;

        public Form1()
        {
            InitializeComponent();
            PopulateDriveComboBox();
            numericPasses.Value = 1;
            trackBarBufferSize.Scroll += new EventHandler(trackBarBufferSize_Scroll);
            comboBoxDrive.SelectedIndexChanged += new EventHandler(comboBoxDrive_SelectedIndexChanged);
            btnWipeDrive.Enabled = false;
            btnCancel.Enabled = false;
            StartUsbWatcher();
        }

        private void StartUsbWatcher()
        {
            usbWatcher = new ManagementEventWatcher();
            WqlEventQuery query = new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 2 OR EventType = 3");
            usbWatcher.EventArrived += new EventArrivedEventHandler(OnUsbChanged);
            usbWatcher.Query = query;
            usbWatcher.Start();
        }

        private void OnUsbChanged(object sender, EventArrivedEventArgs e)
        {
            Invoke(new Action(() =>
            {
                if (comboBoxDrive.DroppedDown)
                {
                    comboBoxDrive.DroppedDown = false;
                }

                comboBoxDrive.SelectedIndex = -1;
                btnWipeDrive.Enabled = false;

                PopulateDriveComboBox();

                if (comboBoxDrive.Items.Count > 0)
                {
                    comboBoxDrive.SelectedIndex = 0;
                    btnWipeDrive.Enabled = true;
                }
                else
                {
                    comboBoxDrive.SelectedIndex = -1;
                    btnWipeDrive.Enabled = false;
                }
            }));
        }


        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            if (usbWatcher != null)
            {
                usbWatcher.Stop();
                usbWatcher.Dispose();
            }
            base.OnFormClosed(e);
        }

        private void comboBoxDrive_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxDrive.SelectedIndex < 0 || comboBoxDrive.SelectedItem == null)
            {
                btnWipeDrive.Enabled = false;
            }
            else
            {
                string selectedDrive = comboBoxDrive.SelectedItem.ToString().ToUpper() + ":\\";

                if (!IsDriveAvailable(selectedDrive))
                {
                    MessageBox.Show("The selected drive is no longer available.");
                    comboBoxDrive.SelectedIndex = -1;
                    btnWipeDrive.Enabled = false;
                }
                else
                {
                    btnWipeDrive.Enabled = true;
                }
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

        private void PopulateDriveComboBox()
        {
            comboBoxDrive.Items.Clear();

            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady && drive.DriveType == DriveType.Removable)
                {
                    comboBoxDrive.Items.Add(drive.Name.Substring(0, 1));
                }
            }

            if (comboBoxDrive.Items.Count > 0)
            {
                comboBoxDrive.SelectedIndex = 0;
            }
            else
            {
                comboBoxDrive.SelectedIndex = -1;
                btnWipeDrive.Enabled = false;
            }
        }

        private async void btnWipeDrive_Click(object sender, EventArgs e)
        {
            if (comboBoxDrive.SelectedIndex < 0)
            {
                Log("No drive selected. Please select a drive and try again.");
                return;
            }

            string driveLetter = comboBoxDrive.Text.ToUpper() + ":\\";

            if (!IsDriveAvailable(driveLetter))
            {
                MessageBox.Show("The selected drive is no longer available. Please select a different drive.");
                return;
            }

            cancellationTokenSource = new CancellationTokenSource();
            CancellationToken token = cancellationTokenSource.Token;

            DisableControls();
            progressBar.Value = 0;
            Log("Starting drive wipe operation...");

            int passes = (int)numericPasses.Value;

            try
            {
                for (int i = 1; i <= passes; i++)
                {
                    // Diskpart formatting stage (disable Cancel)
                    btnCancel.Enabled = false;
                    this.Text = $"Formatting {driveLetter} - Pass {i} of {passes}";
                    Log($"Pass {i} of {passes} - Formatting...");
                    await FormatDriveAsync(driveLetter, token);

                    // Filling with random files stage (enable Cancel)
                    btnCancel.Enabled = true;
                    this.Text = $"Writing to {driveLetter} - Pass {i} of {passes}";
                    Log($"Pass {i} of {passes} - Writing random files...");
                    await FillDriveWithRandomFilesAsync(driveLetter, token);

                    // Post-write formatting stage (disable Cancel)
                    btnCancel.Enabled = false;
                    this.Text = $"Post-write format {driveLetter} - Pass {i} of {passes}";
                    Log($"Pass {i} of {passes} - Post-write formatting...");
                    await FormatDriveAsync(driveLetter, token);
                }

                if (!token.IsCancellationRequested)
                {
                    Log("Drive wipe operation completed.");
                    this.Text = "Drive wipe complete";
                }
            }
            catch (OperationCanceledException)
            {
                Log("Drive wipe operation was canceled by the user.");
            }
            finally
            {
                ResetProgress();
                EnableControls();
                btnCancel.Enabled = false; // Ensure Cancel button is disabled after operation
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            // Trigger cancellation
            cancellationTokenSource?.Cancel();
            Log("Cancellation requested...");
        }


        private void DisableControls()
        {
            comboBoxDrive.Enabled = false;
            trackBarBufferSize.Enabled = false;
            numericPasses.Enabled = false;
            btnWipeDrive.Enabled = false;
            checkBoxQuick.Enabled = false;
        }

        private void EnableControls()
        {
            comboBoxDrive.Enabled = true;
            trackBarBufferSize.Enabled = true;
            numericPasses.Enabled = true;
            checkBoxQuick.Enabled = true;
            btnWipeDrive.Enabled = comboBoxDrive.SelectedIndex >= 0;
        }
        private void ResetProgress()
        {
            progressBar.Value = 0;
            TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.NoProgress);
            this.Text = "Data Purger";
        }
        private async Task FormatDriveAsync(string drive, CancellationToken token)
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
                    process.StandardInput.WriteLine(checkBoxQuick.Checked ? "format fs=ntfs quick label=MyDrive" : "format fs=ntfs label=MyDrive");
                    process.StandardInput.WriteLine("exit");

                    Task outputTask = Task.Run(() =>
                    {
                        string output;
                        while ((output = process.StandardOutput.ReadLine()) != null)
                        {
                            Log($"[Diskpart] {output}");

                            if (output.Contains("percent complete"))
                            {
                                int percent = ExtractPercentage(output);
                                UpdateProgressBar(percent);
                            }

                            if (token.IsCancellationRequested)
                            {
                                process.Kill();
                                throw new OperationCanceledException();
                            }
                        }
                    }, token);

                    await process.WaitForExitAsync();
                    await outputTask;

                    if (process.ExitCode != 0)
                    {
                        throw new Exception("Diskpart encountered an error during formatting.");
                    }
                }

                await WaitForDriveToBeReadyAsync(drive, token);
                Log($"Drive {drive} formatted successfully.");
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Log($"Error formatting drive {drive}: {ex.Message}");
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

            TaskbarManager.Instance.SetProgressValue(percent, 100);
        }

        private async Task WaitForDriveToBeReadyAsync(string drive, CancellationToken token)
        {
            Log($"Waiting for drive {drive} to become ready...");

            DriveInfo driveInfo = new DriveInfo(drive);

            while (true)
            {
                if (token.IsCancellationRequested) throw new OperationCanceledException();

                if (driveInfo.IsReady)
                {
                    Log($"Drive {drive} is now ready.");
                    break;
                }

                await Task.Delay(5000, token);
                driveInfo = new DriveInfo(drive);
            }
        }

        private async Task FillDriveWithRandomFilesAsync(string drive, CancellationToken token)
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
                    if (token.IsCancellationRequested) throw new OperationCanceledException();

                    string fileName = Path.Combine(drive, $"{GenerateRandomFileName(rand)}{fileExtensions[rand.Next(fileExtensions.Length)]}");
                    long fileSize = rand.Next(1024 * 10, 1024 * 1024 * 50);

                    if (totalBytesWritten + fileSize > targetSpace)
                    {
                        fileSize = targetSpace - totalBytesWritten;
                    }

                    await WriteRandomFileAsync(fileName, fileSize, token);

                    totalBytesWritten += fileSize;

                    int progressPercentage = (int)((totalBytesWritten * 100) / targetSpace);
                    progressBar.Value = progressPercentage;

                    TaskbarManager.Instance.SetProgressValue(progressPercentage, 100);

                    Log($"Created {fileName} ({fileSize / 1024} KB)");
                }

                Log($"Drive {drive} filled with random files successfully.");
                TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.NoProgress);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Log($"Error filling drive {drive}: {ex.Message}");
            }
        }

        private async Task WriteRandomFileAsync(string fileName, long fileSize, CancellationToken token)
        {
            byte[] buffer = new byte[bufferSizeInKB * 1024];
            Random rand = new Random();

            using (FileStream fs = new FileStream(fileName, FileMode.Create))
            {
                long bytesWritten = 0;
                while (bytesWritten < fileSize)
                {
                    if (token.IsCancellationRequested) throw new OperationCanceledException();

                    rand.NextBytes(buffer);
                    int bytesToWrite = (int)Math.Min(buffer.Length, fileSize - bytesWritten);
                    await fs.WriteAsync(buffer, 0, bytesToWrite, token);
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