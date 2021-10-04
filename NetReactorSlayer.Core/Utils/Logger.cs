using System;

namespace NETReactorSlayer.Core.Utils
{
    public static class Logger
    {
        public static void Debug(string message)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  [DEBUG] " + message);
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void Info(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  [DONE] " + message);
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void Warn(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  [WARN] " + message);
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void Error(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("  [ERROR] " + message);
            Console.ForegroundColor = ConsoleColor.White;
        }

        static void PrintSupportedVersions()
        {
            foreach (var item in Variables.supportedVersions)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write("(");
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.Write(item);
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write(") ");
            }
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void PrintUsage()
        {
            Console.WriteLine("  Usage: NETReactorSlayer <AssemblyPath> <Options>\r\n  Options:");
            Console.ForegroundColor = ConsoleColor.Gray;
            for (int i = 0; i < Variables.arguments.Length; i += 2)
            {
                Console.WriteLine("  " + Variables.arguments[i] + "   " + Variables.arguments[i + 1]);
            }
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void PrintLogo()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(@"
         __  __  _____     __                 _               __ _                        
      /\ \ \/__\/__   \   /__\ ___  __ _  ___| |_ ___  _ __  / _\ | __ _ _   _  ___ _ __  
     /  \/ /_\    / /\/  / \/// _ \/ _` |/ __| __/ _ \| '__| \ \| |/ _` | | | |/ _ \ '__| 
   _/ /\  //__   / /    / _  \  __/ (_| | (__| || (_) | |    _\ \ | (_| | |_| |  __/ |    
  (_)_\ \/\__/   \/     \/ \_/\___|\__,_|\___|\__\___/|_|    \__/_|\__,_|\__, |\___|_|    
                                                                       |___/");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("  .NET Reactor Slayer by CS-RET");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("  Website: ");
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("www.CodeStrikers.org");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("  Latest version on Github: ");
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("https://github.com/SychicBoy/NetReactorSlayer");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("  Version: ");
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine(Variables.version);
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("  Supported .NET Reactor versions: ");
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            PrintSupportedVersions();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(Environment.NewLine + "  ==========================");
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
