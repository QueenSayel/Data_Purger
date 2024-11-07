# Data Purger
![image](https://github.com/user-attachments/assets/00010da2-5a0c-4122-8e84-5714a2e0cdb1)

**Data Purger** is a C# WinForms application for securely wiping USB drives through formatting and overwriting with random files. This tool supports both a **Graphical User Interface (GUI)** and a **Command-Line Interface (CLI)**, allowing for flexible use in both interactive and automated environments.

---

## Features

- **Multi-Pass Wipe**: Specify the number of passes to overwrite data multiple times for enhanced security.
- **Quick Format Option**: Choose between a full or quick format for faster operations.
- **Random File Overwrite**: Fills the drive with random files, further protecting data from recovery.
- **GUI and CLI Support**: Use the GUI for manual operation or the CLI for batch processing and scripting.
- **USB Detection**: Automatically detects USB drive insertion/removal, refreshing available drive options.

---

## Requirements

- **.NET Framework** or **.NET Core** (version depends on your setup).
- **Administrator privileges** are required to perform drive formatting.

## Installation

1. Clone the repository:
    ```bash
    git clone https://github.com/your-username/DataPurger.git
    ```

2. Open the project in **Visual Studio** and build it.

3. Run the executable (`DataPurger.exe`) to start the application.

---

## Usage

### GUI Mode

Run `DataPurger.exe` without any arguments to open the GUI. From here, select the drive, set the number of passes, and choose between a full or quick format.

### CLI Mode

Run `DataPurger.exe` with command-line arguments for automated operation. CLI mode is ideal for batch operations and scripting.

**Syntax**:

WILL COMPLETE LATER

