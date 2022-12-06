using Microsoft.Win32;
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
#if NET35 || NET40
using System.Configuration;
using WeifenLuo.WinFormsUI.Docking.Configuration;
#endif

namespace WeifenLuo.WinFormsUI.Docking
{
    public static class PatchController
    {
        public static bool? EnableAll { private get; set; }

        public static void Reset()
        {
            EnableAll = _highDpi = _memoryLeakFix 
                = _nestedDisposalFix = _focusLostFix = _contentOrderFix
                = _fontInheritanceFix = _activeXFix = _displayingPaneFix
                = _activeControlFix = _floatSplitterFix = _activateOnDockFix
                = _selectClosestOnClose = _perScreenDpi = null;
        }

#region Copy this section to create new option, and then comment it to show what needs to be modified.
        //*
        private static bool? _highDpi;

        public static bool? EnableHighDpi
        {
            get
            {
                if (_highDpi != null)
                {
                    return _highDpi;
                }

                if (EnableAll != null)
                {
                    return _highDpi = EnableAll;
                }
#if NET35 || NET40
                var section = ConfigurationManager.GetSection("dockPanelSuite") as PatchSection;
                if (section != null)
                {
                    if (section.EnableAll != null)
                    {
                        return _highDpi = section.EnableAll;
                    }

                    return _highDpi = section.EnableHighDpi;
                }
#endif
                (bool found, bool value) = CheckIfVariableSet("EnableHighDpi");
                if (found)
                {
                    return _highDpi = value;
                }

                return _highDpi = true;
            }

            set
            {
                _highDpi = value;
            }
        }
        // */
#endregion

        private static bool? _memoryLeakFix;

        public static bool? EnableMemoryLeakFix
        {
            get
            {
                if (_memoryLeakFix != null)
                {
                    return _memoryLeakFix;
                }

                if (EnableAll != null)
                {
                    return _memoryLeakFix = EnableAll;
                }
#if NET35 || NET40
                var section = ConfigurationManager.GetSection("dockPanelSuite") as PatchSection;
                if (section != null)
                {
                    if (section.EnableAll != null)
                    {
                        return _memoryLeakFix = section.EnableAll;
                    }

                    return _memoryLeakFix = section.EnableMemoryLeakFix;
                }
#endif
                (bool found, bool value) = CheckIfVariableSet("EnableMemoryLeakFix");
                if (found)
                {
                    return _memoryLeakFix = value;
                }

                return _memoryLeakFix = true;
            }

            set
            {
                _memoryLeakFix = value;
            }
        }

        private static bool? _focusLostFix;

        public static bool? EnableMainWindowFocusLostFix
        {
            get
            {
                if (_focusLostFix != null)
                {
                    return _focusLostFix;
                }

                if (EnableAll != null)
                {
                    return _focusLostFix = EnableAll;
                }
#if NET35 || NET40
                var section = ConfigurationManager.GetSection("dockPanelSuite") as PatchSection;
                if (section != null)
                {
                    if (section.EnableAll != null)
                    {
                        return _focusLostFix = section.EnableAll;
                    }

                    return _focusLostFix = section.EnableMainWindowFocusLostFix;
                }
#endif
                (bool found, bool value) = CheckIfVariableSet("EnableMainWindowFocusLostFix");
                if (found)
                {
                    return _focusLostFix = value;
                }

                return _focusLostFix = true;
            }

            set
            {
                _focusLostFix = value;
            }
        }

        private static bool? _nestedDisposalFix;

        public static bool? EnableNestedDisposalFix
        {
            get
            {
                if (_nestedDisposalFix != null)
                {
                    return _nestedDisposalFix;
                }

                if (EnableAll != null)
                {
                    return _nestedDisposalFix = EnableAll;
                }
#if NET35 || NET40
                var section = ConfigurationManager.GetSection("dockPanelSuite") as PatchSection;
                if (section != null)
                {
                    if (section.EnableAll != null)
                    {
                        return _nestedDisposalFix = section.EnableAll;
                    }

                    return _nestedDisposalFix = section.EnableNestedDisposalFix;
                }
#endif
                (bool found, bool value) = CheckIfVariableSet("EnableNestedDisposalFix");
                if (found)
                {
                    return _nestedDisposalFix = value;
                }

                return _nestedDisposalFix = true;
            }

            set
            {
                _focusLostFix = value;
            }
        }

        private static bool? _fontInheritanceFix;

