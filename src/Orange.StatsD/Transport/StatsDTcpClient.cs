using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Orange.StatsD.Transport
{
    public class StatsDTcpClient : StatsDClientBase
    {
        private TcpClient _client;

        public StatsDTcpClient(string url, int port) : base(url, port)
        {
            _client = new TcpClient();
        }

        public override void Connect()
        {
            _client.Connect(Url, Port);
        }

        protected override Task<int> OnSendAsync(byte[] bytes, int bytesLength)
        {
            if (_client.Connected)
            {
                Connect();
            }

            return _client.Client.SendAsync(new ArraySegment<byte>(bytes, 0, bytesLength), SocketFlags.None);
        }

        public override void Close()
        {
            _client?.Close();
        }

        protected override void OnDispose(bool disposing)
        {
            if (!disposing)
                return;

            _client?.Close();
            _client = null;
        }
    }
}
