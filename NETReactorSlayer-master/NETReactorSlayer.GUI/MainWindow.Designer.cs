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

using System.ComponentModel;
using System.Windows.Forms;
using NETReactorSlayer.GUI.UserControls;

namespace NETReactorSlayer.GUI
{
    partial class MainWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.picMenu = new System.Windows.Forms.PictureBox();
            this.panel4 = new System.Windows.Forms.Panel();
            this.picMinimize = new System.Windows.Forms.PictureBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.picExit = new System.Windows.Forms.PictureBox();
            this.picHeader = new System.Windows.Forms.PictureBox();
            this.panel3 = new System.Windows.Forms.Panel();
            this.pnlBase = new System.Windows.Forms.Panel();
            this.pnlButton = new System.Windows.Forms.Panel();
            this.btnStart = new NETReactorSlayer.GUI.UserControls.NrsButton();
            this.pnlSeparator = new System.Windows.Forms.Panel();
            this.panelLogs = new System.Windows.Forms.Panel();
            this.scrollbarLogs = new NETReactorSlayer.GUI.UserControls.NrsScrollBar();
            this.txtLogs = new System.Windows.Forms.RichTextBox();
            this.ctxLogs = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.copyLogsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.pnlInput = new System.Windows.Forms.Panel();
            this.picBrowse = new System.Windows.Forms.PictureBox();
            this.pnlTextBox = new System.Windows.Forms.Panel();
            this.txtInput = new NETReactorSlayer.GUI.UserControls.NrsTextBox();
            this.pnlOptions = new System.Windows.Forms.Panel();
            this.tabelOptions = new System.Windows.Forms.TableLayoutPanel();
            this.chkDumpCosturaAsm = new NETReactorSlayer.GUI.UserControls.NrsCheckBox();
            this.chkDecryptStrings = new NETReactorSlayer.GUI.UserControls.NrsCheckBox();
            this.chkPatchAntiTD = new NETReactorSlayer.GUI.UserControls.NrsCheckBox();
            this.chkDecryptBools = new NETReactorSlayer.GUI.UserControls.NrsCheckBox();
            this.chkDumpDNRAsm = new NETReactorSlayer.GUI.UserControls.NrsCheckBox();
            this.chkDecryptHiddenCalls = new NETReactorSlayer.GUI.UserControls.NrsCheckBox();
            this.chkDeobCFlow = new NETReactorSlayer.GUI.UserControls.NrsCheckBox();
            this.chkDecryptTokens = new NETReactorSlayer.GUI.UserControls.NrsCheckBox();
            this.chkDecryptResources = new NETReactorSlayer.GUI.UserControls.NrsCheckBox();
            this.chkRemoveRefProxies = new NETReactorSlayer.GUI.UserControls.NrsCheckBox();
            this.chkDecryptMethods = new NETReactorSlayer.GUI.UserControls.NrsCheckBox();
            this.chkSelectUnSelectAll = new NETReactorSlayer.GUI.UserControls.NrsCheckBox();
            this.chkRemSn = new NETReactorSlayer.GUI.UserControls.NrsCheckBox();
            this.chkRename = new NETReactorSlayer.GUI.UserControls.NrsCheckBox();
            this.ctxRename = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItem8 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem9 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem10 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem13 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem15 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem14 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem16 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem12 = new System.Windows.Forms.ToolStripMenuItem();
            this.chkRenameShort = new NETReactorSlayer.GUI.UserControls.NrsCheckBox();
            this.chkRemJunks = new NETReactorSlayer.GUI.UserControls.NrsCheckBox();
            this.chkKeepTypes = new NETReactorSlayer.GUI.UserControls.NrsCheckBox();
            this.chkRemCalls = new NETReactorSlayer.GUI.UserControls.NrsCheckBox();
            this.chkPreserveAll = new NETReactorSlayer.GUI.UserControls.NrsCheckBox();
            this.chkKeepOldMaxStack = new NETReactorSlayer.GUI.UserControls.NrsCheckBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.llblGitHub = new System.Windows.Forms.LinkLabel();
            this.label2 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.lblVersion = new System.Windows.Forms.Label();
            this.llblWebsite = new System.Windows.Forms.LinkLabel();
            this.label4 = new System.Windows.Forms.Label();
            this.lblAuthor = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.ctxMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem6 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem7 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem5 = new System.Windows.Forms.ToolStripMenuItem();
            this.pnlHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picMenu)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picMinimize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picExit)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picHeader)).BeginInit();
            this.pnlBase.SuspendLayout();
            this.pnlButton.SuspendLayout();
            this.panelLogs.SuspendLayout();
            this.ctxLogs.SuspendLayout();
            this.pnlInput.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picBrowse)).BeginInit();
            this.pnlTextBox.SuspendLayout();
            this.pnlOptions.SuspendLayout();
            this.tabelOptions.SuspendLayout();
            this.ctxRename.SuspendLayout();
            this.panel1.SuspendLayout();
            this.ctxMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlHeader
            // 
            this.pnlHeader.AutoSize = true;
            this.pnlHeader.Controls.Add(this.picMenu);
            this.pnlHeader.Controls.Add(this.panel4);
            this.pnlHeader.Controls.Add(this.picMinimize);
            this.pnlHeader.Controls.Add(this.panel2);
            this.pnlHeader.Controls.Add(this.picExit);
            this.pnlHeader.Controls.Add(this.picHeader);
            this.pnlHeader.Controls.Add(this.panel3);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(30, 5);
            this.pnlHeader.MinimumSize = new System.Drawing.Size(0, 60);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(997, 60);
            this.pnlHeader.TabIndex = 2;
            // 
            // picMenu
            // 
            this.picMenu.Cursor = System.Windows.Forms.Cursors.Hand;
            this.picMenu.Dock = System.Windows.Forms.DockStyle.Left;
            this.picMenu.Image = global::NETReactorSlayer.GUI.Properties.Resources.Menu;
            this.picMenu.Location = new System.Drawing.Point(10, 0);
            this.picMenu.Name = "picMenu";
            this.picMenu.Size = new System.Drawing.Size(24, 60);
            this.picMenu.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.picMenu.TabIndex = 18;
            this.picMenu.TabStop = false;
            this.picMenu.MouseClick += new System.Windows.Forms.MouseEventHandler(this.picMenu_MouseClick);
            this.picMenu.MouseEnter += new System.EventHandler(this.picMenu_MouseEnter);
            this.picMenu.MouseLeave += new System.EventHandler(this.picMenu_MouseLeave);
            // 
            // panel4
            // 
            this.panel4.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel4.Location = new System.Drawing.Point(0, 0);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(10, 60);
            this.panel4.TabIndex = 19;
            // 
            // picMinimize
            // 
            this.picMinimize.Cursor = System.Windows.Forms.Cursors.Hand;
            this.picMinimize.Dock = System.Windows.Forms.DockStyle.Right;
            this.picMinimize.Image = global::NETReactorSlayer.GUI.Properties.Resources.Minimize;
            this.picMinimize.Location = new System.Drawing.Point(929, 0);
            this.picMinimize.Name = "picMinimize";
            this.picMinimize.Size = new System.Drawing.Size(24, 60);
            this.picMinimize.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.picMinimize.TabIndex = 3;
            this.picMinimize.TabStop = false;
            this.picMinimize.Click += new System.EventHandler(this.picMinimize_Click);
            this.picMinimize.MouseEnter += new System.EventHandler(this.picMinimize_MouseEnter);
            this.picMinimize.MouseLeave += new System.EventHandler(this.picMinimize_MouseLeave);
            // 
            // panel2
            // 
            this.panel2.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel2.Location = new System.Drawing.Point(953, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(10, 60);
            this.panel2.TabIndex = 16;
            // 
            // picExit
            // 
            this.picExit.Cursor = System.Windows.Forms.Cursors.Hand;
            this.picExit.Dock = System.Windows.Forms.DockStyle.Right;
            this.picExit.Image = global::NETReactorSlayer.GUI.Properties.Resources.Close;
            this.picExit.Location = new System.Drawing.Point(963, 0);
            this.picExit.Name = "picExit";
            this.picExit.Size = new System.Drawing.Size(24, 60);
            this.picExit.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.picExit.TabIndex = 2;
            this.picExit.TabStop = false;
            this.picExit.Click += new System.EventHandler(this.picExit_Click);
            this.picExit.MouseEnter += new System.EventHandler(this.picExit_MouseEnter);
            this.picExit.MouseLeave += new System.EventHandler(this.picExit_MouseLeave);
            // 
            // picHeader
            // 
            this.picHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picHeader.Image = global::NETReactorSlayer.GUI.Properties.Resources.Header;
            this.picHeader.Location = new System.Drawing.Point(0, 0);
            this.picHeader.Name = "picHeader";
            this.picHeader.Size = new System.Drawing.Size(987, 60);
            this.picHeader.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.picHeader.TabIndex = 1;
            this.picHeader.TabStop = false;
            this.picHeader.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnMouseDown);
            this.picHeader.MouseMove += new System.Windows.Forms.MouseEventHandler(this.OnMouseMove);
            this.picHeader.MouseUp += new System.Windows.Forms.MouseEventHandler(this.OnMouseUp);
            // 
            // panel3
            // 
            this.panel3.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel3.Location = new System.Drawing.Point(987, 0);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(10, 60);
            this.panel3.TabIndex = 17;
            // 
            // pnlBase
            // 
            this.pnlBase.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(23)))), ((int)(((byte)(23)))), ((int)(((byte)(23)))));
            this.pnlBase.Controls.Add(this.pnlButton);
            this.pnlBase.Controls.Add(this.pnlSeparator);
            this.pnlBase.Controls.Add(this.panelLogs);
            this.pnlBase.Controls.Add(this.pnlInput);
            this.pnlBase.Controls.Add(this.pnlOptions);
            this.pnlBase.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlBase.Location = new System.Drawing.Point(30, 65);
            this.pnlBase.Name = "pnlBase";
            this.pnlBase.Padding = new System.Windows.Forms.Padding(30);
            this.pnlBase.Size = new System.Drawing.Size(997, 526);
            this.pnlBase.TabIndex = 3;
            // 
            // pnlButton
            // 
            this.pnlButton.Controls.Add(this.btnStart);
            this.pnlButton.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlButton.Location = new System.Drawing.Point(30, 450);
            this.pnlButton.Name = "pnlButton";
            this.pnlButton.Padding = new System.Windows.Forms.Padding(200, 5, 200, 5);
            this.pnlButton.Size = new System.Drawing.Size(937, 55);
            this.pnlButton.TabIndex = 16;
            // 
            // btnStart
            // 
            this.btnStart.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(27)))), ((int)(((byte)(27)))));
            this.btnStart.BorderRadius = 25;
            this.btnStart.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnStart.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnStart.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(32)))));
            this.btnStart.FlatAppearance.BorderSize = 0;
            this.btnStart.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(18)))), ((int)(((byte)(18)))));
            this.btnStart.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(34)))), ((int)(((byte)(34)))));
            this.btnStart.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStart.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnStart.ForeColor = System.Drawing.Color.Silver;
            this.btnStart.Location = new System.Drawing.Point(200, 5);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(537, 45);
            this.btnStart.TabIndex = 15;
            this.btnStart.Text = "Start Deobfuscation";
            this.btnStart.TextTransform = NETReactorSlayer.GUI.UserControls.NrsButton.TextTransformEnum.None;
            this.btnStart.UseVisualStyleBackColor = false;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // pnlSeparator
            // 
            this.pnlSeparator.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlSeparator.Location = new System.Drawing.Point(30, 440);
            this.pnlSeparator.Name = "pnlSeparator";
            this.pnlSeparator.Size = new System.Drawing.Size(937, 10);
            this.pnlSeparator.TabIndex = 15;
            // 
            // panelLogs
            // 
            this.panelLogs.Controls.Add(this.scrollbarLogs);
            this.panelLogs.Controls.Add(this.txtLogs);
            this.panelLogs.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelLogs.Location = new System.Drawing.Point(30, 259);
            this.panelLogs.Name = "panelLogs";
            this.panelLogs.Padding = new System.Windows.Forms.Padding(2);
            this.panelLogs.Size = new System.Drawing.Size(937, 181);
            this.panelLogs.TabIndex = 0;
            // 
            // scrollbarLogs
            // 
            this.scrollbarLogs.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(27)))), ((int)(((byte)(27)))));
            this.scrollbarLogs.Dock = System.Windows.Forms.DockStyle.Right;
            this.scrollbarLogs.Location = new System.Drawing.Point(920, 2);
            this.scrollbarLogs.Maximum = 10;
            this.scrollbarLogs.Name = "scrollbarLogs";
            this.scrollbarLogs.Size = new System.Drawing.Size(15, 177);
            this.scrollbarLogs.TabIndex = 17;
            this.scrollbarLogs.Text = "nrsScrollBar1";
            this.scrollbarLogs.ViewSize = 9;
            this.scrollbarLogs.ValueChanged += new System.EventHandler<NETReactorSlayer.GUI.UserControls.ScrollValueEventArgs>(this.scrollbarLogs_ValueChanged);
            // 
            // txtLogs
            // 
            this.txtLogs.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(27)))), ((int)(((byte)(27)))));
            this.txtLogs.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtLogs.ContextMenuStrip = this.ctxLogs;
            this.txtLogs.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtLogs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtLogs.ForeColor = System.Drawing.Color.Silver;
            this.txtLogs.Location = new System.Drawing.Point(2, 2);
            this.txtLogs.Name = "txtLogs";
            this.txtLogs.ReadOnly = true;
            this.txtLogs.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.txtLogs.Size = new System.Drawing.Size(933, 177);
            this.txtLogs.TabIndex = 0;
            this.txtLogs.TabStop = false;
            this.txtLogs.Text = "\n  Logs will appear here...";
            this.txtLogs.TextChanged += new System.EventHandler(this.txtLogs_TextChanged);
            // 
            // ctxLogs
            // 
            this.ctxLogs.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(23)))), ((int)(((byte)(23)))), ((int)(((byte)(23)))));
            this.ctxLogs.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.ctxLogs.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem2,
            this.copyLogsToolStripMenuItem,
            this.toolStripMenuItem1});
            this.ctxLogs.Name = "ctxLogs";
            this.ctxLogs.ShowImageMargin = false;
            this.ctxLogs.Size = new System.Drawing.Size(124, 38);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.AutoSize = false;
            this.toolStripMenuItem2.Enabled = false;
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(185, 5);
            this.toolStripMenuItem2.Text = " ";
            // 
            // copyLogsToolStripMenuItem
            // 
            this.copyLogsToolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.copyLogsToolStripMenuItem.Font = new System.Drawing.Font("Segoe UI Semibold", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.copyLogsToolStripMenuItem.ForeColor = System.Drawing.Color.Silver;
            this.copyLogsToolStripMenuItem.Name = "copyLogsToolStripMenuItem";
            this.copyLogsToolStripMenuItem.Size = new System.Drawing.Size(123, 24);
            this.copyLogsToolStripMenuItem.Text = "  Copy Logs";
            this.copyLogsToolStripMenuItem.Click += new System.EventHandler(this.copyLogsToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.AutoSize = false;
            this.toolStripMenuItem1.Enabled = false;
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(185, 5);
            this.toolStripMenuItem1.Text = " ";
            // 
            // pnlInput
            // 
            this.pnlInput.Controls.Add(this.picBrowse);
            this.pnlInput.Controls.Add(this.pnlTextBox);
            this.pnlInput.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlInput.Location = new System.Drawing.Point(30, 212);
            this.pnlInput.Name = "pnlInput";
            this.pnlInput.Padding = new System.Windows.Forms.Padding(0, 10, 0, 10);
            this.pnlInput.Size = new System.Drawing.Size(937, 47);
            this.pnlInput.TabIndex = 12;
            // 
            // picBrowse
            // 
            this.picBrowse.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(27)))), ((int)(((byte)(27)))));
            this.picBrowse.Cursor = System.Windows.Forms.Cursors.Hand;
            this.picBrowse.Dock = System.Windows.Forms.DockStyle.Right;
            this.picBrowse.Image = global::NETReactorSlayer.GUI.Properties.Resources.Browse;
            this.picBrowse.Location = new System.Drawing.Point(886, 10);
            this.picBrowse.Name = "picBrowse";
            this.picBrowse.Size = new System.Drawing.Size(51, 27);
            this.picBrowse.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.picBrowse.TabIndex = 4;
            this.picBrowse.TabStop = false;
            this.picBrowse.Click += new System.EventHandler(this.picBrowse_Click);
            // 
            // pnlTextBox
            // 
            this.pnlTextBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(27)))), ((int)(((byte)(27)))));
            this.pnlTextBox.Controls.Add(this.txtInput);
            this.pnlTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlTextBox.Location = new System.Drawing.Point(0, 10);
            this.pnlTextBox.Name = "pnlTextBox";
            this.pnlTextBox.Padding = new System.Windows.Forms.Padding(0, 6, 0, 0);
            this.pnlTextBox.Size = new System.Drawing.Size(937, 27);
            this.pnlTextBox.TabIndex = 1;
            // 
            // txtInput
            // 
            this.txtInput.AllowDrop = true;
            this.txtInput.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(27)))), ((int)(((byte)(27)))));
            this.txtInput.BorderRadius = 0;
            this.txtInput.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtInput.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtInput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtInput.Font = new System.Drawing.Font("Segoe UI Semibold", 7.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtInput.ForeColor = System.Drawing.Color.Silver;
            this.txtInput.Location = new System.Drawing.Point(0, 6);
            this.txtInput.Name = "txtInput";
            this.txtInput.PlaceHolderColor = System.Drawing.Color.Gray;
            this.txtInput.PlaceHolderText = "DRAG & DROP TARGET FILE HERE OR ENTER FILE PATH";
            this.txtInput.Progress = 22F;
            this.txtInput.ProgressColor = System.Drawing.Color.MediumSeaGreen;
            this.txtInput.Size = new System.Drawing.Size(937, 21);
            this.txtInput.TabIndex = 2;
            this.txtInput.Tag = "";
            this.txtInput.Text = "DRAG & DROP TARGET FILE HERE OR ENTER FILE PATH";
            this.txtInput.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.txtInput.TextTransform = NETReactorSlayer.GUI.UserControls.NrsTextBox.TextTransformEnum.None;
            this.txtInput.WordWrap = false;
            this.txtInput.DragDrop += new System.Windows.Forms.DragEventHandler(this.txtInput_DragDrop);
            this.txtInput.DragEnter += new System.Windows.Forms.DragEventHandler(this.txtInput_DragEnter);
            // 
            // pnlOptions
            // 
            this.pnlOptions.Controls.Add(this.tabelOptions);
            this.pnlOptions.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlOptions.Location = new System.Drawing.Point(30, 30);
            this.pnlOptions.Name = "pnlOptions";
            this.pnlOptions.Padding = new System.Windows.Forms.Padding(3, 0, 0, 3);
            this.pnlOptions.Size = new System.Drawing.Size(937, 182);
            this.pnlOptions.TabIndex = 11;
            // 
            // tabelOptions
            // 
            this.tabelOptions.ColumnCount = 3;
            this.tabelOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33332F));
            this.tabelOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33334F));
            this.tabelOptions.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33334F));
            this.tabelOptions.Controls.Add(this.chkDumpCosturaAsm, 2, 3);
            this.tabelOptions.Controls.Add(this.chkDecryptStrings, 2, 2);
            this.tabelOptions.Controls.Add(this.chkPatchAntiTD, 2, 1);
            this.tabelOptions.Controls.Add(this.chkDecryptBools, 1, 4);
            this.tabelOptions.Controls.Add(this.chkDumpDNRAsm, 1, 3);
            this.tabelOptions.Controls.Add(this.chkDecryptHiddenCalls, 1, 2);
            this.tabelOptions.Controls.Add(this.chkDeobCFlow, 1, 1);
            this.tabelOptions.Controls.Add(this.chkDecryptTokens, 0, 4);
            this.tabelOptions.Controls.Add(this.chkDecryptResources, 0, 3);
            this.tabelOptions.Controls.Add(this.chkRemoveRefProxies, 0, 2);
            this.tabelOptions.Controls.Add(this.chkDecryptMethods, 0, 1);
            this.tabelOptions.Controls.Add(this.chkSelectUnSelectAll, 0, 0);
            this.tabelOptions.Controls.Add(this.chkRemSn, 2, 4);
            this.tabelOptions.Controls.Add(this.chkRename, 0, 5);
            this.tabelOptions.Controls.Add(this.chkRenameShort, 1, 5);
            this.tabelOptions.Controls.Add(this.chkRemJunks, 0, 6);
            this.tabelOptions.Controls.Add(this.chkKeepTypes, 2, 5);
            this.tabelOptions.Controls.Add(this.chkRemCalls, 1, 6);
            this.tabelOptions.Controls.Add(this.chkPreserveAll, 2, 6);
            this.tabelOptions.Controls.Add(this.chkKeepOldMaxStack, 0, 7);
            this.tabelOptions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabelOptions.Location = new System.Drawing.Point(3, 0);
            this.tabelOptions.Name = "tabelOptions";
            this.tabelOptions.RowCount = 8;
            this.tabelOptions.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 14.28571F));
            this.tabelOptions.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 14.28571F));
            this.tabelOptions.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 14.28571F));
            this.tabelOptions.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 14.28571F));
            this.tabelOptions.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 14.28571F));
            this.tabelOptions.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 14.28571F));
            this.tabelOptions.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 14.28571F));
            this.tabelOptions.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tabelOptions.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tabelOptions.Size = new System.Drawing.Size(934, 179);
            this.tabelOptions.TabIndex = 3;
            // 
            // chkDumpCosturaAsm
            // 
            this.chkDumpCosturaAsm.AutoSize = true;
            this.chkDumpCosturaAsm.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(117)))), ((int)(((byte)(9)))), ((int)(((byte)(12)))));
            this.chkDumpCosturaAsm.Checked = true;
            this.chkDumpCosturaAsm.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkDumpCosturaAsm.Cursor = System.Windows.Forms.Cursors.Hand;
            this.chkDumpCosturaAsm.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkDumpCosturaAsm.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(30)))), ((int)(((byte)(35)))));
            this.chkDumpCosturaAsm.ForeColor = System.Drawing.Color.Silver;
            this.chkDumpCosturaAsm.HoverBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(30)))), ((int)(((byte)(35)))));
            this.chkDumpCosturaAsm.HoverFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(188)))), ((int)(((byte)(14)))), ((int)(((byte)(18)))));
            this.chkDumpCosturaAsm.HoverForeColor = System.Drawing.Color.White;
            this.chkDumpCosturaAsm.Location = new System.Drawing.Point(625, 69);
            this.chkDumpCosturaAsm.Name = "chkDumpCosturaAsm";
            this.chkDumpCosturaAsm.PressBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(30)))), ((int)(((byte)(35)))));
            this.chkDumpCosturaAsm.PressFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(188)))), ((int)(((byte)(14)))), ((int)(((byte)(18)))));
            this.chkDumpCosturaAsm.PressForeColor = System.Drawing.Color.Gray;
            this.chkDumpCosturaAsm.Size = new System.Drawing.Size(306, 16);
            this.chkDumpCosturaAsm.TabIndex = 9;
            this.chkDumpCosturaAsm.Tag = "--dump-costura";
            this.chkDumpCosturaAsm.Text = "Dump Costura-Fody Assemblies";
            this.chkDumpCosturaAsm.UseVisualStyleBackColor = true;
            this.chkDumpCosturaAsm.CheckedChanged += new System.EventHandler(this.CheckedChanged);
            // 
            // chkDecryptStrings
            // 
            this.chkDecryptStrings.AutoSize = true;
            this.chkDecryptStrings.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(117)))), ((int)(((byte)(9)))), ((int)(((byte)(12)))));
            this.chkDecryptStrings.Checked = true;
            this.chkDecryptStrings.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkDecryptStrings.Cursor = System.Windows.Forms.Cursors.Hand;
            this.chkDecryptStrings.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkDecryptStrings.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(30)))), ((int)(((byte)(35)))));
            this.chkDecryptStrings.ForeColor = System.Drawing.Color.Silver;
            this.chkDecryptStrings.HoverBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(30)))), ((int)(((byte)(35)))));
            this.chkDecryptStrings.HoverFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(188)))), ((int)(((byte)(14)))), ((int)(((byte)(18)))));
            this.chkDecryptStrings.HoverForeColor = System.Drawing.Color.White;
            this.chkDecryptStrings.Location = new System.Drawing.Point(625, 47);
            this.chkDecryptStrings.Name = "chkDecryptStrings";
            this.chkDecryptStrings.PressBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(30)))), ((int)(((byte)(35)))));
            this.chkDecryptStrings.PressFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(188)))), ((int)(((byte)(14)))), ((int)(((byte)(18)))));
            this.chkDecryptStrings.PressForeColor = System.Drawing.Color.Gray;
            this.chkDecryptStrings.Size = new System.Drawing.Size(306, 16);
            this.chkDecryptStrings.TabIndex = 13;
            this.chkDecryptStrings.Tag = "--dec-strings";
            this.chkDecryptStrings.Text = "Decrypt Strings";
            this.chkDecryptStrings.UseVisualStyleBackColor = true;
            this.chkDecryptStrings.CheckedChanged += new System.EventHandler(this.CheckedChanged);
            // 
            // chkPatchAntiTD
            // 
            this.chkPatchAntiTD.AutoSize = true;
            this.chkPatchAntiTD.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(117)))), ((int)(((byte)(9)))), ((int)(((byte)(12)))));
            this.chkPatchAntiTD.Checked = true;
            this.chkPatchAntiTD.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkPatchAntiTD.Cursor = System.Windows.Forms.Cursors.Hand;
            this.chkPatchAntiTD.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkPatchAntiTD.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(30)))), ((int)(((byte)(35)))));
            this.chkPatchAntiTD.ForeColor = System.Drawing.Color.Silver;
            this.chkPatchAntiTD.HoverBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(30)))), ((int)(((byte)(35)))));
            this.chkPatchAntiTD.HoverFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(188)))), ((int)(((byte)(14)))), ((int)(((byte)(18)))));
            this.chkPatchAntiTD.HoverForeColor = System.Drawing.Color.White;
            this.chkPatchAntiTD.Location = new System.Drawing.Point(625, 25);
            this.chkPatchAntiTD.Name = "chkPatchAntiTD";
            this.chkPatchAntiTD.PressBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(30)))), ((int)(((byte)(35)))));
            this.chkPatchAntiTD.PressFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(188)))), ((int)(((byte)(14)))), ((int)(((byte)(18)))));
            this.chkPatchAntiTD.PressForeColor = System.Drawing.Color.Gray;
            this.chkPatchAntiTD.Size = new System.Drawing.Size(306, 16);
            this.chkPatchAntiTD.TabIndex = 11;
            this.chkPatchAntiTD.Tag = "--rem-antis";
            this.chkPatchAntiTD.Text = "Remove Anti Tamper & Anti Debugger";
            this.chkPatchAntiTD.UseVisualStyleBackColor = true;
            this.chkPatchAntiTD.CheckedChanged += new System.EventHandler(this.CheckedChanged);
            // 
            // chkDecryptBools
            // 
            this.chkDecryptBools.AutoSize = true;
            this.chkDecryptBools.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(117)))), ((int)(((byte)(9)))), ((int)(((byte)(12)))));
            this.chkDecryptBools.Checked = true;
            this.chkDecryptBools.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkDecryptBools.Cursor = System.Windows.Forms.Cursors.Hand;
            this.chkDecryptBools.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkDecryptBools.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(30)))), ((int)(((byte)(35)))));
            this.chkDecryptBools.ForeColor = System.Drawing.Color.Silver;
            this.chkDecryptBools.HoverBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(30)))), ((int)(((byte)(35)))));
            this.chkDecryptBools.HoverFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(188)))), ((int)(((byte)(14)))), ((int)(((byte)(18)))));
            this.chkDecryptBools.HoverForeColor = System.Drawing.Color.White;
            this.chkDecryptBools.Location = new System.Drawing.Point(314, 91);
            this.chkDecryptBools.Name = "chkDecryptBools";
            this.chkDecryptBools.PressBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(30)))), ((int)(((byte)(35)))));
            this.chkDecryptBools.PressFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(188)))), ((int)(((byte)(14)))), ((int)(((byte)(18)))));
            this.chkDecryptBools.PressForeColor = System.Drawing.Color.Gray;
            this.chkDecryptBools.Size = new System.Drawing.Size(305, 16);
            this.chkDecryptBools.TabIndex = 16;
            this.chkDecryptBools.Tag = "--dec-bools";
            this.chkDecryptBools.Text = "Decrypt Booleans";
            this.chkDecryptBools.UseVisualStyleBackColor = true;
            this.chkDecryptBools.CheckedChanged += new System.EventHandler(this.CheckedChanged);
            // 
            // chkDumpDNRAsm
            // 
            this.chkDumpDNRAsm.AutoSize = true;
            this.chkDumpDNRAsm.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(117)))), ((int)(((byte)(9)))), ((int)(((byte)(12)))));
            this.chkDumpDNRAsm.Checked = true;
            this.chkDumpDNRAsm.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkDumpDNRAsm.Cursor = System.Windows.Forms.Cursors.Hand;
            this.chkDumpDNRAsm.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkDumpDNRAsm.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(30)))), ((int)(((byte)(35)))));
            this.chkDumpDNRAsm.ForeColor = System.Drawing.Color.Silver;
            this.chkDumpDNRAsm.HoverBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(30)))), ((int)(((byte)(35)))));
            this.chkDumpDNRAsm.HoverFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(188)))), ((int)(((byte)(14)))), ((int)(((byte)(18)))));
            this.chkDumpDNRAsm.HoverForeColor = System.Drawing.Color.White;
            this.chkDumpDNRAsm.Location = new System.Drawing.Point(314, 69);
            this.chkDumpDNRAsm.Name = "chkDumpDNRAsm";
            this.chkDumpDNRAsm.PressBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(30)))), ((int)(((byte)(35)))));
            this.chkDumpDNRAsm.PressFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(188)))), ((int)(((byte)(14)))), ((int)(((byte)(18)))));
            this.chkDumpDNRAsm.PressForeColor = System.Drawing.Color.Gray;
            this.chkDumpDNRAsm.Size = new System.Drawing.Size(305, 16);
            this.chkDumpDNRAsm.TabIndex = 10;
            this.chkDumpDNRAsm.Tag = "--dump-asm";
            this.chkDumpDNRAsm.Text = "Dump Embedded Assemblies";
            this.chkDumpDNRAsm.UseVisualStyleBackColor = true;
            this.chkDumpDNRAsm.CheckedChanged += new System.EventHandler(this.CheckedChanged);
            // 
            // chkDecryptHiddenCalls
            // 
            this.chkDecryptHiddenCalls.AutoSize = true;
            this.chkDecryptHiddenCalls.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(117)))), ((int)(((byte)(9)))), ((int)(((byte)(12)))));
            this.chkDecryptHiddenCalls.Checked = true;
            this.chkDecryptHiddenCalls.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkDecryptHiddenCalls.Cursor = System.Windows.Forms.Cursors.Hand;
            this.chkDecryptHiddenCalls.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkDecryptHiddenCalls.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(30)))), ((int)(((byte)(35)))));
            this.chkDecryptHiddenCalls.ForeColor = System.Drawing.Color.Silver;
            this.chkDecryptHiddenCalls.HoverBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(30)))), ((int)(((byte)(35)))));
            this.chkDecryptHiddenCalls.HoverFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(188)))), ((int)(((byte)(14)))), ((int)(((byte)(18)))));
            this.chkDecryptHiddenCalls.HoverForeColor = System.Drawing.Color.White;
            this.chkDecryptHiddenCalls.Location = new System.Drawing.Point(314, 47);
            this.chkDecryptHiddenCalls.Name = "chkDecryptHiddenCalls";
            this.chkDecryptHiddenCalls.PressBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(30)))), ((int)(((byte)(35)))));
            this.chkDecryptHiddenCalls.PressFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(188)))), ((int)(((byte)(14)))), ((int)(((byte)(18)))));
            this.chkDecryptHiddenCalls.PressForeColor = System.Drawing.Color.Gray;
            this.chkDecryptHiddenCalls.Size = new System.Drawing.Size(305, 16);
            this.chkDecryptHiddenCalls.TabIndex = 12;
            this.chkDecryptHiddenCalls.Tag = "--fix-proxy";
            this.chkDecryptHiddenCalls.Text = "Fix proxied calls";
            this.chkDecryptHiddenCalls.UseVisualStyleBackColor = true;
            this.chkDecryptHiddenCalls.CheckedChanged += new System.EventHandler(this.CheckedChanged);
            // 
            // chkDeobCFlow
            // 
            this.chkDeobCFlow.AutoSize = true;
            this.chkDeobCFlow.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(117)))), ((int)(((byte)(9)))), ((int)(((byte)(12)))));
            this.chkDeobCFlow.Checked = true;
            this.chkDeobCFlow.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkDeobCFlow.Cursor = System.Windows.Forms.Cursors.Hand;
            this.chkDeobCFlow.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkDeobCFlow.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(30)))), ((int)(((byte)(35)))));
            this.chkDeobCFlow.ForeColor = System.Drawing.Color.Silver;
            this.chkDeobCFlow.HoverBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(30)))), ((int)(((byte)(35)))));
            this.chkDeobCFlow.HoverFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(188)))), ((int)(((byte)(14)))), ((int)(((byte)(18)))));
            this.chkDeobCFlow.HoverForeColor = System.Drawing.Color.White;
            this.chkDeobCFlow.Location = new System.Drawing.Point(314, 25);
            this.chkDeobCFlow.Name = "chkDeobCFlow";
            this.chkDeobCFlow.PressBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(30)))), ((int)(((byte)(35)))));
            this.chkDeobCFlow.PressFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(188)))), ((int)(((byte)(14)))), ((int)(((byte)(18)))));
            this.chkDeobCFlow.PressForeColor = System.Drawing.Color.Gray;
            this.chkDeobCFlow.Size = new System.Drawing.Size(305, 16);
            this.chkDeobCFlow.TabIndex = 4;
            this.chkDeobCFlow.Tag = "--deob-cflow";
            this.chkDeobCFlow.Text = "Deobfuscate Control Flow";
            this.chkDeobCFlow.UseVisualStyleBackColor = true;
            this.chkDeobCFlow.CheckedChanged += new System.EventHandler(this.CheckedChanged);
            // 
            // chkDecryptTokens
            // 
            this.chkDecryptTokens.AutoSize = true;
            this.chkDecryptTokens.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(117)))), ((int)(((byte)(9)))), ((int)(((byte)(12)))));
            this.chkDecryptTokens.Checked = true;
            this.chkDecryptTokens.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkDecryptTokens.Cursor = System.Windows.Forms.Cursors.Hand;
            this.chkDecryptTokens.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkDecryptTokens.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(30)))), ((int)(((byte)(35)))));
            this.chkDecryptTokens.ForeColor = System.Drawing.Color.Silver;
            this.chkDecryptTokens.HoverBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(30)))), ((int)(((byte)(35)))));
            this.chkDecryptTokens.HoverFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(188)))), ((int)(((byte)(14)))), ((int)(((byte)(18)))));
            this.chkDecryptTokens.HoverForeColor = System.Drawing.Color.White;
            this.chkDecryptTokens.Location = new System.Drawing.Point(3, 91);
            this.chkDecryptTokens.Name = "chkDecryptTokens";
            this.chkDecryptTokens.PressBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(30)))), ((int)(((byte)(35)))));
            this.chkDecryptTokens.PressFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(188)))), ((int)(((byte)(14)))), ((int)(((byte)(18)))));
            this.chkDecryptTokens.PressForeColor = System.Drawing.Color.Gray;
            this.chkDecryptTokens.Size = new System.Drawing.Size(305, 16);
            this.chkDecryptTokens.TabIndex = 8;
            this.chkDecryptTokens.Tag = "--deob-tokens";
            this.chkDecryptTokens.Text = "Deobfuscate Tokens";
            this.chkDecryptTokens.UseVisualStyleBackColor = true;
            this.chkDecryptTokens.CheckedChanged += new System.EventHandler(this.CheckedChanged);
            // 
            // chkDecryptResources
            // 
            this.chkDecryptResources.AutoSize = true;
            this.chkDecryptResources.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(117)))), ((int)(((byte)(9)))), ((int)(((byte)(12)))));
            this.chkDecryptResources.Checked = true;
            this.chkDecryptResources.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkDecryptResources.Cursor = System.Windows.Forms.Cursors.Hand;
            this.chkDecryptResources.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkDecryptResources.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(30)))), ((int)(((byte)(35)))));
            this.chkDecryptResources.ForeColor = System.Drawing.Color.Silver;
            this.chkDecryptResources.HoverBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(30)))), ((int)(((byte)(35)))));
            this.chkDecryptResources.HoverFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(188)))), ((int)(((byte)(14)))), ((int)(((byte)(18)))));
            this.chkDecryptResources.HoverForeColor = System.Drawing.Color.White;
            this.chkDecryptResources.Location = new System.Drawing.Point(3, 69);
            this.chkDecryptResources.Name = "chkDecryptResources";
            this.chkDecryptResources.PressBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(30)))), ((int)(((byte)(35)))));
            this.chkDecryptResources.PressFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(188)))), ((int)(((byte)(14)))), ((int)(((byte)(18)))));
            this.chkDecryptResources.PressForeColor = System.Drawing.Color.Gray;
            this.chkDecryptResources.Size = new System.Drawing.Size(305, 16);
            this.chkDecryptResources.TabIndex = 6;
            this.chkDecryptResources.Tag = "--dec-rsrc";
            this.chkDecryptResources.Text = "Decrypt Resources";
            this.chkDecryptResources.UseVisualStyleBackColor = true;
            this.chkDecryptResources.CheckedChanged += new System.EventHandler(this.CheckedChanged);
            // 
            // chkRemoveRefProxies
            // 
            this.chkRemoveRefProxies.AutoSize = true;
            this.chkRemoveRefProxies.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(117)))), ((int)(((byte)(9)))), ((int)(((byte)(12)))));
            this.chkRemoveRefProxies.Checked = true;
            this.chkRemoveRefProxies.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkRemoveRefProxies.Cursor = System.Windows.Forms.Cursors.Hand;
            this.chkRemoveRefProxies.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkRemoveRefProxies.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(30)))), ((int)(((byte)(35)))));
            this.chkRemoveRefProxies.ForeColor = System.Drawing.Color.Silver;
            this.chkRemoveRefProxies.HoverBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(30)))), ((int)(((byte)(35)))));
            this.chkRemoveRefProxies.HoverFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(188)))), ((int)(((byte)(14)))), ((int)(((byte)(18)))));
            this.chkRemoveRefProxies.HoverForeColor = System.Drawing.Color.White;
            this.chkRemoveRefProxies.Location = new System.Drawing.Point(3, 47);
            this.chkRemoveRefProxies.Name = "chkRemoveRefProxies";
            this.chkRemoveRefProxies.PressBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(30)))), ((int)(((byte)(35)))));
            this.chkRemoveRefProxies.PressFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(188)))), ((int)(((byte)(14)))), ((int)(((byte)(18)))));
            this.chkRemoveRefProxies.PressForeColor = System.Drawing.Color.Gray;
            this.chkRemoveRefProxies.Size = new System.Drawing.Size(305, 16);
            this.chkRemoveRefProxies.TabIndex = 5;
            this.chkRemoveRefProxies.Tag = "--inline-methods";
            this.chkRemoveRefProxies.Text = "Inline Short Methods";
            this.chkRemoveRefProxies.UseVisualStyleBackColor = true;
            this.chkRemoveRefProxies.CheckedChanged += new System.EventHandler(this.CheckedChanged);
            // 
            // chkDecryptMethods
            // 
            this.chkDecryptMethods.AutoSize = true;
            this.chkDecryptMethods.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(117)))), ((int)(((byte)(9)))), ((int)(((byte)(12)))));
            this.chkDecryptMethods.Checked = true;
            this.chkDecryptMethods.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkDecryptMethods.Cursor = System.Windows.Forms.Cursors.Hand;
            this.chkDecryptMethods.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkDecryptMethods.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(30)))), ((int)(((byte)(35)))));
            this.chkDecryptMethods.ForeColor = System.Drawing.Color.Silver;
            this.chkDecryptMethods.HoverBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(30)))), ((int)(((byte)(35)))));
            this.chkDecryptMethods.HoverFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(188)))), ((int)(((byte)(14)))), ((int)(((byte)(18)))));
            this.chkDecryptMethods.HoverForeColor = System.Drawing.Color.White;
            this.chkDecryptMethods.Location = new System.Drawing.Point(3, 25);
            this.chkDecryptMethods.Name = "chkDecryptMethods";
            this.chkDecryptMethods.PressBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(30)))), ((int)(((byte)(35)))));
            this.chkDecryptMethods.PressFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(188)))), ((int)(((byte)(14)))), ((int)(((byte)(18)))));
            this.chkDecryptMethods.PressForeColor = System.Drawing.Color.Gray;
            this.chkDecryptMethods.Size = new System.Drawing.Size(305, 16);
            this.chkDecryptMethods.TabIndex = 7;
            this.chkDecryptMethods.Tag = "--dec-methods";
            this.chkDecryptMethods.Text = "Decrypt Methods Body";
            this.chkDecryptMethods.UseVisualStyleBackColor = true;
            this.chkDecryptMethods.CheckedChanged += new System.EventHandler(this.CheckedChanged);
            // 
            // chkSelectUnSelectAll
            // 
            this.chkSelectUnSelectAll.AutoSize = true;
            this.chkSelectUnSelectAll.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(117)))), ((int)(((byte)(9)))), ((int)(((byte)(12)))));
            this.chkSelectUnSelectAll.Cursor = System.Windows.Forms.Cursors.Hand;
            this.chkSelectUnSelectAll.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkSelectUnSelectAll.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(30)))), ((int)(((byte)(35)))));
            this.chkSelectUnSelectAll.ForeColor = System.Drawing.Color.Silver;
            this.chkSelectUnSelectAll.HoverBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(30)))), ((int)(((byte)(35)))));
            this.chkSelectUnSelectAll.HoverFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(188)))), ((int)(((byte)(14)))), ((int)(((byte)(18)))));
            this.chkSelectUnSelectAll.HoverForeColor = System.Drawing.Color.White;
            this.chkSelectUnSelectAll.Location = new System.Drawing.Point(3, 3);
            this.chkSelectUnSelectAll.Name = "chkSelectUnSelectAll";
            this.chkSelectUnSelectAll.PressBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(30)))), ((int)(((byte)(35)))));
            this.chkSelectUnSelectAll.PressFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(188)))), ((int)(((byte)(14)))), ((int)(((byte)(18)))));
            this.chkSelectUnSelectAll.PressForeColor = System.Drawing.Color.Gray;
            this.chkSelectUnSelectAll.Size = new System.Drawing.Size(305, 16);
            this.chkSelectUnSelectAll.TabIndex = 17;
            this.chkSelectUnSelectAll.Tag = "";
            this.chkSelectUnSelectAll.Text = "Select All";
            this.chkSelectUnSelectAll.UseVisualStyleBackColor = true;
            this.chkSelectUnSelectAll.CheckedChanged += new System.EventHandler(this.chkSelectUnSelectAll_CheckedChanged);
            // 
            // chkRemSn
            // 
            this.chkRemSn.AutoSize = true;
            this.chkRemSn.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(117)))), ((int)(((byte)(9)))), ((int)(((byte)(12)))));
            this.chkRemSn.Checked = true;
            this.chkRemSn.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkRemSn.Cursor = System.Windows.Forms.Cursors.Hand;
            this.chkRemSn.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkRemSn.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(30)))), ((int)(((byte)(35)))));
            this.chkRemSn.ForeColor = System.Drawing.Color.Silver;
            this.chkRemSn.HoverBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(30)))), ((int)(((byte)(35)))));
            this.chkRemSn.HoverFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(188)))), ((int)(((byte)(14)))), ((int)(((byte)(18)))));
            this.chkRemSn.HoverForeColor = System.Drawing.Color.White;
            this.chkRemSn.Location = new System.Drawing.Point(625, 91);
            this.chkRemSn.Name = "chkRemSn";
            this.chkRemSn.PressBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(30)))), ((int)(((byte)(35)))));
            this.chkRemSn.PressFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(188)))), ((int)(((byte)(14)))), ((int)(((byte)(18)))));
            this.chkRemSn.PressForeColor = System.Drawing.Color.Gray;
            this.chkRemSn.Size = new System.Drawing.Size(306, 16);
            this.chkRemSn.TabIndex = 2;
            this.chkRemSn.Tag = "--rem-sn";
            this.chkRemSn.Text = "Remove Strong Name Removal Protection";
            this.chkRemSn.UseVisualStyleBackColor = true;
            this.chkRemSn.CheckedChanged += new System.EventHandler(this.CheckedChanged);
            // 
            // chkRename
            // 
            this.chkRename.AutoSize = true;
            this.chkRename.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(117)))), ((int)(((byte)(9)))), ((int)(((byte)(12)))));
            this.chkRename.Checked = true;
            this.chkRename.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkRename.ContextMenuStrip = this.ctxRename;
            this.chkRename.Cursor = System.Windows.Forms.Cursors.Hand;
            this.chkRename.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkRename.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(30)))), ((int)(((byte)(35)))));
            this.chkRename.ForeColor = System.Drawing.Color.Silver;
            this.chkRename.HoverBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(30)))), ((int)(((byte)(35)))));
            this.chkRename.HoverFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(188)))), ((int)(((byte)(14)))), ((int)(((byte)(18)))));
            this.chkRename.HoverForeColor = System.Drawing.Color.White;
            this.chkRename.Location = new System.Drawing.Point(3, 113);
            this.chkRename.Name = "chkRename";
            this.chkRename.PressBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(30)))), ((int)(((byte)(35)))));
            this.chkRename.PressFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(188)))), ((int)(((byte)(14)))), ((int)(((byte)(18)))));
            this.chkRename.PressForeColor = System.Drawing.Color.Gray;
            this.chkRename.Size = new System.Drawing.Size(305, 16);
            this.chkRename.TabIndex = 17;
            this.chkRename.Tag = "--rename ntmfe";
            this.chkRename.Text = "Rename obfuscated symbols name";
            this.chkRename.UseVisualStyleBackColor = true;
            this.chkRename.CheckedChanged += new System.EventHandler(this.CheckedChanged);
            this.chkRename.MouseClick += new System.Windows.Forms.MouseEventHandler(this.OpenCtxRename);
            // 
            // ctxRename
            // 
            this.ctxRename.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(23)))), ((int)(((byte)(23)))), ((int)(((byte)(23)))));
            this.ctxRename.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.ctxRename.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem8,
            this.toolStripMenuItem9,
            this.toolStripMenuItem10,
            this.toolStripMenuItem13,
            this.toolStripMenuItem15,
            this.toolStripMenuItem14,
            this.toolStripMenuItem16,
            this.toolStripMenuItem12});
            this.ctxRename.Name = "ctxLogs";
            this.ctxRename.ShowImageMargin = false;
            this.ctxRename.Size = new System.Drawing.Size(152, 158);
            this.ctxRename.Tag = "close";
            this.ctxRename.Closing += new System.Windows.Forms.ToolStripDropDownClosingEventHandler(this.ctxRename_Closing);
            // 
            // toolStripMenuItem8
            // 
            this.toolStripMenuItem8.AutoSize = false;
            this.toolStripMenuItem8.Enabled = false;
            this.toolStripMenuItem8.Name = "toolStripMenuItem8";
            this.toolStripMenuItem8.Size = new System.Drawing.Size(185, 5);
            this.toolStripMenuItem8.Text = " ";
            // 
            // toolStripMenuItem9
            // 
            this.toolStripMenuItem9.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripMenuItem9.Font = new System.Drawing.Font("Segoe UI Semibold", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.toolStripMenuItem9.ForeColor = System.Drawing.Color.Silver;
            this.toolStripMenuItem9.Name = "toolStripMenuItem9";
            this.toolStripMenuItem9.Size = new System.Drawing.Size(151, 24);
            this.toolStripMenuItem9.Tag = "n";
            this.toolStripMenuItem9.Text = " ✓  Namespaces";
            this.toolStripMenuItem9.Click += new System.EventHandler(this.SetRenamingOptions);
            this.toolStripMenuItem9.MouseDown += new System.Windows.Forms.MouseEventHandler(this.KeepCtxRenameOpen);
            // 
            // toolStripMenuItem10
            // 
            this.toolStripMenuItem10.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripMenuItem10.Font = new System.Drawing.Font("Segoe UI Semibold", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.toolStripMenuItem10.ForeColor = System.Drawing.Color.Silver;
            this.toolStripMenuItem10.Name = "toolStripMenuItem10";
            this.toolStripMenuItem10.Size = new System.Drawing.Size(151, 24);
            this.toolStripMenuItem10.Tag = "t";
            this.toolStripMenuItem10.Text = " ✓  Types";
            this.toolStripMenuItem10.Click += new System.EventHandler(this.SetRenamingOptions);
            this.toolStripMenuItem10.MouseDown += new System.Windows.Forms.MouseEventHandler(this.KeepCtxRenameOpen);
            // 
            // toolStripMenuItem13
            // 
            this.toolStripMenuItem13.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripMenuItem13.Font = new System.Drawing.Font("Segoe UI Semibold", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.toolStripMenuItem13.ForeColor = System.Drawing.Color.Silver;
            this.toolStripMenuItem13.Name = "toolStripMenuItem13";
            this.toolStripMenuItem13.Size = new System.Drawing.Size(151, 24);
            this.toolStripMenuItem13.Tag = "m";
            this.toolStripMenuItem13.Text = " ✓  Methods";
            this.toolStripMenuItem13.Click += new System.EventHandler(this.SetRenamingOptions);
            this.toolStripMenuItem13.MouseDown += new System.Windows.Forms.MouseEventHandler(this.KeepCtxRenameOpen);
            // 
            // toolStripMenuItem15
            // 
            this.toolStripMenuItem15.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripMenuItem15.Font = new System.Drawing.Font("Segoe UI Semibold", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.toolStripMenuItem15.ForeColor = System.Drawing.Color.Silver;
            this.toolStripMenuItem15.Name = "toolStripMenuItem15";
            this.toolStripMenuItem15.Size = new System.Drawing.Size(151, 24);
            this.toolStripMenuItem15.Tag = "f";
            this.toolStripMenuItem15.Text = " ✓  Fields";
            this.toolStripMenuItem15.Click += new System.EventHandler(this.SetRenamingOptions);
            this.toolStripMenuItem15.MouseDown += new System.Windows.Forms.MouseEventHandler(this.KeepCtxRenameOpen);
            // 
            // toolStripMenuItem14
            // 
            this.toolStripMenuItem14.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripMenuItem14.Font = new System.Drawing.Font("Segoe UI Semibold", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.toolStripMenuItem14.ForeColor = System.Drawing.Color.Silver;
            this.toolStripMenuItem14.Name = "toolStripMenuItem14";
            this.toolStripMenuItem14.Size = new System.Drawing.Size(151, 24);
            this.toolStripMenuItem14.Tag = "p";
            this.toolStripMenuItem14.Text = " X  Properties";
            this.toolStripMenuItem14.Click += new System.EventHandler(this.SetRenamingOptions);
            this.toolStripMenuItem14.MouseDown += new System.Windows.Forms.MouseEventHandler(this.KeepCtxRenameOpen);
            // 
            // toolStripMenuItem16
            // 
            this.toolStripMenuItem16.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripMenuItem16.Font = new System.Drawing.Font("Segoe UI Semibold", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.toolStripMenuItem16.ForeColor = System.Drawing.Color.Silver;
            this.toolStripMenuItem16.Name = "toolStripMenuItem16";
            this.toolStripMenuItem16.Size = new System.Drawing.Size(151, 24);
            this.toolStripMenuItem16.Tag = "e";
            this.toolStripMenuItem16.Text = " ✓  Events";
            this.toolStripMenuItem16.Click += new System.EventHandler(this.SetRenamingOptions);
            this.toolStripMenuItem16.MouseDown += new System.Windows.Forms.MouseEventHandler(this.KeepCtxRenameOpen);
            // 
            // toolStripMenuItem12
            // 
            this.toolStripMenuItem12.AutoSize = false;
            this.toolStripMenuItem12.Enabled = false;
            this.toolStripMenuItem12.Name = "toolStripMenuItem12";
            this.toolStripMenuItem12.Size = new System.Drawing.Size(185, 5);
            this.toolStripMenuItem12.Text = " ";
            // 
            // chkRenameShort
            // 
            this.chkRenameShort.AutoSize = true;
            this.chkRenameShort.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(117)))), ((int)(((byte)(9)))), ((int)(((byte)(12)))));
            this.chkRenameShort.Cursor = System.Windows.Forms.Cursors.Hand;
            this.chkRenameShort.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkRenameShort.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(30)))), ((int)(((byte)(35)))));
            this.chkRenameShort.ForeColor = System.Drawing.Color.Silver;
            this.chkRenameShort.HoverBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(30)))), ((int)(((byte)(35)))));
            this.chkRenameShort.HoverFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(188)))), ((int)(((byte)(14)))), ((int)(((byte)(18)))));
            this.chkRenameShort.HoverForeColor = System.Drawing.Color.White;
            this.chkRenameShort.Location = new System.Drawing.Point(314, 113);
            this.chkRenameShort.Name = "chkRenameShort";
            this.chkRenameShort.PressBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(30)))), ((int)(((byte)(35)))));
            this.chkRenameShort.PressFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(188)))), ((int)(((byte)(14)))), ((int)(((byte)(18)))));
            this.chkRenameShort.PressForeColor = System.Drawing.Color.Gray;
            this.chkRenameShort.Size = new System.Drawing.Size(305, 16);
            this.chkRenameShort.TabIndex = 18;
            this.chkRenameShort.Tag = "--rename-short";
            this.chkRenameShort.Text = "Rename short names";
            this.chkRenameShort.UseVisualStyleBackColor = true;
            this.chkRenameShort.CheckedChanged += new System.EventHandler(this.CheckedChanged);
            // 
            // chkRemJunks
            // 
            this.chkRemJunks.AutoSize = true;
            this.chkRemJunks.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(117)))), ((int)(((byte)(9)))), ((int)(((byte)(12)))));
            this.chkRemJunks.Checked = true;
            this.chkRemJunks.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkRemJunks.Cursor = System.Windows.Forms.Cursors.Hand;
            this.chkRemJunks.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkRemJunks.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(30)))), ((int)(((byte)(35)))));
            this.chkRemJunks.ForeColor = System.Drawing.Color.Silver;
            this.chkRemJunks.HoverBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(30)))), ((int)(((byte)(35)))));
            this.chkRemJunks.HoverFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(188)))), ((int)(((byte)(14)))), ((int)(((byte)(18)))));
            this.chkRemJunks.HoverForeColor = System.Drawing.Color.White;
            this.chkRemJunks.Location = new System.Drawing.Point(3, 135);
            this.chkRemJunks.Name = "chkRemJunks";
            this.chkRemJunks.PressBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(30)))), ((int)(((byte)(35)))));
            this.chkRemJunks.PressFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(188)))), ((int)(((byte)(14)))), ((int)(((byte)(18)))));
            this.chkRemJunks.PressForeColor = System.Drawing.Color.Gray;
            this.chkRemJunks.Size = new System.Drawing.Size(305, 16);
            this.chkRemJunks.TabIndex = 2;
            this.chkRemJunks.Tag = "--rem-junks";
            this.chkRemJunks.Text = "Remove Junks (BETA)";
            this.chkRemJunks.UseVisualStyleBackColor = true;
            this.chkRemJunks.CheckedChanged += new System.EventHandler(this.CheckedChanged);
            // 
            // chkKeepTypes
            // 
            this.chkKeepTypes.AutoSize = true;
            this.chkKeepTypes.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(117)))), ((int)(((byte)(9)))), ((int)(((byte)(12)))));
            this.chkKeepTypes.Cursor = System.Windows.Forms.Cursors.Hand;
            this.chkKeepTypes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkKeepTypes.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(30)))), ((int)(((byte)(35)))));
            this.chkKeepTypes.ForeColor = System.Drawing.Color.Silver;
            this.chkKeepTypes.HoverBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(30)))), ((int)(((byte)(35)))));
            this.chkKeepTypes.HoverFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(188)))), ((int)(((byte)(14)))), ((int)(((byte)(18)))));
            this.chkKeepTypes.HoverForeColor = System.Drawing.Color.White;
            this.chkKeepTypes.Location = new System.Drawing.Point(625, 113);
            this.chkKeepTypes.Name = "chkKeepTypes";
            this.chkKeepTypes.PressBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(30)))), ((int)(((byte)(35)))));
            this.chkKeepTypes.PressFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(188)))), ((int)(((byte)(14)))), ((int)(((byte)(18)))));
            this.chkKeepTypes.PressForeColor = System.Drawing.Color.Gray;
            this.chkKeepTypes.Size = new System.Drawing.Size(306, 16);
            this.chkKeepTypes.TabIndex = 2;
            this.chkKeepTypes.Tag = "--keep-types";
            this.chkKeepTypes.Text = "Keep Obfuscator Types";
            this.chkKeepTypes.UseVisualStyleBackColor = true;
            this.chkKeepTypes.CheckedChanged += new System.EventHandler(this.CheckedChanged);
            // 
            // chkRemCalls
            // 
            this.chkRemCalls.AutoSize = true;
            this.chkRemCalls.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(117)))), ((int)(((byte)(9)))), ((int)(((byte)(12)))));
            this.chkRemCalls.Checked = true;
            this.chkRemCalls.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkRemCalls.Cursor = System.Windows.Forms.Cursors.Hand;
            this.chkRemCalls.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkRemCalls.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(30)))), ((int)(((byte)(35)))));
            this.chkRemCalls.ForeColor = System.Drawing.Color.Silver;
            this.chkRemCalls.HoverBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(30)))), ((int)(((byte)(35)))));
            this.chkRemCalls.HoverFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(188)))), ((int)(((byte)(14)))), ((int)(((byte)(18)))));
            this.chkRemCalls.HoverForeColor = System.Drawing.Color.White;
            this.chkRemCalls.Location = new System.Drawing.Point(314, 135);
            this.chkRemCalls.Name = "chkRemCalls";
            this.chkRemCalls.PressBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(30)))), ((int)(((byte)(35)))));
            this.chkRemCalls.PressFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(188)))), ((int)(((byte)(14)))), ((int)(((byte)(18)))));
            this.chkRemCalls.PressForeColor = System.Drawing.Color.Gray;
            this.chkRemCalls.Size = new System.Drawing.Size(305, 16);
            this.chkRemCalls.TabIndex = 2;
            this.chkRemCalls.Tag = "--rem-calls";
            this.chkRemCalls.Text = "Remove Calls To Obfuscator Types";
            this.chkRemCalls.UseVisualStyleBackColor = true;
            this.chkRemCalls.CheckedChanged += new System.EventHandler(this.CheckedChanged);
            // 
            // chkPreserveAll
            // 
            this.chkPreserveAll.AutoSize = true;
            this.chkPreserveAll.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(117)))), ((int)(((byte)(9)))), ((int)(((byte)(12)))));
            this.chkPreserveAll.Cursor = System.Windows.Forms.Cursors.Hand;
            this.chkPreserveAll.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkPreserveAll.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(30)))), ((int)(((byte)(35)))));
            this.chkPreserveAll.ForeColor = System.Drawing.Color.Silver;
            this.chkPreserveAll.HoverBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(30)))), ((int)(((byte)(35)))));
            this.chkPreserveAll.HoverFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(188)))), ((int)(((byte)(14)))), ((int)(((byte)(18)))));
            this.chkPreserveAll.HoverForeColor = System.Drawing.Color.White;
            this.chkPreserveAll.Location = new System.Drawing.Point(625, 135);
            this.chkPreserveAll.Name = "chkPreserveAll";
            this.chkPreserveAll.PressBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(30)))), ((int)(((byte)(35)))));
            this.chkPreserveAll.PressFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(188)))), ((int)(((byte)(14)))), ((int)(((byte)(18)))));
            this.chkPreserveAll.PressForeColor = System.Drawing.Color.Gray;
            this.chkPreserveAll.Size = new System.Drawing.Size(306, 16);
            this.chkPreserveAll.TabIndex = 15;
            this.chkPreserveAll.Tag = "--preserve-all";
            this.chkPreserveAll.Text = "Preserve All MD Tokens";
            this.chkPreserveAll.UseVisualStyleBackColor = true;
            this.chkPreserveAll.CheckedChanged += new System.EventHandler(this.CheckedChanged);
            // 
            // chkKeepOldMaxStack
            // 
            this.chkKeepOldMaxStack.AutoSize = true;
            this.chkKeepOldMaxStack.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(117)))), ((int)(((byte)(9)))), ((int)(((byte)(12)))));
            this.chkKeepOldMaxStack.Cursor = System.Windows.Forms.Cursors.Hand;
            this.chkKeepOldMaxStack.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkKeepOldMaxStack.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(30)))), ((int)(((byte)(35)))));
            this.chkKeepOldMaxStack.ForeColor = System.Drawing.Color.Silver;
            this.chkKeepOldMaxStack.HoverBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(30)))), ((int)(((byte)(35)))));
            this.chkKeepOldMaxStack.HoverFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(188)))), ((int)(((byte)(14)))), ((int)(((byte)(18)))));
            this.chkKeepOldMaxStack.HoverForeColor = System.Drawing.Color.White;
            this.chkKeepOldMaxStack.Location = new System.Drawing.Point(3, 157);
            this.chkKeepOldMaxStack.Name = "chkKeepOldMaxStack";
            this.chkKeepOldMaxStack.PressBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(238)))), ((int)(((byte)(30)))), ((int)(((byte)(35)))));
            this.chkKeepOldMaxStack.PressFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(188)))), ((int)(((byte)(14)))), ((int)(((byte)(18)))));
            this.chkKeepOldMaxStack.PressForeColor = System.Drawing.Color.Gray;
            this.chkKeepOldMaxStack.Size = new System.Drawing.Size(305, 19);
            this.chkKeepOldMaxStack.TabIndex = 14;
            this.chkKeepOldMaxStack.Tag = "--keep-max-stack";
            this.chkKeepOldMaxStack.Text = "Keep Old Max Stack Value";
            this.chkKeepOldMaxStack.UseVisualStyleBackColor = true;
            this.chkKeepOldMaxStack.CheckedChanged += new System.EventHandler(this.CheckedChanged);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(27)))), ((int)(((byte)(27)))));
            this.panel1.Controls.Add(this.llblGitHub);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.lblVersion);
            this.panel1.Controls.Add(this.llblWebsite);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.lblAuthor);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(30, 591);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(5, 10, 5, 0);
            this.panel1.Size = new System.Drawing.Size(997, 40);
            this.panel1.TabIndex = 17;
            // 
            // llblGitHub
            // 
            this.llblGitHub.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(188)))), ((int)(((byte)(14)))), ((int)(((byte)(18)))));
            this.llblGitHub.AutoSize = true;
            this.llblGitHub.Dock = System.Windows.Forms.DockStyle.Left;
            this.llblGitHub.Font = new System.Drawing.Font("Segoe UI Semibold", 7F, System.Drawing.FontStyle.Bold);
            this.llblGitHub.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(188)))), ((int)(((byte)(14)))), ((int)(((byte)(18)))));
            this.llblGitHub.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(188)))), ((int)(((byte)(14)))), ((int)(((byte)(18)))));
            this.llblGitHub.Location = new System.Drawing.Point(405, 10);
            this.llblGitHub.Name = "llblGitHub";
            this.llblGitHub.Size = new System.Drawing.Size(45, 15);
            this.llblGitHub.TabIndex = 7;
            this.llblGitHub.TabStop = true;
            this.llblGitHub.Tag = "https://github.com/SychicBoy/NETReactorSlayer";
            this.llblGitHub.Text = "GitHub";
            this.llblGitHub.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(188)))), ((int)(((byte)(14)))), ((int)(((byte)(18)))));
            this.llblGitHub.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.llblGitHub_LinkClicked);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Dock = System.Windows.Forms.DockStyle.Left;
            this.label2.Font = new System.Drawing.Font("Segoe UI Semibold", 7F, System.Drawing.FontStyle.Bold);
            this.label2.ForeColor = System.Drawing.Color.Gray;
            this.label2.Location = new System.Drawing.Point(316, 10);
            this.label2.Name = "label2";
            this.label2.Padding = new System.Windows.Forms.Padding(20, 0, 0, 0);
            this.label2.Size = new System.Drawing.Size(89, 15);
            this.label2.TabIndex = 6;
            this.label2.Text = "Repository: ";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Dock = System.Windows.Forms.DockStyle.Right;
            this.label5.Font = new System.Drawing.Font("Segoe UI Semibold", 7F, System.Drawing.FontStyle.Bold);
            this.label5.ForeColor = System.Drawing.Color.Gray;
            this.label5.Location = new System.Drawing.Point(943, 10);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(49, 15);
            this.label5.TabIndex = 4;
            this.label5.Text = "Version:";
            // 
            // lblVersion
            // 
            this.lblVersion.AutoSize = true;
            this.lblVersion.Dock = System.Windows.Forms.DockStyle.Right;
            this.lblVersion.Font = new System.Drawing.Font("Segoe UI Semibold", 7F, System.Drawing.FontStyle.Bold);
            this.lblVersion.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(188)))), ((int)(((byte)(14)))), ((int)(((byte)(18)))));
            this.lblVersion.Location = new System.Drawing.Point(992, 10);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(0, 15);
            this.lblVersion.TabIndex = 5;
            // 
            // llblWebsite
            // 
            this.llblWebsite.ActiveLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(188)))), ((int)(((byte)(14)))), ((int)(((byte)(18)))));
            this.llblWebsite.AutoSize = true;
            this.llblWebsite.Dock = System.Windows.Forms.DockStyle.Left;
            this.llblWebsite.Font = new System.Drawing.Font("Segoe UI Semibold", 7F, System.Drawing.FontStyle.Bold);
            this.llblWebsite.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(188)))), ((int)(((byte)(14)))), ((int)(((byte)(18)))));
            this.llblWebsite.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(188)))), ((int)(((byte)(14)))), ((int)(((byte)(18)))));
            this.llblWebsite.Location = new System.Drawing.Point(193, 10);
            this.llblWebsite.Name = "llblWebsite";
            this.llblWebsite.Size = new System.Drawing.Size(123, 15);
            this.llblWebsite.TabIndex = 3;
            this.llblWebsite.TabStop = true;
            this.llblWebsite.Tag = "https://www.CodeStrikers.org";
            this.llblWebsite.Text = "www.CodeStrikers.org";
            this.llblWebsite.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(188)))), ((int)(((byte)(14)))), ((int)(((byte)(18)))));
            this.llblWebsite.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.llblWebsite_LinkClicked);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Dock = System.Windows.Forms.DockStyle.Left;
            this.label4.Font = new System.Drawing.Font("Segoe UI Semibold", 7F, System.Drawing.FontStyle.Bold);
            this.label4.ForeColor = System.Drawing.Color.Gray;
            this.label4.Location = new System.Drawing.Point(117, 10);
            this.label4.Name = "label4";
            this.label4.Padding = new System.Windows.Forms.Padding(20, 0, 0, 0);
            this.label4.Size = new System.Drawing.Size(76, 15);
            this.label4.TabIndex = 2;
            this.label4.Text = "Website: ";
            // 
            // lblAuthor
            // 
            this.lblAuthor.AutoSize = true;
            this.lblAuthor.Dock = System.Windows.Forms.DockStyle.Left;
            this.lblAuthor.Font = new System.Drawing.Font("Segoe UI Semibold", 7F, System.Drawing.FontStyle.Bold);
            this.lblAuthor.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(188)))), ((int)(((byte)(14)))), ((int)(((byte)(18)))));
            this.lblAuthor.Location = new System.Drawing.Point(55, 10);
            this.lblAuthor.Name = "lblAuthor";
            this.lblAuthor.Size = new System.Drawing.Size(62, 15);
            this.lblAuthor.TabIndex = 1;
            this.lblAuthor.Text = "SychicBoy";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Left;
            this.label1.Font = new System.Drawing.Font("Segoe UI Semibold", 7F, System.Drawing.FontStyle.Bold);
            this.label1.ForeColor = System.Drawing.Color.Gray;
            this.label1.Location = new System.Drawing.Point(5, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(50, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Author: ";
            // 
            // ctxMenu
            // 
            this.ctxMenu.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(23)))), ((int)(((byte)(23)))), ((int)(((byte)(23)))));
            this.ctxMenu.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.ctxMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem3,
            this.toolStripMenuItem6,
            this.toolStripMenuItem7,
            this.toolStripMenuItem4,
            this.toolStripMenuItem5});
            this.ctxMenu.Name = "ctxLogs";
            this.ctxMenu.ShowImageMargin = false;
            this.ctxMenu.Size = new System.Drawing.Size(169, 86);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.AutoSize = false;
            this.toolStripMenuItem3.Enabled = false;
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(185, 5);
            this.toolStripMenuItem3.Text = " ";
            // 
            // toolStripMenuItem6
            // 
            this.toolStripMenuItem6.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripMenuItem6.Font = new System.Drawing.Font("Segoe UI Semibold", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.toolStripMenuItem6.ForeColor = System.Drawing.Color.Silver;
            this.toolStripMenuItem6.Name = "toolStripMenuItem6";
            this.toolStripMenuItem6.Size = new System.Drawing.Size(168, 24);
            this.toolStripMenuItem6.Text = "  Check For Update";
            this.toolStripMenuItem6.Click += new System.EventHandler(this.toolStripMenuItem6_Click);
            // 
            // toolStripMenuItem7
            // 
            this.toolStripMenuItem7.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripMenuItem7.Font = new System.Drawing.Font("Segoe UI Semibold", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.toolStripMenuItem7.ForeColor = System.Drawing.Color.Silver;
            this.toolStripMenuItem7.Name = "toolStripMenuItem7";
            this.toolStripMenuItem7.Size = new System.Drawing.Size(168, 24);
            this.toolStripMenuItem7.Text = "  About";
            this.toolStripMenuItem7.Click += new System.EventHandler(this.toolStripMenuItem7_Click);
            // 
            // toolStripMenuItem4
            // 
            this.toolStripMenuItem4.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripMenuItem4.Font = new System.Drawing.Font("Segoe UI Semibold", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.toolStripMenuItem4.ForeColor = System.Drawing.Color.Silver;
            this.toolStripMenuItem4.Name = "toolStripMenuItem4";
            this.toolStripMenuItem4.Size = new System.Drawing.Size(168, 24);
            this.toolStripMenuItem4.Text = "  Exit";
            this.toolStripMenuItem4.Click += new System.EventHandler(this.toolStripMenuItem4_Click);
            // 
            // toolStripMenuItem5
            // 
            this.toolStripMenuItem5.AutoSize = false;
            this.toolStripMenuItem5.Enabled = false;
            this.toolStripMenuItem5.Name = "toolStripMenuItem5";
            this.toolStripMenuItem5.Size = new System.Drawing.Size(185, 5);
            this.toolStripMenuItem5.Text = " ";
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(27)))), ((int)(((byte)(27)))));
            this.ClientSize = new System.Drawing.Size(1057, 631);
            this.Controls.Add(this.pnlBase);
            this.Controls.Add(this.pnlHeader);
            this.Controls.Add(this.panel1);
            this.Font = new System.Drawing.Font("Consolas", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "MainWindow";
            this.Opacity = 0D;
            this.Padding = new System.Windows.Forms.Padding(30, 5, 30, 0);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = ".NET Reactor Slayer";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Closing);
            this.Shown += new System.EventHandler(this.MainWindow_Shown);
            this.pnlHeader.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picMenu)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picMinimize)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picExit)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picHeader)).EndInit();
            this.pnlBase.ResumeLayout(false);
            this.pnlButton.ResumeLayout(false);
            this.panelLogs.ResumeLayout(false);
            this.ctxLogs.ResumeLayout(false);
            this.pnlInput.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picBrowse)).EndInit();
            this.pnlTextBox.ResumeLayout(false);
            this.pnlOptions.ResumeLayout(false);
            this.tabelOptions.ResumeLayout(false);
            this.tabelOptions.PerformLayout();
            this.ctxRename.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ctxMenu.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private PictureBox picHeader;
        private Panel pnlHeader;
        private Panel pnlBase;
        private RichTextBox txtLogs;
        private NrsCheckBox chkRemJunks;
        private TableLayoutPanel tabelOptions;
        private NrsCheckBox chkDecryptResources;
        private NrsCheckBox chkRemoveRefProxies;
        private NrsCheckBox chkDeobCFlow;
        private NrsCheckBox chkDumpCosturaAsm;
        private NrsCheckBox chkDecryptTokens;
        private NrsCheckBox chkDecryptMethods;
        private Panel pnlOptions;
        private NrsCheckBox chkDumpDNRAsm;
        private NrsCheckBox chkDecryptStrings;
        private NrsCheckBox chkDecryptHiddenCalls;
        private NrsCheckBox chkPatchAntiTD;
        private NrsCheckBox chkPreserveAll;
        private NrsCheckBox chkKeepOldMaxStack;
        private Panel pnlInput;
        private Panel pnlSeparator;
        private PictureBox picExit;
        private PictureBox picMinimize;
        private Panel panel2;
        private Panel panel3;
        private Panel pnlButton;
        private NrsButton btnStart;
        private NrsTextBox txtInput;
        private Panel pnlTextBox;
        private PictureBox picBrowse;
        private Panel panelLogs;
        private NrsScrollBar scrollbarLogs;
        private Panel panel1;
        private Label lblAuthor;
        private Label label1;
        private Label label5;
        private Label lblVersion;
        private LinkLabel llblWebsite;
        private Label label4;
        private LinkLabel llblGitHub;
        private Label label2;
        private ContextMenuStrip ctxLogs;
        private ToolStripMenuItem copyLogsToolStripMenuItem;
        private ToolStripMenuItem toolStripMenuItem2;
        private ToolStripMenuItem toolStripMenuItem1;
        private NrsCheckBox chkDecryptBools;
        private NrsCheckBox chkSelectUnSelectAll;
        private PictureBox picMenu;
        private Panel panel4;
        private ContextMenuStrip ctxMenu;
        private ToolStripMenuItem toolStripMenuItem3;
        private ToolStripMenuItem toolStripMenuItem4;
        private ToolStripMenuItem toolStripMenuItem5;
        private ToolStripMenuItem toolStripMenuItem6;
        private ToolStripMenuItem toolStripMenuItem7;
        private NrsCheckBox chkKeepTypes;
        private NrsCheckBox chkRemCalls;
        private NrsCheckBox chkRemSn;
        private NrsCheckBox chkRenameShort;
        private NrsCheckBox chkRename;
        private ContextMenuStrip ctxRename;
        private ToolStripMenuItem toolStripMenuItem8;
        private ToolStripMenuItem toolStripMenuItem9;
        private ToolStripMenuItem toolStripMenuItem10;
        private ToolStripMenuItem toolStripMenuItem13;
        private ToolStripMenuItem toolStripMenuItem15;
        private ToolStripMenuItem toolStripMenuItem14;
        private ToolStripMenuItem toolStripMenuItem16;
        private ToolStripMenuItem toolStripMenuItem12;
    }
}

