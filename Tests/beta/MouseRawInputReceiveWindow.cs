using Linearstar.Windows.RawInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace beta
{
    public class MouseRawInputReceiveWindow : NativeWindow, IDisposable
    {
        public MouseRawInputReceiveWindow(RawInputMouse mouse)
        {
            base.CreateHandle(new CreateParams());
            Mouse = mouse;
            RawInputMouse.RegisterDevice(Mouse.UsageAndPage, RawInputDeviceFlags.InputSink, this.Handle);
        }

        public RawInputMouse Mouse { get; }
        public event Action<object, RawInputMouseData> RawInputEvent;

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x00FF)
                RawInputEvent?.Invoke(this, (RawInputMouseData)RawInputData.FromHandle(m.LParam));
            else
                base.WndProc(ref m);
        }
        public void Dispose()
        {
            RawInputMouse.UnregisterDevice(Mouse.UsageAndPage);
            base.DestroyHandle();
        }
    }
}
