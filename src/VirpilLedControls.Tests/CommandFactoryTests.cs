using System;
using System.Collections;
using System.Collections.Generic;
using VirpilLedControls.DeviceControl;
using VirpilLedControls.Model;
using VirpilLedControls.Model.Commands;
using VirpilLedControls.SerializationHelpers;
using Xunit;

namespace VirpilLedControls.Tests
{
    public class CommandFactoryTests
    {
        [Theory]
        [ClassData(typeof(CommandTestData))]
        public void CreateLedCommand_ShouldReturnCorrectType(LedCommand command, Type expectedCommandType, string json)
        {
            // Arrange
            var factory = new LedCommandFactory();

            // Act
            var result = factory.Create(json);

            // Assert
            Assert.IsType(expectedCommandType, result);
            Assert.Equal(command.Pid, result.Pid);
        }

        [Theory]
        [ClassData(typeof(LegacyScriptParamsTestData))]
        public void Legacy_CreateLedCommand_ShouldReturnCorrectType(LegacyCommand command, Type expectedCommandType,
            string json)
        {
            // Arrange
            var factory = new LedCommandFactory();

            // Act
            var result = factory.Create(json);

            // Assert
            Assert.IsType(expectedCommandType, result);
            Assert.Equal(command.Pid, result.Pid);
        }

        public class LegacyScriptParamsTestData : IEnumerable<object[]>
        {
            public static LegacyCommand LegacyColorCycleCommand => new LegacyCommand
            {
                LedId = 1,
                Colors = new[]
                {
                    new LedColor { R = ColorIntensity.Full, G = ColorIntensity.Off, B = ColorIntensity.Off },
                    new LedColor { R = ColorIntensity.Off, G = ColorIntensity.Off, B = ColorIntensity.Off }
                },
                BoardType = PacketHandling.BoardType.OnBoard,
                Pid = 0x1234,
                IntervalMs = 500
            };

            public static LegacyCommand LegacySolidColorCommand => new LegacyCommand
            {
                LedId = 1,
                Colors = new[]
                {
                    new LedColor { R = ColorIntensity.Full, G = ColorIntensity.Off, B = ColorIntensity.Off }
                },
                BoardType = PacketHandling.BoardType.OnBoard,
                Pid = 0x1234
            };

            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[]
                {
                    LegacyColorCycleCommand, typeof(ColorCycleCommand), Serialize(LegacyColorCycleCommand)
                };
                yield return new object[]
                {
                    LegacySolidColorCommand, typeof(SolidColorCommand), Serialize(LegacySolidColorCommand)
                };
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            private static string Serialize<T>(T command)
            {
                return System.Text.Json.JsonSerializer.Serialize(command);
            }
        }

        public class CommandTestData : IEnumerable<object[]>
        {
            public static LedCommand SolidColorCommand => new SolidColorCommand
            {
                LedId = 1,
                Color = new LedColor { R = ColorIntensity.Full, G = ColorIntensity.Off, B = ColorIntensity.Off },
                BoardType = PacketHandling.BoardType.OnBoard,
                Pid = 0x1234
            };

            public static LedCommand ColorCycleCommand => new ColorCycleCommand
            {
                LedId = 2,
                Colors = new[]
                {
                    new LedColor { R = ColorIntensity.Full, G = ColorIntensity.Off, B = ColorIntensity.Off },
                    new LedColor { R = ColorIntensity.Off, G = ColorIntensity.Full, B = ColorIntensity.Off },
                    new LedColor { R = ColorIntensity.Off, G = ColorIntensity.Off, B = ColorIntensity.Full }
                },
                IntervalMs = 500,
                BoardType = PacketHandling.BoardType.OnBoard,
                Pid = 0x1234
            };

            public static LedCommand SetDeviceSingleColorCommand => new SetDeviceSingleColorCommand
            {
                Color = new LedColor { R = ColorIntensity.Full, G = ColorIntensity.Off, B = ColorIntensity.Off },
                Pid = 0x1234
            };

            public static LedCommand ResetToDeviceFirmwareDefaultColorCommand => new ResetToFirmwareColoursCommand
            {
                Pid = 0x1234
            };

            public static LedCommand DoNothingCommand => new DoNothingCommand
            {
                Pid = 0x1234
            };


            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[]
                    { SolidColorCommand, typeof(SolidColorCommand), Serialize(SolidColorCommand) };
                yield return new object[]
                    { ColorCycleCommand, typeof(ColorCycleCommand), Serialize(ColorCycleCommand) };
                yield return new object[]
                {
                    SetDeviceSingleColorCommand, typeof(SetDeviceSingleColorCommand),
                    Serialize(SetDeviceSingleColorCommand)
                };
                yield return new object[]
                {
                    ResetToDeviceFirmwareDefaultColorCommand, typeof(ResetToFirmwareColoursCommand),
                    Serialize(ResetToDeviceFirmwareDefaultColorCommand)
                };
                yield return new object[]
                    { DoNothingCommand, typeof(DoNothingCommand), Serialize(DoNothingCommand) };
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            private static string Serialize<T>(T command)
            {
                return System.Text.Json.JsonSerializer.Serialize(command);
            }
        }
    }
}