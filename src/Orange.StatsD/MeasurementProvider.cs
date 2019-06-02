using System;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Orange.StatsD.Transport;

namespace Orange.StatsD
{
    /// <summary>
    /// Measurement provider to send metrics
    /// https://github.com/statsd/statsd/tree/master/docs
    /// </summary>
    public class MeasurementProvider : IMeasurementProvider
    {
        private SpinLock _spinLock;
        private IStatsDClient _client;
        private readonly StringBuilder _buffer;
        private readonly string _scope;
        private const string DoubleStringFormat = "{0:F15}";
        private const string DeltaStringFormat = "{0:+#.###;-#.###;+0}";
        private const int MillisecondsTimeout = 10;

        ///<summary>
        /// Create provider instance
        /// </summary>
        /// <param name="scope">Metrics prefix</param>
        /// <param name="client">StatsD client</param>
        public MeasurementProvider(string scope, IStatsDClient client)
        {
            if (string.IsNullOrWhiteSpace(scope))
            {
                throw new ArgumentException($"{nameof(scope)} can't be null or empty");
            }

            _client = client ?? throw new ArgumentNullException($"{nameof(client)} can't be null");

            _spinLock = new SpinLock(true);
            _scope = scope;

            _client.Connect();
            _buffer = new StringBuilder();
        }

        ~MeasurementProvider()
        {
            OnDispose(false);
        }

        public IMeasurementProvider AddCounter(string key, int value, double? sampleRate = null)
        {
            return AddMetric(key, value, StatsDNamespaces.CountingPostfix, sampleRate);
        }

        public IMeasurementProvider AddCounter(string key, double value, double? sampleRate = null)
        {
            return AddMetric(key, value, StatsDNamespaces.CountingPostfix, sampleRate, DoubleStringFormat);
        }

        public IMeasurementProvider AddTimer(string key, int value, double? sampleRate = null)
        {
            return AddMetric(key, value, StatsDNamespaces.TimingPostfix, sampleRate);
        }

        public IMeasurementProvider AddTimer(string key, double value, double? sampleRate = null)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException($"Argument {nameof(value)} must be greater then zero.");
            }

            return AddMetric(key, value, StatsDNamespaces.TimingPostfix, sampleRate, DoubleStringFormat);
        }

        public IMeasurementProvider AddGauge(string key, double value, bool isDelta = false)
        {
            return AddMetric(key, value, StatsDNamespaces.GaugePostfix, format: isDelta ? DeltaStringFormat : DoubleStringFormat);
        }

        public IMeasurementProvider AddHistogram(string key, int value)
        {
            return AddMetric(key, value, StatsDNamespaces.HistogramPostfix);
        }

        public IMeasurementProvider AddHistogram(string key, double value)
        {
            return AddMetric(key, value, StatsDNamespaces.HistogramPostfix, format: DoubleStringFormat);
        }

        public IMeasurementProvider AddMeter(string key, int value)
        {
            return AddMetric(key, value, StatsDNamespaces.MeterPostfix);
        }

        public IMeasurementProvider AddSet(string key, string value)
        {
            return AddMetric(key, value, StatsDNamespaces.SetPostfix);
        }

        public IMeasurementProvider AddMetric<T>(string key, 
                                                 T value, 
                                                 string statsDNamespace, 
                                                 double? sampleRate = null, 
                                                 string format = null)
        {
            SynchronizeResource(() =>
            {
                _buffer.Append(_scope).Append('.').Append(key).Append(':');

                if (format != null)
                {
                    _buffer.Append(string.Format(NumberFormatInfo.InvariantInfo, format, value));
                }
                else
                {
                    _buffer.Append(value);
                }

                _buffer.Append('|').Append(statsDNamespace);

                if (sampleRate.HasValue)
                {
                    _buffer.Append("|@");
                    _buffer.Append(sampleRate);
                }

                _buffer.Append('\n');
            });

            return this;
        }

        public IMeasurementProvider Flush()
        {
            return FlushAsync().ConfigureAwait(false)
                               .GetAwaiter()
                               .GetResult();
        }

        public async Task<IMeasurementProvider> FlushAsync()
        {
            string message = null;
            SynchronizeResource(() =>
            {
                message = _buffer.ToString();
                _buffer.Clear();
            });

            if (!string.IsNullOrWhiteSpace(message))
            {
                var bytes = Encoding.UTF8.GetBytes(message);
                await _client.SendAsync(bytes, bytes.Length);
            }

            return this;
        }

        public void Dispose()
        {
            Flush();
            OnDispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void OnDispose(bool disposing)
        {
            if (!disposing)
                return;

            //_client?.Close();
            //_client = null;
        }

        private void SynchronizeResource(Action action)
        {
            var isLocked = false;

            try
            {
                do { _spinLock.TryEnter(MillisecondsTimeout, ref isLocked); } while (!isLocked);

                action();
            }
            finally
            {
                if (isLocked)
                {
                    _spinLock.Exit();
                }
            }
        }
    }
}
