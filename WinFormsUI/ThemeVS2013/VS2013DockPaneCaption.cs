using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.ComponentModel;

namespace WeifenLuo.WinFormsUI.Docking
{
    using ThemeVS2012;

    [ToolboxItem(false)]
    internal class VS2013DockPaneCaption : DockPaneCaptionBase
    {
        #region consts
        private const int TextGapTop = 3;
        private const int TextGapBottom = 2;
        private const int TextGapLeft = 2;
        private const int TextGapRight = 3;
        private const int ButtonGapTop = 4;
        private const int ButtonGapBottom = 3;
        private const int ButtonGapBetween = 1;
        private const int ButtonGapLeft = 1;
        private const int ButtonGapRight = 5;
        #endregion

        private InertButtonBase _buttonClose;
        private InertButtonBase ButtonClose
        {
            get
            {
                if (_buttonClose == null)
                {
                    _buttonClose = new VS2012DockPaneCaptionInertButton(this,
                        DockPane.DockPanel.Theme.ImageService.DockPaneHover_Close,
                        DockPane.DockPanel.Theme.ImageService.DockPane_Close,
                        DockPane.DockPanel.Theme.ImageService.DockPanePress_Close,
                        DockPane.DockPanel.Theme.ImageService.DockPaneActiveHover_Close,
                        DockPane.DockPanel.Theme.ImageService.DockPaneActive_Close);
                    _toolTip.SetToolTip(_buttonClose, ToolTipClose);
                    _buttonClose.Click += new EventHandler(Close_Click);
                    Controls.Add(_buttonClose);
                }

                return _buttonClose;
            }
        }

        private InertButtonBase _buttonAutoHide;
        private InertButtonBase ButtonAutoHide
        {
            get
            {
                if (_buttonAutoHide == null)
                {
                    _buttonAutoHide = new VS2012DockPaneCaptionInertButton(this,
                        DockPane.DockPanel.Theme.ImageService.DockPaneHover_Dock,
                        DockPane.DockPanel.Theme.ImageService.DockPane_Dock,
                        DockPane.DockPanel.Theme.ImageService.DockPanePress_Dock,
                        DockPane.DockPanel.Theme.ImageService.DockPaneActiveHover_Dock,
                        DockPane.DockPanel.Theme.ImageService.DockPaneActive_Dock,
                        DockPane.DockPanel.Theme.ImageService.DockPaneActiveHover_AutoHide,
                        DockPane.DockPanel.Theme.ImageService.DockPaneActive_AutoHide,
                        DockPane.DockPanel.Theme.ImageService.DockPanePress_AutoHide);
                    _toolTip.SetToolTip(_buttonAutoHide, ToolTipAutoHide);
                    _buttonAutoHide.Click += new EventHandler(AutoHide_Click);
                    Controls.Add(_buttonAutoHide);
                }

                return _buttonAutoHide;
            }
        }

