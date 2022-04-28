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
using System.Text;
using System.Threading;

namespace NETReactorSlayer.Core;

public class Program
{
    public static Context Context = new();

    public static void Main(string[] args)
    {
        #region Delete Temporary Files

        if (args is {Length: 3} && args[0] == "--delete-native-image" && int.TryParse(args[1], out var id) &&
            File.Exists(args[2]))
            try
            {
                if (Process.GetProcessById(id) is { } process)
                {
                    process.WaitForExit();
                    while (File.Exists(args[2]))
                    {
                        try
                        {
                            File.Delete(args[2]);
                        } catch { }

                        Thread.Sleep(1000);
                    }

                    Process.GetCurrentProcess().Kill();
                    return;
                }
            } catch { }

        #endregion

        Console.Title = ".NET Reactor Slayer";
        Console.OutputEncoding = Encoding.UTF8;
        Console.BackgroundColor = ConsoleColor.Black;
        Console.ForegroundColor = ConsoleColor.White;
        try
        {
            Console.Clear();
            Logger.PrintLogo();
        } catch { }

        Context = new Context();
        if (Context.Parse(args))
        {
            foreach (var deobfuscatorStage in Context.DeobfuscatorOptions.Stages)
                try
                {
                    deobfuscatorStage.Execute();
                } catch (Exception ex)
                {
                    Logger.Error($"{deobfuscatorStage.GetType().Name}: {ex.Message}");
                }

            Context.Save();
        }

        if (!Context.NoPause)
        {
            Console.WriteLine("\r\n  Press any key to exit . . .");
            Console.ReadKey();
        }
    }
}