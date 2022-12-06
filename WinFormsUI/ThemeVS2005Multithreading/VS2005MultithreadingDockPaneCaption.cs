using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.ThemeVS2005;

namespace WeifenLuo.WinFormsUI.Docking
{
    [ToolboxItem(false)]
    internal class VS2005MultithreadingDockPaneCaption : DockPaneCaptionBase
    {
        [ToolboxItem(false)]
        private sealed class InertButton : InertButtonBase
        {
            private readonly Bitmap _image, _imageAutoHide;

            public InertButton(VS2005MultithreadingDockPaneCaption dockPaneCaption, Bitmap image, Bitmap imageAutoHide)
                : base()
            {
                _dockPaneCaption = dockPaneCaption;
                _image = image;
                _imageAutoHide = imageAutoHide;
                RefreshChanges();
            }

            private readonly VS2005MultithreadingDockPaneCaption _dockPaneCaption;
            private VS2005MultithreadingDockPaneCaption DockPaneCaption
            {
                get { return _dockPaneCaption; }
            }

            public bool IsAutoHide
            {
                get { return DockPaneCaption.DockPane.IsAutoHide; }
            }

            public override Bitmap Image
            {
                get { return IsAutoHide ? _imageAutoHide : _image; }
            }

            public override Bitmap HoverImage
            {
                get { return null; }
            }

            public override Bitmap PressImage
            {
                get { return null; }
            }

            protected override void OnRefreshChanges()
            {
                if (DockPaneCaption.DockPane.DockPanel != null && DockPaneCaption.TextColor != ForeColor)
                {
                    ForeColor = DockPaneCaption.TextColor;
                    Invalidate();
                }
            }
        }

        #region consts
        private const int _TextGapTop = 2;
        private const int _TextGapBottom = 0;
        private const int _TextGapLeft = 3;
        private const int _TextGapRight = 3;
        private const int _ButtonGapTop = 2;
        private const int _ButtonGapBottom = 1;
        private const int _ButtonGapBetween = 1;
        private const int _ButtonGapLeft = 1;
        private const int _ButtonGapRight = 2;
        #endregion

        private InertButton _buttonClose;
        private InertButton ButtonClose
        {
            get
            {
                if (_buttonClose == null)
                {
                    _buttonClose = new InertButton(this, _imageButtonClose, _imageButtonClose);
                    _toolTip.SetToolTip(_buttonClose, ToolTipClose);
                    _buttonClose.Click += new EventHandler(Close_Click);
                    Controls.Add(_buttonClose);
                }

                return _buttonClose;
            }
        }

        private InertButton _buttonAutoHide;
        private InertButton ButtonAutoHide
        {
            get
            {
                if (_buttonAutoHide == null)
                {
                    _buttonAutoHide = new InertButton(this, _imageButtonDock, _imageButtonAutoHide);
                    _toolTip.SetToolTip(_buttonAutoHide, ToolTipAutoHide);
                    _buttonAutoHide.Click += new EventHandler(AutoHide_Click);
                    Controls.Add(_buttonAutoHide);
                }

                return _buttonAutoHide;
            }
        }

        private InertButton _buttonOptions;
        private InertButton ButtonOptions
        {
            get
            {
                if (_buttonOptions == null)
                {
                    _buttonOptions = new InertButton(this, _imageButtonOptions, _imageButtonOptions);
                    _toolTip.SetToolTip(_buttonOptions, ToolTipOptions);
                    _buttonOptions.Click += new EventHandler(Options_Click);
                    Controls.Add(_buttonOptions);
                }
                return _buttonOptions;
            }
        }

        private readonly IContainer _components;
        private IContainer Components
        {
            get { return _components; }
        }

        private readonly Bitmap _imageButtonAutoHide;
        private readonly Bitmap _imageButtonClose;
        private readonly Bitmap _imageButtonDock;
        private readonly Bitmap _imageButtonOptions;
        private readonly Blend _activeBackColorGradientBlend;

        private readonly ToolTip _toolTip;
        private readonly object lockObj = new();

        public VS2005MultithreadingDockPaneCaption(DockPane pane) : base(pane)
        {
            SuspendLayout();

            _components = new Container();
            _toolTip = new ToolTip(Components);

            // clone shared resources
            lock (lockObj)
            {
                _imageButtonAutoHide = (Bitmap)Resources.DockPane_AutoHide.Clone();
                _imageButtonClose = (Bitmap)Resources.DockPane_Close.Clone();
                _imageButtonDock = (Bitmap)Resources.DockPane_Dock.Clone();
                _imageButtonOptions = (Bitmap)Resources.DockPane_Option.Clone();
            }

            // create background blend
            _activeBackColorGradientBlend = new Blend(2)
            {
                Factors = new float[] { 0.5F, 1.0F },
                Positions = new float[] { 0.0F, 1.0F },
            };

            ResumeLayout();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Components.Dispose();

                _imageButtonAutoHide.Dispose();
                _imageButtonClose.Dispose();
                _imageButtonDock.Dispose();
                _imageButtonOptions.Dispose();
            }
            base.Dispose(disposing);
        }

