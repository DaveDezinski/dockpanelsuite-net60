using System;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;

// To simplify the process of finding the toolbox bitmap resource:
// #1 Create an internal class called "resfinder" outside of the root namespace.
// #2 Use "Resfinder" in the toolbox bitmap attribute instead of the control name.
// #3 use the "<default namespace>.<resourcename>" string to locate the resource.
// See: http://www.bobpowell.net/toolboxbitmap.htm
internal class Resfinder
{
}

namespace WeifenLuo.WinFormsUI.Docking
{
    /// <summary>
    /// Deserialization handler of layout file/stream.
    /// </summary>
    /// <param name="persistString">Strings stored in layout file/stream.</param>
    /// <returns>Dock content deserialized from layout/stream.</returns>
    /// <remarks>
    /// The deserialization handler method should handle all possible exceptions.
    /// 
    /// If any exception happens during deserialization and is not handled, the program might crash or experience other issues.
    /// </remarks>
    [SuppressMessage("Microsoft.Naming", "CA1720:AvoidTypeNamesInParameters", MessageId = "0#")]
    public delegate IDockContent DeserializeDockContent(string persistString);

    [LocalizedDescription("DockPanel_Description")]
    [Designer("System.Windows.Forms.Design.ControlDesigner, System.Design")]
    [ToolboxBitmap(typeof(Resfinder), "WeifenLuo.WinFormsUI.Docking.DockPanel.bmp")]
    [DefaultProperty("DocumentStyle")]
    [DefaultEvent("ActiveContentChanged")]
    public partial class DockPanel : Panel
    {
        private readonly FocusManagerImpl _focusManager;
        private readonly DockPaneCollection _panes;
        private readonly FloatWindowCollection _floatWindows;
        private AutoHideWindowControl _autoHideWindow;
        private DockWindowCollection _dockWindows;
        private readonly DockContent _dummyContent; 
        private readonly Control _dummyControl;
        
        public DockPanel()
        {
            ShowAutoHideContentOnHover = true;

            _focusManager = new FocusManagerImpl(this);
            _panes = new DockPaneCollection();
            _floatWindows = new FloatWindowCollection();

            SuspendLayout();

            _dummyControl = new DummyControl
            {
                Bounds = new Rectangle(0, 0, 1, 1)
            };
            Controls.Add(_dummyControl);

            Theme.ApplyTo(this);

            _autoHideWindow = Theme.Extender.AutoHideWindowFactory.CreateAutoHideWindow(this);
            _autoHideWindow.Visible = false;
            _autoHideWindow.ActiveContentChanged += AutoHideWindow_ActiveContentChanged; 
            SetAutoHideWindowParent();

            LoadDockWindows();

            _dummyContent = new DockContent();
            ResumeLayout();
        }

        internal void ResetDummy()
        {
            DummyControl.ResetBackColor();
        }

        internal void SetDummy()
        {
            DummyControl.BackColor = DockBackColor;
        }

        private Color _backColor;

        /// <summary>
        /// Determines the color with which the client rectangle will be drawn.
        /// If this property is used instead of the BackColor it will not have any influence on the borders to the surrounding controls (DockPane).
        /// The BackColor property changes the borders of surrounding controls (DockPane).
        /// Alternatively both properties may be used (BackColor to draw and define the color of the borders and DockBackColor to define the color of the client rectangle). 
        /// For Backgroundimages: Set your prefered Image, then set the DockBackColor and the BackColor to the same Color (Control)
        /// </summary>
        [Description("Determines the color with which the client rectangle will be drawn.\r\n" +
            "If this property is used instead of the BackColor it will not have any influence on the borders to the surrounding controls (DockPane).\r\n" +
            "The BackColor property changes the borders of surrounding controls (DockPane).\r\n" +
            "Alternatively both properties may be used (BackColor to draw and define the color of the borders and DockBackColor to define the color of the client rectangle).\r\n" +
            "For Backgroundimages: Set your prefered Image, then set the DockBackColor and the BackColor to the same Color (Control).")]
        public Color DockBackColor
        {
            get
            {
                return !_backColor.IsEmpty ? _backColor : base.BackColor;
            }

            set
            {
                if (_backColor != value)
                {
                    _backColor = value;
                    Refresh();
                }
            }
        }

