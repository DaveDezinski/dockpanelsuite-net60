using System.Drawing;
using System.Windows.Forms;

namespace WeifenLuo.WinFormsUI.Docking
{
    public static class DockHelper
    {
        public static bool IsDockStateAutoHide(DockState dockState)
        {
            if (dockState == DockState.DockLeftAutoHide ||
                dockState == DockState.DockRightAutoHide ||
                dockState == DockState.DockTopAutoHide ||
                dockState == DockState.DockBottomAutoHide)
                return true;
            else
                return false;
        }

        public static bool IsDockStateValid(DockState dockState, DockAreas dockableAreas)
        {
            if (!dockableAreas.HasFlag(DockAreas.Float) &&
                (dockState == DockState.Float))
                return false;
            else if (!dockableAreas.HasFlag(DockAreas.Document) &&
                (dockState == DockState.Document))
                return false;
            else if (!dockableAreas.HasFlag(DockAreas.DockLeft) &&
                (dockState == DockState.DockLeft || dockState == DockState.DockLeftAutoHide))
                return false;
            else if (!dockableAreas.HasFlag(DockAreas.DockRight) &&
                (dockState == DockState.DockRight || dockState == DockState.DockRightAutoHide))
                return false;
            else if (!dockableAreas.HasFlag(DockAreas.DockTop) &&
                (dockState == DockState.DockTop || dockState == DockState.DockTopAutoHide))
                return false;
            else if (!dockableAreas.HasFlag(DockAreas.DockBottom) &&
                (dockState == DockState.DockBottom || dockState == DockState.DockBottomAutoHide))
                return false;
            else
                return true;
        }

        public static bool IsDockWindowState(DockState state)
        {
            if (state == DockState.DockTop || state == DockState.DockBottom || state == DockState.DockLeft ||
                state == DockState.DockRight || state == DockState.Document)
                return true;
            else
                return false;
        }

        public static DockState ToggleAutoHideState(DockState state)
        {
            if (state == DockState.DockLeft)
                return DockState.DockLeftAutoHide;
            else if (state == DockState.DockRight)
                return DockState.DockRightAutoHide;
            else if (state == DockState.DockTop)
                return DockState.DockTopAutoHide;
            else if (state == DockState.DockBottom)
                return DockState.DockBottomAutoHide;
            else if (state == DockState.DockLeftAutoHide)
                return DockState.DockLeft;
            else if (state == DockState.DockRightAutoHide)
                return DockState.DockRight;
            else if (state == DockState.DockTopAutoHide)
                return DockState.DockTop;
            else if (state == DockState.DockBottomAutoHide)
                return DockState.DockBottom;
            else
                return state;
        }

        public static DockPane PaneAtPoint(Point pt, DockPanel dockPanel)
        {
            if (!Win32Helper.IsRunningOnMono)
            {
                for (Control control = Win32Helper.ControlAtPoint(pt); control != null; control = control.Parent)
                {
                    if (control is IDockContent content && content.DockHandler.DockPanel == dockPanel)
                        return content.DockHandler.Pane;

                    if (control is DockPane pane && pane.DockPanel == dockPanel)
                        return pane;
                }
            }
            return null;
        }

        public static FloatWindow FloatWindowAtPoint(Point pt, DockPanel dockPanel)
        {
            if (!Win32Helper.IsRunningOnMono)
            {
                for (Control control = Win32Helper.ControlAtPoint(pt); control != null; control = control.Parent)
                {
                    if (control is FloatWindow floatWindow && floatWindow.DockPanel == dockPanel)
                        return floatWindow;
                }
            }
            return null;
        }
    }
}
