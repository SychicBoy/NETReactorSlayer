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
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace NETReactorSlayer.GUI.UserControls {
    public partial class NrsButton : Button {
        public NrsButton() => InitializeComponent();

        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        public static extern IntPtr CreateRoundRectRgn(
            int nLeftRect, int nTopRect, int nRightRect, int nBottomRect,
            int nWidthEllipse, int nHeightEllipse);

        private void SizeChange(object sender, EventArgs e) =>
            Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, _borderRadius, 20));

        private int _borderRadius;
        private string _text;
        private TextTransformEnum _transform;

        public int BorderRadius {
            get => _borderRadius;
            set {
                _borderRadius = value;
                Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, _borderRadius, 20));
            }
        }

        public new string Text {
            get => _text;
            set {
                _text = value;
                switch (TextTransform) {
                    case TextTransformEnum.Upper:
                        base.Text = Text.ToUpper();
                        break;
                    case TextTransformEnum.Lower:
                        base.Text = Text.ToLower();
                        break;
                    default:
                        base.Text = Text;
                        break;
                }
            }
        }

        public TextTransformEnum TextTransform {
            get => _transform;
            set {
                _transform = value;
                switch (value) {
                    case TextTransformEnum.Upper:
                        base.Text = Text.ToUpper();
                        break;
                    case TextTransformEnum.Lower:
                        base.Text = Text.ToLower();
                        break;
                    default:
                        base.Text = Text;
                        break;
                }
            }
        }

        public enum TextTransformEnum {
            None,
            Upper,
            Lower
        }
    }
}