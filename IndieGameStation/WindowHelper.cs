using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IndieGameStation
{
    public static class WindowHelper
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("USER32.DLL")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("USER32.DLL")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(HandleRef hWnd, [In, Out] ref RECT rect);

        public static int GWL_STYLE = -16;
        public static int WS_CHILD = 0x40000000; //child window
        public static int WS_BORDER = 0x00800000; //window with border
        public static int WS_DLGFRAME = 0x00400000; //window with double border but no title
        public static int WS_CAPTION = WS_BORDER | WS_DLGFRAME; //window with a title bar

        public static bool IsFullscreen(Process p, GraphicsDevice gd)
        {
            var handleRef = new HandleRef(null, p.MainWindowHandle);
            var bounds = new RECT();
            var screen = Screen.PrimaryScreen;

            GetWindowRect(handleRef, ref bounds);

            if (bounds.bottom <= 0 && bounds.top <= 0 && bounds.right <= 0 && bounds.left <= 0)
                return true;

            if (bounds.bottom - bounds.top >= screen.Bounds.Height && bounds.right - bounds.left >= screen.Bounds.Width)
                return true;

            return false;
        }

        public static void Fullscreenize(Process p, GraphicsDevice gd)
        {
            var handle = p.MainWindowHandle;

            int style = GetWindowLong(handle, GWL_STYLE);
            SetWindowLong(handle, GWL_STYLE, (style & ~WS_CAPTION));

            MoveWindow(handle, -12, -12, gd.DisplayMode.Width + 12, gd.DisplayMode.Height + 12, false);
        }

        public static void SetWindowTransparency(IntPtr hWnd, Color color)
        {
            Control ctrl = Control.FromHandle(hWnd);
            Form form = ctrl.FindForm();
            form.TransparencyKey = System.Drawing.Color.FromArgb(color.R, color.G, color.B);
        }
    }
}
