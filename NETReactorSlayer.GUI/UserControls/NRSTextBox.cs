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

namespace NETReactorSlayer.GUI.UserControls
{
    public partial class NRSTextBox : TextBox
    {
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        public static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);
        private int _BorderRadius = 0;
        public int BorderRadius
        {
            get
            {
                return _BorderRadius;
            }
            set
            {
                _BorderRadius = value;
                this.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, _BorderRadius, 20));
            }
        }
        public enum TextTransformEnum { None, Upper, Lower };
        private TextTransformEnum _Transform;
        public TextTransformEnum TextTransform
        {
            get
            {
                return _Transform;
            }
            set
            {
                _Transform = value;
                if (value == TextTransformEnum.Upper)
                    Text = Text.ToUpper();
                else if (value == TextTransformEnum.Lower)
                    Text = Text.ToLower();
                else
                    Text = Text;
            }
        }
        private string _PlaceHolderText = string.Empty;
        public string PlaceHolderText
        {
            get
            {
                return _PlaceHolderText;
            }
            set
            {
                _PlaceHolderText = value;
                if (!DesignMode)
                {
                    if (Focused)
                        return;
                    if (value.Length > 0)
                    {
                        if (Text != value)
                            Text = value;
                        base.ForeColor = PlaceHolderColor;
                    }
                }
            }
        }
        private Color _ForeColor = Color.Silver;
        public new Color ForeColor
        {
            get
            {
                return _ForeColor;
            }
            set
            {
                _ForeColor = value;
                base.ForeColor = ForeColor;
            }
        }
        private Color _PlaceHolderColor = Color.Gray;
        public Color PlaceHolderColor
        {
            get
            {
                return _PlaceHolderColor;
            }
            set
            {
                _PlaceHolderColor = value;
            }
        }
        private void SizeChange(object sender, EventArgs e) => this.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, _BorderRadius, 20));
        private float _Progress = 0;
        private Color _ProgressColor = Color.MediumSeaGreen;
        public float Progress
        {
            get
            {
                return _Progress;
            }
            set
            {
                _Progress = value;
                this.Invalidate();
            }
        }
        public Color ProgressColor
        {
            get
            {
                return _ProgressColor;
            }
            set
            {
                _ProgressColor = value;
                this.Invalidate();
            }
        }

        public NRSTextBox() : base()
        {
            InitializeComponent();
            AutoSize = false;
            GotFocus += new EventHandler((sender, e) =>
            {
                if (base.ForeColor == PlaceHolderColor)
                {
                    base.ForeColor = ForeColor;
                    Text = string.Empty;
                }
            });
            LostFocus += new EventHandler((sender, e) =>
            {
                if (Text.Length < 1 && PlaceHolderText.Length > 0)
                {
                    Text = PlaceHolderText;
                    base.ForeColor = PlaceHolderColor;
                }
            });
            TextChanged += new EventHandler((sender, e) =>
            {
                if (Text.Length > 0)
                {
                    base.ForeColor = ForeColor;
                }
                else if (!DesignMode)
                {
                    if (Focused)
                        return;
                    if (PlaceHolderText.Length > 0)
                    {
                        if (Text != PlaceHolderText)
                            Text = PlaceHolderText;
                        base.ForeColor = PlaceHolderColor;
                    }
                }
            });
        }
    }
}
