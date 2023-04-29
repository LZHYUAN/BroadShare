using System.Net;
using System.Net.Sockets;
using System.Text;

namespace NetTest
{
    internal class Program
    {
        static void Main(string[] args)
        {



            /*
            Task.Run(() =>
            {
                ReceiveBroadcastMessage((EndPoint ep, string data) =>
                {
                    Console.WriteLine(string.Format("received: {0} from: {1}", data, ep.ToString()));


                }, 9051);
            });
            Task.Delay(1000).Wait();
            Console.WriteLine("send?");
            BroadcastMessage("ad", 9051);
            */

            var N = new NetClient();




            Task.Delay(1000).Wait();


            N.BoardcastFinder();

            Task.Delay(-1).Wait();

        }


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
            using (var sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
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

                    Task.Delay(500).Wait();
                }
            }
        }
    }
}