        private AutoHideStripBase _autoHideStripControl;

        internal AutoHideStripBase AutoHideStripControl
        {
            get
            {	
                if (_autoHideStripControl == null)
                {
                    _autoHideStripControl = Theme.Extender.AutoHideStripFactory.CreateAutoHideStrip(this);
                    Controls.Add(_autoHideStripControl);
                }

                return _autoHideStripControl;
            }
        }

        internal void ResetAutoHideStripControl()
        {
            _autoHideStripControl?.Dispose();
            _autoHideStripControl = null;
        }

        private void MdiClientHandleAssigned(object sender, EventArgs e)
        {
            SetMdiClient();
            PerformLayout();
        }

        private void MdiClient_Layout(object sender, LayoutEventArgs e)
        {
            if (DocumentStyle != DocumentStyle.DockingMdi)
                return;

            foreach (DockPane pane in Panes)
                if (pane.DockState == DockState.Document)
                    pane.SetContentBounds();

            InvalidateWindowRegion();
        }

        private bool _disposed;

        protected override void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _focusManager.Dispose();
                if (_mdiClientController != null)
                {
                    _mdiClientController.HandleAssigned -= new EventHandler(MdiClientHandleAssigned);
                    _mdiClientController.MdiChildActivate -= new EventHandler(ParentFormMdiChildActivate);
                    _mdiClientController.Layout -= new LayoutEventHandler(MdiClient_Layout);
                    _mdiClientController.Dispose();
                }
                FloatWindows.Clear();
                Panes.Clear();
                DummyContent.Dispose();

                _disposed = true;
            }
                
            base.Dispose(disposing);
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IDockContent ActiveAutoHideContent
        {
            get { return AutoHideWindow.ActiveContent; }
            set { AutoHideWindow.ActiveContent = value; }
        }

        private bool _allowEndUserDocking = !Win32Helper.IsRunningOnMono;
        [LocalizedCategory("Category_Docking")]
        [LocalizedDescription("DockPanel_AllowEndUserDocking_Description")]
        [DefaultValue(true)]
        public bool AllowEndUserDocking
        {
            get
            {
                if (Win32Helper.IsRunningOnMono && _allowEndUserDocking)
                    _allowEndUserDocking = false;

                return _allowEndUserDocking;
            }

            set
            {
                if (Win32Helper.IsRunningOnMono && value)
                    throw new InvalidOperationException("AllowEndUserDocking can only be false if running on Mono");
                    
                _allowEndUserDocking = value;
            }
        }

        private bool _allowEndUserNestedDocking = !Win32Helper.IsRunningOnMono;
        [LocalizedCategory("Category_Docking")]
        [LocalizedDescription("DockPanel_AllowEndUserNestedDocking_Description")]
        [DefaultValue(true)]
        public bool AllowEndUserNestedDocking
        {
            get
            {
                if (Win32Helper.IsRunningOnMono && _allowEndUserDocking)
                    _allowEndUserDocking = false;
                return _allowEndUserNestedDocking;
            }

            set
            {
                if (Win32Helper.IsRunningOnMono && value)
                    throw new InvalidOperationException("AllowEndUserNestedDocking can only be false if running on Mono");

                _allowEndUserNestedDocking = value;
            }
        }

        private readonly DockContentCollection _contents = new();
        [Browsable(false)]
        public DockContentCollection Contents
        {
            get { return _contents; }
        }

        internal DockContent DummyContent
        {
            get { return _dummyContent; }
        }

        private bool _rightToLeftLayout = false;
        [DefaultValue(false)]
        [LocalizedCategory("Appearance")]
        [LocalizedDescription("DockPanel_RightToLeftLayout_Description")]
        public bool RightToLeftLayout
        {
            get
            {
                return _rightToLeftLayout;
            }

            set
            {
                if (_rightToLeftLayout == value)
                    return;

                _rightToLeftLayout = value;
                foreach (FloatWindow floatWindow in FloatWindows)
                    floatWindow.RightToLeftLayout = value;
            }
        }

        protected override void OnRightToLeftChanged(EventArgs e)
        {
            base.OnRightToLeftChanged(e);
            foreach (FloatWindow floatWindow in FloatWindows)
                floatWindow.RightToLeft = RightToLeft;
        }

