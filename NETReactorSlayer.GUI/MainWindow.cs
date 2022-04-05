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
using dnlib.DotNet;
using dnlib.PE;
using NETReactorSlayer.GUI.Dialogs;
using NETReactorSlayer.GUI.UserControls;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NETReactorSlayer.GUI
{
    public partial class MainWindow : Form
    {
        public MainWindow()
        {
            this.InitializeComponent();
            lblVersion.Text = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;
            txtLogs.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, txtLogs.Width, txtLogs.Height, 25, 20));
            pnlBase.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, pnlBase.Width, pnlBase.Height, 25, 20));
            pnlTextBox.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, pnlTextBox.Width, pnlTextBox.Height, 25, 20));
            picBrowse.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, picBrowse.Width, picBrowse.Height, 25, 20));
            Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 25, 20));
            Logger = new Logger(txtLogs);
            txtLogs.MouseWheel += new MouseEventHandler((sender, e) =>
            {
                if (e.Delta < 0)
                    scrollbarLogs.Value++;
                else
                    scrollbarLogs.Value--;
                HideCaret(txtLogs.Handle);
            });
            txtLogs.KeyDown += new KeyEventHandler((sender, e) =>
            {
                if (e.KeyData == Keys.Down)
                {
                    IsLogsScrollLocked = true;
                    scrollbarLogs.Value++;
                }
                else if (e.KeyData == Keys.Up)
                {
                    IsLogsScrollLocked = true;
                    scrollbarLogs.Value--;
                }
                HideCaret(txtLogs.Handle);
            });
            txtLogs.MouseMove += new MouseEventHandler((sender, e) =>
            {
                if (e.Button == MouseButtons.Left)
                    pnlBase.Focus();
                HideCaret(txtLogs.Handle);
            });
            txtLogs.SelectionChanged += new EventHandler((sender, e) =>
            {
                txtLogs.SelectionLength = 0;
                HideCaret(txtLogs.Handle);
            });
            ctxLogs.Renderer = new ToolStripProfessionalRenderer(new MenuColorTable());
        }

        async void btnStart_Click(object sender, EventArgs e)
        {
            if (btnStart.Tag != null && btnStart.Tag.ToString() == "Busy")
                return;
            if (!CheckInputFile(txtInput.Text))
            {
                btnStart.Tag = "Busy";
                if (File.Exists(txtInput.Text))
                    txtInput.Text = "Access to file path denied";
                else
                    txtInput.Text = "Could not find a part of the file path";
                txtInput.ForeColor = Color.Firebrick;
                await Task.Delay(2000);
                txtInput.ForeColor = Color.Silver;
                txtInput.Text = string.Empty;
                btnStart.Tag = null;
                return;
            }
            SetButtonStatus(true);
            await Task.Delay(500);
            Arguments.Clear();
            Arguments.Append(txtInput.Text);
            Logger.Clear();
            foreach (Control control in tabelOptions.Controls)
                if (control.Tag is string command && control is NRSCheckBox checkBox)
                    Arguments.Append($" {command} {checkBox.Checked}");
            Arguments.Append($" --no-pause True");
            Logger.Write("\r\n  Started deobfuscation: ");
            Logger.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}", Color.SteelBlue);
            bool IsX64 = false;
            try
            {
                using (ModuleDefMD Module = ModuleDefMD.Load(txtInput.Text))
                {
                    Logger.Write("  Assembly: ");
                    Logger.WriteLine(Module.Name, Color.SteelBlue);
                    Logger.Write("  Architecture: ");
                    if (IsX64 = (!Module.Is32BitPreferred && !Module.Is32BitRequired))
                        Logger.WriteLine("X64", Color.SteelBlue);
                    else
                        Logger.WriteLine("X86", Color.SteelBlue);
                }
            }
            catch
            {
                try
                {
                    using (PEImage Image = new PEImage(txtInput.Text))
                    {

                        IsX64 = (Image.ImageNTHeaders.FileHeader.Machine != dnlib.PE.Machine.I386);
                        Logger.Write("  Assembly: ");
                        Logger.WriteLine(Path.GetFileName(Image.Filename), Color.SteelBlue);
                        Logger.Write("  Architecture: ");
                        if (IsX64)
                            Logger.WriteLine("X64", Color.SteelBlue);
                        else
                            Logger.WriteLine("X86", Color.SteelBlue);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Write("  Error: ");
                    Logger.WriteLine(ex.Message.Replace("\r", "").Replace("\n", ". "), Color.Firebrick);
                    SetButtonStatus(false);
                    return;
                }
            }

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8,
                Arguments = Arguments.ToString(),
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false
            };
            if (!IsX64)
                startInfo.FileName = $"{Path.Combine(Environment.CurrentDirectory, "NETReactorSlayer.CLI.exe")}";
            else
                startInfo.FileName = $"{Path.Combine(Environment.CurrentDirectory, "NETReactorSlayer-x64.CLI.exe")}";
            bool IsPIDLogged = false;
            var process = new Process();
            process.OutputDataReceived += new DataReceivedEventHandler((sndr, evnt) =>
            {
                if (!IsPIDLogged)
                {
                    IsPIDLogged = true;
                    Logger.Write("  CLI Started, PID: ");
                    Logger.WriteLine(process.Id.ToString());
                    Logger.WriteLine("  =====================================\r\n");
                }
                if (evnt.Data == null)
                    return;
                string data = evnt.Data;
                string prefix = string.Empty;
                Color prefixColor = Color.Empty;
                if (data.Contains("["))
                    prefix = data.Substring(data.IndexOf("[") + 1, data.IndexOf("]") - 3);
                if (prefix == "X")
                    prefixColor = Color.Firebrick;
                else if (prefix == "!")
                    prefixColor = Color.Gold;
                else if (prefix == "✓")
                    prefixColor = Color.MediumSeaGreen;
                if (!string.IsNullOrWhiteSpace(prefix) && prefixColor != Color.Empty)
                {
                    Logger.Write("  [");
                    Logger.Write(prefix, prefixColor);
                    Logger.Write("] ");
                    data = data.Substring(6);
                    while (data.Length > 0 && data[0] == ' ')
                        data = data.Substring(1);
                }
                Logger.WriteLine(data);
            });
            process.ErrorDataReceived += new DataReceivedEventHandler((sndr, evnt) =>
            {
                if (evnt.Data == null)
                    return;
                Logger.Write("  Error: ");
                Logger.WriteLine(evnt.Data.Replace("\r", "").Replace("\n", ". "), Color.Firebrick);
            });
            process.EnableRaisingEvents = true;
            process.StartInfo = startInfo;
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            while (!process.HasExited)
                await Task.Delay(1000);
            SetButtonStatus(false);
            pnlBase.Focus();
            this.SuspendLayout();
            MsgBox.Show("The deobfuscation process is complete, For more info checkout logs section.", "Completed", MsgBox.MsgButtons.OK, MsgBox.MsgIcon.Info, this);
            this.Focus();
            this.ResumeLayout(false);
        }

        new void Closing(object sender, FormClosingEventArgs e)
        {
            if (IsClosing)
                return;
            IsClosing = true;
            e.Cancel = true;
            CloseAnimated();
        }

        void MainWindow_Shown(object sender, EventArgs e) => ShowAnimated();

        void picExit_Click(object sender, EventArgs e) => Close();

        void picExit_MouseEnter(object sender, EventArgs e) => picExit.Image = Properties.Resources.CloseOver;

        void picExit_MouseLeave(object sender, EventArgs e) => picExit.Image = Properties.Resources.Close;

        void picMinimize_Click(object sender, EventArgs e) => this.WindowState = FormWindowState.Minimized;

        void picMinimize_MouseEnter(object sender, EventArgs e) => picMinimize.Image = Properties.Resources.MinimizeOver;

        void picMinimize_MouseLeave(object sender, EventArgs e) => picMinimize.Image = Properties.Resources.Minimize;

        async void txtInput_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files != null && files.Length != 0)
            {
                if (CheckInputFile(files[0]))
                    txtInput.Text = files[0];
                else
                {
                    if (File.Exists(files[0]))
                        txtInput.Text = "Access to file path denied";
                    else
                        txtInput.Text = "Could not find a part of the file path";
                    txtInput.ForeColor = Color.Firebrick;
                    await Task.Delay(2000);
                    txtInput.ForeColor = Color.Silver;
                    txtInput.Text = String.Empty;
                }
            }
        }

        void txtInput_DragEnter(object sender, DragEventArgs e) => e.Effect = DragDropEffects.All;

        void OnMouseDown(object sender, MouseEventArgs e)
        {
            IsMouseDown = true;
            LastLocation = e.Location;
            Opacity = 0.90;
        }

        void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (IsMouseDown)
            {
                this.Location = new Point(
                    (this.Location.X - LastLocation.X) + e.X, (this.Location.Y - LastLocation.Y) + e.Y);

                this.Update();
            }
        }

        void OnMouseUp(object sender, MouseEventArgs e)
        {
            IsMouseDown = false;
            Opacity = 1.0;
        }

        private void scrollbarLogs_ValueChanged(object sender, ScrollValueEventArgs e)
        {
            if (IsLogsScrollLocked)
            {
                IsLogsScrollLocked = false;
                return;
            }
            try
            {
                BeginControlUpdate(txtLogs);
                if (scrollbarLogs.Value > txtLogs.Lines.Count()) return;
                txtLogs.SelectionStart = txtLogs.Find(txtLogs.Lines[scrollbarLogs.Value]) - 1;
                txtLogs.SelectionLength = 0;
                txtLogs.ScrollToCaret();
            }
            catch { }
            EndControlUpdate(txtLogs);
        }

        private void txtLogs_TextChanged(object sender, EventArgs e)
        {
            scrollbarLogs.SuspendLayout();
            scrollbarLogs.Maximum = txtLogs.Lines.Count();
            scrollbarLogs.Value = txtLogs.Lines.Count();
            scrollbarLogs.Invalidate();
            scrollbarLogs.ResumeLayout();
        }

        protected virtual void OnFormWindowStateChanged(EventArgs e)
        {
            if (WindowState == FormWindowState.Maximized)
                WindowState = FormWindowState.Normal;
            CenterToScreen();
        }

        private void llblWebsite_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) => Process.Start((sender as LinkLabel).Tag.ToString());

        private void llblGitHub_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) => Process.Start((sender as LinkLabel).Tag.ToString());

        private void picBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Assembly (*.exe,*.dll)| *.exe;*.dll";
            openFileDialog.Title = "Select Assembly";
            openFileDialog.Multiselect = false;
            openFileDialog.CheckFileExists = true;
            openFileDialog.CheckPathExists = true;
            openFileDialog.RestoreDirectory = true;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
                txtInput.Text = openFileDialog.FileName;
        }

        private void copyLogsToolStripMenuItem_Click(object sender, EventArgs e) => Clipboard.SetText(txtLogs.Text);

        protected override void WndProc(ref Message m)
        {
            FormWindowState org = this.WindowState;
            base.WndProc(ref m);
            if (this.WindowState != org)
                this.OnFormWindowStateChanged(EventArgs.Empty);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ClassStyle |= 0x00020000;
                return cp;
            }
        }
        void ShowAnimated()
        {
            var timer = new Timer
            {
                Interval = 10
            };
            timer.Tick += delegate
            {
                if (Opacity < 1.0) Opacity += 0.05;
                if (Opacity >= 1.0)
                {
                    timer.Stop();
                    Show();
                    Opacity = 1.0;
                    timer.Dispose();
                }
            };
            timer.Start();
        }

        void CloseAnimated()
        {
            var timer = new Timer
            {
                Interval = 10
            };
            timer.Tick += delegate
            {
                if (Opacity > 0.0) Opacity += -0.1;
                if (Opacity <= 0.0)
                {
                    Opacity = 0.0;
                    timer.Stop();
                    timer.Dispose();
                    Close();
                }
            };
            timer.Start();
        }

        bool CheckInputFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                try
                {
                    File.OpenRead(filePath).Close();
#pragma warning disable CS0642
                    using (FileStream fs = File.Create(Path.Combine(Path.GetDirectoryName(filePath), Path.GetRandomFileName()), 1, FileOptions.DeleteOnClose)) ;
#pragma warning restore CS0642
                    return true;
                }
                catch { }
            }
            return false;
        }

        void SetButtonStatus(bool IsBusy)
        {
            if (IsBusy)
            {
                btnStart.BackColor = Color.FromArgb(32, 32, 32);
                btnStart.FlatAppearance.MouseOverBackColor = btnStart.BackColor;
                btnStart.FlatAppearance.MouseDownBackColor = btnStart.BackColor;
                btnStart.Image = Properties.Resources.Loading;
                btnStart.Cursor = Cursors.WaitCursor;
                btnStart.Text = string.Empty;
                btnStart.Tag = "Busy";
            }
            else
            {
                btnStart.BackColor = Color.FromArgb(27, 27, 27);
                btnStart.FlatAppearance.MouseOverBackColor = Color.FromArgb(32, 32, 32);
                btnStart.FlatAppearance.MouseDownBackColor = Color.FromArgb(18, 18, 18);
                btnStart.Image = null;
                btnStart.Cursor = Cursors.Hand;
                btnStart.Text = "Start Deobfuscation";
                btnStart.Tag = null;
            }
        }

        void BeginControlUpdate(Control control)
        {
            Message msgSuspendUpdate = Message.Create(control.Handle, WM_SETREDRAW, IntPtr.Zero,
                  IntPtr.Zero);

            NativeWindow window = NativeWindow.FromHandle(control.Handle);
            window.DefWndProc(ref msgSuspendUpdate);
        }

        void EndControlUpdate(Control control)
        {
            IntPtr wparam = new IntPtr(1);
            Message msgResumeUpdate = Message.Create(control.Handle, WM_SETREDRAW, wparam,
                  IntPtr.Zero);

            NativeWindow window = NativeWindow.FromHandle(control.Handle);
            window.DefWndProc(ref msgResumeUpdate);
            control.Invalidate();
            control.Refresh();
        }

        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        public static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

        [DllImport("user32.dll")]
        private static extern int HideCaret(IntPtr hwnd);

        bool IsLogsScrollLocked = false;
        private const int WM_SETREDRAW = 11;
        Point LastLocation;
        bool IsMouseDown;
        bool IsClosing = false;
        readonly StringBuilder Arguments = new StringBuilder();
        readonly Logger Logger = null;
    }
}
