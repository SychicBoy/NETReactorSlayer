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

namespace NETReactorSlayer.GUI;

internal class Logger
{
    public Logger(RichTextBox richTextBox) => _richTextBox = richTextBox;

    public void Write(string text, Color? color = null)
    {
        if (_richTextBox.InvokeRequired)
        {
            _richTextBox.Invoke(new MethodInvoker(() => { Write(text, color); }));
            return;
        }

        _richTextBox.SelectionStart = _richTextBox.TextLength;
        _richTextBox.SelectionLength = 0;
        _richTextBox.SelectionColor = color ?? Color.Gray;
        _richTextBox.AppendText(text);
        _richTextBox.SelectionColor = _richTextBox.ForeColor;
        Application.DoEvents();
    }

    public void WriteLine(string text, Color? color = null)
    {
        Write($"{text}", color);
        if (_richTextBox.InvokeRequired)
        {
            _richTextBox.Invoke(new MethodInvoker(() => { _richTextBox.AppendText(Environment.NewLine); }));
            return;
        }

        _richTextBox.AppendText(Environment.NewLine);
    }

    public void Clear() => _richTextBox.Clear();

    private readonly RichTextBox _richTextBox;
}