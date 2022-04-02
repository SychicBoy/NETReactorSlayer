/*
    Copyright (C) 2021 CodeStrikers.org
    This file is part of NetReactorSlayer.
    NetReactorSlayer is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
    NetReactorSlayer is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.
    You should have received a copy of the GNU General Public License
    along with NetReactorSlayer.  If not, see <http://www.gnu.org/licenses/>.
*/
using System;
using System.Diagnostics;
using System.Reflection;

namespace NETReactorSlayer.Core
{
    public static class Logger
    {
        public static bool Prompt(string message)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("  [PROMPT] " + message);
            return Console.ReadLine().ToLower() == "y";
        }

        public static void Done(string message)
        {
            Console.Write("  [");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("✓");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("] ");
            Console.WriteLine(message);
        }

        public static void Warn(string message)
        {
            Console.Write("  [");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write("!");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("] ");
            Console.WriteLine(message);
        }

        public static void Error(string message)
        {
            Console.Write("  [");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("X");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("] ");
            Console.WriteLine(message);
        }

        static void PrintSupportedVersions()
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("(");
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write("From 6.0 To 6.8");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write(") ");
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void PrintUsage()
        {
            Console.Write("  Usage: ");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("NETReactorSlayer <AssemblyPath> <Options>\r\n");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("  Options:");
            Console.ForegroundColor = ConsoleColor.Gray;
            for (int i = 0; i < Program.Context.DeobfuscatorOptions.Arguments.Count; i += 2)
            {
                Console.WriteLine("  " + Program.Context.DeobfuscatorOptions.Arguments[i] + "   " + Program.Context.DeobfuscatorOptions.Arguments[i + 1]);
            }
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void PrintLogo()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(@"
  ░█▄─░█ ░█▀▀▀ ▀▀█▀▀ 
  ░█░█░█ ░█▀▀▀ ─░█── 
  ░█──▀█ ░█▄▄▄ ─░█── 

  ░█▀▀█ ░█▀▀▀ ─█▀▀█ ░█▀▀█ ▀▀█▀▀ ░█▀▀▀█ ░█▀▀█ 
  ░█▄▄▀ ░█▀▀▀ ░█▄▄█ ░█─── ─░█── ░█──░█ ░█▄▄▀ 
  ░█─░█ ░█▄▄▄ ░█─░█ ░█▄▄█ ─░█── ░█▄▄▄█ ░█─░█ 

  ░█▀▀▀█ ░█─── ─█▀▀█ ░█──░█ ░█▀▀▀ ░█▀▀█ 
  ─▀▀▀▄▄ ░█─── ░█▄▄█ ░█▄▄▄█ ░█▀▀▀ ░█▄▄▀ 
  ░█▄▄▄█ ░█▄▄█ ░█─░█ ──░█── ░█▄▄▄ ░█─░█

");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("  .NET Reactor Slayer by CS-RET");
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
            Console.WriteLine(FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion);
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("  Supported .NET Reactor versions: ");
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            PrintSupportedVersions();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(Environment.NewLine + "  ==========================================================\r\n");
        }
    }
}
