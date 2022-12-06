using System;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;

namespace WeifenLuo.WinFormsUI.Docking
{
    partial class DockPanel
    {
        [ToolboxItem(false)]
        public class AutoHideWindowControl : Panel, ISplitterHost
        {
            protected class SplitterControl : SplitterBase
            {
                public SplitterControl(AutoHideWindowControl autoHideWindow)
                {
                    _autoHideWindow = autoHideWindow;
                }

                private readonly AutoHideWindowControl _autoHideWindow;
                private AutoHideWindowControl AutoHideWindow
                {
                    get { return _autoHideWindow; }
                }

                protected override int SplitterSize
                {
                    get { return AutoHideWindow.DockPanel.Theme.Measures.AutoHideSplitterSize; }
                }

                protected override void StartDrag()
                {
                    AutoHideWindow.DockPanel.BeginDrag(AutoHideWindow, AutoHideWindow.RectangleToScreen(Bounds));
                }
            }

            #region consts
            private const int ANIMATE_TIME = 100;    // in mini-seconds
            #endregion

            private readonly Timer _timerMouseTrack;
            protected SplitterBase Splitter { get; private set; }

            public AutoHideWindowControl(DockPanel dockPanel)
            {
                _dockPanel = dockPanel;

                _timerMouseTrack = new Timer();
                _timerMouseTrack.Tick += new EventHandler(TimerMouseTrack_Tick);

                Visible = false;
                Splitter = DockPanel.Theme.Extender.WindowSplitterControlFactory.CreateSplitterControl(this);
                Controls.Add(Splitter);
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    _timerMouseTrack.Dispose();
                }
                base.Dispose(disposing);
            }

            public bool IsDockWindow
            {
                get { return false; }
            }

            private readonly DockPanel _dockPanel = null;
            public DockPanel DockPanel
            {
                get { return _dockPanel; }
            }

            private DockPane _activePane = null;
            public DockPane ActivePane
            {
                get { return _activePane; }
            }

            private void SetActivePane()
            {
                DockPane value = (ActiveContent?.DockHandler.Pane);

                if (value == _activePane)
                    return;

                _activePane = value;
            }

            private static readonly object AutoHideActiveContentChangedEvent = new();
            public event EventHandler ActiveContentChanged
            {
                add { Events.AddHandler(AutoHideActiveContentChangedEvent, value); }
                remove { Events.RemoveHandler(AutoHideActiveContentChangedEvent, value); }
            }

            protected virtual void OnActiveContentChanged(EventArgs e)
            {
                ((EventHandler)Events[ActiveContentChangedEvent])?.Invoke(this, e);
            }

            private IDockContent _activeContent = null;
            public IDockContent ActiveContent
            {
                get { return _activeContent; }
                set
                {
                    if (value == _activeContent)
                        return;

                    if (value != null && (!DockHelper.IsDockStateAutoHide(value.DockHandler.DockState) || value.DockHandler.DockPanel != DockPanel))
                        throw new InvalidOperationException(Strings.DockPanel_ActiveAutoHideContent_InvalidValue);

                    DockPanel.SuspendLayout();

                    if (_activeContent != null)
                    {
                        if (_activeContent.DockHandler.Form.ContainsFocus && !Win32Helper.IsRunningOnMono)
                        {
                            DockPanel.ContentFocusManager.GiveUpFocus(_activeContent);
                        }

                        AnimateWindow(false);
                    }

                    _activeContent = value;
                    SetActivePane();
                    if (ActivePane != null)
                        ActivePane.ActiveContent = _activeContent;

                    if (_activeContent != null)
                        AnimateWindow(true);

                    DockPanel.ResumeLayout();
                    DockPanel.RefreshAutoHideStrip();

                    SetTimerMouseTrack();

                    OnActiveContentChanged(EventArgs.Empty);
                }
            }

            public DockState DockState
            {
                get { return ActiveContent == null ? DockState.Unknown : ActiveContent.DockHandler.DockState; }
            }

            private bool FlagAnimate { get; set; } = true;

            private bool _flagDragging = false;
            internal bool FlagDragging
            {
                get { return _flagDragging; }
                set
                {
                    if (_flagDragging == value)
                        return;

                    _flagDragging = value;
                    SetTimerMouseTrack();
                }
            }

