using System;
using System.Drawing;
using System.Windows.Forms;

namespace WeifenLuo.WinFormsUI.Docking
{
    using System.ComponentModel;
    using WeifenLuo.WinFormsUI.ThemeVS2003;

    /// <include file='CodeDoc/DockPaneCaptionVS2003.xml' path='//CodeDoc/Class[@name="DockPaneCaptionVS2003"]/ClassDef/*'/>
    [ToolboxItem(false)]
    internal class VS2003DockPaneCaption : DockPaneCaptionBase
    {
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

        private readonly InertButton _buttonClose;
        private readonly InertButton _buttonAutoHide;

        /// <include file='CodeDoc/DockPaneCaptionVS2003.xml' path='//CodeDoc/Class[@name="DockPaneCaptionVS2003"]/Construct[@name="(DockPane)"]/*'/>
        protected internal VS2003DockPaneCaption(DockPane pane) : base(pane)
        {
            SuspendLayout();

            Font = SystemInformation.MenuFont;

            _buttonClose = new InertButton(ImageCloseEnabled, ImageCloseDisabled);
            _buttonAutoHide = new InertButton();

            _buttonClose.ToolTipText = ToolTipClose;
            _buttonClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            _buttonClose.Click += new EventHandler(this.Close_Click);

            _buttonAutoHide.ToolTipText = ToolTipAutoHide;
            _buttonAutoHide.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            _buttonAutoHide.Click += new EventHandler(AutoHide_Click);

            Controls.AddRange(new Control[]	{	_buttonClose, _buttonAutoHide });

            ResumeLayout();
        }

        #region Customizable Properties
        /// <include file='CodeDoc/DockPaneCaptionVS2003.xml' path='//CodeDoc/Class[@name="DockPaneCaptionVS2003"]/Property[@name="TextGapTop"]/*'/>
        protected virtual int TextGapTop
        {
            get	{	return _TextGapTop;	}
        }

        /// <include file='CodeDoc/DockPaneCaptionVS2003.xml' path='//CodeDoc/Class[@name="DockPaneCaptionVS2003"]/Property[@name="TextGapBottom"]/*'/>
        protected virtual int TextGapBottom
        {
            get	{	return _TextGapBottom;	}
        }

        /// <include file='CodeDoc/DockPaneCaptionVS2003.xml' path='//CodeDoc/Class[@name="DockPaneCaptionVS2003"]/Property[@name="TextGapLeft"]/*'/>
        protected virtual int TextGapLeft
        {
            get	{	return _TextGapLeft;	}
        }

        /// <include file='CodeDoc/DockPaneCaptionVS2003.xml' path='//CodeDoc/Class[@name="DockPaneCaptionVS2003"]/Property[@name="TextGapRight"]/*'/>
        protected virtual int TextGapRight
        {
            get	{	return _TextGapRight;	}
        }

        /// <include file='CodeDoc/DockPaneCaptionVS2003.xml' path='//CodeDoc/Class[@name="DockPaneCaptionVS2003"]/Property[@name="ButtonGapTop"]/*'/>
        protected virtual int ButtonGapTop
        {
            get	{	return _ButtonGapTop;	}
        }

        /// <include file='CodeDoc/DockPaneCaptionVS2003.xml' path='//CodeDoc/Class[@name="DockPaneCaptionVS2003"]/Property[@name="ButtonGapBottom"]/*'/>
        protected virtual int ButtonGapBottom
        {
            get	{	return _ButtonGapBottom;	}
        }

        /// <include file='CodeDoc/DockPaneCaptionVS2003.xml' path='//CodeDoc/Class[@name="DockPaneCaptionVS2003"]/Property[@name="ButtonGapLeft"]/*'/>
        protected virtual int ButtonGapLeft
        {
            get	{	return _ButtonGapLeft;	}
        }

