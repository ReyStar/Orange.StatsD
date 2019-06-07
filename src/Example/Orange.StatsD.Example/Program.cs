using System;
using System.Threading.Tasks;
using Orange.StatsD.Transport;

namespace Orange.StatsD.Example
{
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
}