        private static int TextGapTop
        {
            get	{	return _TextGapTop;	}
        }

        public Font TextFont
        {
            get { return DockPane.DockPanel.Theme.Skin.DockPaneStripSkin.TextFont; }
        }

        private static int TextGapBottom
        {
            get	{	return _TextGapBottom;	}
        }

        private static int TextGapLeft
        {
            get	{	return _TextGapLeft;	}
        }

        private static int TextGapRight
        {
            get	{	return _TextGapRight;	}
        }

        private static int ButtonGapTop
        {
            get	{	return _ButtonGapTop;	}
        }

        private static int ButtonGapBottom
        {
            get	{	return _ButtonGapBottom;	}
        }

        private static int ButtonGapLeft
        {
            get	{	return _ButtonGapLeft;	}
        }

        private static int ButtonGapRight
        {
            get	{	return _ButtonGapRight;	}
        }

        private static int ButtonGapBetween
        {
            get	{	return _ButtonGapBetween;	}
        }

        private static string _toolTipClose;
        private static string ToolTipClose
        {
            get
            {	
                _toolTipClose ??= ThemeVS2005.Strings.DockPaneCaption_ToolTipClose;
                return _toolTipClose;
            }
        }

        private static string _toolTipOptions;
        private static string ToolTipOptions
        {
            get
            {
                _toolTipOptions ??= ThemeVS2005.Strings.DockPaneCaption_ToolTipOptions;

                return _toolTipOptions;
            }
        }

        private static string _toolTipAutoHide;
        private static string ToolTipAutoHide
        {
            get
            {	
                _toolTipAutoHide ??= ThemeVS2005.Strings.DockPaneCaption_ToolTipAutoHide;
                return _toolTipAutoHide;
            }
        }

        private Color TextColor
        {
            get
            {
                if (DockPane.IsActivated)
                    return DockPane.DockPanel.Theme.Skin.DockPaneStripSkin.ToolWindowGradient.ActiveCaptionGradient.TextColor;
                else
                    return DockPane.DockPanel.Theme.Skin.DockPaneStripSkin.ToolWindowGradient.InactiveCaptionGradient.TextColor;
            }
        }

        private static readonly TextFormatFlags _textFormat =
            TextFormatFlags.SingleLine |
            TextFormatFlags.EndEllipsis |
            TextFormatFlags.VerticalCenter;
        private TextFormatFlags TextFormat
        {
            get
            {
                if (RightToLeft == RightToLeft.No)
                    return _textFormat;
                else
                    return _textFormat | TextFormatFlags.RightToLeft | TextFormatFlags.Right;
            }
        }

        protected internal override int MeasureHeight()
        {
            int height = TextFont.Height + TextGapTop + TextGapBottom;

            if (height < ButtonClose.Image.Height + ButtonGapTop + ButtonGapBottom)
                height = ButtonClose.Image.Height + ButtonGapTop + ButtonGapBottom;

            return height;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint (e);
            DrawCaption(e.Graphics);
        }

        private void DrawCaption(Graphics g)
        {
            if (ClientRectangle.Width == 0 || ClientRectangle.Height == 0)
                return;

            if (DockPane.IsActivated)
            {
                Color startColor = DockPane.DockPanel.Theme.Skin.DockPaneStripSkin.ToolWindowGradient.ActiveCaptionGradient.StartColor;
                Color endColor = DockPane.DockPanel.Theme.Skin.DockPaneStripSkin.ToolWindowGradient.ActiveCaptionGradient.EndColor;
                LinearGradientMode gradientMode = DockPane.DockPanel.Theme.Skin.DockPaneStripSkin.ToolWindowGradient.ActiveCaptionGradient.LinearGradientMode;
                ClientRectangle.SafelyDrawLinearGradient(startColor, endColor, gradientMode, g, _activeBackColorGradientBlend);
            }
            else
            {
                Color startColor = DockPane.DockPanel.Theme.Skin.DockPaneStripSkin.ToolWindowGradient.InactiveCaptionGradient.StartColor;
                Color endColor = DockPane.DockPanel.Theme.Skin.DockPaneStripSkin.ToolWindowGradient.InactiveCaptionGradient.EndColor;
                LinearGradientMode gradientMode = DockPane.DockPanel.Theme.Skin.DockPaneStripSkin.ToolWindowGradient.InactiveCaptionGradient.LinearGradientMode;
                ClientRectangle.SafelyDrawLinearGradient(startColor, endColor, gradientMode, g);
            }

            Rectangle rectCaption = ClientRectangle;

            Rectangle rectCaptionText = rectCaption;
            rectCaptionText.X += TextGapLeft;
            rectCaptionText.Width -= TextGapLeft + TextGapRight;
            rectCaptionText.Width -= ButtonGapLeft + ButtonClose.Width + ButtonGapRight;
            if (ShouldShowAutoHideButton)
                rectCaptionText.Width -= ButtonAutoHide.Width + ButtonGapBetween;
            if (HasTabPageContextMenu)
                rectCaptionText.Width -= ButtonOptions.Width + ButtonGapBetween;
            rectCaptionText.Y += TextGapTop;
            rectCaptionText.Height -= TextGapTop + TextGapBottom;

            Color colorText;
            if (DockPane.IsActivated)
                colorText = DockPane.DockPanel.Theme.Skin.DockPaneStripSkin.ToolWindowGradient.ActiveCaptionGradient.TextColor;
            else
                colorText = DockPane.DockPanel.Theme.Skin.DockPaneStripSkin.ToolWindowGradient.InactiveCaptionGradient.TextColor;

            TextRenderer.DrawText(g, DockPane.CaptionText, TextFont, DrawHelper.RtlTransform(this, rectCaptionText), colorText, TextFormat);
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            SetButtonsPosition();
            base.OnLayout (levent);
        }

