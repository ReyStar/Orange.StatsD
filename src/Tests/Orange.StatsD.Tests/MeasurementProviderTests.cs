using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Orange.StatsD.Transport;

namespace Orange.StatsD.Tests
{
    /// <summary>
    /// Test metrics adding interface
    /// https://statsd.readthedocs.io/en/v3.1/types.html
    /// </summary>
    [Parallelizable(ParallelScope.None)]
    public class MeasurementProviderTests
    {
        private MeasurementProvider _provider;
        private const string Prefix = "application-prefix";
        private readonly Mock<IStatsDClient> _client = new Mock<IStatsDClient>();

        [SetUp]
        public void Setup()
        {
            _provider = new MeasurementProvider(Prefix, _client.Object);
        }

        [Test]
        [TestCase(-1, null, "application-prefix.count-metrics:-1|c\n")]
        [TestCase(1, null, "application-prefix.count-metrics:1|c\n")]
        [TestCase(-1, -1, "application-prefix.count-metrics:-1|c|@-1\n")]
        [TestCase(2, 1, "application-prefix.count-metrics:2|c|@1\n")]
        [TestCase(3, 0.5, "application-prefix.count-metrics:3|c|@0.5\n")]
        [TestCase(0, 0.4, "application-prefix.count-metrics:0|c|@0.4\n")]
        [TestCase(0, 0, "application-prefix.count-metrics:0|c|@0\n")]
        [TestCase(-0, 0, "application-prefix.count-metrics:0|c|@0\n")]
        public async Task AddIntCounter_Tests(int value, double? sampleRate, string message)
        {
            // Arrange
            var byteArray = Encoding.UTF8.GetBytes(message);

            _client.Setup(x => x.SendAsync(It.Is<byte[]>(t => t.SequenceEqual(byteArray)), 
                                           It.Is<int>(t => t == byteArray.Length)))
                   .Returns(Task.FromResult(0));

            // Act
            await _provider.AddCounter("count-metrics", value, sampleRate).FlushAsync();

            // Assert
            _client.VerifyAll();
        }

        [Test]
        [TestCase(-1.1, null, "application-prefix.count-metrics:-1.100000000000000|c\n")]
        [TestCase(1.5, 1, "application-prefix.count-metrics:1.500000000000000|c|@1\n")]
        [TestCase(-1.0, -1.0, "application-prefix.count-metrics:-1.000000000000000|c|@-1\n")]
        [TestCase(2.9, 1, "application-prefix.count-metrics:2.900000000000000|c|@1\n")]
        [TestCase(3.0, 0.5, "application-prefix.count-metrics:3.000000000000000|c|@0.5\n")]
        [TestCase(0.6, 0.4, "application-prefix.count-metrics:0.600000000000000|c|@0.4\n")]
        [TestCase(0.0, 0, "application-prefix.count-metrics:0.000000000000000|c|@0\n")]
        [TestCase(-0.0, 0, "application-prefix.count-metrics:0.000000000000000|c|@0\n")]
        public async Task AddDoubleCounter_Tests(double value, double? sampleRate, string message)
        {
            // Arrange
            var byteArray = Encoding.UTF8.GetBytes(message);

            _client.Setup(x => x.SendAsync(It.Is<byte[]>(t => t.SequenceEqual(byteArray)),
                                           It.Is<int>(t => t == byteArray.Length)))
                   .Returns(Task.FromResult(0));

            // Act
            await _provider.AddCounter("count-metrics", value, sampleRate).FlushAsync();

            // Assert
            _client.VerifyAll();
        }

