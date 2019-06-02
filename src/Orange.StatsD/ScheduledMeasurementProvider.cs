using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Orange.StatsD.Transport;

namespace Orange.StatsD
{
    /// <summary>
    /// Measurement provider to send metrics by timer
    /// </summary>
    public class ScheduledMeasurementProvider : MeasurementProvider
    {
        private readonly TimeSpan _sendPeriod;
        private CancellationTokenSource _tokenSource;
        private Timer _timer;

        /// <summary>
        /// Create provider instance
        /// </summary>
        /// <param name="scope">Metrics prefix</param>
        /// <param name="client">StatsD client</param>
        /// <param name="sendPeriod">period to flush metrics, must be very small</param>
        public ScheduledMeasurementProvider(string scope, IStatsDClient client, TimeSpan sendPeriod)
            : base(scope, client)
        {
            _sendPeriod = sendPeriod;
            _tokenSource = new CancellationTokenSource();
            _timer = new Timer(async o => await SendAsync(_tokenSource.Token), null, Timeout.Infinite, Timeout.Infinite);
            _timer.Change(_sendPeriod, Timeout.InfiniteTimeSpan);
        }

        ~ScheduledMeasurementProvider()
        {
            OnDispose(false);
        }

        protected override void OnDispose(bool disposing)
        {
            if (disposing) return;

            var tokenSource = _tokenSource;
            _tokenSource = null;
            tokenSource?.Cancel();
            tokenSource?.Dispose();

            var timer = _timer;
            _timer = null;
            timer?.Dispose();
        }

        private async Task SendAsync(CancellationToken tokenSourceToken)
        {
            await FlushAsync();

            if (!tokenSourceToken.IsCancellationRequested)
            {
                _timer.Change(_sendPeriod, Timeout.InfiniteTimeSpan);
            }
        }
    }
}
