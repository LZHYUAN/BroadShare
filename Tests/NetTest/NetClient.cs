using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;

namespace NetTest
{
    public class NetClient : IDisposable
    {
        public NetClient(int port)
        {
            Port = port;
            _dataBuilder = new DataBuilder();
            _StartListener();
        }
        public string ClientName { set => _dataBuilder.ClientName = value; get => _dataBuilder.ClientName; }
        public int Port { get; }

        public void ClientRequest()
        {
            using (UdpClient udpClient = new UdpClient())
            {
                udpClient.Send(_dataBuilder.GetClientRequestBytes(), new IPEndPoint(IPAddress.Broadcast, Port));
            }
        }

        public event Action<IPAddress, PhysicalAddress, string> ClientReceived;

        private DataBuilder _dataBuilder;
        private bool _isLintening;
        private Task _udpLintenerTask;
        private Task _tcpLintenerTask;

        private void _StartListener()
        {
            _isLintening = true;
            _udpLintenerTask = Task.Run(_UdpLintener);
            _tcpLintenerTask = Task.Run(_TcpLintener);
        }
        private void _StopLinstener()
        {
            _isLintening = false;
            _udpLintenerTask.Wait();
            _tcpLintenerTask.Wait();
        }

        private Task _UdpLintener()
        {
            using (UdpClient udpClient = new UdpClient())
            {
                IPEndPoint ipep = new IPEndPoint(IPAddress.Any, Port);
                udpClient.Client.Bind(ipep);
                while (_isLintening)
                {
                    if (udpClient.Available > 0)
                        _UdpRecevied(udpClient.Receive(ref ipep), ipep.Address);

                    Task.Delay(100).Wait();
                }
            }
            return Task.CompletedTask;
        }
        private Task _TcpLintener()
        {
            TcpListener tcpListener = new TcpListener(_dataBuilder.LocalAddress, Port);
            tcpListener.Start();
            while (_isLintening)
            {
                _TcpRecevied(tcpListener.AcceptTcpClient());
            }
            return Task.CompletedTask;
        }
        private void _UdpRecevied(byte[] data, IPAddress address)
        {
            var dataType = _dataBuilder.GetDataType(data);
            if (dataType == DataBuilder.DataType.ClientRequest)
                _TrySendClientSync(address);
        }
        private void _TcpRecevied(TcpClient tcpClient)
        {
            byte[] data = new byte[1024];
            NetworkStream stream = tcpClient.GetStream();
            stream.Read(data, 0, 5);
            var dataType = _dataBuilder.GetDataType(data);

            if (dataType == DataBuilder.DataType.ClientReturn)
            {
                Task.Delay(50).Wait();
                int length = stream.Read(data, 5, 1019);
                data = data[..(length + 5)];
                if (_dataBuilder.TryGetDataClientReturn(data, out byte[] mac, out string name))
                {
                    PhysicalAddress macObj = new PhysicalAddress(mac);
                    IPAddress ipObj = (stream.Socket.LocalEndPoint as IPEndPoint).Address;
                    Debug.WriteLine("ClientReceived:" + ipObj + "," + macObj + "," + name);
                    // if (!mac.SequenceEqual(_dataBuilder.LocalMac))
                    Task.Run(() => ClientReceived?.Invoke(ipObj, macObj, name));
                }
            }
        }

        private async Task<bool> _TrySendClientSync(IPAddress address)
        {
            try
            {
                _dataBuilder.RefreshLocalAddress();
                using (TcpClient tcpClient = new TcpClient())
                {
                    await tcpClient.ConnectAsync(address, Port);
                    await tcpClient.GetStream().WriteAsync(_dataBuilder.GetClientReturnBytes());
                    tcpClient.Close();
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        public void Dispose()
        {
            _StopLinstener();
        }

        private class DataBuilder
        {
            public DataBuilder()
            {
                // init
                Title = new byte[] { 1, 2, 3, 4 };
                if (string.IsNullOrEmpty(ClientName))
                    RefreshClientName();
                RefreshLocalMac();
                RefreshLocalAddress();
            }

            public void RefreshLocalMac()
            {
                NetworkInterface? net = NetworkInterface.GetAllNetworkInterfaces()
                    .Where(_ => _.OperationalStatus == OperationalStatus.Up && _.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                    .FirstOrDefault();
                if (net == null)
                    throw new Exception("Can't find Local Mac");
                LocalMac = net.GetPhysicalAddress().GetAddressBytes();
            }
            public void RefreshLocalAddress()
            {
                IPAddress? ip = Dns.GetHostAddresses(Dns.GetHostName())
                    .Where(_ => _.AddressFamily == AddressFamily.InterNetwork)
                    .FirstOrDefault();
                if (ip == null)
                    throw new Exception("Can't find Local ip");
                LocalAddress = ip;
            }
            public void RefreshClientName()
            {
                ClientName = Dns.GetHostName();
            }

            public byte[] Title { get; }
            public byte[] LocalMac { private set; get; }
            public IPAddress LocalAddress { private set; get; }
            public string ClientName { set; get; }

            public byte[] GetClientRequestBytes()
            {
                return Title
                    .Append((byte)DataType.ClientRequest)
                    .ToArray();
            }
            public byte[] GetClientReturnBytes()
            {
                if (string.IsNullOrEmpty(ClientName))
                    RefreshClientName();

                var tmp = Title
                     .Append((byte)DataType.ClientReturn)
                     .Concat(LocalMac)
                     .Concat(Encoding.Unicode.GetBytes(ClientName))
                     .ToArray();

                return Title
                     .Append((byte)DataType.ClientReturn)
                     .Concat(LocalMac)
                     .Concat(Encoding.Unicode.GetBytes(ClientName))
                     .ToArray();
            }

            public DataType GetDataType(byte[] data)
            {
                if (data.Length < 5 || !data[0..4].SequenceEqual(Title) || data[4] > 2)
                    return DataType.Unknow;

                return (DataType)data[4];
            }
            public bool TryGetDataClientReturn(byte[] data, out byte[] mac, out string name)
            {
                mac = new byte[0];
                name = string.Empty;
                if (data.Length < 11 || GetDataType(data) != DataType.ClientReturn)
                    return false;
                mac = data[5..11];
                name = Encoding.Unicode.GetString(data[11..]);
                return true;
            }

            public enum DataType : byte
            {
                ClientRequest,
                ClientReturn,
                SendFile,
                Unknow,
            }
        }
    }
}
