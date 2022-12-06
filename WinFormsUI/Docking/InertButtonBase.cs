using System;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;

namespace WeifenLuo.WinFormsUI.Docking
{
    public abstract class InertButtonBase : Control
    {
        protected InertButtonBase()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
        }

        public abstract Bitmap HoverImage { get; }

        public abstract Bitmap PressImage { get; }

        public abstract Bitmap Image { get; }

        private bool _isMouseOver = false;
        protected bool IsMouseOver
        {
            get { return _isMouseOver; }
            private set
            {
                if (_isMouseOver == value)
                    return;

                _isMouseOver = value;
                Invalidate();
            }
        }

        private bool _isMouseDown = false;
        protected bool IsMouseDown
        {
            get { return _isMouseDown; }
            private set
            {
                if (_isMouseDown == value)
                    return;

                _isMouseDown = value;
                Invalidate();
            }
        }

        protected override Size DefaultSize
        {
            get { return new Size(16, 15); }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            bool over = ClientRectangle.Contains(e.X, e.Y);
            if (IsMouseOver != over)
                IsMouseOver = over;
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            if (!IsMouseOver)
                IsMouseOver = true;
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            if (IsMouseOver)
                IsMouseOver = false;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            if (!IsMouseDown)
                IsMouseDown = true;
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            if (IsMouseDown)
                IsMouseDown = false;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (HoverImage != null)
            {
                if (IsMouseOver && Enabled)
                {
                    e.Graphics.DrawImage(
                       IsMouseDown ? PressImage : HoverImage,
                       PatchController.EnableHighDpi == true
                           ? ClientRectangle
                           : new Rectangle(0, 0, Image.Width, Image.Height));
                }
                else
                {
                    e.Graphics.DrawImage(
                       Image,
                       PatchController.EnableHighDpi == true
                           ? ClientRectangle
                           : new Rectangle(0, 0, Image.Width, Image.Height));
                }

                base.OnPaint(e);
                return;
            }

            if (IsMouseOver && Enabled)
            {
                using Pen pen = new(ForeColor);
                e.Graphics.DrawRectangle(pen, Rectangle.Inflate(ClientRectangle, -1, -1));
            }

            using (ImageAttributes imageAttributes = new())
            {
                ColorMap[] colorMap = new ColorMap[2];
                colorMap[0] = new ColorMap
                {
                    OldColor = Color.FromArgb(0, 0, 0),
                    NewColor = ForeColor
                };
                colorMap[1] = new ColorMap
                {
                    OldColor = Image.GetPixel(0, 0),
                    NewColor = Color.Transparent
                };

                imageAttributes.SetRemapTable(colorMap);

                e.Graphics.DrawImage(
                   Image,
                   new Rectangle(0, 0, Image.Width, Image.Height),
                   0, 0,
                   Image.Width,
                   Image.Height,
                   GraphicsUnit.Pixel,
                   imageAttributes);
            }

            base.OnPaint(e);
        }

        public void RefreshChanges()
        {
            if (IsDisposed)
                return;

            bool mouseOver = ClientRectangle.Contains(PointToClient(Control.MousePosition));
            if (mouseOver != IsMouseOver)
                IsMouseOver = mouseOver;

            OnRefreshChanges();
        }

        protected virtual void OnRefreshChanges()
        {
        }
    }
}
