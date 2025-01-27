# Data Purger

![image](https://github.com/user-attachments/assets/842acb89-8d67-4ea2-af80-9bebee3272eb)


**Data Purger** is an application for securely wiping removable drives through formatting and overwriting with random files. This tool supports both a **Graphical User Interface (GUI)** and a **Command-Line Interface (CLI)**, allowing for flexible use in both interactive and automated environments.

---

## Features

- **Multi-pass wipe**: Specify the number of passes to overwrite data multiple times for enhanced security.
- **Random file overwrite**: Fills the drive with random files, further protecting data from recovery.
- **GUI and CLI support**: Use the GUI for manual operation or the CLI for batch processing and scripting.

## Usage

### GUI Mode

Run `Data_Purger.exe`, select the drive, set the number of passes, and choose between a full or quick format.

### CLI Mode

Run `DataPurger.exe` with command-line arguments for automated operation.

**Syntax**:

- -drive=<drive_letter>   Specifies the drive letter to format and purge
- -passes=<number>        Specifies the number of overwrite passes (default: 1)
- -quick                  Enables quick format mode
- -buffer=<size_kb>       Sets buffer size in KB (between 64 and 1024)
- -help                   Displays help message

