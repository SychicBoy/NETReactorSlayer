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

namespace NETReactorSlayer.GUI.UserControls;

public partial class NrsTextBox : TextBox
{
    public NrsTextBox()
    {
        InitializeComponent();
        AutoSize = false;
        GotFocus += (_, _) =>
        {
            if (base.ForeColor == PlaceHolderColor)
            {
                base.ForeColor = ForeColor;
                Text = string.Empty;
            }
        };
        LostFocus += (_, _) =>
        {
            if (Text.Length < 1 && PlaceHolderText.Length > 0)
            {
                Text = PlaceHolderText;
                base.ForeColor = PlaceHolderColor;
            }
        };
        TextChanged += (_, _) =>
        {
            if (Text.Length > 0)
                base.ForeColor = ForeColor;
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
        };
    }

    [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
    public static extern IntPtr CreateRoundRectRgn(
        int nLeftRect, int nTopRect, int nRightRect, int nBottomRect,
        int nWidthEllipse, int nHeightEllipse);

    private void SizeChange(object sender, EventArgs e)
    {
        Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, _borderRadius, 20));
    }

    private int _borderRadius;
    private Color _foreColor = Color.Silver;
    private string _placeHolderText = string.Empty;
    private float _progress;
    private Color _progressColor = Color.MediumSeaGreen;
    private TextTransformEnum _transform;

    public int BorderRadius
    {
        get => _borderRadius;
        set
        {
            _borderRadius = value;
            Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, _borderRadius, 20));
        }
    }

    public new Color ForeColor
    {
        get => _foreColor;
        set
        {
            _foreColor = value;
            base.ForeColor = ForeColor;
        }
    }

    public Color PlaceHolderColor { get; set; } = Color.Gray;

    public string PlaceHolderText
    {
        get => _placeHolderText;
        set
        {
            _placeHolderText = value;
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

    public float Progress
    {
        get => _progress;
        set
        {
            _progress = value;
            Invalidate();
        }
    }

    public Color ProgressColor
    {
        get => _progressColor;
        set
        {
            _progressColor = value;
            Invalidate();
        }
    }

    public TextTransformEnum TextTransform
    {
        get => _transform;
        set
        {
            _transform = value;
            Text = value switch
            {
                TextTransformEnum.Upper => Text.ToUpper(),
                TextTransformEnum.Lower => Text.ToLower(),
                _ => Text
            };
        }
    }

    public enum TextTransformEnum
    {
        None,
        Upper,
        Lower
    }
}