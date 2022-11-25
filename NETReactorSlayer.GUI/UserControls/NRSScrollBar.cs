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
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using NETReactorSlayer.GUI.Properties;

namespace NETReactorSlayer.GUI.UserControls
{
    public class NrsScrollBar : Control
    {
        public NrsScrollBar()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint, true);

            SetStyle(ControlStyles.Selectable, false);

            _scrollTimer = new Timer
            {
                Interval = 1
            };
            _scrollTimer.Tick += ScrollTimerTick;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            UpdateScrollBar();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (_thumbArea.Contains(e.Location) && e.Button == MouseButtons.Left)
            {
                _isScrolling = true;
                _initialContact = e.Location;

                _initialValue = _thumbArea.Top;

                Invalidate();
                return;
            }

            if (_upArrowArea.Contains(e.Location) && e.Button == MouseButtons.Left)
            {
                _upArrowClicked = true;
                _scrollTimer.Enabled = true;

                Invalidate();
                return;
            }

            if (_downArrowArea.Contains(e.Location) && e.Button == MouseButtons.Left)
            {
                _downArrowClicked = true;
                _scrollTimer.Enabled = true;

                Invalidate();
                return;
            }

            if (!_trackArea.Contains(e.Location) || e.Button != MouseButtons.Left)
                return;
            var modRect = new Rectangle(_thumbArea.Left, _trackArea.Top, _thumbArea.Width,
                _trackArea.Height);
            if (!modRect.Contains(e.Location))
                return;

            var loc = e.Location.Y;
            loc -= _upArrowArea.Bottom - 1;
            loc -= _thumbArea.Height / 2;
            ScrollToPhysical(loc);

            _isScrolling = true;
            _initialContact = e.Location;
            _thumbHot = true;

            _initialValue = _thumbArea.Top;

            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            _isScrolling = false;

            _upArrowClicked = false;
            _downArrowClicked = false;

            Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            switch (_isScrolling)
            {
                case false:
                {
                    var thumbHot = _thumbArea.Contains(e.Location);
                    if (_thumbHot != thumbHot)
                    {
                        _thumbHot = thumbHot;
                        Invalidate();
                    }

                    var upArrowHot = _upArrowArea.Contains(e.Location);
                    if (_upArrowHot != upArrowHot)
                    {
                        _upArrowHot = upArrowHot;
                        Invalidate();
                    }

                    var downArrowHot = _downArrowArea.Contains(e.Location);
                    if (_downArrowHot != downArrowHot)
                    {
                        _downArrowHot = downArrowHot;
                        Invalidate();
                    }

                    break;
                }
                case true when e.Button != MouseButtons.Left:
                    // ReSharper disable once AssignNullToNotNullAttribute
                    OnMouseUp(null);
                    return;
                case true:
                {
                    var difference = new Point(e.Location.X - _initialContact.X, e.Location.Y - _initialContact.Y);

                    var thumbPos = _initialValue - _trackArea.Top;
                    var newPosition = thumbPos + difference.Y;

                    ScrollToPhysical(newPosition);
                    UpdateScrollBar();

                    break;
                }
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            _thumbHot = false;
            _upArrowHot = false;
            _downArrowHot = false;

            Invalidate();
        }

        private void ScrollTimerTick(object sender, EventArgs e)
        {
            switch (_upArrowClicked)
            {
                case false when !_downArrowClicked:
                    _scrollTimer.Enabled = false;
                    return;
                case true:
                    ScrollBy(-1);
                    break;
                default:
                {
                    if (_downArrowClicked)
                        ScrollBy(1);
                    break;
                }
            }
        }

        public void ScrollTo(int position) => Value = position;

        public void ScrollToPhysical(int positionInPixels)
        {
            var trackAreaSize = _trackArea.Height - _thumbArea.Height;

            var positionRatio = positionInPixels / (float)trackAreaSize;
            var viewScrollSize = Maximum - ViewSize;

            var newValue = (int)(positionRatio * viewScrollSize);
            Value = newValue;
        }

        public void ScrollBy(int offset)
        {
            var newValue = Value + offset;
            ScrollTo(newValue);
        }

        public void UpdateScrollBar()
        {
            var area = ClientRectangle;

            _upArrowArea = new Rectangle(area.Left, area.Top, ArrowButtonSize, ArrowButtonSize);
            _downArrowArea = new Rectangle(area.Left, area.Bottom - ArrowButtonSize, ArrowButtonSize,
                ArrowButtonSize);
            _trackArea = new Rectangle(area.Left, area.Top + ArrowButtonSize, area.Width,
                area.Height - ArrowButtonSize * 2);

            UpdateThumb();
            Invalidate();
        }

