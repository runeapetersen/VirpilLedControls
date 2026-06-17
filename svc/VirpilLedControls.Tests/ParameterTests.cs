using System;
using Xunit;

namespace VirpilLedControls.Tests
{
    public class ParameterTests
    {
        [Fact]
        public void SingleColorLightParameters_CorrectArgument_Passes()
        {
            var parameter = "1234 5678 98 76 54 32";
            var parameterDefinition = new SingleColourLightScript.ParameterDefinition(parameter);
            Assert.Equal("1234 5678", parameterDefinition.DeviceId);
            Assert.Equal("98", parameterDefinition.ButtonId);
            Assert.Equal("76 54 32", parameterDefinition.ColorState);
        }
        
        [Fact]
        public void SingleColorLightParameters_IncorrectArgument_Throws()
        {
            var parameter = "1234 5678 98 76 54";
            Assert.Throws<ArgumentException>(() => new SingleColourLightScript.ParameterDefinition(parameter));
        }
        
        [Fact]
        public void BlinkingLightParameters_CorrectArgument_Passes()
        {
            var parameter = "1234 5678 98 76 54 32 10 01 23 10000";
            var parameterDefinition = new BlinkingLightScript.ParameterDefinition(parameter);
            Assert.Equal("1234 5678", parameterDefinition.DeviceId);
            Assert.Equal("98", parameterDefinition.ButtonId);
            Assert.Equal("76 54 32", parameterDefinition.FirstColorState);
            Assert.Equal("10 01 23", parameterDefinition.SecondColorState);
            Assert.Equal(10000, parameterDefinition.BlinkIntervalMs);
        }
        
        [Fact]
        public void BlinkingLightParameters_IncorrectArgument_Throws()
        {
            var parameter = "1234 5678 98 76 54 32 10 01 23";
            Assert.Throws<ArgumentException>(() => new BlinkingLightScript.ParameterDefinition(parameter));
        }
    }
}