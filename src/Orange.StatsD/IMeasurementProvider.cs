using System;
using System.Threading.Tasks;

namespace Orange.StatsD
{
    public interface IMeasurementProvider : IDisposable
    {
        /// <summary>
        /// Add int counter metrics with sample rate
        /// </summary>
        IMeasurementProvider AddCounter(string key, int value, double? sampleRate = null);

        /// <summary>
        /// Add double counter metrics with sample rate
        /// </summary>
        IMeasurementProvider AddCounter(string key, double value, double? sampleRate = null);

        /// <summary>
        /// Add int time metrics in milliseconds with sample rate
        /// </summary>
        IMeasurementProvider AddTimer(string key, int value, double? sampleRate = null);

        /// <summary>
        /// Add double time metrics in milliseconds with sample rate. 
        /// It's must be more than zero
        /// </summary>
        IMeasurementProvider AddTimer(string key, double value, double? sampleRate = null);

        /// <summary>
        /// Add double gauge metrics in milliseconds
        /// It's can be write as a delta
        /// </summary>
        IMeasurementProvider AddGauge(string key, double value, bool isDelta = false);

        /// <summary>
        /// Add int histogram metrics in milliseconds
        /// </summary>
        IMeasurementProvider AddHistogram(string key, int value);

        /// <summary>
        /// Add double histogram metrics in milliseconds
        /// </summary>
        IMeasurementProvider AddHistogram(string key, double value);

        IMeasurementProvider AddMeter(string key, int value);

        /// <summary>
        /// Add set value
        /// </summary>
        IMeasurementProvider AddSet(string key, string value);

        /// <summary>
        /// Add any metrics by StatsD server type
        /// </summary>
        IMeasurementProvider AddMetric<T>(
            string key,
            T value,
            string statsDNamespace,
            double? sampleRate = null,
            string format = null);

        /// <summary>
        /// Send metrics buffer to server
        /// </summary>
        IMeasurementProvider Flush();

        /// <summary>
        /// Send metrics buffer to server in async mode
        /// </summary>
        Task<IMeasurementProvider> FlushAsync();
    }
}