        private void UpdateThumb(bool forceRefresh = false)
        {
            if (ViewSize >= Maximum)
                return;

            var maximumValue = Maximum - ViewSize;
            if (Value > maximumValue)
                Value = maximumValue;

            _viewContentRatio = ViewSize / (float)Maximum;
            var viewAreaSize = Maximum - ViewSize;
            var positionRatio = Value / (float)viewAreaSize;

            var thumbSize = (int)(_trackArea.Height * _viewContentRatio);

            if (thumbSize < MinimumThumbSize)
                thumbSize = MinimumThumbSize;

            var trackAreaSize = _trackArea.Height - thumbSize;
            var thumbPosition = (int)(trackAreaSize * positionRatio);

            _thumbArea = new Rectangle(_trackArea.Left + 3, _trackArea.Top + thumbPosition, ScrollBarSize - 6,
                thumbSize);

            if (!forceRefresh)
                return;

            Invalidate();
            Update();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;

            var upIcon = _upArrowHot ? Resources.scrollbar_arrow_hot : Resources.scrollbar_arrow_standard;

            if (_upArrowClicked)
                upIcon = Resources.scrollbar_arrow_clicked;

            if (!Enabled)
                upIcon = Resources.scrollbar_disabled;

            upIcon.RotateFlip(RotateFlipType.RotateNoneFlipY);

            g.DrawImageUnscaled(upIcon,
                _upArrowArea.Left + _upArrowArea.Width / 2 - upIcon.Width / 2,
                _upArrowArea.Top + _upArrowArea.Height / 2 - upIcon.Height / 2);

            var downIcon = _downArrowHot ? Resources.scrollbar_arrow_hot : Resources.scrollbar_arrow_standard;

            if (_downArrowClicked)
                downIcon = Resources.scrollbar_arrow_clicked;

            if (!Enabled)
                downIcon = Resources.scrollbar_disabled;

            g.DrawImageUnscaled(downIcon,
                _downArrowArea.Left + _downArrowArea.Width / 2 - downIcon.Width / 2,
                _downArrowArea.Top + _downArrowArea.Height / 2 - downIcon.Height / 2);

            if (!Enabled)
                return;
            var scrollColor = _thumbHot ? Color.FromArgb(122, 128, 132) : Color.FromArgb(92, 92, 92);

            if (_isScrolling)
                scrollColor = Color.FromArgb(159, 178, 196);

            using (var b = new SolidBrush(scrollColor)) { g.FillRectangle(b, _thumbArea); }
        }

        [Category("Behavior")]
        [Description("The value that the scroll thumb position represents.")]
        [DefaultValue(0)]
        public int Value
        {
            get => _value;
            set
            {
                if (value < Minimum)
                    value = Minimum;

                var maximumValue = Maximum - ViewSize;
                if (value > maximumValue)
                    value = maximumValue;

                if (_value == value)
                    return;

                _value = value;

                UpdateThumb(true);

                ValueChanged?.Invoke(this, new ScrollValueEventArgs(Value));
            }
        }

        [Category("Behavior")]
        [Description("The lower limit value of the scrollable range.")]
        [DefaultValue(0)]
        public int Minimum
        {
            get => _minimum;
            set
            {
                _minimum = value;
                UpdateScrollBar();
            }
        }

        [Category("Behavior")]
        [Description("The upper limit value of the scrollable range.")]
        [DefaultValue(100)]
        public int Maximum
        {
            get => _maximum;
            set
            {
                _maximum = value;
                UpdateScrollBar();
            }
        }

        [Category("Behavior")]
        [Description("The view size for the scrollable area.")]
        [DefaultValue(0)]
        public int ViewSize
        {
            get => _viewSize;
            set
            {
                _viewSize = value;
                UpdateScrollBar();
            }
        }

        public static int ArrowButtonSize = 15;
        public static int MinimumThumbSize = 11;
        public static int ScrollBarSize = 16;
        private int _value;
        private int _minimum;
        private int _maximum = 100;
        private int _viewSize;
        private float _viewContentRatio;
        private Rectangle _trackArea;
        private Rectangle _thumbArea;
        private Rectangle _upArrowArea;
        private Rectangle _downArrowArea;
        private bool _thumbHot;
        private bool _upArrowHot;
        private bool _downArrowHot;
        private bool _upArrowClicked;
        private bool _downArrowClicked;
        private bool _isScrolling;
        private int _initialValue;
        private Point _initialContact;
        private readonly Timer _scrollTimer;
        public event EventHandler<ScrollValueEventArgs> ValueChanged;
    }

    public class ScrollValueEventArgs : EventArgs
    {
        public ScrollValueEventArgs(int value) => Value = value;

        public int Value { get; }
    }
}