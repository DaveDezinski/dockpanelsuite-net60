using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace WeifenLuo.WinFormsUI.ThemeVS2012
{
    internal class VS2012PanelIndicatorFactory : DockPanelExtender.IPanelIndicatorFactory
    {
        public DockPanel.IPanelIndicator CreatePanelIndicator(DockStyle style, ThemeBase theme)
        {
            return new VS2012PanelIndicator(style, theme);
        }

        [ToolboxItem(false)]
        private sealed class VS2012PanelIndicator : PictureBox, DockPanel.IPanelIndicator
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

            public VS2012PanelIndicator(DockStyle dockStyle, ThemeBase theme)
            {
                _imagePanelLeft = theme.ImageService.DockIndicator_PanelLeft;
                _imagePanelRight = theme.ImageService.DockIndicator_PanelRight;
                _imagePanelTop = theme.ImageService.DockIndicator_PanelTop;
                _imagePanelBottom = theme.ImageService.DockIndicator_PanelBottom;
                _imagePanelFill = theme.ImageService.DockIndicator_PanelFill;
                _imagePanelLeftActive = theme.ImageService.DockIndicator_PanelLeft;
                _imagePanelRightActive = theme.ImageService.DockIndicator_PanelRight;
                _imagePanelTopActive = theme.ImageService.DockIndicator_PanelTop;
                _imagePanelBottomActive = theme.ImageService.DockIndicator_PanelBottom;
                _imagePanelFillActive = theme.ImageService.DockIndicator_PanelFill;

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
}
