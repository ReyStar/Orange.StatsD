using System.Net.Sockets;
using System.Threading.Tasks;

namespace Orange.StatsD.Transport
{
    public class StatsDUdpClient : StatsDClientBase
    {
        private UdpClient _client;

        public StatsDUdpClient(string url, int port) : base(url, port)
        {
            _client = new UdpClient
            {
                ExclusiveAddressUse = false
            };
        }

        public override void Connect()
        {
            _client.Connect(Url, Port);
        }

        protected override Task<int> OnSendAsync(byte[] bytes, int bytesLength)
        {
            if (!_client.Client.Connected)
            {
                Connect();
            }

            return _client.SendAsync(bytes, bytesLength);
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