        /// <include file='CodeDoc/DockPaneCaptionVS2003.xml' path='//CodeDoc/Class[@name="DockPaneCaptionVS2003"]/Property[@name="ButtonGapRight"]/*'/>
        protected virtual int ButtonGapRight
        {
            get	{	return _ButtonGapRight;	}
        }

        /// <include file='CodeDoc/DockPaneCaptionVS2003.xml' path='//CodeDoc/Class[@name="DockPaneCaptionVS2003"]/Property[@name="ButtonGapBetween"]/*'/>
        protected virtual int ButtonGapBetween
        {
            get	{	return _ButtonGapBetween;	}
        }

        private static Image _imageCloseEnabled = null;
        /// <include file='CodeDoc/DockPaneCaptionVS2003.xml' path='//CodeDoc/Class[@name="DockPaneCaptionVS2003"]/Property[@name="ImageCloseEnabled"]/*'/>
        protected virtual Image ImageCloseEnabled
        {
            get
            {	
                _imageCloseEnabled ??= Resources.DockPaneCaption_CloseEnabled;
                return _imageCloseEnabled;
            }
        }

        private static Image _imageCloseDisabled = null;
        /// <include file='CodeDoc/DockPaneCaptionVS2003.xml' path='//CodeDoc/Class[@name="DockPaneCaptionVS2003"]/Property[@name="ImageCloseDisabled"]/*'/>
        protected virtual Image ImageCloseDisabled
        {
            get
            {	
                _imageCloseDisabled ??= Resources.DockPaneCaption_CloseDisabled;
                return _imageCloseDisabled;
            }
        }

        private static Image _imageAutoHideYes = null;
        /// <include file='CodeDoc/DockPaneCaptionVS2003.xml' path='//CodeDoc/Class[@name="DockPaneCaptionVS2003"]/Property[@name="ImageAutoHideYes"]/*'/>
        protected virtual Image ImageAutoHideYes
        {
            get
            {	
                _imageAutoHideYes ??= Resources.DockPaneCaption_AutoHideYes;
                return _imageAutoHideYes;
            }
        }

        private static Image _imageAutoHideNo = null;
        /// <include file='CodeDoc/DockPaneCaptionVS2003.xml' path='//CodeDoc/Class[@name="DockPaneCaptionVS2003"]/Property[@name="ImageAutoHideNo"]/*'/>
        protected virtual Image ImageAutoHideNo
        {
            get
            {	
                _imageAutoHideNo ??= Resources.DockPaneCaption_AutoHideNo;
                return _imageAutoHideNo;
            }
        }

        private static string _toolTipClose = null;
        /// <include file='CodeDoc/DockPaneCaptionVS2003.xml' path='//CodeDoc/Class[@name="DockPaneCaptionVS2003"]/Property[@name="ToolTipClose"]/*'/>
        protected virtual string ToolTipClose
        {
            get
            {	
                _toolTipClose ??= ThemeVS2003.Strings.DockPaneCaption_ToolTipClose;
                return _toolTipClose;
            }
        }

        private static string _toolTipAutoHide = null;
        /// <include file='CodeDoc/DockPaneCaptionVS2003.xml' path='//CodeDoc/Class[@name="DockPaneCaptionVS2003"]/Property[@name="ToolTipAutoHide"]/*'/>
        protected virtual string ToolTipAutoHide
        {
            get
            {	
                _toolTipAutoHide ??= ThemeVS2003.Strings.DockPaneCaption_ToolTipAutoHide;
                return _toolTipAutoHide;
            }
        }

        /// <include file='CodeDoc/DockPaneCaptionVS2003.xml' path='//CodeDoc/Class[@name="DockPaneCaptionVS2003"]/Property[@name="ActiveBackColor"]/*'/>
        protected virtual Color ActiveBackColor
        {
            get	{	return SystemColors.ActiveCaption;	}
        }

