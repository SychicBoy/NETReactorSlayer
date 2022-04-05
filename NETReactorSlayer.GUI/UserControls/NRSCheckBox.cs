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

namespace NETReactorSlayer.GUI.UserControls
{
    public class NRSCheckBox : CheckBox
    {
        public enum ControlState { Normal, Hover, Pressed }

        #region Field Region

        private ControlState _controlState = ControlState.Normal;

        private bool _spacePressed;

        #endregion

        #region Constructor Region

        public NRSCheckBox()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint, true);
            this.Cursor = Cursors.Hand;
        }

        #endregion

        #region Method Region

        private void SetControlState(ControlState controlState)
        {
            if (_controlState != controlState)
            {
                _controlState = controlState;
                Invalidate();
            }
        }

        #endregion

        #region Event Handler Region

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_spacePressed)
                return;

            if (e.Button == MouseButtons.Left)
            {
                if (ClientRectangle.Contains(e.Location))
                    SetControlState(ControlState.Pressed);
                else
                    SetControlState(ControlState.Hover);
            }
            else
            {
                SetControlState(ControlState.Hover);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (!ClientRectangle.Contains(e.Location))
                return;

            SetControlState(ControlState.Pressed);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (_spacePressed)
                return;

            SetControlState(ControlState.Normal);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            if (_spacePressed)
                return;

            SetControlState(ControlState.Normal);
        }

        protected override void OnMouseCaptureChanged(EventArgs e)
        {
            base.OnMouseCaptureChanged(e);

            if (_spacePressed)
                return;

            var location = Cursor.Position;

            if (!ClientRectangle.Contains(location))
                SetControlState(ControlState.Normal);
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);

            Invalidate();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);

            _spacePressed = false;

            var location = Cursor.Position;

            if (!ClientRectangle.Contains(location))
                SetControlState(ControlState.Normal);
            else
                SetControlState(ControlState.Hover);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.KeyCode == Keys.Space)
            {
                _spacePressed = true;
                SetControlState(ControlState.Pressed);
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);

            if (e.KeyCode == Keys.Space)
            {
                _spacePressed = false;

                var location = Cursor.Position;

                if (!ClientRectangle.Contains(location))
                    SetControlState(ControlState.Normal);
                else
                    SetControlState(ControlState.Hover);
            }
        }

        #endregion

        #region Paint Region

        #region OhHover
        private Color _BorderColor = Color.Silver;
        public Color BorderColor
        {
            get
            {
                return _BorderColor;
            }
            set
            {
                _BorderColor = value;
                this.Invalidate();
            }
        }

        private Color _HoverBorderColor = Color.Gray;
        public Color HoverBorderColor
        {
            get
            {
                return _HoverBorderColor;
            }
            set
            {
                _HoverBorderColor = value;
                this.Invalidate();
            }
        }

        private Color _PressBorderColor = Color.Gray;
        public Color PressBorderColor
        {
            get
            {
                return _PressBorderColor;
            }
            set
            {
                _PressBorderColor = value;
                this.Invalidate();
            }
        }

        private Color _HoverForeColor = Color.Gray;
        public Color HoverForeColor
        {
            get
            {
                return _HoverForeColor;
            }
            set
            {
                _HoverForeColor = value;
                this.Invalidate();
            }
        }

        private Color _PressForeColor = Color.Gray;
        public Color PressForeColor
        {
            get
            {
                return _PressForeColor;
            }
            set
            {
                _PressForeColor = value;
                this.Invalidate();
            }
        }
        #endregion

        private Color _FillColor = Color.FromArgb(238, 30, 35);
        public Color FillColor
        {
            get
            {
                return _FillColor;
            }
            set
            {
                _FillColor = value;
                this.Invalidate();
            }
        }
        private Color _HoverFillColor = Color.FromArgb(188, 14, 18);
        public Color HoverFillColor
        {
            get
            {
                return _HoverFillColor;
            }
            set
            {
                _HoverFillColor = value;
                this.Invalidate();
            }
        }

        private Color _PressFillColor = Color.FromArgb(117, 9, 12);
        public Color PressFillColor
        {
            get
            {
                return _PressFillColor;
            }
            set
            {
                _PressFillColor = value;
                this.Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            var rect = new Rectangle(0, 0, ClientSize.Width, ClientSize.Height);

            var size = 12;

            var textColor = ForeColor;
            var borderColor = BorderColor;
            var fillColor = FillColor;

            if (Enabled)
            {
                if (_controlState == ControlState.Hover)
                {
                    borderColor = HoverBorderColor;
                    textColor = HoverForeColor;
                    fillColor = HoverFillColor;
                }

                else if (_controlState == ControlState.Pressed)
                {
                    borderColor = PressBorderColor;
                    textColor = PressForeColor;
                    fillColor = PressFillColor;
                }
            }

            using (var b = new SolidBrush(BackColor))
            {
                g.FillRectangle(b, rect);
            }

            using (var p = new Pen(borderColor))
            {
                var boxRect = new Rectangle(0, (rect.Height / 2) - (size / 2), size, size);
                g.DrawRectangle(p, boxRect);
            }

            if (Checked)
            {
                using (var b = new SolidBrush(fillColor))
                {
                    Rectangle boxRect = new Rectangle(2, (rect.Height / 2) - ((size - 4) / 2), size - 3, size - 3);
                    g.FillRectangle(b, boxRect);
                }
            }

            using (var b = new SolidBrush(textColor))
            {
                var stringFormat = new StringFormat
                {
                    LineAlignment = StringAlignment.Center,
                    Alignment = StringAlignment.Near
                };

                var modRect = new Rectangle(size + 4, 0, rect.Width - size, rect.Height);
                g.DrawString(Text, Font, b, modRect, stringFormat);
            }
        }

        #endregion
    }
}