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
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using dnlib.DotNet;
using dnlib.PE;
using NETReactorSlayer.GUI.Dialogs;
using NETReactorSlayer.GUI.Properties;
using NETReactorSlayer.GUI.UserControls;

namespace NETReactorSlayer.GUI;

public partial class MainWindow : Form
{
    public MainWindow(string arg = "")
    {
        if (arg == "updated")
            MsgBox.Show(Resources.ChangeLogs.Replace("\n-", "\n● "), "What's New", MsgBox.MsgButtons.Ok,
                MsgBox.MsgIcon.Info, this);
        InitializeComponent();
        lblVersion.Text = InformationalVersion;
        txtLogs.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, txtLogs.Width, txtLogs.Height, 25, 20));
        pnlBase.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, pnlBase.Width, pnlBase.Height, 25, 20));
        pnlTextBox.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, pnlTextBox.Width, pnlTextBox.Height, 25, 20));
        picBrowse.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, picBrowse.Width, picBrowse.Height, 25, 20));
        Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 25, 20));
        _logger = new Logger(txtLogs);
        txtLogs.MouseWheel += (_, e) =>
        {
            if (e.Delta < 0)
                scrollbarLogs.Value++;
            else
                scrollbarLogs.Value--;
            HideCaret(txtLogs.Handle);
        };
        txtLogs.KeyDown += (_, e) =>
        {
            switch (e.KeyData)
            {
                case Keys.Down:
                    _isLogsScrollLocked = true;
                    scrollbarLogs.Value++;
                    break;
                case Keys.Up:
                    _isLogsScrollLocked = true;
                    scrollbarLogs.Value--;
                    break;
            }

            HideCaret(txtLogs.Handle);
        };
        txtLogs.MouseMove += (_, e) =>
        {
            if (e.Button == MouseButtons.Left)
                pnlBase.Focus();
            HideCaret(txtLogs.Handle);
        };
        txtLogs.SelectionChanged += (_, _) =>
        {
            txtLogs.SelectionLength = 0;
            HideCaret(txtLogs.Handle);
        };
        ctxLogs.Renderer = new ToolStripProfessionalRenderer(new MenuColorTable());
        ctxMenu.Renderer = new ToolStripProfessionalRenderer(new MenuColorTable());
        ctxRename.Renderer = new ToolStripProfessionalRenderer(new MenuColorTable());
    }

    private async void btnStart_Click(object sender, EventArgs e)
    {
        if (btnStart.Tag != null && btnStart.Tag.ToString() == "Busy")
            return;
        if (!CheckInputFile(txtInput.Text))
        {
            btnStart.Tag = "Busy";
            txtInput.Text = File.Exists(txtInput.Text)
                ? @"Access to file path denied"
                : @"Could not find a part of the file path";
            txtInput.ForeColor = Color.Firebrick;
            await Task.Delay(2000);
            txtInput.ForeColor = Color.Silver;
            txtInput.Text = string.Empty;
            btnStart.Tag = null;
            return;
        }

        SetButtonStatus(true);
        await Task.Delay(500);
        _arguments.Clear();
        _arguments.Append($"\"{txtInput.Text}\"");
        _logger.Clear();
        foreach (Control control in tabelOptions.Controls)
            if (control.Tag is string command && control is NrsCheckBox checkBox)
                _arguments.Append($" {command} {checkBox.Checked}");
        _arguments.Append(" --no-pause True");
        _logger.Write("\r\n  Started deobfuscation: ");
        _logger.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}", Color.SteelBlue);
        bool isX64;
        try
        {
            using var module = ModuleDefMD.Load(txtInput.Text);
            _logger.Write("  Assembly: ");
            _logger.WriteLine(module.Name, Color.SteelBlue);
            _logger.Write("  Architecture: ");
            if (isX64 = !module.Is32BitPreferred && !module.Is32BitRequired)
                _logger.WriteLine("X64", Color.SteelBlue);
            else
                _logger.WriteLine("X86", Color.SteelBlue);
        }
        catch
        {
            try
            {
                using var image = new PEImage(txtInput.Text);
                isX64 = image.ImageNTHeaders.FileHeader.Machine != Machine.I386;
                _logger.Write("  Assembly: ");
                _logger.WriteLine(Path.GetFileName(image.Filename), Color.SteelBlue);
                _logger.Write("  Architecture: ");
                _logger.WriteLine(isX64 ? "X64" : "X86", Color.SteelBlue);
            }
            catch (Exception ex)
            {
                _logger.Write("  Error: ");
                _logger.WriteLine(ex.Message.Replace("\r", "").Replace("\n", ". "), Color.Firebrick);
                SetButtonStatus(false);
                return;
            }
        }

        var startInfo = new ProcessStartInfo
        {
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = Encoding.UTF8,
            Arguments = _arguments.ToString(),
            WindowStyle = ProcessWindowStyle.Hidden,
            UseShellExecute = false,
            FileName = !isX64
                ? $"{Path.Combine(Environment.CurrentDirectory, "NETReactorSlayer.CLI.exe")}"
                : $"{Path.Combine(Environment.CurrentDirectory, "NETReactorSlayer-x64.CLI.exe")}"
        };
        var isPidLogged = false;
        var process = new Process();
        process.OutputDataReceived += (_, evnt) =>
        {
            if (!isPidLogged)
            {
                isPidLogged = true;
                _logger.Write("  CLI Started, PID: ");
                _logger.WriteLine(process.Id.ToString());
                _logger.WriteLine("  =====================================\r\n");
            }

            if (evnt.Data == null)
                return;
            var data = evnt.Data;
            var prefix = string.Empty;
            var prefixColor = Color.Empty;
            if (data.Contains("["))
                prefix = data.Substring(data.IndexOf("[", StringComparison.Ordinal) + 1,
                    data.IndexOf("]", StringComparison.Ordinal) - 3);
            prefixColor = prefix switch
            {
                "X" => Color.Firebrick,
                "!" => Color.Gold,
                "✓" => Color.MediumSeaGreen,
                _ => prefixColor
            };
            if (!string.IsNullOrWhiteSpace(prefix) && prefixColor != Color.Empty)
            {
                _logger.Write("  [");
                _logger.Write(prefix, prefixColor);
                _logger.Write("] ");
                data = data.Substring(6);
                while (data.Length > 0 && data[0] == ' ')
                    data = data.Substring(1);
            }

            _logger.WriteLine(data);
        };
        process.ErrorDataReceived += (_, evnt) =>
        {
            if (evnt.Data == null)
                return;
            _logger.Write("  Error: ");
            _logger.WriteLine(evnt.Data.Replace("\r", "").Replace("\n", ". "), Color.Firebrick);
        };
        process.EnableRaisingEvents = true;
        process.StartInfo = startInfo;
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        while (!process.HasExited)
            await Task.Delay(1000);
        SetButtonStatus(false);
        pnlBase.Focus();
        SuspendLayout();
        MsgBox.Show("The deobfuscation process is complete, For more info checkout logs section.", "Completed",
            MsgBox.MsgButtons.Ok, MsgBox.MsgIcon.Info, this);
        Focus();
        ResumeLayout(false);
    }

    private new void Closing(object sender, FormClosingEventArgs e)
    {
        if (_isClosing)
            return;
        _isClosing = true;
        e.Cancel = true;
        CloseAnimated();
    }

    private void MainWindow_Shown(object sender, EventArgs e) => ShowAnimated();

    private void picExit_Click(object sender, EventArgs e) => Close();

    private void picExit_MouseEnter(object sender, EventArgs e) => picExit.Image = Resources.CloseOver;

    private void picExit_MouseLeave(object sender, EventArgs e) => picExit.Image = Resources.Close;

    private void picMinimize_Click(object sender, EventArgs e) => WindowState = FormWindowState.Minimized;

    private void picMinimize_MouseEnter(object sender, EventArgs e) => picMinimize.Image = Resources.MinimizeOver;

    private void picMinimize_MouseLeave(object sender, EventArgs e) => picMinimize.Image = Resources.Minimize;

    private async void txtInput_DragDrop(object sender, DragEventArgs e)
    {
        if ((string[])e.Data.GetData(DataFormats.FileDrop) is { } files && files.Length != 0)
        {
            if (CheckInputFile(files[0]))
                txtInput.Text = files[0];
            else
            {
                txtInput.Text = File.Exists(files[0])
                    ? @"Access to file path denied"
                    : @"Could not find a part of the file path";
                txtInput.ForeColor = Color.Firebrick;
                await Task.Delay(2000);
                txtInput.ForeColor = Color.Silver;
                txtInput.Text = string.Empty;
            }
        }
    }

    private void txtInput_DragEnter(object sender, DragEventArgs e) => e.Effect = DragDropEffects.All;

    private void OnMouseDown(object sender, MouseEventArgs e)
    {
        _isMouseDown = true;
        _lastLocation = e.Location;
        Opacity = 0.90;
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        if (_isMouseDown)
        {
            Location = new Point(
                Location.X - _lastLocation.X + e.X, Location.Y - _lastLocation.Y + e.Y);

            Update();
        }
    }

    private void OnMouseUp(object sender, MouseEventArgs e)
    {
        _isMouseDown = false;
        Opacity = 1.0;
    }

    private void scrollbarLogs_ValueChanged(object sender, ScrollValueEventArgs e)
    {
        if (_isLogsScrollLocked)
        {
            _isLogsScrollLocked = false;
            return;
        }

        try
        {
            BeginControlUpdate(txtLogs);
            txtLogs.SelectionStart = txtLogs.Find(txtLogs.Lines[scrollbarLogs.Value]) - 1;
            txtLogs.SelectionLength = 0;
            txtLogs.ScrollToCaret();
        }
        catch
        {
        }

        EndControlUpdate(txtLogs);
    }

    private void txtLogs_TextChanged(object sender, EventArgs e)
    {
        scrollbarLogs.SuspendLayout();
        scrollbarLogs.Maximum = txtLogs.Lines.Length;
        scrollbarLogs.Value = txtLogs.Lines.Length;
        scrollbarLogs.Invalidate();
        scrollbarLogs.ResumeLayout();
    }

    protected virtual void OnFormWindowStateChanged(EventArgs e)
    {
        if (WindowState == FormWindowState.Maximized)
            WindowState = FormWindowState.Normal;
        CenterToScreen();
    }

    private void llblWebsite_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) =>
        Process.Start((sender as LinkLabel)?.Tag.ToString() ?? throw new InvalidOperationException());

    private void llblGitHub_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) =>
        Process.Start((sender as LinkLabel)?.Tag.ToString() ?? throw new InvalidOperationException());

    private void picBrowse_Click(object sender, EventArgs e)
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = @"Assembly (*.exe,*.dll)| *.exe;*.dll",
            Title = @"Select Assembly",
            Multiselect = false,
            CheckFileExists = true,
            CheckPathExists = true,
            RestoreDirectory = true
        };
        if (openFileDialog.ShowDialog() == DialogResult.OK)
            txtInput.Text = openFileDialog.FileName;
    }

    private void copyLogsToolStripMenuItem_Click(object sender, EventArgs e) => Clipboard.SetText(txtLogs.Text);

    protected override void WndProc(ref Message m)
    {
        var org = WindowState;
        base.WndProc(ref m);
        if (WindowState != org)
            OnFormWindowStateChanged(EventArgs.Empty);
    }

    private void ShowAnimated()
    {
        var timer = new Timer
        {
            Interval = 10
        };
        timer.Tick += async delegate
        {
            if (Opacity < 1.0) Opacity += 0.05;
            if (Opacity >= 1.0)
            {
                timer.Stop();
                Show();
                Opacity = 1.0;
                timer.Dispose();
                try
                {
                    if (!await IsLatestVersion())
                        if (MsgBox.Show(@"New version is available, Do you want to install it?",
                                ".NETReactorSlayer",
                                MsgBox.MsgButtons.YesNoCancel, MsgBox.MsgIcon.Question, this) == DialogResult.Yes)
                            InstallLatestVersion();
                }
                catch
                {
                }
            }
        };
        timer.Start();
    }

    private void CloseAnimated()
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

    private static bool CheckInputFile(string filePath)
    {
        if (File.Exists(filePath))
            try
            {
                File.OpenRead(filePath).Close();
                using (File.Create(
                           Path.Combine(Path.GetDirectoryName(filePath) ?? throw new InvalidOperationException(),
                               Path.GetRandomFileName()),
                           1, FileOptions.DeleteOnClose))
                {
                }

                return true;
            }
            catch
            {
            }

        return false;
    }

    private void SetButtonStatus(bool isBusy)
    {
        if (isBusy)
        {
            btnStart.BackColor = Color.FromArgb(32, 32, 32);
            btnStart.FlatAppearance.MouseOverBackColor = btnStart.BackColor;
            btnStart.FlatAppearance.MouseDownBackColor = btnStart.BackColor;
            btnStart.Image = Resources.Loading;
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

    private static void BeginControlUpdate(IWin32Window control)
    {
        var msgSuspendUpdate = Message.Create(control.Handle, WmSetredraw, IntPtr.Zero,
            IntPtr.Zero);

        var window = NativeWindow.FromHandle(control.Handle);
        window.DefWndProc(ref msgSuspendUpdate);
    }

    private static void EndControlUpdate(Control control)
    {
        var wparam = new IntPtr(1);
        var msgResumeUpdate = Message.Create(control.Handle, WmSetredraw, wparam,
            IntPtr.Zero);

        var window = NativeWindow.FromHandle(control.Handle);
        window.DefWndProc(ref msgResumeUpdate);
        control.Invalidate();
        control.Refresh();
    }

    [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
    public static extern IntPtr CreateRoundRectRgn(
        int nLeftRect, int nTopRect, int nRightRect, int nBottomRect,
        int nWidthEllipse, int nHeightEllipse);

    [DllImport("user32.dll")]
    private static extern int HideCaret(IntPtr hwnd);

    private void CheckedChanged(object sender, EventArgs e)
    {
        if (sender as NrsCheckBox == chkPreserveAll && chkPreserveAll!.Checked)
            chkKeepTypes.Checked = true;
        else if (sender as NrsCheckBox == chkKeepTypes && chkPreserveAll!.Checked && !chkKeepTypes!.Checked)
            chkPreserveAll.Checked = false;

        if ((from x in tabelOptions.Controls.OfType<NrsCheckBox>()
                where x.Name != "chkSelectUnSelectAll"
                select x).Any(control => !control.Checked))
        {
            _return = true;
            chkSelectUnSelectAll.Checked = false;
            _return = false;
            return;
        }

        _return = true;
        chkSelectUnSelectAll.Checked = true;
        _return = false;
    }

    private void chkSelectUnSelectAll_CheckedChanged(object sender, EventArgs e)
    {
        chkSelectUnSelectAll.Text = !chkSelectUnSelectAll.Checked ? @"Select All" : @"Unselect All";
        if (_return)
        {
            _return = false;
            return;
        }

        var @checked = chkSelectUnSelectAll.Checked;
        if (!@checked)
        {
            chkRename.Tag = "--dont-rename";
            chkRename.Checked = false;
            foreach (ToolStripMenuItem control in ctxRename.Items)
                control.Text = control.Text.Replace("✓", "X");
        }
        else
        {
            chkRename.Tag = "--rename --rename ntmfpe";
            chkRename.Checked = true;
            foreach (ToolStripMenuItem control in ctxRename.Items)
                control.Text = control.Text.Replace("X", "✓");
        }

        foreach (var control in from x in tabelOptions.Controls.OfType<NrsCheckBox>()
                 where x.Name != @"chkSelectUnSelectAll"
                 select x)
            if (control.Checked != @checked)
            {
                _return = true;
                control.Checked = @checked;
            }
    }

    private void picMenu_MouseEnter(object sender, EventArgs e) => picMenu.Image = Resources.MenuOver;

    private void picMenu_MouseLeave(object sender, EventArgs e) => picMenu.Image = Resources.Menu;

    private void picMenu_MouseClick(object sender, MouseEventArgs e) =>
        ctxMenu.Show(sender as Control ?? throw new InvalidOperationException(), new Point(e.X, e.Y));

    private void toolStripMenuItem4_Click(object sender, EventArgs e) => Close();

    private void toolStripMenuItem7_Click(object sender, EventArgs e) =>
        MsgBox.Show(
            $@"Product Name: .NETReactorSlayer
Version: {lblVersion.Text}
Description: An open source (GPLv3) deobfuscator for Eziriz .NET Reactor
Author: SychicBoy
Company: CS-RET
Website: CodeStrikers.org", "About .NETReactorSlayer", MsgBox.MsgButtons.Ok, MsgBox.MsgIcon.Info, this);

    private async void toolStripMenuItem6_Click(object sender, EventArgs e)
    {
        try
        {
            if (await IsLatestVersion())
                MsgBox.Show(@"Congratulations, You are using the latest version!", ".NETReactorSlayer",
                    MsgBox.MsgButtons.Ok, MsgBox.MsgIcon.Info, this);
            else
            {
                if (MsgBox.Show(@"New version is available, Do you want to install it?",
                        ".NETReactorSlayer",
                        MsgBox.MsgButtons.YesNoCancel, MsgBox.MsgIcon.Question, this) == DialogResult.Yes)
                    InstallLatestVersion();
            }
        }
        catch (Exception exception)
        {
            MsgBox.Show(exception.Message, ".NETReactorSlayer", MsgBox.MsgButtons.Ok, MsgBox.MsgIcon.Error, this);
        }
    }

    private static async Task<bool> IsLatestVersion()
    {
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        var client = new HttpClient();
        var response =
            await client.GetAsync("https://github.com/SychicBoy/NETReactorSlayer/releases/latest");
        response.EnsureSuccessStatusCode();
        var responseUri = response.RequestMessage.RequestUri.ToString();
        response.Dispose();
        client.Dispose();
        var latestVersionStr = _lastVersion = responseUri.Substring(responseUri.LastIndexOf('/') + 1);
        var currentVersionStr = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;

        if (int.TryParse(Regex.Match(latestVersionStr, @"\d+\.\d+\.\d+\.\d+").Value.Replace(".", string.Empty),
                out var latestVersion) && int.TryParse(
                Regex.Match(currentVersionStr, @"\d+\.\d+\.\d+\.\d+").Value.Replace(".", string.Empty),
                out var currentVersion))
            return latestVersion <= currentVersion;

        return true;
    }

    private static async void InstallLatestVersion()
    {
        var tmpPath = Path.GetTempFileName();
        var tmpDir = Path.GetDirectoryName(tmpPath);
        AddTrailing(ref tmpDir);
        var tmpDestDir = Path.Combine(tmpDir,
            $".NETReactorSlayer_v{_lastVersion}_{DateTime.Now:yyyy-MM-dd-HH-mmmm-ss}\\");
        AddTrailing(ref tmpDestDir);
        var baseDir = Environment.CurrentDirectory;
        RemoveTrailing(ref baseDir);
        var downloadUrl =
            $"https://github.com/SychicBoy/NETReactorSlayer/releases/download/{_lastVersion}/NETReactorSlayer.zip";
        var client = new HttpClient();
        var response = await client.GetAsync(downloadUrl);
        File.WriteAllBytes(tmpPath, new byte[] { 0 });
        using var fs = new FileStream(tmpPath, FileMode.Open, FileAccess.ReadWrite);
        await response.Content.CopyToAsync(fs);

        if (!Directory.Exists(tmpDestDir))
            Directory.CreateDirectory(tmpDestDir);
        ZipFile.ExtractToDirectory(tmpPath, tmpDestDir);
        RemoveTrailing(ref tmpDestDir);
        var command = $"DEL /Q \"{baseDir}\\*\"" +
                      " & " +
                      $"XCOPY /S /Q \"{tmpDestDir}\" \"{baseDir}\"" +
                      " &" +
                      $"RMDIR /S /Q \"{tmpDestDir}\"" +
                      " & " +
                      $"\"{baseDir}\\NETReactorSlayer.exe\" updated";
        Process.Start(new ProcessStartInfo("cmd.exe",
                "/C ping 1.1.1.1 -n 1 -w 3000 > Nul & " + command)
            {
                WindowStyle = ProcessWindowStyle.Hidden
            })
            ?.Dispose();
        Process.GetCurrentProcess().Kill();
    }

    private static void AddTrailing(ref string directory)
    {
        if (!directory.EndsWith("\\"))
            directory += "\\";
    }

    private static void RemoveTrailing(ref string directory)
    {
        if (directory.EndsWith("\\"))
            directory = directory.Substring(0, directory.Length - 1);
    }

    private void SetRenamingOptions(object sender, EventArgs e)
    {
        if (sender is not ToolStripMenuItem control) return;
        if (control.Tag is not string option || chkRename.Tag is not string tag) return;
        tag = tag.Replace("--rename ", string.Empty).Replace("--dont-rename", string.Empty);
        var text = control.Text;
        if (text.Contains("✓"))
        {
            control.Text = text.Replace("✓", "X");
            chkRename.Tag = "--rename " + tag.Replace(option, string.Empty);
        }
        else if (text.Contains("X") && !tag.Contains(option))
        {
            control.Text = text.Replace("X", "✓");
            chkRename.Tag = $"--rename {tag}{option}";
        }

        if (chkRename.Tag.ToString().Replace("--rename ", string.Empty).Length < 1)
        {
            if (!chkRename.Checked) return;
            chkRename.Checked = false;
            chkRename.Tag = "--dont-rename";
        }
        else if (!chkRename.Checked) chkRename.Checked = true;
    }

    private void KeepCtxRenameOpen(object sender, MouseEventArgs e) => ctxRename.Tag = "open";

    private void ctxRename_Closing(object sender, ToolStripDropDownClosingEventArgs e)
    {
        if (ctxRename.Tag is not "open") return;
        ctxRename.Tag = "close";
        e.Cancel = true;
    }

    private void OpenCtxRename(object sender, MouseEventArgs e)
    {
        if (chkRename.Tag.ToString() == "--dont-rename" ||
            chkRename.Tag.ToString().Replace("--rename ", string.Empty).Length < 1)
        {
            if (chkRename.Checked)
                chkRename.Checked = false;
        }
        else if (!chkRename.Checked) chkRename.Checked = true;

        ctxRename.Show(chkRename, new Point(e.X, e.Y));
    }

    private readonly StringBuilder _arguments = new();
    private readonly Logger _logger;
    private bool _isClosing;
    private bool _isLogsScrollLocked;
    private bool _isMouseDown;
    private Point _lastLocation;
    private bool _return;

    private const int WmSetredraw = 11;

    private static readonly string InformationalVersion = (Attribute.GetCustomAttribute(
        Assembly.GetEntryAssembly() ?? throw new InvalidOperationException(),
        typeof(AssemblyInformationalVersionAttribute)) as AssemblyInformationalVersionAttribute)?.InformationalVersion;

    private static string _lastVersion;

    protected override CreateParams CreateParams
    {
        get
        {
            var cp = base.CreateParams;
            cp.ClassStyle |= 0x00020000;
            return cp;
        }
    }
}