        public static bool? EnableFontInheritanceFix
        {
            get
            {
                if (_fontInheritanceFix != null)
                {
                    return _fontInheritanceFix;
                }

                if (EnableAll != null)
                {
                    return _fontInheritanceFix = EnableAll;
                }
#if NET35 || NET40
                var section = ConfigurationManager.GetSection("dockPanelSuite") as PatchSection;
                if (section != null)
                {
                    if (section.EnableAll != null)
                    {
                        return _fontInheritanceFix = section.EnableAll;
                    }

                    return _fontInheritanceFix = section.EnableFontInheritanceFix;
                }
#endif
                (bool found, bool value) = CheckIfVariableSet("EnableFontInheritanceFix");
                if (found)
                {
                    return _fontInheritanceFix = value;
                }

                return _fontInheritanceFix = true;
            }

            set
            {
                _fontInheritanceFix = value;
            }
        }

        private static bool? _contentOrderFix;

        public static bool? EnableContentOrderFix
        {
            get
            {
                if (_contentOrderFix != null)
                {
                    return _contentOrderFix;
                }

                if (EnableAll != null)
                {
                    return _contentOrderFix = EnableAll;
                }
#if NET35 || NET40
                var section = ConfigurationManager.GetSection("dockPanelSuite") as PatchSection;
                if (section != null)
                {
                    if (section.EnableAll != null)
                    {
                        return _contentOrderFix = section.EnableAll;
                    }

                    return _contentOrderFix = section.EnableContentOrderFix;
                }
#endif
                (bool found, bool value) = CheckIfVariableSet("EnableContentOrderFix");
                if (found)
                {
                    return _contentOrderFix = value;
                }

                return _contentOrderFix = true;
            }

            set
            {
                _contentOrderFix = value;
            }
        }

        private static bool? _activeXFix;

        public static bool? EnableActiveXFix
        {
            get
            {
                if (_activeXFix != null)
                {
                    return _activeXFix;
                }

                if (EnableAll != null)
                {
                    return _activeXFix = EnableAll;
                }
#if NET35 || NET40
                var section = ConfigurationManager.GetSection("dockPanelSuite") as PatchSection;
                if (section != null)
                {
                    if (section.EnableAll != null)
                    {
                        return _activeXFix = section.EnableAll;
                    }

                    return _activeXFix = section.EnableActiveXFix;
                }
#endif
                (bool found, bool value) = CheckIfVariableSet("EnableActiveXFix");
                if (found)
                {
                    return _activeXFix = value;
                }

                return _activeXFix = false; // not enabled by default as it has side effect.
            }

            set
            {
                _activeXFix = value;
            }
        }

        private static bool? _displayingPaneFix;

        public static bool? EnableDisplayingPaneFix
        {
            get
            {
                if (_displayingPaneFix != null)
                {
                    return _displayingPaneFix;
                }

                if (EnableAll != null)
                {
                    return _displayingPaneFix = EnableAll;
                }
#if NET35 || NET40
                var section = ConfigurationManager.GetSection("dockPanelSuite") as PatchSection;
                if (section != null)
                {
                    if (section.EnableAll != null)
                    {
                        return _displayingPaneFix = section.EnableAll;
                    }

                    return _displayingPaneFix = section.EnableDisplayingPaneFix;
                }
#endif
                (bool found, bool value) = CheckIfVariableSet("EnableDisplayingPaneFix");
                if (found)
                {
                    return _displayingPaneFix = value;
                }

                return _displayingPaneFix = true;
            }

            set
            {
                _displayingPaneFix = value;
            }
        }

        private static bool? _activeControlFix;

        public static bool? EnableActiveControlFix
        {
            get
            {
                if (_activeControlFix != null)
                {
                    return _activeControlFix;
                }

                if (EnableAll != null)
                {
                    return _activeControlFix = EnableAll;
                }
#if NET35 || NET40
                var section = ConfigurationManager.GetSection("dockPanelSuite") as PatchSection;
                if (section != null)
                {
                    if (section.EnableAll != null)
                    {
                        return _activeControlFix = section.EnableAll;
                    }

                    return _activeControlFix = section.EnableActiveControlFix;
                }
#endif
                (bool found, bool value) = CheckIfVariableSet("EnableActiveControlFix");
                if (found)
                {
                    return _activeControlFix = value;
                }

                return _activeControlFix = true;
            }

            set
            {
                _activeControlFix = value;
            }
        }

        private static bool? _floatSplitterFix;

        public static bool? EnableFloatSplitterFix
        {
            get
            {
                if (_floatSplitterFix != null)
                {
                    return _floatSplitterFix;
                }

                if (EnableAll != null)
                {
                    return _floatSplitterFix = EnableAll;
                }
#if NET35 || NET40
                var section = ConfigurationManager.GetSection("dockPanelSuite") as PatchSection;
                if (section != null)
                {
                    if (section.EnableAll != null)
                    {
                        return _floatSplitterFix = section.EnableAll;
                    }

                    return _floatSplitterFix = section.EnableFloatSplitterFix;
                }
#endif
                (bool found, bool value) = CheckIfVariableSet("EnableFloatSplitterFix");
                if (found)
                {
                    return _floatSplitterFix = value;
                }

                return _floatSplitterFix = true;
            }

            set
            {
                _floatSplitterFix = value;
            }
        }

