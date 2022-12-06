namespace WeifenLuo.WinFormsUI.Docking
{
    using System;
    using System.ComponentModel;

    public partial class DockPanel
    {
        [LocalizedCategory("Category_Docking")]
        [LocalizedDescription("DockPanel_DockPanelSkin")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        [Obsolete("Use Theme.Skin instead.")]
        public DockPanelSkin Skin
        {
            get { return null;  }
        }

        private ThemeBase _dockPanelTheme = new DefaultTheme();

        [LocalizedCategory("Category_Docking")]
        [LocalizedDescription("DockPanel_DockPanelTheme")]
        public ThemeBase Theme
        {
            get { return _dockPanelTheme; }
            set
            {
                if (value == null)
                {
                    return;
                }

                if (_dockPanelTheme.GetType() == value.GetType())
                {
                    return;
                }

                _dockPanelTheme?.CleanUp(this);
                _dockPanelTheme = value;
                _dockPanelTheme.ApplyTo(this);
                _dockPanelTheme.PostApply(this);
            }
        }
    }
}