        private bool _showDocumentIcon = false;
        [DefaultValue(false)]
        [LocalizedCategory("Category_Docking")]
        [LocalizedDescription("DockPanel_ShowDocumentIcon_Description")]
        public bool ShowDocumentIcon
        {
            get	{	return _showDocumentIcon;	}
            set
            {
                if (_showDocumentIcon == value)
                    return;

                _showDocumentIcon = value;
                Refresh();
            }
        }

        [DefaultValue(DocumentTabStripLocation.Top)]
        [LocalizedCategory("Category_Docking")]
        [LocalizedDescription("DockPanel_DocumentTabStripLocation")]
        public DocumentTabStripLocation DocumentTabStripLocation { get; set; } = DocumentTabStripLocation.Top;

        [Browsable(false)]
        [Obsolete("Use Theme.Extender instead.")]
        public DockPanelExtender Extender
        {
            get { return null; }
        }

        [Browsable(false)]
        [Obsolete("Use Theme.Extender instead.")]
        public DockPanelExtender.IDockPaneFactory DockPaneFactory
        {
            get { return null; }
        }

        [Browsable(false)]
        [Obsolete("Use Theme.Extender instead.")]
        public DockPanelExtender.IFloatWindowFactory FloatWindowFactory
        {
            get { return null; }
        }

        [Browsable(false)]
        [Obsolete("Use Theme.Extender instead.")]
        public DockPanelExtender.IDockWindowFactory DockWindowFactory
        {
            get { return null; }
        }

        [Browsable(false)]
        public DockPaneCollection Panes
        {
            get { return _panes; }
        }

        /// <summary>
        /// Dock area.
        /// </summary>
        /// <remarks>
        /// This <see cref="Rectangle"/> is the center rectangle of <see cref="DockPanel"/> control.
        /// 
        /// Excluded spaces are for the following visual elements,
        /// * Auto hide strips on four sides.
        /// * Necessary paddings defined in themes.
        /// 
        /// Therefore, all dock contents mainly fall into this area (except auto hide window, which might slightly move beyond this area).
        /// </remarks>
        public Rectangle DockArea
        {
            get
            {
                return new Rectangle(DockPadding.Left, DockPadding.Top,
                    ClientRectangle.Width - DockPadding.Left - DockPadding.Right,
                    ClientRectangle.Height - DockPadding.Top - DockPadding.Bottom);
            }
        }

        private double _dockBottomPortion = 0.25;

        [LocalizedCategory("Category_Docking")]
        [LocalizedDescription("DockPanel_DockBottomPortion_Description")]
        [DefaultValue(0.25)]
        public double DockBottomPortion
        {
            get
            {
                return _dockBottomPortion;
            }

            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(value));

                if (Math.Abs(value - _dockBottomPortion) < double.Epsilon)
                    return;

                _dockBottomPortion = value;

                if (_dockBottomPortion < 1 && _dockTopPortion < 1 && _dockTopPortion + _dockBottomPortion > 1)
                    _dockTopPortion = 1 - _dockBottomPortion;

