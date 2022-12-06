using System.ComponentModel;
using System.Drawing;
using WeifenLuo.WinFormsUI.Docking;

namespace WeifenLuo.WinFormsUI.ThemeVS2012
{
    [ToolboxItem(false)]
    public class VS2012DockPaneCaptionInertButton : InertButtonBase
    {
        private readonly Bitmap _hovered;
        private readonly Bitmap _normal;
        private readonly Bitmap _active;
        private readonly Bitmap _pressed;
        private readonly Bitmap _hoveredActive;
        private readonly Bitmap _hoveredAutoHide;
        private readonly Bitmap _autoHide;
        private readonly Bitmap _pressedAutoHide;

        public VS2012DockPaneCaptionInertButton(DockPaneCaptionBase dockPaneCaption, Bitmap hovered, Bitmap normal, Bitmap pressed, Bitmap hoveredActive, Bitmap active, Bitmap hoveredAutoHide = null, Bitmap autoHide = null, Bitmap pressedAutoHide = null)
        {
            _dockPaneCaption = dockPaneCaption;
            _hovered = hovered;
            _normal = normal;
            _pressed = pressed;
            _hoveredActive = hoveredActive;
            _active = active;
            _hoveredAutoHide = hoveredAutoHide ?? hoveredActive;
            _autoHide = autoHide ?? active;
            _pressedAutoHide = pressedAutoHide ?? pressed;
            RefreshChanges();
        }

        private readonly DockPaneCaptionBase _dockPaneCaption;
        private DockPaneCaptionBase DockPaneCaption
        {
            get { return _dockPaneCaption; }
        }

        public bool IsAutoHide
        {
            get { return DockPaneCaption.DockPane.IsAutoHide; }
        }

        public bool IsActive
        {
            get { return DockPaneCaption.DockPane.IsActivePane; }
        }

        public override Bitmap Image
        {
            get { return IsActive ? IsAutoHide ? _autoHide : _active : _normal; }
        }

        public override Bitmap HoverImage
        {
            get { return IsActive ? IsAutoHide ? _hoveredAutoHide : _hoveredActive : _hovered; }
        }

        public override Bitmap PressImage
        {
            get { return IsAutoHide ? _pressedAutoHide : _pressed; }
        }
    }
}
