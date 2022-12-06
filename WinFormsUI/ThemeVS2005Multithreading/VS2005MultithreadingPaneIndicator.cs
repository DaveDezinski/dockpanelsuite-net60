using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.ThemeVS2005;

namespace WeifenLuo.WinFormsUI.Docking
{
    [ToolboxItem(false)]
    internal class VS2005MultithreadingPaneIndicator : PictureBox, DockPanel.IPaneIndicator
    {
        private readonly Bitmap _bitmapPaneDiamond;
        private readonly Bitmap _bitmapPaneDiamondLeft;
        private readonly Bitmap _bitmapPaneDiamondRight;
        private readonly Bitmap _bitmapPaneDiamondTop;
        private readonly Bitmap _bitmapPaneDiamondBottom;
        private readonly Bitmap _bitmapPaneDiamondFill;
        private readonly Bitmap _bitmapPaneDiamondHotSpot;
        private readonly Bitmap _bitmapPaneDiamondHotSpotIndex;

        private static readonly DockPanel.HotSpotIndex[] _hotSpots = new[]
        {
                new DockPanel.HotSpotIndex(1, 0, DockStyle.Top),
                new DockPanel.HotSpotIndex(0, 1, DockStyle.Left),
                new DockPanel.HotSpotIndex(1, 1, DockStyle.Fill),
                new DockPanel.HotSpotIndex(2, 1, DockStyle.Right),
                new DockPanel.HotSpotIndex(1, 2, DockStyle.Bottom)
            };

        private readonly GraphicsPath _displayingGraphicsPath;

        private readonly object lockObj = new();

        public VS2005MultithreadingPaneIndicator()
        {
            lock (lockObj)
            {
                _bitmapPaneDiamond = (Bitmap)Resources.DockIndicator_PaneDiamond.Clone();
                _bitmapPaneDiamondLeft = (Bitmap)Resources.DockIndicator_PaneDiamond_Left.Clone();
                _bitmapPaneDiamondRight = (Bitmap)Resources.DockIndicator_PaneDiamond_Right.Clone();
                _bitmapPaneDiamondTop = (Bitmap)Resources.DockIndicator_PaneDiamond_Top.Clone();
                _bitmapPaneDiamondBottom = (Bitmap)Resources.DockIndicator_PaneDiamond_Bottom.Clone();
                _bitmapPaneDiamondFill = (Bitmap)Resources.DockIndicator_PaneDiamond_Fill.Clone();
                _bitmapPaneDiamondHotSpot = (Bitmap)Resources.DockIndicator_PaneDiamond_HotSpot.Clone();
                _bitmapPaneDiamondHotSpotIndex = (Bitmap)Resources.DockIndicator_PaneDiamond_HotSpotIndex.Clone();
            }

            _displayingGraphicsPath = DrawHelper.CalculateGraphicsPathFromBitmap(_bitmapPaneDiamond);

            SizeMode = PictureBoxSizeMode.AutoSize;
            Image = _bitmapPaneDiamond;
            Region = new Region(DisplayingGraphicsPath);
        }

        public GraphicsPath DisplayingGraphicsPath
        {
            get { return _displayingGraphicsPath; }
        }

        public DockStyle HitTest(Point pt)
        {
            if (!Visible)
                return DockStyle.None;

            pt = PointToClient(pt);
            if (!ClientRectangle.Contains(pt))
                return DockStyle.None;

            for (int i = _hotSpots.GetLowerBound(0); i <= _hotSpots.GetUpperBound(0); i++)
            {
                if (_bitmapPaneDiamondHotSpot.GetPixel(pt.X, pt.Y) == _bitmapPaneDiamondHotSpotIndex.GetPixel(_hotSpots[i].X, _hotSpots[i].Y))
                    return _hotSpots[i].DockStyle;
            }

            return DockStyle.None;
        }

        private DockStyle _status = DockStyle.None;
        public DockStyle Status
        {
            get { return _status; }
            set
            {
                _status = value;
                if (_status == DockStyle.None)
                    Image = _bitmapPaneDiamond;
                else if (_status == DockStyle.Left)
                    Image = _bitmapPaneDiamondLeft;
                else if (_status == DockStyle.Right)
                    Image = _bitmapPaneDiamondRight;
                else if (_status == DockStyle.Top)
                    Image = _bitmapPaneDiamondTop;
                else if (_status == DockStyle.Bottom)
                    Image = _bitmapPaneDiamondBottom;
                else if (_status == DockStyle.Fill)
                    Image = _bitmapPaneDiamondFill;
            }
        }
    }
}
