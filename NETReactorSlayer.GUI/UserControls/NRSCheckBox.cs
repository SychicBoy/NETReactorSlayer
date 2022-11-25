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
    public sealed class NrsCheckBox : CheckBox
    {
        public NrsCheckBox()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint, true);
            Cursor = Cursors.Hand;
        }

        private void SetControlState(ControlState controlState)
        {
            if (_controlState == controlState)
                return;
            _controlState = controlState;
            Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_spacePressed)
                return;

            if (e.Button == MouseButtons.Left)
                SetControlState(ClientRectangle.Contains(e.Location) ? ControlState.Pressed : ControlState.Hover);
            else
                SetControlState(ControlState.Hover);
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

            SetControlState(!ClientRectangle.Contains(location) ? ControlState.Normal : ControlState.Hover);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.KeyCode != Keys.Space)
                return;
            _spacePressed = true;
            SetControlState(ControlState.Pressed);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);

            if (e.KeyCode != Keys.Space)
                return;
            _spacePressed = false;

            var location = Cursor.Position;

            SetControlState(!ClientRectangle.Contains(location) ? ControlState.Normal : ControlState.Hover);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            var rect = new Rectangle(0, 0, ClientSize.Width, ClientSize.Height);

            const int size = 12;

            var textColor = ForeColor;
            var borderColor = BorderColor;
            var fillColor = FillColor;

            if (Enabled)
                switch (_controlState)
                {
                    case ControlState.Hover:
                        borderColor = HoverBorderColor;
                        textColor = HoverForeColor;
                        fillColor = HoverFillColor;
                        break;
                    case ControlState.Pressed:
                        borderColor = PressBorderColor;
                        textColor = PressForeColor;
                        fillColor = PressFillColor;
                        break;
                }

            using (var b = new SolidBrush(BackColor)) { g.FillRectangle(b, rect); }

            using (var p = new Pen(borderColor))
            {
                var boxRect = new Rectangle(0, rect.Height / 2 - size / 2, size, size);
                g.DrawRectangle(p, boxRect);
            }

            if (Checked)
            {
                using var b = new SolidBrush(fillColor);
                var boxRect = new Rectangle(2, rect.Height / 2 - (size - 4) / 2, size - 3, size - 3);
                g.FillRectangle(b, boxRect);
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

        public enum ControlState { Normal, Hover, Pressed }

        public Color BorderColor
        {
            get => _borderColor;
            set
            {
                _borderColor = value;
                Invalidate();
            }
        }

        private Color _hoverBorderColor = Color.Gray;

        public Color HoverBorderColor
        {
            get => _hoverBorderColor;
            set
            {
                _hoverBorderColor = value;
                Invalidate();
            }
        }

        private Color _pressBorderColor = Color.Gray;

        public Color PressBorderColor
        {
            get => _pressBorderColor;
            set
            {
                _pressBorderColor = value;
                Invalidate();
            }
        }

        private Color _hoverForeColor = Color.Gray;

        public Color HoverForeColor
        {
            get => _hoverForeColor;
            set
            {
                _hoverForeColor = value;
                Invalidate();
            }
        }

        private Color _pressForeColor = Color.Gray;

        public Color PressForeColor
        {
            get => _pressForeColor;
            set
            {
                _pressForeColor = value;
                Invalidate();
            }
        }

        private Color _fillColor = Color.FromArgb(238, 30, 35);

        public Color FillColor
        {
            get => _fillColor;
            set
            {
                _fillColor = value;
                Invalidate();
            }
        }

        private Color _hoverFillColor = Color.FromArgb(188, 14, 18);

        public Color HoverFillColor
        {
            get => _hoverFillColor;
            set
            {
                _hoverFillColor = value;
                Invalidate();
            }
        }

        private Color _pressFillColor = Color.FromArgb(117, 9, 12);

        public Color PressFillColor
        {
            get => _pressFillColor;
            set
            {
                _pressFillColor = value;
                Invalidate();
            }
        }

        private ControlState _controlState = ControlState.Normal;

        private Color _borderColor = Color.Silver;

        private bool _spacePressed;
    }
}