        private InertButtonBase _buttonOptions;
        private InertButtonBase ButtonOptions
        {
            get
            {
                if (_buttonOptions == null)
                {
                    _buttonOptions = new VS2012DockPaneCaptionInertButton(this,
                        DockPane.DockPanel.Theme.ImageService.DockPaneHover_Option,
                        DockPane.DockPanel.Theme.ImageService.DockPane_Option,
                        DockPane.DockPanel.Theme.ImageService.DockPanePress_Option,
                        DockPane.DockPanel.Theme.ImageService.DockPaneActiveHover_Option,
                        DockPane.DockPanel.Theme.ImageService.DockPaneActive_Option);
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

        private readonly ToolTip _toolTip;

        public VS2013DockPaneCaption(DockPane pane) : base(pane)
        {
            SuspendLayout();

            _components = new Container();
            _toolTip = new ToolTip(Components);

            ResumeLayout();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                Components.Dispose();
            base.Dispose(disposing);
        }

        public Font TextFont
        {
            get { return DockPane.DockPanel.Theme.Skin.DockPaneStripSkin.TextFont; }
        }

        private static string _toolTipClose;
        private static string ToolTipClose
        {
            get
            {	
                _toolTipClose ??= ThemeVS2012.Strings.DockPaneCaption_ToolTipClose;
                return _toolTipClose;
            }
        }

        private static string _toolTipOptions;
        private static string ToolTipOptions
        {
            get
            {
                _toolTipOptions ??= ThemeVS2012.Strings.DockPaneCaption_ToolTipOptions;
                return _toolTipOptions;
            }
        }

        private static string _toolTipAutoHide;
        private static string ToolTipAutoHide
        {
            get
            {	
                _toolTipAutoHide ??= ThemeVS2012.Strings.DockPaneCaption_ToolTipAutoHide;
                return _toolTipAutoHide;
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

            Rectangle rect = ClientRectangle;
            var border = DockPane.DockPanel.Theme.ColorPalette.ToolWindowBorder;
            ToolWindowCaptionPalette palette;
            if (DockPane.IsActivePane)
            {
                palette = DockPane.DockPanel.Theme.ColorPalette.ToolWindowCaptionActive;
            }
            else
            {
                palette = DockPane.DockPanel.Theme.ColorPalette.ToolWindowCaptionInactive;
            }

            SolidBrush captionBrush = DockPane.DockPanel.Theme.PaintingService.GetBrush(palette.Background);
            g.FillRectangle(captionBrush, rect);

            g.DrawLine(DockPane.DockPanel.Theme.PaintingService.GetPen(border), rect.Left, rect.Top,
                rect.Left, rect.Bottom);
            g.DrawLine(DockPane.DockPanel.Theme.PaintingService.GetPen(border), rect.Left, rect.Top,
                rect.Right, rect.Top);
            g.DrawLine(DockPane.DockPanel.Theme.PaintingService.GetPen(border), rect.Right - 1, rect.Top,
                rect.Right - 1, rect.Bottom);

            Rectangle rectCaption = rect;

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

            TextRenderer.DrawText(g, DockPane.CaptionText, TextFont, DrawHelper.RtlTransform(this, rectCaptionText), palette.Text, TextFormat);

            Rectangle rectDotsStrip = rectCaptionText;
            int textLength = (int)g.MeasureString(DockPane.CaptionText, TextFont).Width + TextGapLeft;
            rectDotsStrip.X += textLength;
            rectDotsStrip.Width -= textLength;
            rectDotsStrip.Height = ClientRectangle.Height;

            DrawDotsStrip(g, rectDotsStrip, palette.Grip);
        }

        protected void DrawDotsStrip(Graphics g, Rectangle rectStrip, Color colorDots)
        {
            if (rectStrip.Width <= 0 || rectStrip.Height <= 0)
                return;

            var penDots = DockPane.DockPanel.Theme.PaintingService.GetPen(colorDots, 1);
            penDots.DashStyle = DashStyle.Custom;
            penDots.DashPattern = new float[] { 1, 3 };
            int positionY = rectStrip.Height / 2;

            g.DrawLine(penDots, rectStrip.X + 2, positionY, rectStrip.X + rectStrip.Width - 2, positionY);

            g.DrawLine(penDots, rectStrip.X, positionY - 2, rectStrip.X + rectStrip.Width, positionY - 2);
            g.DrawLine(penDots, rectStrip.X, positionY + 2, rectStrip.X + rectStrip.Width, positionY + 2);
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
            get	{ return (DockPane.ActiveContent != null) && DockPane.ActiveContent.DockHandler.CloseButton; }
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

            Size buttonSize = new(buttonWidth, buttonHeight);
            int x = rectCaption.X + rectCaption.Width - ButtonGapRight - _buttonClose.Width;
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

        protected override bool CanDragAutoHide
        {
            get { return true; }
        }
    }
}
