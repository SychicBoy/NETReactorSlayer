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
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using NETReactorSlayer.GUI.Properties;

namespace NETReactorSlayer.GUI.Dialogs
{
    internal sealed class MsgBox : Form
    {
        private MsgBox()
        {
            FormBorderStyle = FormBorderStyle.None;
            BackColor = Color.Black;
            StartPosition = FormStartPosition.CenterParent;
            Padding = new Padding(2);
            Width = 400;
            AutoSize = false;
            _lblTitle = new Label
            {
                ForeColor = Color.Silver,
                Font = new Font("Consolas", 14, FontStyle.Bold, GraphicsUnit.Point, 0),
                Dock = DockStyle.Top,
                Height = 50,
                UseCompatibleTextRendering = false
            };

            _lblMessage = new Label
            {
                ForeColor = Color.Silver,
                Font = new Font("Consolas", 10),
                Dock = DockStyle.Fill,
                UseCompatibleTextRendering = false,
                AutoEllipsis = true,
                AutoSize = false
            };

            _flpButtons.FlowDirection = FlowDirection.RightToLeft;
            _flpButtons.Dock = DockStyle.Fill;

            _plHeader.Dock = DockStyle.Fill;
            _plHeader.Padding = new Padding(20, 30, 5, 0);
            _plHeader.Controls.Add(_lblMessage);
            _plHeader.Controls.Add(_lblTitle);

            _plFooter.Dock = DockStyle.Bottom;
            _plFooter.Padding = new Padding(15, 15, 15, 15);
            _plFooter.BackColor = Color.FromArgb(25, 25, 25);
            _plFooter.Height = 80;
            _plFooter.Controls.Add(_flpButtons);

            _picIcon.SizeMode = PictureBoxSizeMode.CenterImage;
            _picIcon.Location = new Point(30, 50);
            _picIcon.Dock = DockStyle.Fill;
            _plIcon.Dock = DockStyle.Left;
            _plIcon.Padding = new Padding(30, 0, 0, 30);
            _plIcon.Width = 90;
            _plIcon.Controls.Add(_picIcon);

            _plBase.Dock = DockStyle.Fill;
            _plBase.Padding = new Padding(2);
            _plBase.BackColor = Color.FromArgb(22, 22, 22);

            _plBase.Controls.Add(_plHeader);
            _plBase.Controls.Add(_plIcon);
            _plBase.Controls.Add(_plFooter);
            Controls.Add(_plBase);
            Opacity = 0;
            Shown += delegate { ShowAnimated(); };
        }

        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        public static extern IntPtr CreateRoundRectRgn(
            int nLeftRect, int nTopRect, int nRightRect, int nBottomRect,
            int nWidthEllipse, int nHeightEllipse);


        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool MessageBeep(uint type);

