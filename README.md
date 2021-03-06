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

| Method | Description |
| ------ | ------ |
|IMeasurementProvider AddCounter(string key, int value, double? sampleRate = null);|Add int counter metrics with sample rate|
|IMeasurementProvider AddCounter(string key, double value, double? sampleRate = null);|Add double counter metrics with sample rate|
|IMeasurementProvider AddTimer(string key, int value, double? sampleRate = null);|Add int time metrics in milliseconds with sample rate|
|IMeasurementProvider AddTimer(string key, double value, double? sampleRate = null);|It's must be more than zero|Add double time metrics in milliseconds with sample rate.|
|IMeasurementProvider AddGauge(string key, double value, bool isDelta = false);|Add double gauge metrics in milliseconds. It's can be write as a delta|
|IMeasurementProvider AddHistogram(string key, int value);|Add int histogram metrics in milliseconds|
|IMeasurementProvider AddHistogram(string key, double value);|Add double histogram metrics in milliseconds|
|IMeasurementProvider AddMeter(string key, int value);|Add meters statistics|
|IMeasurementProvider AddSet(string key, string value);|Add set value|
|IMeasurementProvider AddMetric<T>(string key, T value, string statsDNamespace, double? sampleRate = null, string format = null);|Add any metrics by StatsD server type|
|IMeasurementProvider Flush();|Sync-send metrics buffer to StatsD server|
|Task<IMeasurementProvider> FlushAsync();|Async-Send metrics buffer to StatsS server|
|void Dispose()|Sync-send metrics buffer to StatsD server then dispose provider instance|

## Example
For example I recommendate start docker container configuration from 'environment' directory and then compile and run example project 'src/Example/Orange.StatsD.Example/Orange.StatsD.Example.csproj'

Before all tests need download or clone reposotory on u computer. U can do it use other ways and I won't explain anything.

<img src="https://raw.githubusercontent.com/ReyStar/Orange.StatsD/master/documentation/img/how_it's_download.png" alt="how_it's_download" width="800"/>

### Up docker container:
For run docker containers need in 'environment' directory do next command: 'docker-compose up -d'. It's run graphite and graphana containers. 

<img src="https://raw.githubusercontent.com/ReyStar/Orange.StatsD/master/documentation/img/environment_up.png" alt="environment_up" width="800"/>

### Compule and run example project:
For Compule and run example project need execute next DotNet commands: dotnet restore | dotnet build | dotnet run

<img src="https://raw.githubusercontent.com/ReyStar/Orange.StatsD/master/documentation/img/launched_example_application.png" alt="launched_example_application" width="800"/>

#### Example application text
```csharp
class Program
{
    public static async Task<int> Main(string[] values)
    {
        var arr = new int[] { -1, 1 };

        using (var transport = new ClientWrapperToTesting<StatsDUdpClient>(new StatsDUdpClient("localhost", 8125)))
        {
            using (var client = new MeasurementProvider("application", transport))
            {
                var random = new Random(Guid.NewGuid().GetHashCode());
                var count = 0;

                while (++count < 10000)
                {
                    var r1 = random.Next(1, 100);
                    await client.AddTimer("metric-timer-1", r1)
                                .AddTimer("metric-timer-2", r1, 2)
                                .AddTimer("metric-timer-3", 1.685)
                                .FlushAsync();

                    var r2 = random.Next(1, 100);
                    await client.AddCounter("metric-counter-1", r2)
                                .AddCounter("metric-counter-2", -r2, .75)
                                .AddCounter("metric-counter-3", random.NextDouble())
                                .FlushAsync();

                    await client.AddGauge("metric-gauge-1", random.NextDouble())
                                .AddGauge("metric-gauge-2", -random.NextDouble(), true)
                                .AddGauge("metric-gauge-3", -random.NextDouble())
                                .FlushAsync();

                    await client.AddHistogram("metric-histogram-1", random.Next(-100, 100))
                                .AddHistogram("metric-histogram-2", random.NextDouble() * arr[random.Next(0, 2)])
                                .FlushAsync();

                    await client.AddMeter("metric-meter-1", random.Next(1, 100))
                                .AddMeter("metric-meter-2", -random.Next(1, 100))
                                .FlushAsync();

                    await client.AddSet("metric-set-1", DateTime.UtcNow.ToString("HH:mm:ss"))
                                .AddSet("metric-set-2", random.Next(1, 100).ToString())
                                .AddSet("metric-set-3", random.Next(-100, 100).ToString())
                                .FlushAsync();

                    await Task.Delay(random.Next(0, 100));
                }
            }
        }

        return 0;
    }
}
```
### How metrics look

U can analisis metrics results in two applications: Graphite and Grafana.

#### Graphite

<img src="https://raw.githubusercontent.com/ReyStar/Orange.StatsD/master/documentation/img/view_example_4.png" alt="view_example_4.png" width="800"/>

#### Grafana

<img src="https://raw.githubusercontent.com/ReyStar/Orange.StatsD/master/documentation/img/choice_data_source.png" alt="choice_data_source.png" width="800"/>

<img src="https://raw.githubusercontent.com/ReyStar/Orange.StatsD/master/documentation/img/dashbord_creating.png" alt="dashbord_creating.png" width="800"/>

<img src="https://raw.githubusercontent.com/ReyStar/Orange.StatsD/master/documentation/img/data_source_configuration_step_1.png" alt="data_source_configuration_step_1.png" width="800"/>

<img src="https://raw.githubusercontent.com/ReyStar/Orange.StatsD/master/documentation/img/data_source_configuration_step_2.png" alt="data_source_configuration_step_2.png" width="800"/>

<img src="https://raw.githubusercontent.com/ReyStar/Orange.StatsD/master/documentation/img/data_source_configuration_step_3.png" alt="data_source_configuration_step_3.png" width="800"/>


<img src="https://raw.githubusercontent.com/ReyStar/Orange.StatsD/master/documentation/img/data_source_configuration_step_4.png" alt="data_source_configuration_step_4.png" width="800"/>

<img src="https://raw.githubusercontent.com/ReyStar/Orange.StatsD/master/documentation/img/data_source_configuration_step_5.png" alt="data_source_configuration_step_5.png" width="800"/>

<img src="https://raw.githubusercontent.com/ReyStar/Orange.StatsD/master/documentation/img/default_login.png" alt="default_login.png" width="800"/>

<img src="https://raw.githubusercontent.com/ReyStar/Orange.StatsD/master/documentation/img/graphana_sing_in.png" alt="graphana_sing_in.png" width="800"/>

<img src="https://raw.githubusercontent.com/ReyStar/Orange.StatsD/master/documentation/img/view_example.png" alt="view_example.png" width="800"/>

<img src="https://raw.githubusercontent.com/ReyStar/Orange.StatsD/master/documentation/img/view_example_2.png" alt="view_example_2.png" width="800"/>

<img src="https://raw.githubusercontent.com/ReyStar/Orange.StatsD/master/documentation/img/view_example_3.png" alt="view_example_3.png" width="800"/>