        private static bool? _activateOnDockFix;

        public static bool? EnableActivateOnDockFix
        {
            get
            {
                if (_activateOnDockFix != null)
                {
                    return _activateOnDockFix;
                }

                if (EnableAll != null)
                {
                    return _activateOnDockFix = EnableAll;
                }
#if NET35 || NET40
                var section = ConfigurationManager.GetSection("dockPanelSuite") as PatchSection;
                if (section != null)
                {
                    if (section.EnableAll != null)
                    {
                        return _activateOnDockFix = section.EnableAll;
                    }

                    return _activateOnDockFix = section.EnableActivateOnDockFix;
                }
#endif
                (bool found, bool value) = CheckIfVariableSet("EnableActivateOnDockFix");
                if (found)
                {
                    return _activateOnDockFix = value;
                }

                return _activateOnDockFix = true;
            }

            set
            {
                _activateOnDockFix = value;
            }
        }

        private static bool? _selectClosestOnClose;

        public static bool? EnableSelectClosestOnClose
        {
            get
            {
                if (_selectClosestOnClose != null)
                {
                    return _selectClosestOnClose;
                }

                if (EnableAll != null)
                {
                    return _selectClosestOnClose = EnableAll;
                }
#if NET35 || NET40
                var section = ConfigurationManager.GetSection("dockPanelSuite") as PatchSection;
                if (section != null)
                {
                    if (section.EnableAll != null)
                    {
                        return _selectClosestOnClose = section.EnableAll;
                    }

                    return _selectClosestOnClose = section.EnableSelectClosestOnClose;
                }
#endif
                (bool found, bool value) = CheckIfVariableSet("EnableSelectClosestOnClose");
                if (found)
                {
                    return _selectClosestOnClose = value;
                }

                return _selectClosestOnClose = true;
            }

            set
            {
                _selectClosestOnClose = value;
            }
        }

        private static bool? _perScreenDpi;

        public static bool? EnablePerScreenDpi
        {
            get
            {
                if (_perScreenDpi != null)
                {
                    return _perScreenDpi;
                }

                if (EnableAll != null)
                {
                    return _perScreenDpi = EnableAll;
                }
#if NET35 || NET40
                var section = ConfigurationManager.GetSection("dockPanelSuite") as PatchSection;
                if (section != null)
                {
                    if (section.EnableAll != null)
                    {
                        return _perScreenDpi = section.EnableAll;
                    }

                    return _perScreenDpi = section.EnablePerScreenDpi;
                }
#endif
                (bool found, bool value) = CheckIfVariableSet("EnablePerScreenDpi");
                if (found)
                {
                    return _perScreenDpi = value;
                }

                return _perScreenDpi = false;
            }

            set
            {
                _perScreenDpi = value;
            }
        }

        private static (bool found, bool value) CheckIfVariableSet(string variable)
        {
            (bool found, bool enabled) = CheckEnvironmentVariable($"DPS_{variable}");
            if (found)
            {
                return (found, enabled);
            }
            else
            {
                return CheckRegistry(variable);
            }
        }
        
        private static (bool found, bool value) CheckEnvironmentVariable(string environmentVariable)
        {
            var environment = Environment.GetEnvironmentVariable(environmentVariable);
            if (!string.IsNullOrEmpty(environment) && bool.TryParse(environment, out bool enableEnvironmentVariable))
            {
                return (true, enableEnvironmentVariable);
            }
            return (false, false);
        }

        private static (bool found, bool value) CheckRegistry(string registryValue)
        {
            var key = Registry.CurrentUser.OpenSubKey(@"Software\DockPanelSuite");
            if (key != null)
            {
                var pair = key.GetValue(registryValue);
                key.Close();
                if (pair != null && bool.TryParse(pair.ToString(), out bool enableCurrentUser))
                {
                    return (true, enableCurrentUser);
                }
            }

            key = Registry.LocalMachine.OpenSubKey(@"Software\DockPanelSuite");
            if (key != null)
            {
                var pair = key.GetValue(registryValue);
                key.Close();
                if (pair != null && bool.TryParse(pair.ToString(), out bool enableLocalMachine))
                {
                    return (true, enableLocalMachine);
                }
            }
            return (false, false);
        }
    }
}