        /// <include file='CodeDoc/DockPaneCaptionVS2003.xml' path='//CodeDoc/Class[@name="DockPaneCaptionVS2003"]/Property[@name="InactiveBackColor"]/*'/>
        protected virtual Color InactiveBackColor
        {
            get	{	return SystemColors.Control;	}
        }

        /// <include file='CodeDoc/DockPaneCaptionVS2003.xml' path='//CodeDoc/Class[@name="DockPaneCaptionVS2003"]/Property[@name="ActiveTextColor"]/*'/>
        protected virtual Color ActiveTextColor
        {
            get	{	return SystemColors.ActiveCaptionText;	}
        }

        /// <include file='CodeDoc/DockPaneCaptionVS2003.xml' path='//CodeDoc/Class[@name="DockPaneCaptionVS2003"]/Property[@name="InactiveTextColor"]/*'/>
        protected virtual Color InactiveTextColor
        {
            get	{	return SystemColors.ControlText;	}
        }

        /// <include file='CodeDoc/DockPaneCaptionVS2003.xml' path='//CodeDoc/Class[@name="DockPaneCaptionVS2003"]/Property[@name="InactiveBorderColor"]/*'/>
        protected virtual Color InactiveBorderColor
        {
            get	{	return SystemColors.GrayText; }
        }

        /// <include file='CodeDoc/DockPaneCaptionVS2003.xml' path='//CodeDoc/Class[@name="DockPaneCaptionVS2003"]/Property[@name="ActiveButtonBorderColor"]/*'/>
        protected virtual Color ActiveButtonBorderColor
        {
            get	{	return ActiveTextColor;	}
        }

        /// <include file='CodeDoc/DockPaneCaptionVS2003.xml' path='//CodeDoc/Class[@name="DockPaneCaptionVS2003"]/Property[@name="InactiveButtonBorderColor"]/*'/>
        protected virtual Color InactiveButtonBorderColor
        {
            get	{	return Color.Empty;	}
        }

        private static readonly TextFormatFlags _textFormat =
            TextFormatFlags.SingleLine |
            TextFormatFlags.EndEllipsis |
            TextFormatFlags.VerticalCenter;
        /// <include file='CodeDoc/DockPaneCaptionVS2003.xml' path='//CodeDoc/Class[@name="DockPaneCaptionVS2003"]/Property[@name="TextStringFormat"]/*'/>
        protected virtual TextFormatFlags TextFormat
        {
            get
            {
                return _textFormat;
            }
        }

        #endregion

        /// <exclude/>
        protected internal override int MeasureHeight()
        {
            int height = Font.Height + TextGapTop + TextGapBottom;

            if (height < ImageCloseEnabled.Height + ButtonGapTop + ButtonGapBottom)
                height = ImageCloseEnabled.Height + ButtonGapTop + ButtonGapBottom;

            return height;
        }

        /// <exclude/>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint (e);
            DrawCaption(e.Graphics);
        }

