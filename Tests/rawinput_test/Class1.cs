using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace rawinput_test
{
    public partial class Form1 : Form
    {
        [DllImport("user32.dll")]
        static extern bool UpdateWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll", SetLastError = true, EntryPoint = "CreateWindowEx")]
        public static extern IntPtr CreateWindowEx(
           int dwExStyle,
           //UInt16 regResult,
           [MarshalAs(UnmanagedType.LPStr)]
       string lpClassName,
           [MarshalAs(UnmanagedType.LPStr)]
       string lpWindowName,
           UInt32 dwStyle,
           int x,
           int y,
           int nWidth,
           int nHeight,
           IntPtr hWndParent,
           IntPtr hMenu,
           IntPtr hInstance,
           IntPtr lpParam);
        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
        static extern System.UInt16 RegisterClassW(
            [System.Runtime.InteropServices.In] ref WNDCLASSEX lpWndClass
        );
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        struct WNDCLASSEX
        {
            [MarshalAs(UnmanagedType.U4)]
            public int cbSize;
            [MarshalAs(UnmanagedType.U4)]
            public int style;
            public IntPtr lpfnWndProc;
            public int cbClsExtra;
            public int cbWndExtra;
            public IntPtr hInstance;
            public IntPtr hIcon;
            public IntPtr hCursor;
            public IntPtr hbrBackground;
            [MarshalAs(UnmanagedType.LPStr)]
            public string lpszMenuName;
            [MarshalAs(UnmanagedType.LPStr)]
            public string lpszClassName;
            public IntPtr hIconSm;
        }
        [DllImport("user32.dll")]
        static extern IntPtr DefWindowProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);


        public void button1_Click()
        {
            WNDCLASSEX wind_class = new WNDCLASSEX();
            wind_class.cbSize = Marshal.SizeOf(typeof(WNDCLASSEX));
            wind_class.style = 0x00020000;
            wind_class.hbrBackground = (IntPtr)5;
            wind_class.cbClsExtra = 0;
            wind_class.cbWndExtra = 0;
            wind_class.hInstance = Marshal.GetHINSTANCE(GetType().Module);
            wind_class.hIcon = this.Icon.Handle;
            wind_class.hCursor = IntPtr.Zero;
            wind_class.lpszMenuName = string.Empty;
            wind_class.lpszClassName = "MyClass";
            wind_class.lpfnWndProc = DefWindowProc(IntPtr.Zero, 0, IntPtr.Zero, IntPtr.Zero);
            RegisterClassW(ref wind_class);
            IntPtr lolo = CreateWindowEx(0, "MyClass", "MyClass", 0, 0, 0, 30, 40, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
            ShowWindow(lolo, 1);
            UpdateWindow(lolo);
        }
    }

}
