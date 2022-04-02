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
using System.IO;
using System.Threading;

namespace NETReactorSlayer.Core
{
    public class Program
    {
        public static void Main(string[] args)
        {
            #region Delete Temporary Files
            if (args != null && args.Length == 3 && args[0] == "--delete-native-image" && int.TryParse(args[1], out int id) && File.Exists(args[2]))
            {
                try
                {
                    var process = Process.GetProcessById(id);
                    if (process != null)
                    {
                        process.WaitForExit();
                        while (File.Exists(args[2]))
                        {
                            try
                            {
                                File.Delete(args[2]);
                            }
                            catch { }
                            Thread.Sleep(1000);
                        }
                        Process.GetCurrentProcess().Kill();
                        return;
                    }
                }
                catch { }
            }
            #endregion
            Console.Title = ".NET Reactor Slayer";
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Clear();
            Logger.PrintLogo();
            Context = new DeobfuscatorContext();
            if (Context.Parse(args))

            {
                Logger.Done($"{Context.DeobfuscatorOptions.Stages.Count}/{Context.DeobfuscatorOptions.Dictionary.Count} Modules loaded...");
                foreach (var DeobfuscatorStage in Context.DeobfuscatorOptions.Stages)
                {
                    try
                    {
                        DeobfuscatorStage.Execute();
                    }
                    catch (Exception exception)
                    {
                        Logger.Error($"{DeobfuscatorStage.GetType().Name} => {exception.Message}");
                    }
                }
                Context.Save();
            }
            if (!Context.NoPause)
            {
                Console.WriteLine("\r\n  Press any key to exit . . .");
                Console.ReadKey();
            }
        }
        public static DeobfuscatorContext Context = new DeobfuscatorContext();
    }
}
