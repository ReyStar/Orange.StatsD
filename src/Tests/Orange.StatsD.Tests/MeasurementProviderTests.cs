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
    }
}