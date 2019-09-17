﻿using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace FadeOutDemo
{
    class FadeOutLabel : Label
    {
        public FadeOutLabel()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            _fadeoutTimer.Interval = 1500;
            _fadeoutTimer.Tick += StartAnimation;

            _animationTimer.Interval = 25;
            _animationTimer.Tick += AnimationFrame;
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var param = base.CreateParams;
                param.ExStyle |= 0x00000020; //WS_EX_TRANSPARENT = 0x00000020
                return param;
            }
        }

        Timer _fadeoutTimer = new Timer();
        Timer _animationTimer = new Timer();
        decimal _currentStep;

        #region Properties

        public override string Text
        {
            get { return base.Text; }
            set
            {
                Reset();
                base.Text = value;
                StartAnimation();
            }
        }

        public int FadeOutTimerInterval
        {
            get { return _fadeoutTimer.Interval; }
            set { _fadeoutTimer.Interval = value; }
        }

        public int AnimationFrameInterval
        {
            get { return _animationTimer.Interval; }
            set { _animationTimer.Interval = value; }
        }

        public int FadeoutStep { get; set; }

        public decimal StepAcceleration { get; set; }

        #endregion

        protected override void OnPaint(PaintEventArgs e)
        {
            var bmp = new Bitmap(Width, Height, PixelFormat.Format32bppPArgb);

            using (var grfx = Graphics.FromImage(bmp))
            {
                grfx.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                grfx.Clear(BackColor);
                var args = new PaintEventArgs(grfx, e.ClipRectangle);
                base.OnPaint(args);
            }

            short a = BackColor.A;

            if (a != 255)
            {
                for (int y = 0; y < Height - 1; y++)
                {
                    for (int x = 0; x < Width - 1; x++)
                    {
                        var pxColor = bmp.GetPixel(x, y);

                        if (pxColor.A != a)
                            bmp.SetPixel(x, y, Color.FromArgb(a, pxColor));
                    }
                }
            }

            e.Graphics.DrawImage(bmp, 0, 0);
        }

        private void StartAnimation(object sender, EventArgs e)
        {
            _currentStep = FadeoutStep;
            _fadeoutTimer.Stop();
            _animationTimer.Start();
        }

        private void AnimationFrame(object sender, EventArgs e)
        {
            var a = BackColor.A;

            if (a == 0)
            {
                Visible = false;
                _animationTimer.Stop();
            }

            if (Visible)
            {
                BackColor = Color.FromArgb(Math.Max(a - (int)Math.Round(_currentStep), 0), BackColor);
                _currentStep *= StepAcceleration / 10;
            }
        }

        public void StartAnimation()
        {
            _fadeoutTimer.Start();
        }

        public void Reset()
        {
            _animationTimer.Stop();
            _fadeoutTimer.Stop();

            BackColor = Color.FromArgb(255, BackColor);
        }
    }
}