            private void AnimateWindow(bool show)
            {
                if (!FlagAnimate && Visible != show)
                {
                    Visible = show;
                    return;
                }

                Parent.SuspendLayout();

                Rectangle rectSource = GetRectangle(!show);
                Rectangle rectTarget = GetRectangle(show);
                int dxLoc, dyLoc;
                int dWidth, dHeight;
                dxLoc = dyLoc = dWidth = dHeight = 0;
                if (DockState == DockState.DockTopAutoHide)
                    dHeight = show ? 1 : -1;
                else if (DockState == DockState.DockLeftAutoHide)
                    dWidth = show ? 1 : -1;
                else if (DockState == DockState.DockRightAutoHide)
                {
                    dxLoc = show ? -1 : 1;
                    dWidth = show ? 1 : -1;
                }
                else if (DockState == DockState.DockBottomAutoHide)
                {
                    dyLoc = (show ? -1 : 1);
                    dHeight = (show ? 1 : -1);
                }

                if (show)
                {
                    Bounds = DockPanel.GetAutoHideWindowBounds(new Rectangle(-rectTarget.Width, -rectTarget.Height, rectTarget.Width, rectTarget.Height));
                    if (!Visible)
                        Visible = true;
                    PerformLayout();
                }

                SuspendLayout();

                LayoutAnimateWindow(rectSource);
                if (!Visible)
                    Visible = true;

                int speedFactor = 1;
                int totalPixels = (rectSource.Width != rectTarget.Width) ?
                    Math.Abs(rectSource.Width - rectTarget.Width) :
                    Math.Abs(rectSource.Height - rectTarget.Height);
                int remainPixels = totalPixels;
                DateTime startingTime = DateTime.Now;
                while (rectSource != rectTarget)
                {
                    DateTime startPerMove = DateTime.Now;

                    rectSource.X += dxLoc * speedFactor;
                    rectSource.Y += dyLoc * speedFactor;
                    rectSource.Width += dWidth * speedFactor;
                    rectSource.Height += dHeight * speedFactor;
                    if (Math.Sign(rectTarget.X - rectSource.X) != Math.Sign(dxLoc))
                        rectSource.X = rectTarget.X;
                    if (Math.Sign(rectTarget.Y - rectSource.Y) != Math.Sign(dyLoc))
                        rectSource.Y = rectTarget.Y;
                    if (Math.Sign(rectTarget.Width - rectSource.Width) != Math.Sign(dWidth))
                        rectSource.Width = rectTarget.Width;
                    if (Math.Sign(rectTarget.Height - rectSource.Height) != Math.Sign(dHeight))
                        rectSource.Height = rectTarget.Height;

                    LayoutAnimateWindow(rectSource);
                    Parent?.Update();

                    remainPixels -= speedFactor;

                    while (true)
                    {
                        TimeSpan time = new(0, 0, 0, 0, ANIMATE_TIME);
                        TimeSpan elapsedPerMove = DateTime.Now - startPerMove;
                        TimeSpan elapsedTime = DateTime.Now - startingTime;
                        if (((int)((time - elapsedTime).TotalMilliseconds)) <= 0)
                        {
                            speedFactor = remainPixels;
                            break;
                        }
                        else
                            speedFactor = remainPixels * (int)elapsedPerMove.TotalMilliseconds / (int)((time - elapsedTime).TotalMilliseconds);
                        if (speedFactor >= 1)
                            break;
                    }
                }
                ResumeLayout();
                Parent.ResumeLayout();
            }

            private void LayoutAnimateWindow(Rectangle rect)
            {
                Bounds = DockPanel.GetAutoHideWindowBounds(rect);

                Rectangle rectClient = ClientRectangle;

                if (DockState == DockState.DockLeftAutoHide)
                    ActivePane.Location = new Point(rectClient.Right - 2 - DockPanel.Theme.Measures.AutoHideSplitterSize - ActivePane.Width, ActivePane.Location.Y);
                else if (DockState == DockState.DockTopAutoHide)
                    ActivePane.Location = new Point(ActivePane.Location.X, rectClient.Bottom - 2 - DockPanel.Theme.Measures.AutoHideSplitterSize - ActivePane.Height);
            }

            private Rectangle GetRectangle(bool show)
            {
                if (DockState == DockState.Unknown)
                    return Rectangle.Empty;

                Rectangle rect = DockPanel.AutoHideWindowRectangle;

                if (show)
                    return rect;

                if (DockState == DockState.DockLeftAutoHide)
                    rect.Width = 0;
                else if (DockState == DockState.DockRightAutoHide)
                {
                    rect.X += rect.Width;
                    rect.Width = 0;
                }
                else if (DockState == DockState.DockTopAutoHide)
                    rect.Height = 0;
                else
                {
                    rect.Y += rect.Height;
                    rect.Height = 0;
                }

                return rect;
            }

