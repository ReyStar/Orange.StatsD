using System;
using System.Threading.Tasks;

namespace Orange.StatsD.Transport
{
    public interface IStatsDClient:IDisposable
    {
        void Connect();
        Task SendAsync(byte[] bytes, int bytesLength);
        void Close();
    }
}
