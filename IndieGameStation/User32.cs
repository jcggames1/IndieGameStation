using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace IndieGameStation
{
    class User32
    {
        [DllImport("user32.dll")]
        public static extern void SetWindowPos(uint Hwnd, int Level, int X, int Y, int W, int H, uint Flags);
    }
}
