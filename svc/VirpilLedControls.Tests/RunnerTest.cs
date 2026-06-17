using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Newtonsoft.Json;
using SPAD.neXt.Interfaces.Logging;
using Xunit;

namespace VirpilLedControls.Tests
{
    public class RunnerTest : IDisposable
    {
        Mock<ILogger> _loggerMock = new Mock<ILogger>();
        [Fact]
        [Trait("Category", "Integration")]
        public void SharedContext_ExecutionTest()
        {
            var sharedContext = SetupSharedContext();
            sharedContext.LaunchToolWithParameters("3344 4259 12 ff ff ff", _loggerMock.Object);
        }

        private static SharedContext SetupSharedContext()
        {
            var configFileName = @"c:\config.json";
            SharedContext.ConfigFileResolver = new FakeFileResolver(configFileName);
            var testFileContent = JsonConvert.SerializeObject(new ConfigFile{ ExternalToolPath = @"C:\Program Files (x86)\VPC Software Suite\tools\VPC_LED_Control.exe"});
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                { configFileName, new MockFileData(testFileContent) }
            });
            SharedContext.FileSystem = fileSystem;
            var sharedContext = SharedContext.Instance;
            return sharedContext;
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task BlinkingLogic_WillBlink()
        {
            var sharedContext = SetupSharedContext();
            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var ct = cancellationTokenSource.Token;
            await Task.Factory.StartNew(async () =>
            {
                while (!ct.IsCancellationRequested)
                {
                    sharedContext.LaunchToolWithParameters("3344 4259 01 ff ff 00", _loggerMock.Object);
                    await Task.Delay(TimeSpan.FromMilliseconds(500), ct);
                    sharedContext.LaunchToolWithParameters("3344 4259 01 00 00 00", _loggerMock.Object);
                    await Task.Delay(TimeSpan.FromMilliseconds(500), ct);
                }
            }, cancellationTokenSource.Token);
            try
            {
                await Task.Delay(10000, ct);
            }
            catch(TaskCanceledException){}
        }
        
        
        public void Dispose()
        {
            SharedContext.Reset();
        }
    }

    public class FakeFileResolver : IConfigFileResolver
    {
        public FakeFileResolver(string configFileName)
        {
            FileName = configFileName;
        }

        public string FileName { get; set; }
    }
}