        protected override void OnRefreshChanges()
        {
            SetButtons();
            Invalidate();
        }

        private bool CloseButtonEnabled
        {
            get	{	return (DockPane.ActiveContent != null) && DockPane.ActiveContent.DockHandler.CloseButton;	}
        }

        /// <summary>
        /// Determines whether the close button is visible on the content
        /// </summary>
        private bool CloseButtonVisible
        {
            get { return (DockPane.ActiveContent != null) && DockPane.ActiveContent.DockHandler.CloseButtonVisible; }
        }

        private bool ShouldShowAutoHideButton
        {
            get	{	return !DockPane.IsFloat;	}
        }

        private void SetButtons()
        {
            ButtonClose.Enabled = CloseButtonEnabled;
            ButtonClose.Visible = CloseButtonVisible;
            ButtonAutoHide.Visible = ShouldShowAutoHideButton;
            ButtonOptions.Visible = HasTabPageContextMenu;
            ButtonClose.RefreshChanges();
            ButtonAutoHide.RefreshChanges();
            ButtonOptions.RefreshChanges();
            
            SetButtonsPosition();
        }

        private void SetButtonsPosition()
        {
            // set the size and location for close and auto-hide buttons
            Rectangle rectCaption = ClientRectangle;
            int buttonWidth = ButtonClose.Image.Width;
            int buttonHeight = ButtonClose.Image.Height;
            int height = rectCaption.Height - ButtonGapTop - ButtonGapBottom;
            if (buttonHeight < height)
            {
                buttonWidth = buttonWidth * height / buttonHeight;
                buttonHeight = height;
            }
            Size buttonSize = new(buttonWidth, buttonHeight);
            int x = rectCaption.X + rectCaption.Width - 1 - ButtonGapRight - ButtonClose.Width;
            int y = rectCaption.Y + ButtonGapTop;
            Point point = new(x, y);
            ButtonClose.Bounds = DrawHelper.RtlTransform(this, new Rectangle(point, buttonSize));

            // If the close button is not visible draw the auto hide button overtop.
            // Otherwise it is drawn to the left of the close button.
            if (CloseButtonVisible)
                point.Offset(-(buttonWidth + ButtonGapBetween), 0);
            
            ButtonAutoHide.Bounds = DrawHelper.RtlTransform(this, new Rectangle(point, buttonSize));
            if (ShouldShowAutoHideButton)
                point.Offset(-(buttonWidth + ButtonGapBetween), 0);
            ButtonOptions.Bounds = DrawHelper.RtlTransform(this, new Rectangle(point, buttonSize));
        }

        private void Close_Click(object sender, EventArgs e)
        {
            DockPane.CloseActiveContent();
        }

        private void AutoHide_Click(object sender, EventArgs e)
        {
            DockPane.DockState = DockHelper.ToggleAutoHideState(DockPane.DockState);
            if (DockHelper.IsDockStateAutoHide(DockPane.DockState))
            {
                DockPane.DockPanel.ActiveAutoHideContent = null;
                DockPane.NestedDockingStatus.NestedPanes.SwitchPaneWithFirstChild(DockPane);
            }
        }

        private void Options_Click(object sender, EventArgs e)
        {
            ShowTabPageContextMenu(PointToClient(Control.MousePosition));
        }

        protected override void OnRightToLeftChanged(EventArgs e)
        {
            base.OnRightToLeftChanged(e);
            PerformLayout();
        }
    }
}
