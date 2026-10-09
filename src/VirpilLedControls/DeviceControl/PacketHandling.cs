using System;
using VirpilLedControls.Model.Commands;

namespace VirpilLedControls.DeviceControl
{
    public static class PacketHandling
    {
        // HID Packet Structure Constants
        private const int PacketLength = 38;
        private const byte HeaderByte = 0x02;
        private const byte FooterByte = 0xF0;
        private const int ColorOffsetIndex = 4;

        public static byte[] CreatePacket(BoardType boardType, uint ledNumber, ColorIntensity red, ColorIntensity green,
            ColorIntensity blue)
        {
            if (ledNumber >= PacketLength - ColorOffsetIndex)
                throw new ArgumentOutOfRangeException(nameof(ledNumber), "LED index out of range for HID report.");
            var data = new byte[PacketLength];
            data[0] = HeaderByte;
            data[1] = (byte)boardType;
            if (boardType == BoardType.ResetToColorReserved ||
                boardType ==
                BoardType.ResetToDefaultsReserved) // Force colour info to first LED slot for reset commands
                ledNumber = 1;
            data[ledNumber + ColorOffsetIndex] = ByteForColors(red, green, blue);
            data[PacketLength - 1] = FooterByte;

            return data;
        }

        private static byte ByteForColors(ColorIntensity red, ColorIntensity green, ColorIntensity blue)
        {
            byte b = 0b_1000_0000;
            b |= ByteForColor(red);
            b |= (byte)(ByteForColor(green) << 2);
            b |= (byte)(ByteForColor(blue) << 4);
            return b;
        }

        private static byte ByteForColor(ColorIntensity color)
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

        /// <summary>
        /// Notes to self: The board type is determined by looking at the VPC Configuration Tool.
        /// A controller seems to consist of a an on-board controller and up to 4 slave boards.
        /// The on-board controller is always present, and the slave boards are optional. The board type is used to determine which group of LEDs to control.
        /// Joystick attachments and slaved control panels are listed as slave boards in the VPC Configuration Tool, and are assigned a slave board number 1-4.
        /// </summary>
        public enum BoardType : byte
        {
            /// <summary>
            /// Reserved type for setting the LEDs to their firmware defaults. Ignores color information in the packet.
            /// </summary>
            ResetToDefaultsReserved = 0x64,

            /// <summary>
            /// Reserved type for setting the LEDs to a specific color
            /// </summary>
            ResetToColorReserved = 0x65,

            /// <summary>
            /// On-board controller
            /// </summary>
            OnBoard = 0x66,

            /// <summary>
            /// Slave Board 1 controller
            /// </summary>
            SlaveBoard1 = 0x67,

            /// <summary>
            /// Slave Board 2 controller
            /// </summary>
            SlaveBoard2 = 0x68,

            /// <summary>
            /// Slave Board 3 controller
            /// </summary>
            SlaveBoard3 = 0x69,

            /// <summary>
            /// Slave Board 4 controller
            /// </summary>
            SlaveBoard4 = 0x6A
        }
    }
}