        [Test]
        [TestCase(-1, null, "application-prefix.time-metrics:-1|ms\n")]
        [TestCase(1, null, "application-prefix.time-metrics:1|ms\n")]
        [TestCase(-1, -1, "application-prefix.time-metrics:-1|ms|@-1\n")]
        [TestCase(2, 1, "application-prefix.time-metrics:2|ms|@1\n")] 
        [TestCase(3, 0.5, "application-prefix.time-metrics:3|ms|@0.5\n")]
        [TestCase(0, 0.4, "application-prefix.time-metrics:0|ms|@0.4\n")]
        [TestCase(0, 0, "application-prefix.time-metrics:0|ms|@0\n")]
        [TestCase(-0, 0, "application-prefix.time-metrics:0|ms|@0\n")]
        public async Task AddIntTime_Tests(int value, double? sampleRate, string message)
        {
            // Arrange
            var byteArray = Encoding.UTF8.GetBytes(message);

            _client.Setup(x => x.SendAsync(It.Is<byte[]>(t => t.SequenceEqual(byteArray)),
                                           It.Is<int>(t => t == byteArray.Length)))
                .Returns(Task.FromResult(0));

            // Act
            await _provider.AddTimer("time-metrics", value, sampleRate).FlushAsync();

            // Assert
            _client.VerifyAll();
        }

        [Test]
        [TestCase(1.5, 1, "application-prefix.time-metrics:1.500000000000000|ms|@1\n")]
        [TestCase(2.9, 1, "application-prefix.time-metrics:2.900000000000000|ms|@1\n")]
        [TestCase(3.0, 0.5, "application-prefix.time-metrics:3.000000000000000|ms|@0.5\n")]
        [TestCase(0.6, 0.4, "application-prefix.time-metrics:0.600000000000000|ms|@0.4\n")]
        [TestCase(0.0, 0, "application-prefix.time-metrics:0.000000000000000|ms|@0\n")]
        public async Task AddDoubleTime_Tests(double value, double? sampleRate, string message)
        {
            // Arrange
            var byteArray = Encoding.UTF8.GetBytes(message);

            _client.Setup(x => x.SendAsync(It.Is<byte[]>(t => t.SequenceEqual(byteArray)),
                                           It.Is<int>(t => t == byteArray.Length)))
                .Returns(Task.FromResult(0));

            // Act
            await _provider.AddTimer("time-metrics", value, sampleRate).FlushAsync();

            // Assert
            _client.VerifyAll();
        }

        [Test]
        public void AddDoubleTime_Tests_ThrowExceptionOnNegativeTimeValue()
        {
            // Arrange && Act
            var ex = Assert.Catch<ArgumentOutOfRangeException>(() =>  _provider.AddTimer("time-metrics", -1D));

            // Assert
            ex.Should().NotBeNull();
        }

        [Test]
        [TestCase(3.0, false, "application-prefix.gauge-metrics:3.000000000000000|g\n")]
        [TestCase(1.5, false, "application-prefix.gauge-metrics:1.500000000000000|g\n")]
        [TestCase(-0.6, false, "application-prefix.gauge-metrics:-0.600000000000000|g\n")]
        [TestCase(0.0, false, "application-prefix.gauge-metrics:0.000000000000000|g\n")]
        [TestCase(-0.0, false, "application-prefix.gauge-metrics:0.000000000000000|g\n")]
        [TestCase(3.0, true, "application-prefix.gauge-metrics:+3|g\n")]
        [TestCase(1.5, true, "application-prefix.gauge-metrics:+1.5|g\n")]
        [TestCase(-0.6, true, "application-prefix.gauge-metrics:-.6|g\n")]
        [TestCase(0.0, true, "application-prefix.gauge-metrics:+0|g\n")]
        [TestCase(-0.0, true, "application-prefix.gauge-metrics:+0|g\n")]
        public async Task AddGauge_Tests(double value, bool isDelta, string message)
        {
            // Arrange
            var byteArray = Encoding.UTF8.GetBytes(message);

            _client.Setup(x => x.SendAsync(It.Is<byte[]>(t => t.SequenceEqual(byteArray)),
                                           It.Is<int>(t => t == byteArray.Length)))
                .Returns(Task.FromResult(0));

            // Act
            await _provider.AddGauge("gauge-metrics", value, isDelta).FlushAsync();

            // Assert
            _client.VerifyAll();
        }

