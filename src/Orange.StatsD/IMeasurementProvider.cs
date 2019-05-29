using System;
using System.Threading.Tasks;

namespace Orange.StatsD
{
    public interface IMeasurementProvider : IDisposable
    {
        void AddCounter(string key, int value, double sampleRate = 1.0);

        void AddCounter(string key, double value, double sampleRate = 1.0);

        void AddTimer(string key, int value, double sampleRate = 1.0);

        void AddTimer(string key, double value, double sampleRate = 1.0);

        void AddGauge(string key, double value, bool isDelta = false);

        void AddHistogram(string key, int value);

        void AddHistogram(string key, double value);

        void AddMeter(string key, int value);

        void AddSet(string key, string value);

        void AddMetric<T>(string key, 
                          T value, 
                          string statsDNamespace, 
                          double? sampleRate = null,
                          string format = null);

        void Flush();

        Task FlushAsync();
    }
}