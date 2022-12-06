using System;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.ComponentModel.Design;

namespace WeifenLuo.WinFormsUI.Docking
{
    partial class DockPanel
    {
        //  This class comes from Jacob Slusser's MdiClientController class:
        //  http://www.codeproject.com/cs/miscctrl/mdiclientcontroller.asp
        private class MdiClientController : NativeWindow, IComponent
        {
            private bool _autoScroll = true;
            private BorderStyle _borderStyle = BorderStyle.Fixed3D;
            private MdiClient _mdiClient = null;
            private Form _parentForm = null;
            private ISite _site = null;

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (disposing)
                {
                    if (Site != null && Site.Container != null)
                        Site.Container.Remove(this);

                    Disposed?.Invoke(this, EventArgs.Empty);
                }
            }

            public bool AutoScroll
            {
                get { return _autoScroll; }
                set
                {
                    // By default the MdiClient control scrolls. It can appear though that
                    // there are no scrollbars by turning them off when the non-client
                    // area is calculated. I decided to expose this method following
                    // the .NET vernacular of an AutoScroll property.
                    _autoScroll = value;
                    if (MdiClient != null)
                        UpdateStyles();
                }
            }

            public BorderStyle BorderStyle
            {
                set
                {
                    // Error-check the enum.
                    if (!Enum.IsDefined(typeof(BorderStyle), value))
                        throw new InvalidEnumArgumentException();

                    _borderStyle = value;

                    if (MdiClient == null)
                        return;

                    // This property can actually be visible in design-mode,
                    // but to keep it consistent with the others,
                    // prevent this from being show at design-time.
                    if (Site != null && Site.DesignMode)
                        return;

                    // There is no BorderStyle property exposed by the MdiClient class,
                    // but this can be controlled by Win32 functions. A Win32 ExStyle
                    // of WS_EX_CLIENTEDGE is equivalent to a Fixed3D border and a
                    // Style of WS_BORDER is equivalent to a FixedSingle border.

                    // This code is inspired Jason Dori's article:
                    // "Adding designable borders to user controls".
                    // http://www.codeproject.com/cs/miscctrl/CsAddingBorders.asp

                    if (!Win32Helper.IsRunningOnMono)
                    {
                        // Get styles using Win32 calls
                        int style = NativeMethods.GetWindowLong(MdiClient.Handle, (int)Win32.GetWindowLongIndex.GWL_STYLE);
                        int exStyle = NativeMethods.GetWindowLong(MdiClient.Handle, (int)Win32.GetWindowLongIndex.GWL_EXSTYLE);

                        // Add or remove style flags as necessary.
                        switch (_borderStyle)
                        {
                            case BorderStyle.Fixed3D:
                                exStyle |= (int)Win32.WindowExStyles.WS_EX_CLIENTEDGE;
                                style &= ~((int)Win32.WindowStyles.WS_BORDER);
                                break;

                            case BorderStyle.FixedSingle:
                                exStyle &= ~((int)Win32.WindowExStyles.WS_EX_CLIENTEDGE);
                                style |= (int)Win32.WindowStyles.WS_BORDER;
                                break;

                            case BorderStyle.None:
                                style &= ~((int)Win32.WindowStyles.WS_BORDER);
                                exStyle &= ~((int)Win32.WindowExStyles.WS_EX_CLIENTEDGE);
                                break;
                        }

                        // Set the styles using Win32 calls
                        _ = NativeMethods.SetWindowLong(MdiClient.Handle, (int)Win32.GetWindowLongIndex.GWL_STYLE, style);
                        _ = NativeMethods.SetWindowLong(MdiClient.Handle, (int)Win32.GetWindowLongIndex.GWL_EXSTYLE, exStyle);
                    }

                    // Cause an update of the non-client area.
                    UpdateStyles();
                }
            }

            public MdiClient MdiClient
            {
                get { return _mdiClient; }
            }

            [Browsable(false)]
            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public Form ParentForm
            {
                get { return _parentForm; }
                set
                {
                    // If the ParentForm has previously been set,
                    // unwire events connected to the old parent.
                    if (_parentForm != null)
                    {
                        _parentForm.HandleCreated -= new EventHandler(ParentFormHandleCreated);
                        _parentForm.MdiChildActivate -= new EventHandler(ParentFormMdiChildActivate);
                    }

                    _parentForm = value;

                    if (_parentForm == null)
                        return;

                    // If the parent form has not been created yet,
                    // wait to initialize the MDI client until it is.
                    if (_parentForm.IsHandleCreated)
                    {
                        InitializeMdiClient();
                        RefreshProperties();
                    }
                    else
                        _parentForm.HandleCreated += new EventHandler(ParentFormHandleCreated);

                    _parentForm.MdiChildActivate += new EventHandler(ParentFormMdiChildActivate);
                }
            }

