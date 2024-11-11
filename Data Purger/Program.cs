using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Data_Purger
{
    internal static class Program
    {
        [DllImport("kernel32.dll")]
        private static extern bool AttachConsole(int dwProcessId);

        [DllImport("kernel32.dll")]
        private static extern bool FreeConsole();

        private const int ATTACH_PARENT_PROCESS = -1;

        [STAThread]
        static async Task Main(string[] args)
        {
            if (args.Length > 0)
            {
                if (args.Contains("-help"))
                {
                    AttachConsole(ATTACH_PARENT_PROCESS);
                    DisplayHelp();
                    FreeConsole();
                    return;
                }

                string drive = GetArgument(args, "-drive");
                string passesStr = GetArgument(args, "-passes");
                string bufferStr = GetArgument(args, "-buffer");
                bool quickFormat = args.Contains("-quick");

                int passes = 1;
                int bufferSizeInKB = 64;

                if (int.TryParse(passesStr, out int parsedPasses))
                {
                    passes = parsedPasses;
                }

                if (bufferStr != null && int.TryParse(bufferStr, out int parsedBufferSize))
                {
                    bufferSizeInKB = Math.Clamp(parsedBufferSize, 64, 1024);
                }

                if (drive != null)
                {
                    AttachConsole(ATTACH_PARENT_PROCESS);
                    Console.WriteLine("Starting drive wipe operation...");

                    var purger = new CLI_Mode();
                    await purger.PurgeDrive(drive, passes, quickFormat, bufferSizeInKB);

                    Console.WriteLine("Drive wipe operation completed.");
                    FreeConsole();
                    return;
                }
            }

            ApplicationConfiguration.Initialize();
            Application.Run(new GUI_Mode());
        }

        private static void DisplayHelp()
        {
            Console.WriteLine();
            Console.WriteLine("  Options:");
            Console.WriteLine("  -drive=<drive_letter>   Specifies the drive letter to format and purge.");
            Console.WriteLine("  -passes=<number>        Specifies the number of overwrite passes (default: 1).");
            Console.WriteLine("  -quick                  Enables quick format mode.");
            Console.WriteLine("  -buffer=<size_kb>       Sets buffer size in KB (between 64 and 1024).");
            Console.WriteLine("  -help                   Displays this help message.");
        }

        private static string GetArgument(string[] args, string option)
        {
            string arg = args.FirstOrDefault(a => a.StartsWith(option + "="));
            return arg?.Substring(option.Length + 1);
        }
    }
}