        private void DrawCaption(Graphics g)
        {
            BackColor = DockPane.IsActivated ? ActiveBackColor : InactiveBackColor;

            Rectangle rectCaption = ClientRectangle;

            if (!DockPane.IsActivated)
            {
                using Pen pen = new(InactiveBorderColor);
                g.DrawLine(pen, rectCaption.X + 1, rectCaption.Y, rectCaption.X + rectCaption.Width - 2, rectCaption.Y);
                g.DrawLine(pen, rectCaption.X + 1, rectCaption.Y + rectCaption.Height - 1, rectCaption.X + rectCaption.Width - 2, rectCaption.Y + rectCaption.Height - 1);
                g.DrawLine(pen, rectCaption.X, rectCaption.Y + 1, rectCaption.X, rectCaption.Y + rectCaption.Height - 2);
                g.DrawLine(pen, rectCaption.X + rectCaption.Width - 1, rectCaption.Y + 1, rectCaption.X + rectCaption.Width - 1, rectCaption.Y + rectCaption.Height - 2);
            }

            _buttonClose.ForeColor = _buttonAutoHide.ForeColor = (DockPane.IsActivated ? ActiveTextColor : InactiveTextColor);
            _buttonClose.BorderColor = _buttonAutoHide.BorderColor = (DockPane.IsActivated ? ActiveButtonBorderColor : InactiveButtonBorderColor);

            Rectangle rectCaptionText = rectCaption;
            rectCaptionText.X += TextGapLeft;
            if (ShouldShowCloseButton && ShouldShowAutoHideButton)
                rectCaptionText.Width = rectCaption.Width - ButtonGapRight
                    - ButtonGapLeft	- TextGapLeft - TextGapRight -
                    (_buttonAutoHide.Width + ButtonGapBetween + _buttonClose.Width);
            else if (ShouldShowCloseButton || ShouldShowAutoHideButton)
                rectCaptionText.Width = rectCaption.Width - ButtonGapRight
                    - ButtonGapLeft	- TextGapLeft - TextGapRight - _buttonClose.Width;
            else
                rectCaptionText.Width = rectCaption.Width - TextGapLeft - TextGapRight;
            rectCaptionText.Y += TextGapTop;
            rectCaptionText.Height -= TextGapTop + TextGapBottom;
            TextRenderer.DrawText(g, DockPane.CaptionText, Font, rectCaptionText, DockPane.IsActivated ? ActiveTextColor : InactiveTextColor, TextFormat);
        }

        /// <exclude/>
        protected override void OnLayout(LayoutEventArgs levent)
        {
            SetButtonsPosition();
            base.OnLayout (levent);
        }

        /// <exclude/>
        protected override void OnRefreshChanges()
        {
            SetButtons();
            Invalidate();
        }

        private bool ShouldShowCloseButton
        {
            get	{	return (DockPane.ActiveContent != null) && DockPane.ActiveContent.DockHandler.CloseButton;	}
        }

        private bool ShouldShowAutoHideButton
        {
            get	{	return !DockPane.IsFloat;	}
        }

        private void SetButtons()
        {
            _buttonClose.Visible = ShouldShowCloseButton;
            _buttonAutoHide.Visible = ShouldShowAutoHideButton;
            _buttonAutoHide.ImageEnabled = DockPane.IsAutoHide ? ImageAutoHideYes : ImageAutoHideNo;
            
            SetButtonsPosition();
        }

        private void SetButtonsPosition()
        {
            // set the size and location for close and auto-hide buttons
            Rectangle rectCaption = ClientRectangle;
            int buttonWidth = ImageCloseEnabled.Width;
            int buttonHeight = ImageCloseEnabled.Height;
            int height = rectCaption.Height - ButtonGapTop - ButtonGapBottom;
            if (buttonHeight < height)
            {
                buttonWidth = buttonWidth * height / buttonHeight;
                buttonHeight = height;
            }
            _buttonClose.SuspendLayout();
            _buttonAutoHide.SuspendLayout();
            Size buttonSize = new(buttonWidth, buttonHeight);
            _buttonClose.Size = _buttonAutoHide.Size = buttonSize;
            int x = rectCaption.X + rectCaption.Width - 1 - ButtonGapRight - _buttonClose.Width;
            int y = rectCaption.Y + ButtonGapTop;
            Point point = _buttonClose.Location = new Point(x, y);
            if (ShouldShowCloseButton)
                point.Offset(-(_buttonAutoHide.Width + ButtonGapBetween), 0);
            _buttonAutoHide.Location = point;
            _buttonClose.ResumeLayout();
            _buttonAutoHide.ResumeLayout();
        }

        private void Close_Click(object sender, EventArgs e)
        {
            DockPane.CloseActiveContent();
        }

        private void AutoHide_Click(object sender, EventArgs e)
        {
            DockPane.DockState = DockHelper.ToggleAutoHideState(DockPane.DockState);
            if (!DockPane.IsAutoHide)
                DockPane.Activate();
        }
    }
}
