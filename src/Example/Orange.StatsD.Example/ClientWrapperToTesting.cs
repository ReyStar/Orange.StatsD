using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Orange.StatsD.Transport;

namespace Orange.StatsD.Example
{
    public class ClientWrapperToTesting<T> : IStatsDClient
        where T : class, IStatsDClient
    {
        private T _statsDClientImplementation;
        private volatile int _messageCounter;
        private SpinLock _spinLock;

        public ClientWrapperToTesting(T statsDClientImplementation)
        {
            _statsDClientImplementation = statsDClientImplementation;
            _messageCounter = 0;
            _spinLock = new SpinLock(true);
        }

        public void Dispose()
        {
            _statsDClientImplementation?.Dispose();
            _statsDClientImplementation = null;
        }

        public void Connect()
        {
            _statsDClientImplementation.Connect();
        }

        public Task SendAsync(byte[] bytes, int bytesLength)
        {
            var isLocked = false;

            try
            {
                do { _spinLock.TryEnter(10, ref isLocked); } while (!isLocked);

                Console.WriteLine($"Sending message # {_messageCounter}");
                Console.Write($"{DateTime.UtcNow} - {Encoding.UTF8.GetString(bytes)}");

                return _statsDClientImplementation.SendAsync(bytes, bytesLength);
            }
            finally
            {
                if (isLocked)
                {
                    Console.WriteLine($"Sent message # {_messageCounter}");
                    _spinLock.Exit();
                }
            }

        }

        public void Close()
        {
            _statsDClientImplementation.Close();
        }
    }
}
