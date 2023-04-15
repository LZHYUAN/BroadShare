using Linearstar.Windows.RawInput;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace beta
{
    public enum Side
    {
        Left,
        Right,
        Top,
        Bottom,
    }

    public class SideTrigger
    {
        public SideTrigger(Side side = Side.Right, int screenIndex = 2)
        {
            Side = side;
            ScreenIndex = screenIndex;
            FrontRange = 0;
            BottomRange = 0;
            Duration = 200;

            var Ms = RawInputMouse.GetDevices().OfType<RawInputMouse>();
            var M = Ms.Where(_ => _.ProductId != 0 && _.ManufacturerName == "Microsoft").First();
            _rawinput = new MouseRawInputReceiveWindow(M);
            _rawinput.RawInputEvent += _rawinput_RawInputEvent;
            Task.Run(() => Application.Run());

        }

        public int Duration { get; set; } // ms
        public Side Side { get; set; }
        public int ScreenIndex { get; set; }
        public int FrontRange { get; set; } // pixel
        public int BottomRange { get; set; } // pixel

        public event Action OnTriggered;

        private MouseRawInputReceiveWindow _rawinput;
        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);
        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;

            public static implicit operator Point(POINT point)
            {
                return new Point(point.X, point.Y);
            }
        }

        long lastTime = 0;
        long startTime = 0;
        private void _rawinput_RawInputEvent(object sender, RawInputMouseData data)
        {
            GetCursorPos(out POINT pos);
            Debug.WriteLine((Point)pos);
            bool trigger = false;

            int axis = 0;
            Rectangle screen = Screen.AllScreens[ScreenIndex].Bounds;
            if (Side == Side.Left || Side == Side.Right)
            {
                axis = screen.X;
                if (Side == Side.Right)
                    axis += screen.Width-1;
                if(pos.X == axis && pos.Y>(screen.Y+FrontRange) && pos.Y<(screen.Y+screen.Height-BottomRange))
                    trigger = true;
            }
            else
            {
                axis = screen.Y;
                if (Side == Side.Bottom)
                    axis += screen.Height-1;
                if (pos.Y == axis && pos.X > (screen.X + FrontRange) && pos.X < (screen.X + screen.Width - BottomRange))
                    trigger = true;
            }

            

            if (trigger)
            {
                if (DateTime.Now.Ticks - lastTime > 100_000)
                    startTime = DateTime.Now.Ticks;
                lastTime = DateTime.Now.Ticks;
                if (DateTime.Now.Ticks - startTime > Duration * 10_000)
                {
                    lastTime = 0;
                    _Trigger();
                }
            }
        }

        private void _Trigger()
        {
            OnTriggered?.Invoke();
        }
    }
}