        [Test]
        [TestCase(-1, "application-prefix.histogram-metrics:-1|h\n")]
        [TestCase(1, "application-prefix.histogram-metrics:1|h\n")]
        [TestCase(-1, "application-prefix.histogram-metrics:-1|h\n")]
        [TestCase(2, "application-prefix.histogram-metrics:2|h\n")]
        [TestCase(3, "application-prefix.histogram-metrics:3|h\n")]
        [TestCase(0, "application-prefix.histogram-metrics:0|h\n")]
        [TestCase(-0, "application-prefix.histogram-metrics:0|h\n")]
        public async Task AddIntHistogram_Tests(int value, string message)
        {
            // Arrange
            var byteArray = Encoding.UTF8.GetBytes(message);

            _client.Setup(x => x.SendAsync(It.Is<byte[]>(t => t.SequenceEqual(byteArray)),
                                           It.Is<int>(t => t == byteArray.Length)))
                .Returns(Task.FromResult(0));

            // Act
            await _provider.AddHistogram("histogram-metrics", value).FlushAsync();

            // Assert
            _client.VerifyAll();
        }

        [Test]
        [TestCase(-1, "application-prefix.histogram-metrics:-1.000000000000000|h\n")]
        [TestCase(1, "application-prefix.histogram-metrics:1.000000000000000|h\n")]
        [TestCase(-1, "application-prefix.histogram-metrics:-1.000000000000000|h\n")]
        [TestCase(2.48316464, "application-prefix.histogram-metrics:2.483164640000000|h\n")]
        [TestCase(-3.1561875416, "application-prefix.histogram-metrics:-3.156187541600000|h\n")]
        [TestCase(0, "application-prefix.histogram-metrics:0.000000000000000|h\n")]
        [TestCase(-0, "application-prefix.histogram-metrics:0.000000000000000|h\n")]
        public async Task AddDoubleHistogram_Tests(double value, string message)
        {
            // Arrange
            var byteArray = Encoding.UTF8.GetBytes(message);

            _client.Setup(x => x.SendAsync(It.Is<byte[]>(t => t.SequenceEqual(byteArray)),
                                           It.Is<int>(t => t == byteArray.Length)))
                .Returns(Task.FromResult(0));

            // Act
            await _provider.AddHistogram("histogram-metrics", value).FlushAsync();

            // Assert
            _client.VerifyAll();
        }

        [Test]
        [TestCase(-1, "application-prefix.meter-metrics:-1|m\n")]
        [TestCase(1, "application-prefix.meter-metrics:1|m\n")]
        [TestCase(-1, "application-prefix.meter-metrics:-1|m\n")]
        [TestCase(2, "application-prefix.meter-metrics:2|m\n")]
        [TestCase(3, "application-prefix.meter-metrics:3|m\n")]
        [TestCase(0, "application-prefix.meter-metrics:0|m\n")]
        [TestCase(-0, "application-prefix.meter-metrics:0|m\n")]
        public async Task AddMeter_Tests(int value, string message)
        {
            // Arrange
            var byteArray = Encoding.UTF8.GetBytes(message);

            _client.Setup(x => x.SendAsync(It.Is<byte[]>(t => t.SequenceEqual(byteArray)),
                                           It.Is<int>(t => t == byteArray.Length)))
                .Returns(Task.FromResult(0));

            // Act
            await _provider.AddMeter("meter-metrics", value).FlushAsync();

            // Assert
            _client.VerifyAll();
        }

        [Test]
        [TestCase("-1", "application-prefix.set-metrics:-1|s\n")]
        [TestCase("1", "application-prefix.set-metrics:1|s\n")]
        [TestCase("-1", "application-prefix.set-metrics:-1|s\n")]
        [TestCase("2.0", "application-prefix.set-metrics:2.0|s\n")]
        [TestCase("3.14159265359", "application-prefix.set-metrics:3.14159265359|s\n")]
        [TestCase("0", "application-prefix.set-metrics:0|s\n")]
        [TestCase("-0", "application-prefix.set-metrics:-0|s\n")]
        public async Task AddMeter_Tests(string value, string message)
        {
            // Arrange
            var byteArray = Encoding.UTF8.GetBytes(message);

            _client.Setup(x => x.SendAsync(It.Is<byte[]>(t => t.SequenceEqual(byteArray)),
                                           It.Is<int>(t => t == byteArray.Length)))
                .Returns(Task.FromResult(0));

            // Act
            await _provider.AddSet("set-metrics", value).FlushAsync();

            // Assert
            _client.VerifyAll();
        }

