using System.ComponentModel;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace NetTest
{
    internal class Program
    {
        static void Main(string[] args)
        {

            //var CC = new NetClient(12345);

            //CC.ClientReceived += CC_ClientReceived;

            //while (true)
            //{
            //    Console.ReadKey();
            //    CC.ClientRequest();
            //}

            Task.Run(() =>
            {


                ReceiveBroadcastMessage((EndPoint ep, string s) =>
                {
                    Console.WriteLine(ep + " , " + s);

                }, 12345);

            });
            while (true)
            {
                BroadcastMessage(Console.ReadLine(), 12345);
            }

            Task.Delay(-1).Wait();

        }

        //private static void CC_ClientReceived(IPAddress arg1, PhysicalAddress arg2, string arg3)
        //{
        //    Console.WriteLine(arg1 + " , " + arg2 + " , " + arg3);
        //}


        private static void BroadcastMessage(string message, int port)
        {
            BroadcastMessage(Encoding.ASCII.GetBytes(message), port);
        }

        private static void BroadcastMessage(byte[] message, int port)
        {
            using (var sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram,
             ProtocolType.Udp))
            {
                sock.EnableBroadcast = true;
                sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);

                var iep = new IPEndPoint(IPAddress.Broadcast, port);

                sock.SendTo(message, iep);
            }
        }

        private static void ReceiveBroadcastMessage(Action<EndPoint, string> receivedAction, int port)
        {
            ReceiveBroadcastMessage((ep, data) =>
            {
                var stringData = Encoding.ASCII.GetString(data);
                receivedAction(ep, stringData);
            }, port);
        }

        private static void ReceiveBroadcastMessage(Action<EndPoint, byte[]> receivedAction, int port)
        {
            using (var sock = new Socket(AddressFamily.InterNetwork,
 SocketType.Dgram, ProtocolType.Udp))
            {
                var ep = new IPEndPoint(IPAddress.Any, port) as EndPoint;
                sock.Bind(ep);

                while (true)
                {
                    var buffer = new byte[1024];
                    var recv = sock.ReceiveFrom(buffer, ref ep);

                    var data = new byte[recv];

                    Array.Copy(buffer, 0, data, 0, recv);

                    receivedAction(ep, data);

                    Thread.Sleep(500);
                }
            }
        }


    }
}