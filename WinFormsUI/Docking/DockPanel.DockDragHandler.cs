using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace WeifenLuo.WinFormsUI.Docking
{
    partial class DockPanel
    {
        #region IHitTest
        public interface IHitTest
        {
            DockStyle HitTest(Point pt);
            DockStyle Status { get; set; }
        }

        public interface IPaneIndicator : IHitTest
        {
            Point Location { get; set; }
            bool Visible { get; set; }
            int Left { get; }
            int Top { get; }
            int Right { get; }
            int Bottom { get; }
            Rectangle ClientRectangle { get; }
            int Width { get; }
            int Height { get; }
            GraphicsPath DisplayingGraphicsPath { get; }
        }

        public interface IPanelIndicator : IHitTest
        {
            Point Location { get; set; }
            bool Visible { get; set; }
            Rectangle Bounds { get; }
            int Width { get; }
            int Height { get; }
        }

        public readonly struct HotSpotIndex
        {
            public HotSpotIndex(int x, int y, DockStyle dockStyle)
            {
                _x = x;
                _y = y;
                _dockStyle = dockStyle;
            }

            private readonly int _x;
            public int X
            {
                get { return _x; }
            }

            private readonly int _y;
            public int Y
            {
                get { return _y; }
            }

            private readonly DockStyle _dockStyle;
            public DockStyle DockStyle
            {
                get { return _dockStyle; }
            }
        }

        #endregion

        public sealed class DockDragHandler : DragHandler
        {
            public class DockIndicator : DragForm
            {
                #region consts
                private readonly int _PanelIndicatorMargin = 10;
                #endregion

                private readonly DockDragHandler _dragHandler;

                public DockIndicator(DockDragHandler dragHandler)
                {
                    _dragHandler = dragHandler;
                    Controls.AddRange(new[] {
                        (Control)PaneDiamond,
                        (Control)PanelLeft,
                        (Control)PanelRight,
                        (Control)PanelTop,
                        (Control)PanelBottom,
                        (Control)PanelFill
                        });
                    Region = new Region(Rectangle.Empty);
                }

                private IPaneIndicator _paneDiamond = null;
                private IPaneIndicator PaneDiamond
                {
                    get
                    {
                        _paneDiamond ??= _dragHandler.DockPanel.Theme.Extender.PaneIndicatorFactory.CreatePaneIndicator(_dragHandler.DockPanel.Theme);
                        return _paneDiamond;
                    }
                }

                private IPanelIndicator _panelLeft = null;
                private IPanelIndicator PanelLeft
                {
                    get
                    {
                        _panelLeft ??= _dragHandler.DockPanel.Theme.Extender.PanelIndicatorFactory.CreatePanelIndicator(DockStyle.Left, _dragHandler.DockPanel.Theme);
                        return _panelLeft;
                    }
                }

                private IPanelIndicator _panelRight = null;
                private IPanelIndicator PanelRight
                {
                    get
                    {
                        _panelRight ??= _dragHandler.DockPanel.Theme.Extender.PanelIndicatorFactory.CreatePanelIndicator(DockStyle.Right, _dragHandler.DockPanel.Theme);
                        return _panelRight;
                    }
                }

                private IPanelIndicator _panelTop = null;
                private IPanelIndicator PanelTop
                {
                    get
                    {
                        _panelTop ??= _dragHandler.DockPanel.Theme.Extender.PanelIndicatorFactory.CreatePanelIndicator(DockStyle.Top, _dragHandler.DockPanel.Theme);
                        return _panelTop;
                    }
                }

                private IPanelIndicator _panelBottom = null;
                private IPanelIndicator PanelBottom
                {
                    get
                    {
                        _panelBottom ??= _dragHandler.DockPanel.Theme.Extender.PanelIndicatorFactory.CreatePanelIndicator(DockStyle.Bottom, _dragHandler.DockPanel.Theme);
                        return _panelBottom;
                    }
                }

                private IPanelIndicator _panelFill = null;
                private IPanelIndicator PanelFill
                {
                    get
                    {
                        _panelFill ??= _dragHandler.DockPanel.Theme.Extender.PanelIndicatorFactory.CreatePanelIndicator(DockStyle.Fill, _dragHandler.DockPanel.Theme);
                        return _panelFill;
                    }
                }

                private bool _fullPanelEdge = false;
                public bool FullPanelEdge
                {
                    get { return _fullPanelEdge; }
                    set
                    {
                        if (_fullPanelEdge == value)
                            return;

                        _fullPanelEdge = value;
                        RefreshChanges();
                    }
                }

                public DockDragHandler DragHandler
                {
                    get { return _dragHandler; }
                }

                public DockPanel DockPanel
                {
                    get { return DragHandler.DockPanel; }
                }

                private DockPane _dockPane = null;
                public DockPane DockPane
                {
                    get { return _dockPane; }
                    internal set
                    {
                        if (_dockPane == value)
                            return;

                        DockPane oldDisplayingPane = DisplayingPane;
                        _dockPane = value;
                        if (oldDisplayingPane != DisplayingPane)
                            RefreshChanges();
                    }
                }

                private IHitTest _hitTest = null;
                private IHitTest HitTestResult
                {
                    get { return _hitTest; }
                    set
                    {
                        if (_hitTest == value)
                            return;

                        if (_hitTest != null)
                            _hitTest.Status = DockStyle.None;

                        _hitTest = value;
                    }
                }

                private DockPane DisplayingPane
                {
                    get { return ShouldPaneDiamondVisible() ? DockPane : null; }
                }

                private void RefreshChanges()
                {
                    if (PatchController.EnablePerScreenDpi == true)
                    {
                        var allScreens = Screen.AllScreens;
                        var mousePos = Control.MousePosition;
                        foreach (var screen in allScreens)
                        {
                            if (screen.Bounds.Contains(mousePos))
                            {
                                Bounds = screen.Bounds;
                            }
                        }
                    }

                    Region region = new(Rectangle.Empty);
                    Rectangle rectDockArea = FullPanelEdge ? DockPanel.DockArea : DockPanel.DocumentWindowBounds;

                    rectDockArea = RectangleToClient(DockPanel.RectangleToScreen(rectDockArea));
                    if (ShouldPanelIndicatorVisible(DockState.DockLeft))
                    {
                        PanelLeft.Location = new Point(rectDockArea.X + _PanelIndicatorMargin, rectDockArea.Y + (rectDockArea.Height - PanelRight.Height) / 2);
                        PanelLeft.Visible = true;
                        region.Union(PanelLeft.Bounds);
                    }
                    else
                        PanelLeft.Visible = false;

                    if (ShouldPanelIndicatorVisible(DockState.DockRight))
                    {
                        PanelRight.Location = new Point(rectDockArea.X + rectDockArea.Width - PanelRight.Width - _PanelIndicatorMargin, rectDockArea.Y + (rectDockArea.Height - PanelRight.Height) / 2);
                        PanelRight.Visible = true;
                        region.Union(PanelRight.Bounds);
                    }
                    else
                        PanelRight.Visible = false;

                    if (ShouldPanelIndicatorVisible(DockState.DockTop))
                    {
                        PanelTop.Location = new Point(rectDockArea.X + (rectDockArea.Width - PanelTop.Width) / 2, rectDockArea.Y + _PanelIndicatorMargin);
                        PanelTop.Visible = true;
                        region.Union(PanelTop.Bounds);
                    }
                    else
                        PanelTop.Visible = false;

                    if (ShouldPanelIndicatorVisible(DockState.DockBottom))
                    {
                        PanelBottom.Location = new Point(rectDockArea.X + (rectDockArea.Width - PanelBottom.Width) / 2, rectDockArea.Y + rectDockArea.Height - PanelBottom.Height - _PanelIndicatorMargin);
                        PanelBottom.Visible = true;
                        region.Union(PanelBottom.Bounds);
                    }
                    else
                        PanelBottom.Visible = false;

                    if (ShouldPanelIndicatorVisible(DockState.Document))
                    {
                        Rectangle rectDocumentWindow = RectangleToClient(DockPanel.RectangleToScreen(DockPanel.DocumentWindowBounds));
                        PanelFill.Location = new Point(rectDocumentWindow.X + (rectDocumentWindow.Width - PanelFill.Width) / 2, rectDocumentWindow.Y + (rectDocumentWindow.Height - PanelFill.Height) / 2);
                        PanelFill.Visible = true;
                        region.Union(PanelFill.Bounds);
                    }
                    else
                        PanelFill.Visible = false;

                    if (ShouldPaneDiamondVisible())
                    {
                        Rectangle rect = RectangleToClient(DockPane.RectangleToScreen(DockPane.ClientRectangle));
                        PaneDiamond.Location = new Point(rect.Left + (rect.Width - PaneDiamond.Width) / 2, rect.Top + (rect.Height - PaneDiamond.Height) / 2);
                        PaneDiamond.Visible = true;
                        using GraphicsPath graphicsPath = PaneDiamond.DisplayingGraphicsPath.Clone() as GraphicsPath;
                        Point[] pts =
                            {
                                new Point(PaneDiamond.Left, PaneDiamond.Top),
                                new Point(PaneDiamond.Right, PaneDiamond.Top),
                                new Point(PaneDiamond.Left, PaneDiamond.Bottom)
                            };
                        using (Matrix matrix = new(PaneDiamond.ClientRectangle, pts))
                        {
                            graphicsPath.Transform(matrix);
                        }

                        region.Union(graphicsPath);
                    }
                    else
                        PaneDiamond.Visible = false;

                    Region = region;
                }

                private bool ShouldPanelIndicatorVisible(DockState dockState)
                {
                    if (!Visible)
                        return false;

                    if (DockPanel.DockWindows[dockState].Visible)
                        return false;

                    return DragHandler.DragSource.IsDockStateValid(dockState);
                }

                private bool ShouldPaneDiamondVisible()
                {
                    if (DockPane == null)
                        return false;

                    if (!DockPanel.AllowEndUserNestedDocking)
                        return false;

                    return DragHandler.DragSource.CanDockTo(DockPane);
                }

                public override void Show(bool bActivate)
                {
                    base.Show(bActivate);
                    if (PatchController.EnablePerScreenDpi != true)
                    {
                        Bounds = SystemInformation.VirtualScreen;
                    }

                    RefreshChanges();
                }

                public void TestDrop()
                {
                    Point pt = Control.MousePosition;
                    DockPane = DockHelper.PaneAtPoint(pt, DockPanel);

                    if (TestDrop(PanelLeft, pt) != DockStyle.None)
                        HitTestResult = PanelLeft;
                    else if (TestDrop(PanelRight, pt) != DockStyle.None)
                        HitTestResult = PanelRight;
                    else if (TestDrop(PanelTop, pt) != DockStyle.None)
                        HitTestResult = PanelTop;
                    else if (TestDrop(PanelBottom, pt) != DockStyle.None)
                        HitTestResult = PanelBottom;
                    else if (TestDrop(PanelFill, pt) != DockStyle.None)
                        HitTestResult = PanelFill;
                    else if (TestDrop(PaneDiamond, pt) != DockStyle.None)
                        HitTestResult = PaneDiamond;
                    else
                        HitTestResult = null;

                    if (HitTestResult != null)
                    {
                        if (HitTestResult is IPaneIndicator)
                            DragHandler.Outline.Show(DockPane, HitTestResult.Status);
                        else
                            DragHandler.Outline.Show(DockPanel, HitTestResult.Status, FullPanelEdge);
                    }
                }

                private static DockStyle TestDrop(IHitTest hitTest, Point pt)
                {
                    return hitTest.Status = hitTest.HitTest(pt);
                }
            }

            public DockDragHandler(DockPanel panel)
                : base(panel)
            {
            }

            public new IDockDragSource DragSource
            {
                get { return base.DragSource as IDockDragSource; }
                set { base.DragSource = value; }
            }

            private DockOutlineBase _outline;
            public DockOutlineBase Outline
            {
                get { return _outline; }
                private set { _outline = value; }
            }

            private DockIndicator Indicator { get; set; }

            private Rectangle FloatOutlineBounds { get; set; }

            public void BeginDrag(IDockDragSource dragSource)
            {
                DragSource = dragSource;

                if (!BeginDrag())
                {
                    DragSource = null;
                    return;
                }

                Outline = DockPanel.Theme.Extender.DockOutlineFactory.CreateDockOutline();
                Indicator = DockPanel.Theme.Extender.DockIndicatorFactory.CreateDockIndicator(this);
                Indicator.Show(false);

                FloatOutlineBounds = DragSource.BeginDrag(StartMousePosition);
            }

            protected override void OnDragging()
            {
                TestDrop();
            }

            protected override void OnEndDrag(bool abort)
            {
                DockPanel.SuspendLayout(true);

                Outline.Close();
                Indicator.Close();

                EndDrag(abort);

                // Queue a request to layout all children controls
                DockPanel.PerformMdiClientLayout();

                DockPanel.ResumeLayout(true, true);

                DragSource.EndDrag();

                DragSource = null;

                // Fire notification
                DockPanel.OnDocumentDragged();
            }

            private void TestDrop()
            {
                Outline.FlagTestDrop = false;

                Indicator.FullPanelEdge = ((Control.ModifierKeys & Keys.Shift) != 0);

                if ((Control.ModifierKeys & Keys.Control) == 0)
                {
                    Indicator.TestDrop();

                    if (!Outline.FlagTestDrop)
                    {
                        DockPane pane = DockHelper.PaneAtPoint(Control.MousePosition, DockPanel);
                        if (pane != null && DragSource.IsDockStateValid(pane.DockState))
                            pane.TestDrop(DragSource, Outline);
                    }

                    if (!Outline.FlagTestDrop && DragSource.IsDockStateValid(DockState.Float))
                    {
                        FloatWindow floatWindow = DockHelper.FloatWindowAtPoint(Control.MousePosition, DockPanel);
                        floatWindow?.TestDrop(DragSource, Outline);
                    }
                }
                else
                    Indicator.DockPane = DockHelper.PaneAtPoint(Control.MousePosition, DockPanel);

                if (!Outline.FlagTestDrop && DragSource.IsDockStateValid(DockState.Float))
                {
                    Rectangle rect = FloatOutlineBounds;
                    rect.Offset(Control.MousePosition.X - StartMousePosition.X, Control.MousePosition.Y - StartMousePosition.Y);
                    Outline.Show(rect);
                }

                if (!Outline.FlagTestDrop)
                {
                    Cursor.Current = Cursors.No;
                    Outline.Show();
                }
                else
                    Cursor.Current = DragControl.Cursor;
            }

            private void EndDrag(bool abort)
            {
                if (abort)
                    return;

                if (!Outline.FloatWindowBounds.IsEmpty)
                    DragSource.FloatAt(Outline.FloatWindowBounds);
                else if (Outline.DockTo is DockPane)
                {
                    DockPane pane = Outline.DockTo as DockPane;
                    DragSource.DockTo(pane, Outline.Dock, Outline.ContentIndex);
                }
                else if (Outline.DockTo is DockPanel)
                {
                    DockPanel panel = Outline.DockTo as DockPanel;
                    panel.UpdateDockWindowZOrder(Outline.Dock, Outline.FlagFullEdge);
                    DragSource.DockTo(panel, Outline.Dock);
                }
            }
        }

        private DockDragHandler _dockDragHandler = null;
        private DockDragHandler GetDockDragHandler()
        {
            _dockDragHandler ??= new DockDragHandler(this);
            return _dockDragHandler;
        }

        internal void BeginDrag(IDockDragSource dragSource)
        {
            GetDockDragHandler().BeginDrag(dragSource);
        }
    }
}
