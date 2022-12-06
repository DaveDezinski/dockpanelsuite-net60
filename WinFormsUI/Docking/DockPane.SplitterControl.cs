using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace WeifenLuo.WinFormsUI.Docking
{
    partial class DockPane
    {
        [ToolboxItem(false)]
        public class SplitterControlBase : Control, ISplitterDragSource
        {
            readonly DockPane _pane;

            public SplitterControlBase(DockPane pane)
            {
                SetStyle(ControlStyles.Selectable, false);
                _pane = pane;
            }

            public DockPane DockPane
            {
                get { return _pane; }
            }

            private DockAlignment _alignment;
            public DockAlignment Alignment
            {
                get { return _alignment; }
                set
                {
                    _alignment = value;
                    if (_alignment == DockAlignment.Left || _alignment == DockAlignment.Right)
                        Cursor = Cursors.VSplit;
                    else if (_alignment == DockAlignment.Top || _alignment == DockAlignment.Bottom)
                        Cursor = Cursors.HSplit;
                    else
                        Cursor = Cursors.Default;

                    if (DockPane.DockState == DockState.Document)
                        Invalidate();
                }
            }

            protected override void OnMouseDown(MouseEventArgs e)
            {
                base.OnMouseDown(e);

                if (e.Button != MouseButtons.Left)
                    return;

                DockPane.DockPanel.BeginDrag(this, Parent.RectangleToScreen(Bounds));
            }

            #region ISplitterDragSource Members

            void ISplitterDragSource.BeginDrag(Rectangle rectSplitter)
            {
                // No Implementation
            }

            void ISplitterDragSource.EndDrag()
            {
                // No Implementation
            }

            bool ISplitterDragSource.IsVertical
            {
                get
                {
                    NestedDockingStatus status = DockPane.NestedDockingStatus;
                    return (status.DisplayingAlignment == DockAlignment.Left ||
                        status.DisplayingAlignment == DockAlignment.Right);
                }
            }

            Rectangle ISplitterDragSource.DragLimitBounds
            {
                get
                {
                    NestedDockingStatus status = DockPane.NestedDockingStatus;
                    Rectangle rectLimit = Parent.RectangleToScreen(status.LogicalBounds);
                    if (((ISplitterDragSource)this).IsVertical)
                    {
                        rectLimit.X += MeasurePane.MinSize;
                        rectLimit.Width -= 2 * MeasurePane.MinSize;
                    }
                    else
                    {
                        rectLimit.Y += MeasurePane.MinSize;
                        rectLimit.Height -= 2 * MeasurePane.MinSize;
                    }

                    return rectLimit;
                }
            }

            void ISplitterDragSource.MoveSplitter(int offset)
            {
                NestedDockingStatus status = DockPane.NestedDockingStatus;
                double proportion = status.Proportion;
                if (status.LogicalBounds.Width <= 0 || status.LogicalBounds.Height <= 0)
                    return;
                else if (status.DisplayingAlignment == DockAlignment.Left)
                    proportion += ((double)offset) / (double)status.LogicalBounds.Width;
                else if (status.DisplayingAlignment == DockAlignment.Right)
                    proportion -= ((double)offset) / (double)status.LogicalBounds.Width;
                else if (status.DisplayingAlignment == DockAlignment.Top)
                    proportion += ((double)offset) / (double)status.LogicalBounds.Height;
                else
                    proportion -= ((double)offset) / (double)status.LogicalBounds.Height;

                DockPane.SetNestedDockingProportion(proportion);
            }

            #region IDragSource Members

            Control IDragSource.DragControl
            {
                get { return this; }
            }

            #endregion

            #endregion
        }
        
        private SplitterControlBase _splitter;
        private SplitterControlBase Splitter
        {
            get { return _splitter; }
        }

        internal Rectangle SplitterBounds
        {
            set { Splitter.Bounds = value; }
        }

        internal DockAlignment SplitterAlignment
        {
            set { Splitter.Alignment = value; }
        }
    }
}