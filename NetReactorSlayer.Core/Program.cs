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
using NETReactorSlayer.Core.Protections;
using NETReactorSlayer.Core.Utils;
using System;
using System.Diagnostics;

namespace NetReactorSlayer.Core
{
    public class Program
    {
        static void OnExit(object sender, EventArgs e)
        {
            if (!Context.IsNative) return;
            Process.Start(new ProcessStartInfo("cmd.exe", "/C ping 1.1.1.1 -n 1 -w 3000 > Nul & Del \"" + Context.FilePath + "\"") { WindowStyle = ProcessWindowStyle.Hidden }).Dispose();
            Process.GetCurrentProcess().Kill();
        }

        public static void Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnExit);
            Console.Title = ".NET Reactor Slayer v" + Variables.version + " by CS-RET";
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Clear();
            Logger.PrintLogo();
            if (Context.Parse(args))
            {
                try
                {
                    if (Variables.options["necrobit"]) NecroBit.Execute();
                }
                catch (Exception ex)
                {
                    Logger.Error("Failed to decrypt methods. " + ex.Message);
                }
                try
                {
                    ControlFlow.Execute();
                }
                catch (Exception ex)
                {
                    Logger.Error("Failed clean cflow. " + ex.Message);
                }
                try
                {
                    Anti.Execute(
                        Variables.options["antidebug"],
                        Variables.options["antitamper"]);
                }
                catch (Exception ex)
                {
                    Logger.Error("Failed to remove anti debugger or anti tamper. " + ex.Message);
                }
                try
                {
                    if (Variables.options["proxycall"]) ProxyCall.Execute();
                }
                catch (Exception ex)
                {
                    Logger.Error("Failed to remove proxied calls. " + ex.Message);
                }
                try
                {
                    if (Variables.options["hidecall"]) HideCall.Execute();
                }
                catch (Exception ex)
                {
                    Logger.Error("Failed to restore hidden calls. " + ex.Message);
                }
                try
                {
                    if (Variables.options["str"]) Strings.Execute();
                }
                catch (Exception ex)
                {
                    Logger.Error("Failed to decrypt strings. " + ex.Message);
                }
                try
                {
                    if (Variables.options["rsrc"]) Resources.Execute();
                }
                catch (Exception ex)
                {
                    Logger.Error("Failed to decrypt resources. " + ex.Message);
                }
                try
                {
                    if (Variables.options["dump"]) EmbeddedAsm.Execute();
                }
                catch (Exception ex)
                {
                    Logger.Error("Failed to dump embedded assemblies. " + ex.Message);
                }
                try
                {
                    if (Variables.options["decrypttoken"]) Token.Execute();
                }
                catch (Exception ex)
                {
                    Logger.Error("Failed decrypt tokens. " + ex.Message);
                }
                try
                {
                    if (Variables.options["remove"]) Remover.Execute();
                }
                catch { }
                Context.Save();
            }
            Console.WriteLine("\r\n  Press any key to exit . . .");
            Console.ReadKey();
            if (!Context.IsNative) return;
            Process.Start(new ProcessStartInfo("cmd.exe", "/C ping 1.1.1.1 -n 1 -w 3000 > Nul & Del \"" + Context.FilePath + "\"") { WindowStyle = ProcessWindowStyle.Hidden }).Dispose();
            Process.GetCurrentProcess().Kill();
        }
    }
}
