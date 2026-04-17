using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace EDGBAPRO_Rom_Manager.Services
{
    public static class WindowsThemeHelper
    {
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int dwAttribute, ref int pvAttribute, int cbAttribute);

        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int SetWindowTheme(IntPtr hWnd, string pszSubAppName, string? pszSubIdList);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        public static void ApplyDarkTitleBar(Form form)
        {
            void Apply()
            {
                try
                {
                    int useDark = 1;
                    DwmSetWindowAttribute(form.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref useDark, sizeof(int));
                }
                catch { }
            }

            if (form.IsHandleCreated) Apply();
            else form.HandleCreated += (_, __) => Apply();
        }

        public static void ApplyDarkExplorerTheme(Control control)
        {
            void Apply()
            {
                try { SetWindowTheme(control.Handle, "DarkMode_Explorer", null); } catch { }
            }

            if (control.IsHandleCreated) Apply();
            else control.HandleCreated += (_, __) => Apply();
        }
    }
}
