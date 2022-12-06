using System;
using System.Drawing;
using System.Windows.Forms;

namespace WeifenLuo.WinFormsUI.Docking
{
    public static class Win32Helper
    {
        public static bool IsRunningOnMono { get; } = Type.GetType("Mono.Runtime") != null;

        internal static Control ControlAtPoint(Point pt)
        {
            return Control.FromChildHandle(NativeMethods.WindowFromPoint(pt));
        }

        internal static uint MakeLong(int low, int high)
        {
            return (uint)((high << 16) + low);
        }

        internal static uint HitTestCaption(Control control)
        {
            var captionRectangle = new Rectangle(0, 0, control.Width, control.ClientRectangle.Top - control.PointToClient(control.Location).X);
            return captionRectangle.Contains(Control.MousePosition) ? (uint)2 : 0;
        }
    }
}