            private void SetTimerMouseTrack()
            {
                if (ActivePane == null || ActivePane.IsActivated || FlagDragging)
                {
                    _timerMouseTrack.Enabled = false;
                    return;
                }

                // start the timer
                int hovertime = SystemInformation.MouseHoverTime ;

                // assign a default value 400 in case of setting Timer.Interval invalid value exception
                if (hovertime <= 0)
                    hovertime = 400;

                _timerMouseTrack.Interval = 2 * hovertime;
                _timerMouseTrack.Enabled = true;
            }

            protected virtual Rectangle DisplayingRectangle
            {
                get
                {
                    Rectangle rect = ClientRectangle;

                    // exclude the border and the splitter
                    if (DockState == DockState.DockBottomAutoHide)
                    {
                        rect.Y += 2 + DockPanel.Theme.Measures.AutoHideSplitterSize;
                        rect.Height -= 2 + DockPanel.Theme.Measures.AutoHideSplitterSize;
                    }
                    else if (DockState == DockState.DockRightAutoHide)
                    {
                        rect.X += 2 + DockPanel.Theme.Measures.AutoHideSplitterSize;
                        rect.Width -= 2 + DockPanel.Theme.Measures.AutoHideSplitterSize;
                    }
                    else if (DockState == DockState.DockTopAutoHide)
                        rect.Height -= 2 + DockPanel.Theme.Measures.AutoHideSplitterSize;
                    else if (DockState == DockState.DockLeftAutoHide)
                        rect.Width -= 2 + DockPanel.Theme.Measures.AutoHideSplitterSize;

                    return rect;
                }
            }

            public void RefreshActiveContent()
            {
                if (ActiveContent == null)
                    return;

                if (!DockHelper.IsDockStateAutoHide(ActiveContent.DockHandler.DockState))
                {
                    FlagAnimate = false;
                    ActiveContent = null;
                    FlagAnimate = true;
                }
            }

            public void RefreshActivePane()
            {
                SetTimerMouseTrack();
            }

            private void TimerMouseTrack_Tick(object sender, EventArgs e)
            {
                if (IsDisposed)
                    return;

                if (ActivePane == null || ActivePane.IsActivated)
                {
                    _timerMouseTrack.Enabled = false;
                    return;
                }

                DockPane pane = ActivePane;
                Point ptMouseInAutoHideWindow = PointToClient(Control.MousePosition);
                Point ptMouseInDockPanel = DockPanel.PointToClient(Control.MousePosition);

                Rectangle rectTabStrip = DockPanel.GetTabStripRectangle(pane.DockState);

                if (!ClientRectangle.Contains(ptMouseInAutoHideWindow) && !rectTabStrip.Contains(ptMouseInDockPanel))
                {
                    ActiveContent = null;
                    _timerMouseTrack.Enabled = false;
                }
            }

            #region ISplitterDragSource Members

            void ISplitterDragSource.BeginDrag(Rectangle rectSplitter)
            {
                FlagDragging = true;
            }

            void ISplitterDragSource.EndDrag()
            {
                FlagDragging = false;
            }

            bool ISplitterDragSource.IsVertical
            {
                get { return (DockState == DockState.DockLeftAutoHide || DockState == DockState.DockRightAutoHide); }
            }

            Rectangle ISplitterDragSource.DragLimitBounds
            {
                get
                {
                    Rectangle rectLimit = DockPanel.DockArea;

                    if ((this as ISplitterDragSource).IsVertical)
                    {
                        rectLimit.X += MeasurePane.MinSize;
                        rectLimit.Width -= 2 * MeasurePane.MinSize;
                    }
                    else
                    {
                        rectLimit.Y += MeasurePane.MinSize;
                        rectLimit.Height -= 2 * MeasurePane.MinSize;
                    }

                    return DockPanel.RectangleToScreen(rectLimit);
                }
            }

            void ISplitterDragSource.MoveSplitter(int offset)
            {
                Rectangle rectDockArea = DockPanel.DockArea;
                IDockContent content = ActiveContent;
                if (DockState == DockState.DockLeftAutoHide && rectDockArea.Width > 0)
                {
                    if (content.DockHandler.AutoHidePortion < 1)
                        content.DockHandler.AutoHidePortion += ((double)offset) / (double)rectDockArea.Width;
                    else
                        content.DockHandler.AutoHidePortion = Width + offset;
                }
                else if (DockState == DockState.DockRightAutoHide && rectDockArea.Width > 0)
                {
                    if (content.DockHandler.AutoHidePortion < 1)
                        content.DockHandler.AutoHidePortion -= ((double)offset) / (double)rectDockArea.Width;
                    else
                        content.DockHandler.AutoHidePortion = Width - offset;
                }
                else if (DockState == DockState.DockBottomAutoHide && rectDockArea.Height > 0)
                {
                    if (content.DockHandler.AutoHidePortion < 1)
                        content.DockHandler.AutoHidePortion -= ((double)offset) / (double)rectDockArea.Height;
                    else
                        content.DockHandler.AutoHidePortion = Height - offset;
                }
                else if (DockState == DockState.DockTopAutoHide && rectDockArea.Height > 0)
                {
                    if (content.DockHandler.AutoHidePortion < 1)
                        content.DockHandler.AutoHidePortion += ((double)offset) / (double)rectDockArea.Height;
                    else
                        content.DockHandler.AutoHidePortion = Height + offset;
                }
            }

