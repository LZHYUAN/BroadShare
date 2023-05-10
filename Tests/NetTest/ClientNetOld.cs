using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetTest
{
    public class NetClientOld
    {
        public string Name;
        public string IP;
        public int Status;
        public readonly byte[] Title = { 152, 2, 84, 123 };
        public readonly int Port;

        public bool IsLintening = false;
        private Task _UDPListenerTask;
        private Task _TCPListenerTask;
        public NetClientOld(int port = 5543)
        {
            Port = port;
            IsLintening = true;
            _UDPListenerTask = Task.Run(_UDPListenerSync);
            _TCPListenerTask = Task.Run(_TCPListenerSync);
        }
        private void StopListener()
        {
            IsLintening = false;
            _UDPListenerTask.Wait();
            _TCPListenerTask.Wait();
        }
        private Task _TCPListenerSync()
        {
            var sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            sock.Bind(new IPEndPoint(IPAddress.Any, Port));
            sock.Listen(1);
            while (IsLintening)
            {

           var C= sock.Accept();
                Task.Delay(100).Wait();
                C.Dispose();
            }


            return Task.CompletedTask;
        }
        private Task _UDPListenerSync()
        {
            var sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            EndPoint ep = new IPEndPoint(IPAddress.Any, Port);
            sock.Bind(ep);

            while (IsLintening)
            {
                var buffer = new byte[5];

                var length = sock.ReceiveFrom(buffer, ref ep);

                if ( buffer.SequenceEqual(Title.Concat(new byte[] { 0 })))
                    UDPReceived(ep);

                Task.Delay(500).Wait();
            }
            sock.Dispose();
            return Task.CompletedTask;
        }
        private void UDPReceived(EndPoint ep)
        {
            var sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var iep = new IPEndPoint( (ep as IPEndPoint).Address,Port);
            try
            {
                sock.Connect(iep);

            }
            catch (Exception ex)
            {
            }
            finally
            {
                sock.Dispose(); 
            }
        }

        public void BoardcastFinder()
        {
            var ep = new IPEndPoint(IPAddress.Broadcast, Port);
            var udp = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            udp.EnableBroadcast = true;
            udp.SendTo(Title.Concat(new byte[] { 0 }).ToArray(), ep);
            udp.Dispose();
        }
        public event Action ReceiveFinder;


    }
}