            public ISite Site
            {
                get { return _site; }
                set
                {
                    _site = value;

                    if (_site == null)
                        return;

                    // If the component is dropped onto a form during design-time,
                    // set the ParentForm property.
                    IDesignerHost host = (value.GetService(typeof(IDesignerHost)) as IDesignerHost);
                    if (host != null && host.RootComponent is Form parent)
                        ParentForm = parent;
                }
            }

            public void RenewMdiClient()
            {
                // Reinitialize the MdiClient and its properties.
                InitializeMdiClient();
                RefreshProperties();
            }

            public event EventHandler Disposed;

            public event EventHandler HandleAssigned;

            public event EventHandler MdiChildActivate;

            public event LayoutEventHandler Layout;

            protected virtual void OnHandleAssigned(EventArgs e)
            {
                // Raise the HandleAssigned event.
                HandleAssigned?.Invoke(this, e);
            }

            protected virtual void OnMdiChildActivate(EventArgs e)
            {
                // Raise the MdiChildActivate event
                MdiChildActivate?.Invoke(this, e);
            }

            protected virtual void OnLayout(LayoutEventArgs e)
            {
                // Raise the Layout event
                Layout?.Invoke(this, e);
            }

            public event PaintEventHandler Paint;

            protected virtual void OnPaint(PaintEventArgs e)
            {
                // Raise the Paint event.
                Paint?.Invoke(this, e);
            }

            protected override void WndProc(ref Message m)
            {
                switch (m.Msg)
                {
                    case (int)Win32.Msgs.WM_NCCALCSIZE:
                        // If AutoScroll is set to false, hide the scrollbars when the control
                        // calculates its non-client area.
                        if (!AutoScroll && !Win32Helper.IsRunningOnMono)
                        {
                            _ = NativeMethods.ShowScrollBar(m.HWnd, (int)Win32.ScrollBars.SB_BOTH, 0 /*false*/);
                        }

                        break;
                }

                base.WndProc(ref m);
            }

            private void ParentFormHandleCreated(object sender, EventArgs e)
            {
                // The form has been created, unwire the event, and initialize the MdiClient.
                this._parentForm.HandleCreated -= new EventHandler(ParentFormHandleCreated);
                InitializeMdiClient();
                RefreshProperties();
            }

            private void ParentFormMdiChildActivate(object sender, EventArgs e)
            {
                OnMdiChildActivate(e);
            }

            private void MdiClientLayout(object sender, LayoutEventArgs e)
            {
                OnLayout(e);
            }

            private void MdiClientHandleDestroyed(object sender, EventArgs e)
            {
                // If the MdiClient handle has been released, drop the reference and
                // release the handle.
                if (_mdiClient != null)
                {
                    _mdiClient.HandleDestroyed -= new EventHandler(MdiClientHandleDestroyed);
                    _mdiClient = null;
                }

                ReleaseHandle();
            }

            private void InitializeMdiClient()
            {
                // If the mdiClient has previously been set, unwire events connected
                // to the old MDI.
                if (MdiClient != null)
                {
                    MdiClient.HandleDestroyed -= new EventHandler(MdiClientHandleDestroyed);
                    MdiClient.Layout -= new LayoutEventHandler(MdiClientLayout);
                }

                if (ParentForm == null)
                    return;

                // Get the MdiClient from the parent form.
                foreach (Control control in ParentForm.Controls)
                {
                    // If the form is an MDI container, it will contain an MdiClient control
                    // just as it would any other control.

                    _mdiClient = control as MdiClient;
                    if (_mdiClient == null)
                        continue;

                    // Assign the MdiClient Handle to the NativeWindow.
                    ReleaseHandle();
                    AssignHandle(MdiClient.Handle);

                    // Raise the HandleAssigned event.
                    OnHandleAssigned(EventArgs.Empty);

                    // Monitor the MdiClient for when its handle is destroyed.
                    MdiClient.HandleDestroyed += new EventHandler(MdiClientHandleDestroyed);
                    MdiClient.Layout += new LayoutEventHandler(MdiClientLayout);

                    break;
                }
            }

