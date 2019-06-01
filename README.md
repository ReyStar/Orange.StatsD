[![license](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/ReyStar/Orange.StatsD/blob/master/LICENSE)
[![Build Status](https://dev.azure.com/starandrey/starandrey/_apis/build/status/ReyStar.Orange.StatsD?branchName=master)](https://dev.azure.com/starandrey/starandrey/_build/latest?definitionId=1&branchName=master) [![NuGet](https://buildstats.info/nuget/Orange.StatsD?includePreReleases=true)](https://www.nuget.org/packages/Orange.StatsD/)
# Orange.StatsD
Simple buffered StatsD client implementation
## Project target
Create a small and light library for sending metrics to graphite using [StatsD protocol](https://github.com/statsd/statsd). This library provide collection next metrics types: Timing, Gauges, Sets, Histogram, Counting.
## Components
### Transport
  - StatsDUdpClient - UDP client to StatsD server 
  - StatsDTcpClient - TCP client to StatsD server
### Metrics collector
  - MeasurementProvider - buffered metrics collecror
  - ScheduledMeasurementProvider - buffered metrics collector with flush by timer functionality
  
MeasurementProvider inmpelemetn IMeasurementProvider interface with next methods:

```csharp
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

```
  
## Example
```csharp
public class Program
{
    public static async Task<int> Main(string[] values)
    {
        var arr = new int[] {-1, 1};
        using (var transport = new Orange.StatsD.Transport.StatsDUdpClient("localhost", 8125))
        {
            using (var client = new Orange.StatsD.MeasurementProvider("application-name", transport))
            {
                var random = new Random(Guid.NewGuid().GetHashCode());
                var count = 0;

                while (++count < 100)
                {
                    var r1 = random.Next(1, 100);
                    client.AddTimer("metric-timer-1", r1);
                    client.AddTimer("metric-timer-2", r1, 2);
                    client.AddTimer("metric-timer-3", 1.685);

                    var r2 = random.Next(1, 100);
                    client.AddCounter("metric-counter-1", r2);
                    client.AddCounter("metric-counter-2", -r2, .75);
                    client.AddCounter("metric-counter-3", random.NextDouble());

                    client.AddGauge("metric-gauge-1", random.NextDouble());
                    client.AddGauge("metric-gauge-2", -random.NextDouble(), true);
                    client.AddGauge("metric-gauge-3", -random.NextDouble());

                    client.AddHistogram("metric-histogram-1", random.Next(-100, 100));
                    client.AddHistogram("metric-histogram-2", random.NextDouble() * arr[random.Next(0, 2)]);

                    client.AddMeter("metric-meter-1", random.Next(1, 100));
                    client.AddMeter("metric-meter-2", -random.Next(1, 100));

                    client.AddSet("metric-set-1", DateTime.UtcNow.ToString("HH:mm:ss"));
                    client.AddSet("metric-set-2", random.Next(1, 100).ToString());
                    client.AddSet("metric-set-3", random.Next(-100, 100).ToString());

                    await Task.Delay(random.Next(0, 100));
                    await client.FlushAsync();
                }
            }

        }

        return 0;
    }
}
```