        [Test]
        [TestCase("count-metrics", 3.14159265359D, StatsDNamespaces.CountingPostfix, null, null, "application-prefix.count-metrics:3.14159265359|c\n")]
        [TestCase("count-metrics", 3.14159265359D, StatsDNamespaces.CountingPostfix, 0.88, null, "application-prefix.count-metrics:3.14159265359|c|@0.88\n")]
        [TestCase("count-metrics", 3.14159265359D, StatsDNamespaces.CountingPostfix, 0.88, "{0:F4}", "application-prefix.count-metrics:3.1416|c|@0.88\n")]
        [TestCase("time-metrics", -0, StatsDNamespaces.TimingPostfix, 0, null, "application-prefix.time-metrics:0|ms|@0\n")]
        [TestCase("histogram-metrics", -1, StatsDNamespaces.HistogramPostfix, null, "{0:F15}", "application-prefix.histogram-metrics:-1.000000000000000|h\n")]
        [TestCase("meter-metrics", 3, StatsDNamespaces.MeterPostfix, null, null, "application-prefix.meter-metrics:3|m\n")]
        public async Task AddTemplateMetrics_Test(string metricsName, object value, string statsDNamespace, double? sampleRate, string format, string message)
        {
            // Arrange
            var byteArray = Encoding.UTF8.GetBytes(message);

            _client.Setup(x => x.SendAsync(It.Is<byte[]>(t => t.SequenceEqual(byteArray)),
                                           It.Is<int>(t => t == byteArray.Length)))
                .Returns(Task.FromResult(0));

            // Act
            await _provider.AddMetric(metricsName, value, statsDNamespace, sampleRate, format).FlushAsync();

            // Assert
            _client.VerifyAll();
        }


        [Test]
        public async Task AddMultipleMetrics_Test()
        {
            // Arrange
            var message =
                "application-prefix.count-metrics:1|c\n" +
                "application-prefix.count-metrics:1.500000000000000|c\n" +
                "application-prefix.time-metrics:-1|ms|@0.12\n" +
                "application-prefix.time-metrics:6.300000000000000|ms\n" +
                "application-prefix.gauge-metrics:0.800000000000000|g\n" +
                "application-prefix.gauge-metrics:+7|g\n" +
                "application-prefix.histogram-metrics:0.250000000000000|h\n" +
                "application-prefix.histogram-metrics:-1|h\n" +
                "application-prefix.meter-metrics:-1152|m\n" +
                "application-prefix.meter-metrics:0|m\n" +
                "application-prefix.meter-metrics:633|m\n" +
                "application-prefix.set-metrics:42|s\n" +
                "application-prefix.count-metrics:3.1416|c|@0.88\n";

            var byteArray = Encoding.UTF8.GetBytes(message);

            _client.Setup(x => x.SendAsync(It.Is<byte[]>(t => t.SequenceEqual(byteArray)),
                    It.Is<int>(t => t == byteArray.Length)))
                .Returns(Task.FromResult(0));

            // Act
            await _provider.AddCounter("count-metrics", 1)
                           .AddCounter("count-metrics", 1.5)
                           .AddTimer("time-metrics", -1, sampleRate: 0.12)
                           .AddTimer("time-metrics", 6.3)
                           .AddGauge("gauge-metrics", 0.8)
                           .AddGauge("gauge-metrics", 7, isDelta: true)
                           .AddHistogram("histogram-metrics", 0.25)
                           .AddHistogram("histogram-metrics", -1)
                           .AddMeter("meter-metrics", -1152)
                           .AddMeter("meter-metrics", 0)
                           .AddMeter("meter-metrics", 633)
                           .AddSet("set-metrics", "42")
                           .AddMetric("count-metrics", 3.14159265359D, StatsDNamespaces.CountingPostfix, 0.88, "{0:F4}")
                           .FlushAsync();

            // Assert
            _client.VerifyAll();
        }
    }
}