using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using SPAD.neXt.Interfaces;
using SPAD.neXt.Interfaces.HID;
using VirpilLedControls.Interfaces;
using Xunit;

namespace VirpilLedControls.Tests
{
    public class VirpilDeviceTests
    {
        private Mock<ILockFactory> _mockLockFactory = new Mock<ILockFactory>();
        private Mock<ISmartLock> _mockLock = new Mock<ISmartLock>();
        private Mock<IHidDevice> _mockHidDevice = new Mock<IHidDevice>();

        private Mock<SPAD.neXt.Interfaces.Logging.ILogger> _mockLogger =
            new Mock<SPAD.neXt.Interfaces.Logging.ILogger>();

        private void WireMocks()
        {
            _mockLockFactory.Setup(m => m.CreateLock(It.IsAny<string>())).Returns(_mockLock.Object);
            _mockLock.Setup(m => m.Lock(It.IsAny<Action>())).Callback((Action a) => { a(); });
            _mockLogger.Setup(m => m.CreateChildLogger(It.IsAny<string>())).Returns(_mockLogger.Object);
        }

        [Fact]
        public void Device_NoColorsDefined_Fails()
        {
            WireMocks();
            VirpilDevice device =
                new VirpilDevice(1999, _mockHidDevice.Object, _mockLogger.Object, _mockLockFactory.Object);
            Assert.Throws<ArgumentException>(() => device.SetColors(1, Enumerable.Empty<LedColor>().ToArray(), 50));
        }

        [Fact]
        public void Device_ColorsNull_Fails()
        {
            WireMocks();
            VirpilDevice device =
                new VirpilDevice(1999, _mockHidDevice.Object, _mockLogger.Object, _mockLockFactory.Object);
            Assert.Throws<ArgumentNullException>(() => device.SetColors(1, null, 50));
        }

        [Fact]
        public async Task Device_ColorCycle_Successful_MultipleInvocations()
        {
            WireMocks();
            VirpilDevice device =
                new VirpilDevice(1999, _mockHidDevice.Object, _mockLogger.Object, _mockLockFactory.Object);
            device.SetColors(1, GrabColors(2), 50);
            var cancellationTokenSource =CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
            cancellationTokenSource.CancelAfter(200);
            while (_mockHidDevice.Invocations.Count < 2)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(50), cancellationTokenSource.Token);
            }
            _mockHidDevice.Verify(x => x.WriteFeatureData(It.IsAny<byte[]>()), Times.AtLeast(2));
        }
        
        [Fact]
        public async Task Device_SingleColor_Successful_OneInvocation()
        {
            WireMocks();
            VirpilDevice device =
                new VirpilDevice(1999, _mockHidDevice.Object, _mockLogger.Object, _mockLockFactory.Object);
            device.SetColors(1, GrabColors(1), 50);
            var cancellationTokenSource =CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
            cancellationTokenSource.CancelAfter(200);
            await Task.Delay(TimeSpan.FromMilliseconds(150), cancellationTokenSource.Token);
            _mockHidDevice.Verify(x => x.WriteFeatureData(It.IsAny<byte[]>()), Times.AtMostOnce);
        }

        private LedColor[] GrabColors(uint count)
        {
            var rand = new Random();
            Array values = Enum.GetValues(typeof(ColorIntensity));
            Func<ColorIntensity> randColor = () => (ColorIntensity)values.GetValue(rand.Next(values.Length));
            var res = new LedColor[count];
            for (int i = 0; i < count; i++)
            {
                res[i] = new LedColor { R = randColor(), G = randColor(), B = randColor() };
            }
            return res;
        }
    }
}