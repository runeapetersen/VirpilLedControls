using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using SPAD.neXt.Interfaces;
using SPAD.neXt.Interfaces.HID;
using SPAD.neXt.Interfaces.Logging;
using VirpilLedControls.DeviceControl;
using VirpilLedControls.Exceptions;
using VirpilLedControls.Interfaces;
using VirpilLedControls.Model;
using VirpilLedControls.Model.Commands;
using Xunit;

namespace VirpilLedControls.Tests
{
    public class VirpilDeviceTests
    {
        private Mock<ILockFactory> _mockLockFactory = new Mock<ILockFactory>();
        private Mock<ISmartLock> _mockLock = new Mock<ISmartLock>();
        private Mock<IHidDevice> _mockHidDevice = new Mock<IHidDevice>();

        private Mock<ILogger> _mockLoggerInstance = new Mock<ILogger>();

        private void WireMocks()
        {
            _mockLockFactory.Setup(m => m.CreateLock(It.IsAny<string>())).Returns(_mockLock.Object);
            _mockLock.Setup(m => m.Lock(It.IsAny<Action>())).Callback((Action a) => { a(); });
            _mockLoggerInstance.Setup(m => m.CreateChildLogger(It.IsAny<string>())).Returns(_mockLoggerInstance.Object);
        }

        [Fact]
        public async Task Device_ColorCycle_Successful_MultipleInvocations()
        {
            WireMocks();
            VirpilDevice device =
                new VirpilDevice(1999, _mockHidDevice.Object, _mockLoggerInstance.Object, _mockLockFactory.Object);
            device.SetColors(
                new ColorCycleCommand
                {
                    Pid = 1999,
                    LedId = 1,
                    BoardType = PacketHandling.BoardType.OnBoard,
                    Colors = GrabColors(2),
                    IntervalMs = 250
                });

            var cancellationTokenSource =
                CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
            cancellationTokenSource.CancelAfter(500);
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
                new VirpilDevice(1999, _mockHidDevice.Object, _mockLoggerInstance.Object, _mockLockFactory.Object);
            device.SetColors(
                new SolidColorCommand
                {
                    Pid = 1999,
                    LedId = 1,
                    BoardType = PacketHandling.BoardType.OnBoard,
                    Color = GrabColors(1)[0]
                });
            var cancellationTokenSource =
                CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
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