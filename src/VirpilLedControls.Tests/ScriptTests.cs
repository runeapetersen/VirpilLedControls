using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Moq;
using SPAD.neXt.Interfaces;
using SPAD.neXt.Interfaces.Events;
using SPAD.neXt.Interfaces.HID;
using SPAD.neXt.Interfaces.Logging;
using VirpilLedControls.DeviceControl;
using VirpilLedControls.Interfaces;
using VirpilLedControls.Model.Commands;
using VirpilLedControls.Script;
using VirpilLedControls.SerializationHelpers;
using Xunit;

namespace VirpilLedControls.Tests
{
    public class ScriptTests : IDisposable
    {
        private readonly Mock<IScriptLoggerFactory> _scriptLoggerFactory =
            new Mock<IScriptLoggerFactory>();

        private readonly Mock<ILockFactory> _lockFactory = new Mock<ILockFactory>();
        private readonly Mock<IHidDevices> _hidDevices = new Mock<IHidDevices>();
        private readonly Mock<ISmartLock> _smartLock = new Mock<ISmartLock>();
        private readonly Mock<ILogger> _logger = new Mock<ILogger>();
        private readonly TestableScript _script;

        public ScriptTests()
        {
            _scriptLoggerFactory
                .Setup(factory => factory.CreateLogger(It.IsAny<string>()))
                .Returns(_logger.Object);
            _lockFactory
                .Setup(factory => factory.CreateLock(It.IsAny<string>()))
                .Returns(_smartLock.Object);
            _smartLock
                .Setup(scriptLock => scriptLock.Lock(It.IsAny<Action>()))
                .Callback<Action>(action => action());
            _logger.Setup(logger => logger.CreateChildLogger(It.IsAny<string>()))
                .Returns(_logger.Object);

            _script = new TestableScript(
                _scriptLoggerFactory.Object,
                _lockFactory.Object,
                _hidDevices.Object,
                new LedCommandFactory());
            _script.Initialize();
        }

        [Fact]
        public void CallScript_ValidParams_WillSucceed()
        {
            const ushort pid = 0x4259;
            const string json =
                "{\"Pid\":\"0x4259\",\"LedId\":1,\"BoardType\":\"SlaveBoard4\",\"Colors\":[{\"R\":\"Full\",\"G\":\"Off\",\"B\":\"Sixty\"}]}";

            var hidDevice = new Mock<IHidDevice>();
            var hidCapabilities = new IHidDeviceCapabilities { FeatureReportByteLength = 1 };
            hidDevice.Setup(d => d.ProductId).Returns(pid);
            hidDevice.Setup(d => d.Capabilities).Returns(hidCapabilities);
            _hidDevices.Setup(m => m.Enumerate(It.Is<int>(vid => vid == VirpilDevice.VendorId)))
                .Returns(new List<IHidDevice>
                {
                    hidDevice.Object
                });

            var parameter = new Mock<IEventActionParameter>();
            parameter.Setup(p => p.ToString()).Returns(json);

            _script.Execute(
                new Mock<IApplication>().Object,
                new List<IEventActionParameter> { parameter.Object });

            var deadline = DateTime.UtcNow.AddSeconds(2);
            while (hidDevice.Invocations.All(i => i.Method.Name != nameof(IHidDevice.WriteFeatureData)) &&
                   DateTime.UtcNow < deadline)
            {
                Thread.Sleep(20);
            }

            hidDevice.Verify(d => d.WriteFeatureData(It.IsAny<byte[]>()), Times.Once);
        }

        public void Dispose()
        {
            _script.Deinitialize();
        }

        private sealed class TestableScript : VirpilLightAutomationScript
        {
            public TestableScript(
                IScriptLoggerFactory scriptLoggerFactory,
                ILockFactory lockFactory,
                IHidDevices hidDevices,
                ILedCommandFactory commandFactory)
                : base(scriptLoggerFactory, lockFactory, hidDevices, commandFactory)
            {
            }

            public void Initialize()
            {
                base.InitializeScript();
            }

            public new void Deinitialize()
            {
                base.DeinitializeScript();
            }
        }
    }
}