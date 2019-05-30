using System;
using System.Threading.Tasks;

namespace Orange.StatsD
{
    public interface IMeasurementProvider : IDisposable
    {
        // Add int counter metrics with sample rate
        void AddCounter(string key, int value, double sampleRate = 1.0);

        // Add double counter metrics with sample rate
        void AddCounter(string key, double value, double sampleRate = 1.0);

        // Add int time metrics in milliseconds with sample rate
        void AddTimer(string key, int value, double sampleRate = 1.0);

        // Add double time metrics in milliseconds with sample rate. 
        // It's must be more than zero
        void AddTimer(string key, double value, double sampleRate = 1.0);

        // Add double gauge metrics in milliseconds
        // It's can be write as a delta
        void AddGauge(string key, double value, bool isDelta = false);

        // Add int histogram metrics in milliseconds
        void AddHistogram(string key, int value);

        // Add double histogram metrics in milliseconds
        void AddHistogram(string key, double value);

        void AddMeter(string key, int value);

        //Add set value
        void AddSet(string key, string value);

        //Add any metrics by StatsD server type
        void AddMetric<T>(
            string key,
            T value,
            string statsDNamespace,
            double? sampleRate = null,
            string format = null);

        //Send metrics buffer to server
        void Flush();

        //Send metrics buffer to server in async mode
        Task FlushAsync();
    }
}