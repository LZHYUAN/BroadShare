using System.Net;
using System.Net.Sockets;

namespace NetTest
{
    public class DynamicCenter
    {
        public DynamicCenter(byte[] title, int port)
        {
            Title = title;
            Port = port;
        }
        public void Start()
        {
            //SockUDP


            _sockUDP = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
            {
                EnableBroadcast = true,
            };
            //SockTCP
            _sockTCP = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //SockTCPListener

            var ep = (EndPoint)new IPEndPoint(IPAddress.Any, Port);
            _sockTCP.Bind(ep);
            byte[] buffer = new byte[256];
            _sockTCP.ReceiveFrom(buffer, ref ep);


            _GetCenter();
        }
        public void Stop()
        {
            _sockUDP.Dispose();
            _sockTCP.Dispose();
        }
        public byte[] Title { get; }
        public int Port { get; }
        public bool IsCenter { private set; get; }

        private Socket _sockUDP;
        private Socket _sockTCP;
        private Socket _sockTCPListener;
        private List<Clent> _lastClients;
        private void _CenterService()
        {

        }
        private void _GetCenter()
        {
            IsCenter = false;

            _sockUDP.SendTo(Title.Append((byte)DataType.GetCenter).ToArray(), new IPEndPoint(IPAddress.Broadcast, Port));
            //receive from server
            byte[] buffer = new byte[256];
            _sockTCPListener.Receive(buffer, SocketFlags.None);



        }
        private void _RefreshLastClients()
        {

        }
        private void _CheckLastClients()
        {
            if (!IsCenter)
                throw new Exception("only center can check clients");


        }
        private enum DataType : byte
        {
            GetCenter,
            ClientList,
        }

        public Clent[] GetClients()
        {
            _RefreshLastClients();

            return _lastClients.ToArray();
        }


    }
    public struct Clent
    {

    }
}
