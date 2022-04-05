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
using System.Drawing;
using System.Windows.Forms;

namespace NETReactorSlayer.GUI
{
    internal class Logger
    {
        readonly RichTextBox RichTextBox = null;
        public Logger(RichTextBox RichTextBox) => this.RichTextBox = RichTextBox;

        public void Write(string text, Color? color = null)
        {
            if (RichTextBox.InvokeRequired)
            {
                RichTextBox.Invoke(new MethodInvoker(() =>
                {
                    Write(text, color);
                }));
                return;
            }
            RichTextBox.SelectionStart = RichTextBox.TextLength;
            RichTextBox.SelectionLength = 0;
            RichTextBox.SelectionColor = color ?? Color.Gray;
            RichTextBox.AppendText(text);
            RichTextBox.SelectionColor = RichTextBox.ForeColor;
            Application.DoEvents();
        }

        public void WriteLine(string text, Color? color = null)
        {
            Write($"{text}", color);
            if (RichTextBox.InvokeRequired)
            {
                RichTextBox.Invoke(new MethodInvoker(() =>
                {
                    RichTextBox.AppendText(Environment.NewLine);
                }));
                return;
            }
            RichTextBox.AppendText(Environment.NewLine);
        }

        public void Clear() => RichTextBox.Clear();
    }
}
