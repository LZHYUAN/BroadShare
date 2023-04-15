using System.Diagnostics;
using System.Runtime.InteropServices;
using Linearstar.Windows.RawInput;

namespace rawinput_test
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var C = new Form1();
            C.button1_Click();


            Task.Delay(-1).Wait();

            var Ms = RawInputMouse.GetDevices().OfType<RawInputMouse>();

            var M = Ms.Where(_ => _.ProductId != 0 && _.ManufacturerName == "Microsoft").First();
            var window = new MouseRawInputReceiverWindow(M);
            Task.Run(() => Application.Run());
            window.RawInputEvent += (sender, data) =>
            {
                Console.WriteLine(data);
            };
        }
    }

    class MouseRawInputReceiverWindow : NativeWindow, IDisposable
    {
        public MouseRawInputReceiverWindow(RawInputMouse mouse)
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
