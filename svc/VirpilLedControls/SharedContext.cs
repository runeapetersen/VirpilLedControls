using System;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SPAD.neXt.Interfaces.Logging;

namespace VirpilLedControls
{
    public class SharedContext
    {
        private SharedContext()
        {
            var configFileName = ConfigFileResolver.FileName;
            var configFileFileInfo = FileSystem.FileInfo.New(configFileName);
            if (!configFileFileInfo.Exists)
            {
                throw new FileNotFoundException($"Config file not found: {configFileName}");
            }

            var content = configFileFileInfo.OpenText().ReadToEnd();
            if (string.IsNullOrEmpty(content))
            {
                throw new InvalidOperationException($"Config file is empty: {configFileName}");
            }
            
            var configFile = JsonConvert.DeserializeObject<ConfigFile>(content);
            if (configFile == null)
            {
                throw new InvalidOperationException($"Config file is null: {configFileName}");
            }
            
            _externalToolPath = configFile.ExternalToolPath;
            
            Devices = new Devices();
        }

        public Devices Devices { get; private set; }

        static SharedContext()
        {
            var fs = FileSystem = new FileSystem();
            ConfigFileResolver = new DefaultConfigFileResolver(fs);
            LazyContext = new Lazy<SharedContext>(() => new SharedContext(), LazyThreadSafetyMode.ExecutionAndPublication);
        }
        
        public static IFileSystem FileSystem { get; set; }
        public static IConfigFileResolver ConfigFileResolver { get; set; }
        private readonly string _externalToolPath;
        private static Lazy<SharedContext> LazyContext;
        public static SharedContext Instance => LazyContext.Value;

        public void LaunchToolWithParameters(string parameters, ILogger logger)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = _externalToolPath,
                Arguments = parameters,
                CreateNoWindow = true,      
                UseShellExecute = true,    
                RedirectStandardOutput = false,
                RedirectStandardError = false
            };

            var p = Process.Start(startInfo);
            if (p == null)
            {
                logger?.Error($"Failed to start external tool: {_externalToolPath}");
                return;
            }

            // Detach process execution from the calling thread to prevent UI hitching and blocking.
            Task.Run(() =>
            {
                using (p)
                {
                    p.WaitForExit();
                        
                    // Drain streams after exit to prevent buffer deadlocks in the child process.
                    //string output = p.StandardOutput.ReadToEnd();
                    //string error = p.StandardError.ReadToEnd();

                    if (p.ExitCode != 0)
                        logger?.Error($"External tool failed ({p.ExitCode}) for: {parameters}.");
                }
            });
        }

        public static void Reset()
        {
            LazyContext = new Lazy<SharedContext>(() => new SharedContext(), LazyThreadSafetyMode.ExecutionAndPublication);
        }
    }

    public class ConfigFile
    {
        public string ExternalToolPath { get; set; }
    }

    public class DefaultConfigFileResolver : IConfigFileResolver
    {
        public DefaultConfigFileResolver(IFileSystem fileSystem)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "VirpilLedControls.config.json";
            string location = assembly.Location;
            var directoryName = fileSystem.Path.GetDirectoryName(location);
            if (string.IsNullOrEmpty(directoryName))
                throw new InvalidOperationException("Could not determine directory name for config file");
            FileName = fileSystem.Path.Combine(directoryName, resourceName);
        }
        public string FileName { get; private set; }
    }

    public interface IConfigFileResolver
    {
        string FileName { get; }
    }
}