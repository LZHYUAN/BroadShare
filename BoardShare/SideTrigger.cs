using Linearstar.Windows.RawInput;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RawInput;
using System.Drawing;
using Linearstar.Windows.RawInput.Native;

namespace BoardShare
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
        public int CenterWidth { get; set; } // pixel

        public bool WithHover { get; set; } = false;

        public event Action TriggerEvent;
        public event Action LeaveEvent;

        private MouseRawInputReceiveWindow _rawinput;

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);
        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        long _lastTime = 0;
        long _startTime = 0;
        bool _leftDown= false;
        bool _rightDown= false;
        POINT _lastPos =new POINT { X=0,Y =0} ;
        private void _rawinput_RawInputEvent(object sender, RawInputMouseData data)
        {
            GetCursorPos(out POINT pos);

            bool trigger = true;

            //--PosCheck
            int axis = 0;
            Rectangle screen = Screen.AllScreens[ScreenIndex].Bounds;
            if (Side == Side.Left || Side == Side.Right)
            {
                axis = screen.X;
                if (Side == Side.Right)
                    axis += screen.Width - 1;
                if (!(pos.X == axis && pos.Y > (screen.Y + FrontRange) && pos.Y < (screen.Y + screen.Height - BottomRange)))
                    trigger = false;
            }
            else
            {
                axis = screen.Y;
                if (Side == Side.Bottom)
                    axis += screen.Height - 1;
                if (!(pos.Y == axis && pos.X > (screen.X + FrontRange) && pos.X < (screen.X + screen.Width - BottomRange)))
                    trigger = false;
            }

            //--HoverCheck
            if (data.Mouse.Buttons == RawMouseButtonFlags.LeftButtonDown) _leftDown=true;
            if (data.Mouse.Buttons == RawMouseButtonFlags.RightButtonDown) _rightDown=true;
            if (data.Mouse.Buttons == RawMouseButtonFlags.LeftButtonUp) _leftDown = false;
            if (data.Mouse.Buttons == RawMouseButtonFlags.RightButtonUp) _rightDown = false;
            if (WithHover && !(_leftDown||_rightDown))
                trigger = false;


            if (trigger)
            {
                //--TimeCheck
                if (DateTime.Now.Ticks - _lastTime > 50_000)
                    _startTime = DateTime.Now.Ticks;
                _lastTime = DateTime.Now.Ticks;
                if (DateTime.Now.Ticks - _startTime > Duration * 10_000)
                {
                    _lastTime = 0;
                    _Trigger();
                }
            }
        }

        private void _Trigger()
        {
            TriggerEvent?.Invoke();
            Debug.WriteLine("TriggerEvent");
        }
        private void _Leave()
        {
            LeaveEvent?.Invoke();
            Debug.WriteLine("LeaveEvent");
        }
    }
}
