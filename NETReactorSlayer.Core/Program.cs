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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using NETReactorSlayer.Core.Deobfuscators;

namespace NETReactorSlayer.Core
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (!CheckArguments(args))
                return;

            Console.Title = ".NET Reactor Slayer";
            Console.OutputEncoding = Encoding.UTF8;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            try
            {
                Console.Clear();
                Logger.PrintLogo();
            }
            catch
            {
            }

            Context = new Context();
            if (Context.Load(new Options(args)))
            {
                DeobfuscateBegin();
                DeobfuscateEnd();
            }

            if (Context.Options.NoPause)
                return;

            Console.WriteLine("\r\n  Press any key to exit . . .");
            Console.ReadKey();
        }

        #region Fields

        public static Context Context = new Context();

        #endregion

        #region Private Methods

        private static bool CheckArguments(string[] args)
        {
            if (args.Length != 3 || args[0] != "--del-temp" ||
                !int.TryParse(args[1], out var id) || !File.Exists(args[2])) return true;

            try
            {
                if (Process.GetProcessById(id) is Process process)
                {
                    process.WaitForExit();
                    while (File.Exists(args[2]))
                    {
                        try
                        {
                            File.Delete(args[2]);
                        }
                        catch
                        {
                        }

                        Thread.Sleep(1000);
                    }

                    Process.GetCurrentProcess().Kill();
                    return false;
                }
            }
            catch
            {
            }

            return true;
        }

        private static void DeobfuscateBegin()
        {
            foreach (var deobfuscatorStage in Context.Options.Stages)
            {
                var thread = new Thread(() =>
                {
                    try
                    {
                        deobfuscatorStage.Execute();
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"{deobfuscatorStage.GetType().Name}: {ex.Message}");
                    }
                }, 1024 * 1024 * 64);
                thread.Start();
                thread.Join();
                while (thread.IsAlive) Thread.Sleep(500);
            }
        }

        private static void DeobfuscateEnd()
        {
            if (Context.Options.Stages.Any(x => x.GetType().Name.Equals(nameof(MethodInliner))))
                if (MethodInliner.InlinedMethods > 0)
                    Logger.Done(MethodInliner.InlinedMethods + " Methods inlined.");
                else Logger.Warn("Couldn't find any outline method.");

            Context.Save();
        }

        #endregion
    }
}