            private void RefreshProperties()
            {
                // Refresh all the properties
                BorderStyle = _borderStyle;
                AutoScroll = _autoScroll;
            }

            private void UpdateStyles()
            {
                // To show style changes, the non-client area must be repainted. Using the
                // control's Invalidate method does not affect the non-client area.
                // Instead use a Win32 call to signal the style has changed.
                if (!Win32Helper.IsRunningOnMono)
                    _ = NativeMethods.SetWindowPos(MdiClient.Handle, IntPtr.Zero, 0, 0, 0, 0,
                        Win32.FlagsSetWindowPos.SWP_NOACTIVATE |
                        Win32.FlagsSetWindowPos.SWP_NOMOVE |
                        Win32.FlagsSetWindowPos.SWP_NOSIZE |
                        Win32.FlagsSetWindowPos.SWP_NOZORDER |
                        Win32.FlagsSetWindowPos.SWP_NOOWNERZORDER |
                        Win32.FlagsSetWindowPos.SWP_FRAMECHANGED);
            }
        }

        private MdiClientController _mdiClientController = null;
        private MdiClientController GetMdiClientController()
        {
            if (_mdiClientController == null)
            {
                _mdiClientController = new MdiClientController();
                _mdiClientController.HandleAssigned += new EventHandler(MdiClientHandleAssigned);
                _mdiClientController.MdiChildActivate += new EventHandler(ParentFormMdiChildActivate);
                _mdiClientController.Layout += new LayoutEventHandler(MdiClient_Layout);
            }

            return _mdiClientController;
        }

        private void ParentFormMdiChildActivate(object sender, EventArgs e)
        {
            if (GetMdiClientController().ParentForm == null)
                return;

            if (GetMdiClientController().ParentForm.ActiveMdiChild is not IDockContent content)
                return;

            if (content.DockHandler.DockPanel == this && content.DockHandler.Pane != null)
            {
                if (content.DockHandler.Pane.DisplayingContents.Contains(content))
                {
                    content.DockHandler.Pane.ActiveContent = content;
                }
                else if (PatchController.EnableActiveControlFix != true)
                {
                    content.DockHandler.Pane.ActiveContent = content;
                }
            }
        }

        private bool MdiClientExists
        {
            get { return GetMdiClientController().MdiClient != null; }
        }

        private void SetMdiClientBounds(Rectangle bounds)
        {
            GetMdiClientController().MdiClient.Bounds = bounds;
        }

        private void SuspendMdiClientLayout()
        {
            GetMdiClientController().MdiClient?.SuspendLayout();
        }

        private void ResumeMdiClientLayout(bool perform)
        {
            GetMdiClientController().MdiClient?.ResumeLayout(perform);
        }

        private void PerformMdiClientLayout()
        {
            GetMdiClientController().MdiClient?.PerformLayout();
        }

        // Called when:
        // 1. DockPanel.DocumentStyle changed
        // 2. DockPanel.Visible changed
        // 3. MdiClientController.Handle assigned
        private void SetMdiClient()
        {
            MdiClientController controller = GetMdiClientController();

            if (this.DocumentStyle == DocumentStyle.DockingMdi)
            {
                controller.AutoScroll = false;
                controller.BorderStyle = BorderStyle.None;
                if (MdiClientExists)
                    controller.MdiClient.Dock = DockStyle.Fill;
            }
            else if (DocumentStyle == DocumentStyle.DockingSdi || DocumentStyle == DocumentStyle.DockingWindow)
            {
                controller.AutoScroll = true;
                controller.BorderStyle = BorderStyle.Fixed3D;
                if (MdiClientExists)
                    controller.MdiClient.Dock = DockStyle.Fill;
            }
            else if (this.DocumentStyle == DocumentStyle.SystemMdi)
            {
                controller.AutoScroll = true;
                controller.BorderStyle = BorderStyle.Fixed3D;
                if (controller.MdiClient != null)
                {
                    controller.MdiClient.Dock = DockStyle.None;
                    controller.MdiClient.Bounds = SystemMdiClientBounds;
                }
            }
        }

        internal Rectangle RectangleToMdiClient(Rectangle rect)
        {
            if (MdiClientExists)
                return GetMdiClientController().MdiClient.RectangleToClient(rect);
            else
                return Rectangle.Empty;
        }
    }
}