                PerformLayout();
            }
        }

        private double _dockLeftPortion = 0.25;

        [LocalizedCategory("Category_Docking")]
        [LocalizedDescription("DockPanel_DockLeftPortion_Description")]
        [DefaultValue(0.25)]
        public double DockLeftPortion
        {
            get
            {
                return _dockLeftPortion;
            }

            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(value));

                if (Math.Abs(value - _dockLeftPortion) < double.Epsilon)
                    return;

                _dockLeftPortion = value;

                if (_dockLeftPortion < 1 && _dockRightPortion < 1 && _dockLeftPortion + _dockRightPortion > 1)
                    _dockRightPortion = 1 - _dockLeftPortion;
                PerformLayout();
            }
        }

        private double _dockRightPortion = 0.25;

        [LocalizedCategory("Category_Docking")]
        [LocalizedDescription("DockPanel_DockRightPortion_Description")]
        [DefaultValue(0.25)]
        public double DockRightPortion
        {
            get
            {
                return _dockRightPortion;
            }

            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(value));

                if (Math.Abs(value - _dockRightPortion) < double.Epsilon)
                    return;

                _dockRightPortion = value;

                if (_dockLeftPortion < 1 && _dockRightPortion < 1 && _dockLeftPortion + _dockRightPortion > 1)
                    _dockLeftPortion = 1 - _dockRightPortion;

                PerformLayout();
            }
        }

        private double _dockTopPortion = 0.25;

        [LocalizedCategory("Category_Docking")]
        [LocalizedDescription("DockPanel_DockTopPortion_Description")]
        [DefaultValue(0.25)]
        public double DockTopPortion
        {
            get
            {
                return _dockTopPortion;
            }

            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(value));

                if (Math.Abs(value - _dockTopPortion) < double.Epsilon)
                    return;

                _dockTopPortion = value;

                if (_dockTopPortion < 1 && _dockBottomPortion < 1 && _dockTopPortion + _dockBottomPortion > 1)
                    _dockBottomPortion = 1 - _dockTopPortion;
                PerformLayout();
            }
        }

        [Browsable(false)]
        public DockWindowCollection DockWindows
        {
            get { return _dockWindows; }
        }

        public void UpdateDockWindowZOrder(DockStyle dockStyle, bool fullPanelEdge)
        {
            if (dockStyle == DockStyle.Left)
            {
                if (fullPanelEdge)
                    DockWindows[DockState.DockLeft].SendToBack();
                else
                    DockWindows[DockState.DockLeft].BringToFront();
            }
            else if (dockStyle == DockStyle.Right)
            {
                if (fullPanelEdge)
                    DockWindows[DockState.DockRight].SendToBack();
                else
                    DockWindows[DockState.DockRight].BringToFront();
            }
            else if (dockStyle == DockStyle.Top)
            {
                if (fullPanelEdge)
                    DockWindows[DockState.DockTop].SendToBack();
                else
                    DockWindows[DockState.DockTop].BringToFront();
            }
            else if (dockStyle == DockStyle.Bottom)
            {
                if (fullPanelEdge)
                    DockWindows[DockState.DockBottom].SendToBack();
                else
                    DockWindows[DockState.DockBottom].BringToFront();
            }
        }

        [Browsable(false)]
        public int DocumentsCount
        {
            get
            {
                int count = 0;
                foreach (IDockContent content in Documents)
                    count++;

                return count;
            }
        }

        public IDockContent[] DocumentsToArray()
        {
            int count = DocumentsCount;
            IDockContent[] documents = new IDockContent[count];
            int i = 0;
            foreach (IDockContent content in Documents)
            {
                documents[i] = content;
                i++;
            }

            return documents;
        }

        [Browsable(false)]
        public IEnumerable<IDockContent> Documents
        {
            get
            {
                foreach (IDockContent content in Contents)
                {
                    if (content.DockHandler.DockState == DockState.Document)
                        yield return content;
                }
            }
        }

        private Control DummyControl
        {
            get { return _dummyControl; }
        }

        [Browsable(false)]
        public FloatWindowCollection FloatWindows
        {
            get { return _floatWindows; }
        }

        [Category("Layout")]
        [LocalizedDescription("DockPanel_DefaultFloatWindowSize_Description")]
        public Size DefaultFloatWindowSize { get; set; } = new Size(300, 300);

        private DocumentStyle _documentStyle = DocumentStyle.DockingWindow;
        [LocalizedCategory("Category_Docking")]
        [LocalizedDescription("DockPanel_DocumentStyle_Description")]
        [DefaultValue(DocumentStyle.DockingWindow)]
        public DocumentStyle DocumentStyle
        {
            get	{	return _documentStyle;	}
            set
            {
                if (value == _documentStyle)
                    return;

                if (!Enum.IsDefined(typeof(DocumentStyle), value))
                    throw new InvalidEnumArgumentException();

                if (value == DocumentStyle.SystemMdi && DockWindows[DockState.Document].VisibleNestedPanes.Count > 0)
                    throw new InvalidEnumArgumentException();

                _documentStyle = value;

                SuspendLayout(true);

                SetAutoHideWindowParent();
                SetMdiClient();
                InvalidateWindowRegion();

                foreach (IDockContent content in Contents)
                {
                    if (content.DockHandler.DockState == DockState.Document)
                        content.DockHandler.SetPaneAndVisible(content.DockHandler.Pane);
                }

                PerformMdiClientLayout();

                ResumeLayout(true, true);
            }
        }

        [LocalizedCategory("Category_Performance")]
        [LocalizedDescription("DockPanel_SupportDeeplyNestedContent_Description")]
        [DefaultValue(false)]
        public bool SupportDeeplyNestedContent { get; set; }

        /// <summary>
        /// Flag to show autohide content on mouse hover. Default value is <code>true</code>.
        /// </summary>
        /// <remarks>
        /// This flag is ignored in VS2012/2013 themes. Such themes assume it is always <code>false</code>.
        /// </remarks>
        [LocalizedCategory("Category_Docking")]
        [LocalizedDescription("DockPanel_ShowAutoHideContentOnHover_Description")]
        [DefaultValue(true)]
        public bool ShowAutoHideContentOnHover { get; set; }

        public int GetDockWindowSize(DockState dockState)
        {
            if (dockState == DockState.DockLeft || dockState == DockState.DockRight)
            {
                int width = ClientRectangle.Width - DockPadding.Left - DockPadding.Right;
                int dockLeftSize = _dockLeftPortion >= 1 ? (int)_dockLeftPortion : (int)(width * _dockLeftPortion);
                int dockRightSize = _dockRightPortion >= 1 ? (int)_dockRightPortion : (int)(width * _dockRightPortion);

                if (dockLeftSize < MeasurePane.MinSize)
                    dockLeftSize = MeasurePane.MinSize;
                if (dockRightSize < MeasurePane.MinSize)
                    dockRightSize = MeasurePane.MinSize;

                if (dockLeftSize + dockRightSize > width - MeasurePane.MinSize)
                {
                    int adjust = (dockLeftSize + dockRightSize) - (width - MeasurePane.MinSize);
                    dockLeftSize -= adjust / 2;
                    dockRightSize -= adjust / 2;
                }

                return dockState == DockState.DockLeft ? dockLeftSize : dockRightSize;
            }

            if (dockState == DockState.DockTop || dockState == DockState.DockBottom)
            {
                int height = ClientRectangle.Height - DockPadding.Top - DockPadding.Bottom;
                int dockTopSize = _dockTopPortion >= 1 ? (int)_dockTopPortion : (int)(height * _dockTopPortion);
                int dockBottomSize = _dockBottomPortion >= 1 ? (int)_dockBottomPortion : (int)(height * _dockBottomPortion);

                if (dockTopSize < MeasurePane.MinSize)
                    dockTopSize = MeasurePane.MinSize;
                if (dockBottomSize < MeasurePane.MinSize)
                    dockBottomSize = MeasurePane.MinSize;

                if (dockTopSize + dockBottomSize > height - MeasurePane.MinSize)
                {
                    int adjust = (dockTopSize + dockBottomSize) - (height - MeasurePane.MinSize);
                    dockTopSize -= adjust / 2;
                    dockBottomSize -= adjust / 2;
                }

                return dockState == DockState.DockTop ? dockTopSize : dockBottomSize;
            }

            return 0;
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            SuspendLayout(true);

            AutoHideStripControl.Bounds = ClientRectangle;

            CalculateDockPadding();

            DockWindows[DockState.DockLeft].Width = GetDockWindowSize(DockState.DockLeft);
            DockWindows[DockState.DockRight].Width = GetDockWindowSize(DockState.DockRight);
            DockWindows[DockState.DockTop].Height = GetDockWindowSize(DockState.DockTop);
            DockWindows[DockState.DockBottom].Height = GetDockWindowSize(DockState.DockBottom);

            AutoHideWindow.Bounds = GetAutoHideWindowBounds(AutoHideWindowRectangle);

            DockWindow documentDockWindow = DockWindows[DockState.Document];

            if (ReferenceEquals(documentDockWindow.Parent, AutoHideWindow.Parent))
            {
                AutoHideWindow.Parent.Controls.SetChildIndex(AutoHideWindow, 0);
                documentDockWindow.Parent.Controls.SetChildIndex(documentDockWindow, 1);
            }
            else
            {
                documentDockWindow.BringToFront();
                AutoHideWindow.BringToFront();
            }

            base.OnLayout(levent);

            if (DocumentStyle == DocumentStyle.SystemMdi && MdiClientExists)
            {
                SetMdiClientBounds(SystemMdiClientBounds);
                InvalidateWindowRegion();
            }
            else if (DocumentStyle == DocumentStyle.DockingMdi)
            {
                InvalidateWindowRegion();
            }

            ResumeLayout(true, true);
        }

        internal Rectangle GetTabStripRectangle(DockState dockState)
        {
            return AutoHideStripControl.GetTabStripRectangle(dockState);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (DockBackColor.ToArgb() == BackColor.ToArgb())
                return;

            using Graphics g = e.Graphics;
            SolidBrush bgBrush = new(DockBackColor);
            g.FillRectangle(bgBrush, ClientRectangle);
        }

        internal void AddContent(IDockContent content)
        {
            if (content == null)
                throw new ArgumentNullException(nameof(content));

            if (!Contents.Contains(content))
            {
                Contents.Add(content);
                OnContentAdded(new DockContentEventArgs(content));
            }
        }

        internal void AddPane(DockPane pane)
        {
            if (Panes.Contains(pane))
                return;

            Panes.Add(pane);
        }

        internal void AddFloatWindow(FloatWindow floatWindow)
        {
            if (FloatWindows.Contains(floatWindow))
                return;

            FloatWindows.Add(floatWindow);
        }

        private void CalculateDockPadding()
        {
            DockPadding.All = Theme.Measures.DockPadding;
            int standard = AutoHideStripControl.MeasureHeight();
            if (AutoHideStripControl.GetNumberOfPanes(DockState.DockLeftAutoHide) > 0)
                DockPadding.Left = standard;
            if (AutoHideStripControl.GetNumberOfPanes(DockState.DockRightAutoHide) > 0)
                DockPadding.Right = standard;
            if (AutoHideStripControl.GetNumberOfPanes(DockState.DockTopAutoHide) > 0)
                DockPadding.Top = standard;
            if (AutoHideStripControl.GetNumberOfPanes(DockState.DockBottomAutoHide) > 0)
                DockPadding.Bottom = standard;
        }

        internal void RemoveContent(IDockContent content)
        {
            if (content == null)
                throw new ArgumentNullException(nameof(content));
            
            if (Contents.Contains(content))
            {
                Contents.Remove(content);
                OnContentRemoved(new DockContentEventArgs(content));
            }
        }

        internal void RemovePane(DockPane pane)
        {
            if (!Panes.Contains(pane))
                return;

            Panes.Remove(pane);
        }

        internal void RemoveFloatWindow(FloatWindow floatWindow)
        {
            if (!FloatWindows.Contains(floatWindow))
                return;

            FloatWindows.Remove(floatWindow);
            if (FloatWindows.Count != 0)
                return;

            if (ParentForm == null) 
                return;

            ParentForm.Focus();
        }

        public void SetPaneIndex(DockPane pane, int index)
        {
            int oldIndex = Panes.IndexOf(pane);
            if (oldIndex == -1)
                throw(new ArgumentException(Strings.DockPanel_SetPaneIndex_InvalidPane));

            if ((index < 0 || index > Panes.Count - 1) && index != -1)
                throw (new ArgumentOutOfRangeException(Strings.DockPanel_SetPaneIndex_InvalidIndex));

            if (oldIndex == index)
                return;
            if (oldIndex == Panes.Count - 1 && index == -1)
                return;

            Panes.Remove(pane);
            if (index == -1)
                Panes.Add(pane);
            else if (oldIndex < index)
                Panes.AddAt(pane, index - 1);
            else
                Panes.AddAt(pane, index);
        }

        public void SuspendLayout(bool allWindows)
        {
            FocusManager.SuspendFocusTracking();
            SuspendLayout();
            if (allWindows)
                SuspendMdiClientLayout();
        }

        public void ResumeLayout(bool performLayout, bool allWindows)
        {
            FocusManager.ResumeFocusTracking();
            ResumeLayout(performLayout);
            if (allWindows)
                ResumeMdiClientLayout(performLayout);
        }

        internal Form ParentForm
        {
            get
            {
                if (!IsParentFormValid())
                    throw new InvalidOperationException(Strings.DockPanel_ParentForm_Invalid);

                return GetMdiClientController().ParentForm;
            }
        }

        private bool IsParentFormValid()
        {
            if (DocumentStyle == DocumentStyle.DockingSdi || DocumentStyle == DocumentStyle.DockingWindow)
                return true;

            if (!MdiClientExists)
                GetMdiClientController().RenewMdiClient();

            return (MdiClientExists);
        }

        protected override void OnParentChanged(EventArgs e)
        {
            SetAutoHideWindowParent();
            GetMdiClientController().ParentForm = (this.Parent as Form);
            base.OnParentChanged (e);
        }

        private void SetAutoHideWindowParent()
        {
            Control parent;
            if (DocumentStyle == DocumentStyle.DockingMdi ||
                DocumentStyle == DocumentStyle.SystemMdi)
                parent = this.Parent;
            else
                parent = this;
            if (AutoHideWindow.Parent != parent)
            {
                AutoHideWindow.Parent = parent;
                AutoHideWindow.BringToFront();
            }
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged (e);

            if (Visible)
                SetMdiClient();
        }

        private Rectangle SystemMdiClientBounds
        {
            get
            {
                if (!IsParentFormValid() || !Visible)
                    return Rectangle.Empty;

                Rectangle rect = ParentForm.RectangleToClient(RectangleToScreen(DocumentWindowBounds));
                return rect;
            }
        }

        public Rectangle DocumentWindowBounds
        {
            get
            {
                Rectangle rectDocumentBounds = DisplayRectangle;
                if (DockWindows[DockState.DockLeft].Visible)
                {
                    rectDocumentBounds.X += DockWindows[DockState.DockLeft].Width;
                    rectDocumentBounds.Width -= DockWindows[DockState.DockLeft].Width;
                }
                if (DockWindows[DockState.DockRight].Visible)
                    rectDocumentBounds.Width -= DockWindows[DockState.DockRight].Width;
                if (DockWindows[DockState.DockTop].Visible)
                {
                    rectDocumentBounds.Y += DockWindows[DockState.DockTop].Height;
                    rectDocumentBounds.Height -= DockWindows[DockState.DockTop].Height;
                }
                if (DockWindows[DockState.DockBottom].Visible)
                    rectDocumentBounds.Height -= DockWindows[DockState.DockBottom].Height;

                return rectDocumentBounds;

            }
        }

        private PaintEventHandler _dummyControlPaintEventHandler = null;
        private void InvalidateWindowRegion()
        {
            if (DesignMode)
                return;

            _dummyControlPaintEventHandler ??= new PaintEventHandler(DummyControl_Paint);

            DummyControl.Paint += _dummyControlPaintEventHandler;
            DummyControl.Invalidate();
        }

        void DummyControl_Paint(object sender, PaintEventArgs e)
        {
            DummyControl.Paint -= _dummyControlPaintEventHandler;
            UpdateWindowRegion();
        }

        private void UpdateWindowRegion()
        {
            if (this.DocumentStyle == DocumentStyle.DockingMdi)
                UpdateWindowRegion_ClipContent();
            else if (this.DocumentStyle == DocumentStyle.DockingSdi ||
                this.DocumentStyle == DocumentStyle.DockingWindow)
                UpdateWindowRegion_FullDocumentArea();
            else if (this.DocumentStyle == DocumentStyle.SystemMdi)
                UpdateWindowRegion_EmptyDocumentArea();
        }

        private void UpdateWindowRegion_FullDocumentArea()
        {
            SetRegion(null);
        }

        private void UpdateWindowRegion_EmptyDocumentArea()
        {
            Rectangle rect = DocumentWindowBounds;
            SetRegion(new Rectangle[] { rect });
        }

        private void UpdateWindowRegion_ClipContent()
        {
            int count = 0;
            foreach (DockPane pane in this.Panes)
            {
                if (!pane.Visible || pane.DockState != DockState.Document)
                    continue;

                count ++;
            }

            if (count == 0)
            {
                SetRegion(null);
                return;
            }

            Rectangle[] rects = new Rectangle[count];
            int i = 0;
            foreach (DockPane pane in this.Panes)
            {
                if (!pane.Visible || pane.DockState != DockState.Document)
                    continue;

                rects[i] = RectangleToClient(pane.RectangleToScreen(pane.ContentRectangle));
                i++;
            }

            SetRegion(rects);
        }

        private Rectangle[] _clipRects = null;
        private void SetRegion(Rectangle[] clipRects)
        {
            if (!IsClipRectsChanged(clipRects))
                return;

            _clipRects = clipRects;

            if (_clipRects == null || _clipRects.GetLength(0) == 0)
                Region = null;
            else
            {
                Region region = new(new Rectangle(0, 0, this.Width, this.Height));
                foreach (Rectangle rect in _clipRects)
                    region.Exclude(rect);
                Region?.Dispose();
                Region = region;
            }
        }

        private bool IsClipRectsChanged(Rectangle[] clipRects)
        {
            if (clipRects == null && _clipRects == null)
                return false;
            else if ((clipRects == null) != (_clipRects == null))
                return true;

            foreach (Rectangle rect in clipRects)
            {
                bool matched = false;
                foreach (Rectangle rect2 in _clipRects)
                {
                    if (rect == rect2)
                    {
                        matched = true;
                        break;
                    }
                }
                if (!matched)
                    return true;
            }

            foreach (Rectangle rect2 in _clipRects)
            {
                bool matched = false;
                foreach (Rectangle rect in clipRects)
                {
                    if (rect == rect2)
                    {
                        matched = true;
                        break;
                    }
                }
                if (!matched)
                    return true;
            }
            return false;
        }

        private static readonly object ActiveAutoHideContentChangedEvent = new();
        [LocalizedCategory("Category_DockingNotification")]
        [LocalizedDescription("DockPanel_ActiveAutoHideContentChanged_Description")]
        public event EventHandler ActiveAutoHideContentChanged
        {
            add { Events.AddHandler(ActiveAutoHideContentChangedEvent, value); }
            remove { Events.RemoveHandler(ActiveAutoHideContentChangedEvent, value); }
        }
        protected virtual void OnActiveAutoHideContentChanged(EventArgs e)
        {
            ((EventHandler)Events[ActiveAutoHideContentChangedEvent])?.Invoke(this, e);
        }
        private void AutoHideWindow_ActiveContentChanged(object sender, EventArgs e)
        {
            OnActiveAutoHideContentChanged(e);
        }


        private static readonly object ContentAddedEvent = new();
        [LocalizedCategory("Category_DockingNotification")]
        [LocalizedDescription("DockPanel_ContentAdded_Description")]
        public event EventHandler<DockContentEventArgs> ContentAdded
        {
            add	{	Events.AddHandler(ContentAddedEvent, value);	}
            remove	{	Events.RemoveHandler(ContentAddedEvent, value);	}
        }
        protected virtual void OnContentAdded(DockContentEventArgs e)
        {
            ((EventHandler<DockContentEventArgs>)Events[ContentAddedEvent])?.Invoke(this, e);
        }

        private static readonly object ContentRemovedEvent = new();
        [LocalizedCategory("Category_DockingNotification")]
        [LocalizedDescription("DockPanel_ContentRemoved_Description")]
        public event EventHandler<DockContentEventArgs> ContentRemoved
        {
            add	{	Events.AddHandler(ContentRemovedEvent, value);	}
            remove	{	Events.RemoveHandler(ContentRemovedEvent, value);	}
        }
        protected virtual void OnContentRemoved(DockContentEventArgs e)
        {
            ((EventHandler<DockContentEventArgs>)Events[ContentRemovedEvent])?.Invoke(this, e);
        }

        internal void ResetDockWindows()
        {
            if (_autoHideWindow == null)
            {
                return;
            }

            var old = _dockWindows;
            LoadDockWindows();
            foreach (var dockWindow in old)
            {
                Controls.Remove(dockWindow);
                dockWindow.Dispose();
            }
        }

        internal void LoadDockWindows()
        {
            _dockWindows = new DockWindowCollection(this);
            foreach (var dockWindow in DockWindows)
            {
                Controls.Add(dockWindow);
            }
        }

        public void ResetAutoHideStripWindow()
        {
            var old = _autoHideWindow;
            _autoHideWindow = Theme.Extender.AutoHideWindowFactory.CreateAutoHideWindow(this);
            _autoHideWindow.Visible = false;
            SetAutoHideWindowParent();

            old.Visible = false;
            old.Parent = null;
            old.Dispose();
        }
    }
}
