using System;
using System.Threading.Tasks;
using Orange.StatsD.Exceptions;

namespace Orange.StatsD.Transport
{
    public abstract class StatsDClientBase: IStatsDClient
    {
        protected readonly string Url;
        protected readonly int Port;

        protected StatsDClientBase(string url, int port)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentException($"{nameof(url)} can't be null or empty");
            }

            Url = url;

            if (port>65535 && port < 1)
            {
                throw new ArgumentException($"{nameof(port)} must be between 0 and 65535");
            }

            Port = port;
        }

        ~StatsDClientBase()
        {
            OnDispose(false);
        }

        public void Dispose()
        {
            Close();
            OnDispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void OnDispose(bool disposing)
        {
        }

        public abstract void Connect();

        public async Task SendAsync(byte[] bytes, int bytesLength)
        {
            var sentBytes = await OnSendAsync(bytes, bytesLength);
            if (sentBytes != bytesLength)
            {
                throw new SendPackageException($"Send package error: {sentBytes} bytes sent, {bytesLength} bytes should be sent.");
            }
        }

        protected abstract Task<int> OnSendAsync(byte[] bytes, int bytesLength);

        public abstract void Close();
    }
}
