using NETReactorSlayer.Core.Protections;
using NETReactorSlayer.Core.Utils;
using System;
using System.Diagnostics;

namespace NetReactorSlayer.Core
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.Title = ".NET Reactor Slayer v" + Variables.version + " by CS-RET";
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Clear();
            Logger.PrintLogo();
            if (Context.Parse(args))
            {
                if (Variables.options["necrobit"]) NecroBit.Execute();

                ControlFlow.Execute();

                Anti.Execute(
                    Variables.options["antidebug"],
                    Variables.options["antitamper"]);

                if (Variables.options["proxycall"]) ProxyCall.Execute();

                if (Variables.options["hidecall"]) HideCall.Execute();

                if (Variables.options["str"]) Strings.Execute();

                if (Variables.options["rsrc"]) Resources.Execute();

                if (Variables.options["dump"]) EmbeddedAsm.Execute();

                Remover.Execute();
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
