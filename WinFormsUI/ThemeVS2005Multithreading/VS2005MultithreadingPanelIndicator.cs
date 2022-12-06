using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.ThemeVS2005;

namespace WeifenLuo.WinFormsUI.Docking
{
    [ToolboxItem(false)]
    internal class VS2005MultithreadingPanelIndicator : PictureBox, DockPanel.IPanelIndicator
    {
        private readonly Image _imagePanelLeft;
        private readonly Image _imagePanelRight;
        private readonly Image _imagePanelTop;
        private readonly Image _imagePanelBottom;
        private readonly Image _imagePanelFill;
        private readonly Image _imagePanelLeftActive;
        private readonly Image _imagePanelRightActive;
        private readonly Image _imagePanelTopActive;
        private readonly Image _imagePanelBottomActive;
        private readonly Image _imagePanelFillActive;

        private readonly object lockObj = new();

        public VS2005MultithreadingPanelIndicator(DockStyle dockStyle)
        {
            lock (lockObj)
            {
                _imagePanelLeft = (Image)Resources.DockIndicator_PanelLeft.Clone();
                _imagePanelRight = (Image)Resources.DockIndicator_PanelRight.Clone();
                _imagePanelTop = (Image)Resources.DockIndicator_PanelTop.Clone();
                _imagePanelBottom = (Image)Resources.DockIndicator_PanelBottom.Clone();
                _imagePanelFill = (Image)Resources.DockIndicator_PanelFill.Clone();
                _imagePanelLeftActive = (Image)Resources.DockIndicator_PanelLeft_Active.Clone();
                _imagePanelRightActive = (Image)Resources.DockIndicator_PanelRight_Active.Clone();
                _imagePanelTopActive = (Image)Resources.DockIndicator_PanelTop_Active.Clone();
                _imagePanelBottomActive = (Image)Resources.DockIndicator_PanelBottom_Active.Clone();
                _imagePanelFillActive = (Image)Resources.DockIndicator_PanelFill_Active.Clone();
            }

            _dockStyle = dockStyle;
            SizeMode = PictureBoxSizeMode.AutoSize;
            Image = ImageInactive;
        }

        private readonly DockStyle _dockStyle;
        private DockStyle DockStyle
        {
            get { return _dockStyle; }
        }

        private DockStyle _status;
        public DockStyle Status
        {
            get { return _status; }
            set
            {
                if (value != DockStyle && value != DockStyle.None)
                    throw new InvalidEnumArgumentException();

                if (_status == value)
                    return;

                _status = value;
                IsActivated = (_status != DockStyle.None);
            }
        }

        private Image ImageInactive
        {
            get
            {
                if (DockStyle == DockStyle.Left)
                    return _imagePanelLeft;
                else if (DockStyle == DockStyle.Right)
                    return _imagePanelRight;
                else if (DockStyle == DockStyle.Top)
                    return _imagePanelTop;
                else if (DockStyle == DockStyle.Bottom)
                    return _imagePanelBottom;
                else if (DockStyle == DockStyle.Fill)
                    return _imagePanelFill;
                else
                    return null;
            }
        }

        private Image ImageActive
        {
            get
            {
                if (DockStyle == DockStyle.Left)
                    return _imagePanelLeftActive;
                else if (DockStyle == DockStyle.Right)
                    return _imagePanelRightActive;
                else if (DockStyle == DockStyle.Top)
                    return _imagePanelTopActive;
                else if (DockStyle == DockStyle.Bottom)
                    return _imagePanelBottomActive;
                else if (DockStyle == DockStyle.Fill)
                    return _imagePanelFillActive;
                else
                    return null;
            }
        }

        private bool _isActivated = false;
        private bool IsActivated
        {
            get { return _isActivated; }
            set
            {
                _isActivated = value;
                Image = IsActivated ? ImageActive : ImageInactive;
            }
        }

        public DockStyle HitTest(Point pt)
        {
            return this.Visible && ClientRectangle.Contains(PointToClient(pt)) ? DockStyle : DockStyle.None;
        }
    }
}
