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

                // Parse CLI arguments for headless operation
                string drive = GetArgument(args, "-drive");
                string passesStr = GetArgument(args, "-passes");
                bool quickFormat = args.Contains("-quick");

                int passes = 1; // default number of passes
                if (int.TryParse(passesStr, out int parsedPasses))
                {
                    passes = parsedPasses;
                }

                if (drive != null)
                {
                    // Headless operation with parsed arguments
                    AttachConsole(ATTACH_PARENT_PROCESS);
                    Console.WriteLine("Starting drive wipe operation...");

                    var purger = new HeadlessPurger();
                    await purger.PurgeDrive(drive, passes, quickFormat);

                    Console.WriteLine("Drive wipe operation completed.");
                    FreeConsole();
                    return;
                }
            }

            // Run GUI application if no CLI arguments or -help detected
            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());
        }

        private static void DisplayHelp()
        {
            Console.WriteLine();
            Console.WriteLine("  Options:");
            Console.WriteLine("  -drive=<drive_letter>   Specifies the drive letter to format and purge.");
            Console.WriteLine("  -passes=<number>        Specifies the number of overwrite passes (default: 1).");
            Console.WriteLine("  -quick                  Enables quick format mode.");
            Console.WriteLine("  -help                   Displays this help message.");
        }

        private static string GetArgument(string[] args, string option)
        {
            string arg = args.FirstOrDefault(a => a.StartsWith(option + "="));
            if (arg != null)
            {
                return arg.Substring(option.Length + 1);
            }
            return null;
        }
    }
}
