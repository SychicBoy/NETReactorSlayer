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
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace NETReactorSlayer.GUI;

internal static class Program
{
    /// <summary>
    ///     The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main(string[] args)
    {
        if (Environment.Version.Major >= 3)
            SetProcessDPIAware();
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        if (args is {Length: > 0})
            if (args[0] == "updated")
            {
                Application.Run(new MainWindow(args[0]));
                return;
            }

        Application.Run(new MainWindow());
    }

    [DllImport("user32.dll")] private static extern void SetProcessDPIAware();
}