            #region IDragSource Members

            Control IDragSource.DragControl
            {
                get { return this; }
            }

            #endregion

            #endregion
        }

        private AutoHideWindowControl AutoHideWindow
        {
            get { return _autoHideWindow; }
        }

        internal Control AutoHideControl
        {
            get { return _autoHideWindow; }
        }

        internal void RefreshActiveAutoHideContent()
        {
            AutoHideWindow.RefreshActiveContent();
        }

        internal Rectangle AutoHideWindowRectangle
        {
            get
            {
                DockState state = AutoHideWindow.DockState;
                Rectangle rectDockArea = DockArea;
                if (ActiveAutoHideContent == null)
                    return Rectangle.Empty;

                if (Parent == null)
                    return Rectangle.Empty;

                Rectangle rect = Rectangle.Empty;
                double autoHideSize = ActiveAutoHideContent.DockHandler.AutoHidePortion;
                if (state == DockState.DockLeftAutoHide)
                {
                    if (autoHideSize < 1)
                        autoHideSize = rectDockArea.Width * autoHideSize;
                    if (autoHideSize > rectDockArea.Width - MeasurePane.MinSize)
                        autoHideSize = rectDockArea.Width - MeasurePane.MinSize;
                    rect.X = rectDockArea.X - Theme.Measures.DockPadding;
                    rect.Y = rectDockArea.Y;
                    rect.Width = (int)autoHideSize;
                    rect.Height = rectDockArea.Height;
                }
                else if (state == DockState.DockRightAutoHide)
                {
                    if (autoHideSize < 1)
                        autoHideSize = rectDockArea.Width * autoHideSize;
                    if (autoHideSize > rectDockArea.Width - MeasurePane.MinSize)
                        autoHideSize = rectDockArea.Width - MeasurePane.MinSize;
                    rect.X = rectDockArea.X + rectDockArea.Width - (int)autoHideSize + Theme.Measures.DockPadding;
                    rect.Y = rectDockArea.Y;
                    rect.Width = (int)autoHideSize;
                    rect.Height = rectDockArea.Height;
                }
                else if (state == DockState.DockTopAutoHide)
                {
                    if (autoHideSize < 1)
                        autoHideSize = rectDockArea.Height * autoHideSize;
                    if (autoHideSize > rectDockArea.Height - MeasurePane.MinSize)
                        autoHideSize = rectDockArea.Height - MeasurePane.MinSize;
                    rect.X = rectDockArea.X;
                    rect.Y = rectDockArea.Y - Theme.Measures.DockPadding;
                    rect.Width = rectDockArea.Width;
                    rect.Height = (int)autoHideSize;
                }
                else if (state == DockState.DockBottomAutoHide)
                {
                    if (autoHideSize < 1)
                        autoHideSize = rectDockArea.Height * autoHideSize;
                    if (autoHideSize > rectDockArea.Height - MeasurePane.MinSize)
                        autoHideSize = rectDockArea.Height - MeasurePane.MinSize;
                    rect.X = rectDockArea.X;
                    rect.Y = rectDockArea.Y + rectDockArea.Height - (int)autoHideSize + Theme.Measures.DockPadding;
                    rect.Width = rectDockArea.Width;
                    rect.Height = (int)autoHideSize;
                }

                return rect;
            }
        }

        internal Rectangle GetAutoHideWindowBounds(Rectangle rectAutoHideWindow)
        {
            if (DocumentStyle == DocumentStyle.SystemMdi ||
                DocumentStyle == DocumentStyle.DockingMdi)
                return (Parent == null) ? Rectangle.Empty : Parent.RectangleToClient(RectangleToScreen(rectAutoHideWindow));
            else
                return rectAutoHideWindow;
        }

        internal void RefreshAutoHideStrip()
        {
            AutoHideStripControl.RefreshChanges();
        }
    }
}