        public static void Show(string message, Form owner)
        {
            MessageBeep(0);
            _msgBox = new MsgBox();
            _msgBox._lblMessage.Text = message;
            InitButtons(MsgButtons.Ok);
            _msgBox._picIcon.Image = Resources.Info;
            _msgBox._lblTitle.Text = @".NET Reactor Slayer";
            _msgBox.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, _msgBox.Width, _msgBox.Height, 20, 20));
            _msgBox._plBase.Region =
                Region.FromHrgn(CreateRoundRectRgn(0, 0, _msgBox._plBase.Width, _msgBox._plBase.Height, 20, 20));
            if (owner != null)
                _msgBox.ShowDialog(owner);
            else
                _msgBox.ShowDialog();
        }

        public static void Show(string message, string title, Form owner)
        {
            MessageBeep(0);
            _msgBox = new MsgBox();
            _msgBox._lblTitle.Text = title;
            _msgBox._lblMessage.Text = message;
            InitButtons(MsgButtons.Ok);
            _msgBox._picIcon.Image = Resources.Info;
            _msgBox.Size = MessageSize(message);
            _msgBox.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, _msgBox.Width, _msgBox.Height, 20, 20));
            _msgBox._plBase.Region =
                Region.FromHrgn(CreateRoundRectRgn(0, 0, _msgBox._plBase.Width, _msgBox._plBase.Height, 20, 20));
            if (owner != null)
                _msgBox.ShowDialog(owner);
            else
                _msgBox.ShowDialog();
        }

        public static DialogResult Show(string message, string title, MsgButtons buttons, Form owner)
        {
            MessageBeep(0);
            _msgBox = new MsgBox();
            _msgBox._lblMessage.Text = message;
            _msgBox._lblTitle.Text = title;
            _msgBox._picIcon.Image = Resources.Info;

            InitButtons(buttons);

            _msgBox.Size = MessageSize(message);
            _msgBox.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, _msgBox.Width, _msgBox.Height, 20, 20));
            _msgBox._plBase.Region =
                Region.FromHrgn(CreateRoundRectRgn(0, 0, _msgBox._plBase.Width, _msgBox._plBase.Height, 20, 20));
            if (owner != null)
                _msgBox.ShowDialog(owner);
            else
                _msgBox.ShowDialog();
            return _buttonResult;
        }

        public static DialogResult Show(string message, string title, MsgButtons buttons, MsgIcon icon, Form owner)
        {
            MessageBeep(0);
            _msgBox = new MsgBox();
            _msgBox._lblMessage.Text = message;
            _msgBox._lblTitle.Text = title;

            InitButtons(buttons);
            InitIcon(icon);

            _msgBox.Size = MessageSize(message);
            _msgBox.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, _msgBox.Width, _msgBox.Height, 20, 20));
            _msgBox._plBase.Region =
                Region.FromHrgn(CreateRoundRectRgn(0, 0, _msgBox._plBase.Width, _msgBox._plBase.Height, 20, 20));
            if (owner != null)
                _msgBox.ShowDialog(owner);
            else
                _msgBox.ShowDialog();
            return _buttonResult;
        }

        public static DialogResult Show(
            string message, string title, MsgButtons buttons, MsgIcon icon,
            AnimateStyle style, Form owner)
        {
            MessageBeep(0);
            _msgBox = new MsgBox();
            _msgBox._lblMessage.Text = message;
            _msgBox._lblTitle.Text = title;
            _msgBox.Height = 0;

            InitButtons(buttons);
            InitIcon(icon);

            _timer = new Timer();
            var formSize = MessageSize(message);

            switch (style)
            {
                case AnimateStyle.SlideDown:
                    _msgBox.Size = new Size(formSize.Width, 0);
                    _timer.Interval = 1;
                    _timer.Tag = new AnimateMsgBox(formSize, style);
                    break;

                case AnimateStyle.FadeIn:
                    _msgBox.Size = formSize;
                    _msgBox.Opacity = 0;
                    _timer.Interval = 20;
                    _timer.Tag = new AnimateMsgBox(formSize, style);
                    break;

                case AnimateStyle.ZoomIn:
                    _msgBox.Size = new Size(formSize.Width + 100, formSize.Height + 100);
                    _timer.Tag = new AnimateMsgBox(formSize, style);
                    _timer.Interval = 1;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(style), style, null);
            }

            _timer.Tick += timer_Tick;
            _timer.Start();
            _msgBox.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, _msgBox.Width, _msgBox.Height, 20, 20));
            _msgBox._plBase.Region =
                Region.FromHrgn(CreateRoundRectRgn(0, 0, _msgBox._plBase.Width, _msgBox._plBase.Height, 20, 20));

            if (owner != null)
                _msgBox.ShowDialog(owner);
            else
                _msgBox.ShowDialog();
            return _buttonResult;
        }

        private static void timer_Tick(object sender, EventArgs e)
        {
            var timer = (Timer)sender;
            var animate = (AnimateMsgBox)timer.Tag;

            switch (animate.Style)
            {
                case AnimateStyle.SlideDown:
                    if (_msgBox.Height < animate.FormSize.Height)
                    {
                        _msgBox.Height += 17;
                        _msgBox.Invalidate();
                    }
                    else
                    {
                        _timer.Stop();
                        _timer.Dispose();
                    }

                    break;

                case AnimateStyle.FadeIn:
                    if (_msgBox.Opacity < 0.95)
                    {
                        _msgBox.Opacity += 0.1;
                        _msgBox.Invalidate();
                    }
                    else
                    {
                        _timer.Stop();
                        _timer.Dispose();
                    }

                    break;

                case AnimateStyle.ZoomIn:
                    if (_msgBox.Width > animate.FormSize.Width)
                    {
                        _msgBox.Width -= 17;
                        _msgBox.Invalidate();
                    }

                    if (_msgBox.Height > animate.FormSize.Height)
                    {
                        _msgBox.Height -= 17;
                        _msgBox.Invalidate();
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void InitButtons(MsgButtons buttons)
        {
            switch (buttons)
            {
                case MsgButtons.AbortRetryIgnore:
                    _msgBox.InitAbortRetryIgnoreButtons();
                    break;

                case MsgButtons.Ok:
                    _msgBox.InitOkButton();
                    break;

                case MsgButtons.OkCancel:
                    _msgBox.InitOkCancelButtons();
                    break;

                case MsgButtons.RetryCancel:
                    _msgBox.InitRetryCancelButtons();
                    break;

                case MsgButtons.YesNo:
                    _msgBox.InitYesNoButtons();
                    break;

                case MsgButtons.YesNoCancel:
                    _msgBox.InitYesNoCancelButtons();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(buttons), buttons, null);
            }

            foreach (var btn in _msgBox._buttonCollection)
            {
                btn.ForeColor = Color.Silver;
                btn.Font = new Font("Consolas", 8, FontStyle.Bold, GraphicsUnit.Point, 0);
                btn.Padding = new Padding(3);
                btn.FlatStyle = FlatStyle.Flat;
                btn.Height = 30;
                btn.FlatAppearance.BorderColor = Color.Black;
                btn.FlatAppearance.BorderSize = 1;
                btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(32, 32, 32);
                btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(22, 22, 22);
                btn.Cursor = Cursors.Hand;
                btn.UseCompatibleTextRendering = false;
                btn.AutoSize = true;
                btn.TabIndex = _msgBox._buttonCollection.IndexOf(btn);
                btn.TabStop = true;
                _msgBox._flpButtons.Controls.Add(btn);
            }

            _msgBox._flpButtons.Controls[0].Focus();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            Focus();
        }

        private static void InitIcon(MsgIcon icon)
        {
            switch (icon)
            {
                case MsgIcon.Error:
                    _msgBox._picIcon.Image = Resources.Error;
                    break;
                case MsgIcon.Info:
                    _msgBox._picIcon.Image = Resources.Info;
                    break;
                case MsgIcon.Question:
                    _msgBox._picIcon.Image = Resources.Question;
                    break;
                case MsgIcon.Warning:
                    _msgBox._picIcon.Image = Resources.Warning;
                    break;
                default:
                    _msgBox._picIcon.Image = _msgBox._picIcon.Image;
                    break;
            }
        }

        private void InitAbortRetryIgnoreButtons()
        {
            var btnAbort = new Button
            {
                Text = @"Abort"
            };
            btnAbort.Click += ButtonClick;

            var btnRetry = new Button
            {
                Text = @"Retry"
            };
            btnRetry.Click += ButtonClick;

            var btnIgnore = new Button
            {
                Text = @"Ignore"
            };
            btnIgnore.Click += ButtonClick;

            _buttonCollection.Add(btnIgnore);
            _buttonCollection.Add(btnRetry);
            _buttonCollection.Add(btnAbort);
        }

        private void InitOkButton()
        {
            var btnOk = new Button
            {
                Text = @"OK"
            };
            btnOk.Click += ButtonClick;

            _buttonCollection.Add(btnOk);
        }

        private void InitOkCancelButtons()
        {
            var btnOk = new Button
            {
                Text = @"OK"
            };
            btnOk.Click += ButtonClick;

            var btnCancel = new Button
            {
                Text = @"Cancel"
            };
            btnCancel.Click += ButtonClick;


            _buttonCollection.Add(btnCancel);
            _buttonCollection.Add(btnOk);
        }

        private void InitRetryCancelButtons()
        {
            var btnRetry = new Button
            {
                Text = @"Retry"
            };
            btnRetry.Click += ButtonClick;

            var btnCancel = new Button
            {
                Text = @"Cancel"
            };
            btnCancel.Click += ButtonClick;


            _buttonCollection.Add(btnCancel);
            _buttonCollection.Add(btnRetry);
        }

        private void InitYesNoButtons()
        {
            var btnYes = new Button
            {
                Text = @"Yes"
            };
            btnYes.Click += ButtonClick;

            var btnNo = new Button
            {
                Text = @"No"
            };
            btnNo.Click += ButtonClick;

            _buttonCollection.Add(btnNo);
            _buttonCollection.Add(btnYes);
        }

        private void InitYesNoCancelButtons()
        {
            var btnYes = new Button
            {
                Text = @"Yes"
            };
            btnYes.Click += ButtonClick;

            var btnNo = new Button
            {
                Text = @"No"
            };
            btnNo.Click += ButtonClick;

            var btnCancel = new Button
            {
                Text = @"Cancel"
            };
            btnCancel.Click += ButtonClick;

            _buttonCollection.Add(btnCancel);
            _buttonCollection.Add(btnNo);
            _buttonCollection.Add(btnYes);
        }

        private void ButtonClick(object sender, EventArgs e)
        {
            var btn = (Button)sender;

            switch (btn.Text)
            {
                case @"Abort":
                    _buttonResult = DialogResult.Abort;
                    break;
                case @"Retry":
                    _buttonResult = DialogResult.Retry;
                    break;
                case @"Ignore":
                    _buttonResult = DialogResult.Ignore;
                    break;
                case @"OK":
                    _buttonResult = DialogResult.OK;
                    break;
                case @"Cancel":
                    _buttonResult = DialogResult.Cancel;
                    break;
                case @"Yes":
                    _buttonResult = DialogResult.Yes;
                    break;
                case @"No":
                    _buttonResult = DialogResult.No;
                    break;
            }

            CloseAnimated();
        }

        private static Size MessageSize(string message)
        {
            var g = _msgBox.CreateGraphics();
            var width = 400;
            var height = 230;

            var size = g.MeasureString(message, new Font("Consolas", 10));

            if (message.Length < 150)
            {
                if ((int)size.Width > 350) width = (int)size.Width;
            }
            else
            {
                var groups = (from Match m in Regex.Matches(message, ".{1,180}") select m.Value).ToArray();
                var lines = groups.Length;
                width = 800;
                height += lines * 15;
            }

            if (width < 600) width = 600;
            return new Size(width, height);
        }

        private void ShowAnimated()
        {
            var timer = new Timer
            {
                Interval = 10
            };
            timer.Tick += delegate
            {
                if (Opacity < 0.95) Opacity += 0.05;
                if (Opacity >= 0.95)
                {
                    timer.Stop();
                    Show();
                    Opacity = 0.95;
                    timer.Dispose();
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
                    //Prevent Flicker
                    FormClosed += async (_, e) =>
                    {
                        await Task.Delay(500);
                        Dispose();
                    };
                    Close();
                }
            };
            timer.Start();
        }

        private readonly List<Button> _buttonCollection = new List<Button>();
        private readonly FlowLayoutPanel _flpButtons = new FlowLayoutPanel();
        private readonly Label _lblMessage;
        private readonly Label _lblTitle;
        private readonly PictureBox _picIcon = new PictureBox();
        private readonly Panel _plBase = new Panel();
        private readonly Panel _plFooter = new Panel();
        private readonly Panel _plHeader = new Panel();
        private readonly Panel _plIcon = new Panel();

        private static DialogResult _buttonResult;

        private static MsgBox _msgBox;
        private static Timer _timer;

        public enum AnimateStyle
        {
            SlideDown = 1,
            FadeIn = 2,
            ZoomIn = 3
        }

        public enum MsgButtons
        {
            AbortRetryIgnore = 1,
            Ok = 2,
            OkCancel = 3,
            RetryCancel = 4,
            YesNo = 5,
            YesNoCancel = 6
        }

        public enum MsgIcon
        {
            Error = 3,
            Warning = 4,
            Info = 5,
            Question = 6
        }
    }

    internal class AnimateMsgBox
    {
        public AnimateMsgBox(Size formSize, MsgBox.AnimateStyle style)
        {
            FormSize = formSize;
            Style = style;
        }

        public Size FormSize;
        public MsgBox.AnimateStyle Style;
    }
}