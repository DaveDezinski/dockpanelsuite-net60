using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace WeifenLuo.WinFormsUI.Docking
{
    internal class DockAreasEditor : UITypeEditor
    {
        private class DockAreasEditorControl : UserControl
        {
            private readonly CheckBox _checkBoxFloat;
            private readonly CheckBox _checkBoxDockLeft;
            private readonly CheckBox _checkBoxDockRight;
            private readonly CheckBox _checkBoxDockTop;
            private readonly CheckBox _checkBoxDockBottom;
            private readonly CheckBox _checkBoxDockFill;
            private DockAreas _oldDockAreas;

            public DockAreas DockAreas
            {
                get
                {
                    DockAreas dockAreas = 0;
                    if (_checkBoxFloat.Checked)
                        dockAreas |= DockAreas.Float;
                    if (_checkBoxDockLeft.Checked)
                        dockAreas |= DockAreas.DockLeft;
                    if (_checkBoxDockRight.Checked)
                        dockAreas |= DockAreas.DockRight;
                    if (_checkBoxDockTop.Checked)
                        dockAreas |= DockAreas.DockTop;
                    if (_checkBoxDockBottom.Checked)
                        dockAreas |= DockAreas.DockBottom;
                    if (_checkBoxDockFill.Checked)
                        dockAreas |= DockAreas.Document;

                    if (dockAreas == 0)
                        return _oldDockAreas;
                    else
                        return dockAreas;
                }
            }

            public DockAreasEditorControl()
            {
                _checkBoxFloat = new CheckBox();
                _checkBoxDockLeft = new CheckBox();
                _checkBoxDockRight = new CheckBox();
                _checkBoxDockTop = new CheckBox();
                _checkBoxDockBottom = new CheckBox();
                _checkBoxDockFill = new CheckBox();

                SuspendLayout();

                _checkBoxFloat.Appearance = Appearance.Button;
                _checkBoxFloat.Dock = DockStyle.Top;
                _checkBoxFloat.Height = 24;
                _checkBoxFloat.Text = Strings.DockAreaEditor_FloatCheckBoxText;
                _checkBoxFloat.TextAlign = ContentAlignment.MiddleCenter;
                _checkBoxFloat.FlatStyle = FlatStyle.System;
            
                _checkBoxDockLeft.Appearance = Appearance.Button;
                _checkBoxDockLeft.Dock = DockStyle.Left;
                _checkBoxDockLeft.Width = 24;
                _checkBoxDockLeft.FlatStyle = FlatStyle.System;

                _checkBoxDockRight.Appearance = Appearance.Button;
                _checkBoxDockRight.Dock = DockStyle.Right;
                _checkBoxDockRight.Width = 24;
                _checkBoxDockRight.FlatStyle = FlatStyle.System;

                _checkBoxDockTop.Appearance = Appearance.Button;
                _checkBoxDockTop.Dock = DockStyle.Top;
                _checkBoxDockTop.Height = 24;
                _checkBoxDockTop.FlatStyle = FlatStyle.System;

                _checkBoxDockBottom.Appearance = Appearance.Button;
                _checkBoxDockBottom.Dock = DockStyle.Bottom;
                _checkBoxDockBottom.Height = 24;
                _checkBoxDockBottom.FlatStyle = FlatStyle.System;
            
                _checkBoxDockFill.Appearance = Appearance.Button;
                _checkBoxDockFill.Dock = DockStyle.Fill;
                _checkBoxDockFill.FlatStyle = FlatStyle.System;

                this.Controls.AddRange(new Control[] {   _checkBoxDockFill,
                                                         _checkBoxDockBottom,
                                                         _checkBoxDockTop,
                                                         _checkBoxDockRight,
                                                         _checkBoxDockLeft,
                                                         _checkBoxFloat});

                Size = new Size(160, 144);
                BackColor = SystemColors.Control;
                ResumeLayout();
            }

            public void SetStates(DockAreas dockAreas)
            {
                _oldDockAreas = dockAreas;
                if (dockAreas.HasFlag(DockAreas.DockLeft))
                    _checkBoxDockLeft.Checked = true;
                if (dockAreas.HasFlag(DockAreas.DockRight))
                    _checkBoxDockRight.Checked = true;
                if (dockAreas.HasFlag(DockAreas.DockTop))
                    _checkBoxDockTop.Checked = true;
                if (dockAreas.HasFlag(DockAreas.DockBottom))
                    _checkBoxDockBottom.Checked = true;
                if (dockAreas.HasFlag(DockAreas.Document))
                    _checkBoxDockFill.Checked = true;
                if (dockAreas.HasFlag(DockAreas.Float))
                    _checkBoxFloat.Checked = true;
            }
        }

        private DockAreasEditor.DockAreasEditorControl _ui = null;

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            _ui ??= new DockAreasEditor.DockAreasEditorControl();

            _ui.SetStates((DockAreas)value);

            IWindowsFormsEditorService edSvc = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
            edSvc.DropDownControl(_ui);

            return _ui.DockAreas;
        }
    }
}
