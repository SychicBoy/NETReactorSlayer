/*
    Copyright (C) 2021 CodeStrikers.org
    This file is part of NETReactorSlayer.
    NETReactorSlayer is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
    NETReactorSlayer is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.
    You should have received a copy of the GNU General Public License
    along with NETReactorSlayer.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Reflection;

namespace NETReactorSlayer.Core {
    internal class Logger {
        public static void Done(string message) {
            Console.Write("  [");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("✓");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("] ");
            Console.WriteLine(message);
        }

        public static void Warn(string message) {
            Console.Write("  [");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write("!");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("] ");
            Console.WriteLine(message);
        }

        public static void Error(string message, Exception ex = null) {
            if (Context.Options.Verbose && ex is { Message: { } })
                message += $" {ex.Message}.";
            Console.Write("  [");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("X");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("] ");
            Console.WriteLine(message);
        }

        private static void PrintSupportedVersions() {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("(");
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write("From 6.0 To 6.9");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write(") ");
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void PrintUsage() {
            var arguments = new List<string> {
                "--dec-methods BOOL", "              Decrypt methods body (True)",
                "--fix-proxy BOOL", "                Fix proxied calls (True)",
                "--dec-strings BOOL", "              Decrypt strings (True)",
                "--dec-rsrc BOOL", "                 Decrypt assembly resources (True)",
                "--dec-bools BOOL", "                Decrypt booleans (True)",
                "--deob-cflow BOOL", "               Deobfuscate control flow (True)",
                "--deob-tokens BOOL", "              Deobfuscate tokens (True)",
                "--dump-asm BOOL", "                 Dump embedded assemblies (True)",
                "--dump-costura BOOL", "             Dump assemblies that embedded by \"Costura.Fody\" (True)",
                "--inline-methods BOOL", "           Inline short methods (True)",
                "--rem-antis BOOL", "                Remove anti tamper & anti debugger (True)",
                "--rem-sn BOOL", "                   Remove strong name removal protection (True)",
                "--rem-calls BOOL", "                Remove calls to obfuscator methods (True)",
                "--rem-junks BOOL", "                Remove junk types, methods, fields, etc... (True)",
                "--rename FLAGS",
                "                  Rename n(amespaces), t(ypes), m(ethods), p(rops), e(vents), f(ields)",
                "--rename-short BOOL", "             Remove short names (False)",
                "--dont-rename BOOL", "              Don't rename classes, methods, etc... (False)",
                "--keep-types BOOL", "               Keep obfuscator types, methods, fields, etc... (False)",
                "--preserve-all BOOL", "             Preserve all metadata tokens (False)",
                "--keep-max-stack BOOL", "           Keep old max stack value (False)",
                "--no-pause BOOL", "                 Close cli immediately after deobfuscation (False)",
                "--verbose BOOL", "                  Verbose mode (False)"
            };
            Console.Write("  Usage: ");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("NETReactorSlayer <AssemblyPath> <Options>\r\n");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("  Options:");
            Console.ForegroundColor = ConsoleColor.Gray;
            for (var i = 0; i < arguments.Count; i += 2)
                Console.WriteLine("  " + arguments[i] + "   " + arguments[i + 1]);
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void PrintLogo() {
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
            Console.WriteLine("https://github.com/SychicBoy/NETReactorSlayer");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("  Version: ");
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine((Attribute.GetCustomAttribute(
                    Assembly.GetEntryAssembly() ?? throw new InvalidOperationException(),
                    typeof(AssemblyInformationalVersionAttribute)) as AssemblyInformationalVersionAttribute)
                ?.InformationalVersion);
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("  Supported .NET Reactor versions: ");
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            PrintSupportedVersions();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(Environment.NewLine + "  ==========================================================\r\n");
        }
    }
}