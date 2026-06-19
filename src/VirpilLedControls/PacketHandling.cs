using System;

namespace VirpilLedControls
{
    internal static class PacketHandling
    {
        // HID Packet Structure Constants
        private const int PacketLength = 38;
        private const byte HeaderByte = 0x02;
        private const byte FooterByte = 0xF0;
        private const int ColorOffsetIndex = 4;

        // LED Group Base Indices for Command ID Calculation
        private const byte DefaultGroupBase = 0;
        private const byte AddBoardGroupBase = 0;       // Uses raw ledNumber
        private const byte OnBoardGroupBase = 4;
        private const byte SlaveBoardGroupBase = 24;
        private const byte ExtraBoardGroupBase = 44;

        internal static byte[] CreatePacket(BoardType boardType, int ledNumber, ColorIntensity red, ColorIntensity green, ColorIntensity blue)
        {
            if (ledNumber < 0 || ledNumber >= PacketLength - ColorOffsetIndex)
                throw new ArgumentOutOfRangeException(nameof(ledNumber), "LED index out of range for HID report.");
            var data = new byte[PacketLength];
            data[0] = HeaderByte;
            data[1] = (byte)boardType;
            data[2] = CommandIdForCommand(boardType, ledNumber);
            data[ledNumber + ColorOffsetIndex] = ByteForColors(red, green, blue);
            data[PacketLength - 1] = FooterByte;

            return data;
        }

        internal static byte CommandIdForCommand(BoardType boardType, int ledNumber)
        {
            switch (boardType)
            {
                case BoardType.Default:
                    return DefaultGroupBase;
                case BoardType.AddBoard:
                    return (byte)(AddBoardGroupBase + ledNumber);
                case BoardType.OnBoard:
                    return (byte)(OnBoardGroupBase + ledNumber);
                case BoardType.SlaveBoard:
                    return (byte)(SlaveBoardGroupBase + ledNumber);
                case BoardType.ExtraBoard:
                    return (byte)(ExtraBoardGroupBase + ledNumber);
                default:
                    throw new ArgumentOutOfRangeException(nameof(boardType), boardType, null);
            }
        }

        internal static byte ByteForColors(ColorIntensity red, ColorIntensity green, ColorIntensity blue)
        {
            byte b = 0b_1000_0000;
            b |= ByteForColor(red);
            b |= (byte)(ByteForColor(green) << 2);
            b |= (byte)(ByteForColor(blue) << 4);
            return b;
        }

        internal static byte ByteForColor(ColorIntensity color)
        {
            switch (color)
            {
                case ColorIntensity.Off:
                    return 0;
                case ColorIntensity.Thirty:
                    return 1;
                case ColorIntensity.Sixty:
                    return 2;
                case ColorIntensity.Full:
                    return 3;
                default:
                    throw new ArgumentOutOfRangeException(nameof(color), color, null);
            }
        }

        public enum BoardType : byte
        {
            /// <summary>
            /// Not necessarily a board type, but is used when setting the LEDs to default
            /// </summary>
            Default = 0x64,

            /// <summary>
            /// LEDs which are part of a board that is directly attached to the target board (e.g. a joystick)
            /// </summary>
            AddBoard = 0x65,

            /// <summary>
            /// LEDs which are on the board that is directly connected to USB
            /// </summary>
            OnBoard = 0x66,

            /// <summary>
            /// LEDs which are on a slave board connected to a parent board which is directly connected to USB
            /// </summary>
            SlaveBoard = 0x67,

            /// <summary>
            /// LEDs used by boards such as the Alpha Prime (why? who knows!)
            /// </summary>
            ExtraBoard = 0x68,
        }
    }
}