using Linearstar.Windows.RawInput;

namespace BroadShare
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();


            var F = new Form1();
            F.Text = "init";
            Console.WriteLine("init");


            var Ms = RawInputMouse.GetDevices().OfType<RawInputMouse>().Where(_ => _.ProductId != 0 && _.ManufacturerName == "Microsoft").ToArray();

            var M = Ms[0];
            var window = new MouseRawInputReceiverWindow(M);
            window.RawInputEvent += (sender, data) =>
            {
                F.Invoke(() => F.Text = data.ToString());
                Console.WriteLine(data);
            };







            Application.Run(F);
        }








        class MouseRawInputReceiverWindow : NativeWindow, IDisposable
        {
            public MouseRawInputReceiverWindow(RawInputMouse mouse)
            {
                CreateHandle(new CreateParams());
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
                base.ReleaseHandle();
            }
        }



    }
}