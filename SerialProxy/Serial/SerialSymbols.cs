using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SerialProxy.Serial
{
    public static class SerialSymbols
    {

        public const byte FrameStart = 0xAB;

        public const int MinFrameLength = 5; // 0xAB <Length> <Type> <Value> <Checksum>

        public const int MaxDataLength = 10; // 8-byte coordinates + <Type> + <Checksum>

        public const int MaxFrameLength = MaxDataLength + 2;

        public enum FrameType
        {
            // Set
            SetMousePos = 0xAA,
            SetMouseScroll = 0xAB,
            SetMousePress = 0xAC,
            SetMouseRelease = 0xAD,
            SetKeyboardPress = 0xBB,
            SetKeyboardRelease = 0xBC,

            // Get
            GetMouseData = 0xCA,
            GetKeyboardData = 0xDA,

            Unknown = 0xFF
        }

        public const int ReleaseAllKeys = 0x00;

        [Flags]
        public enum MouseButton
        {
            Left = 1,
            Right = 2,
            Middle = 4,
            Back = 8,
            Forward = 16,
        }

        /// <summary>
        /// Dictionary mapped frame type to frame length
        /// </summary>
        public static Dictionary<FrameType, int> FrameLengthLookup =
            new Dictionary<FrameType, int>
            {
                // Set
                {FrameType.SetMousePos, 6}, // 0xAB 0x06 0xAA <byte x> <byte y> <Checksum>
                {FrameType.SetMouseScroll, 5}, // 0xAB 0x03 0xAB <Value> <Checksum>
                {FrameType.SetMousePress, 5}, // 0xAB 0x03 0xAC <Key> <Checksum>
                {FrameType.SetMouseRelease, 5}, // 0xAB 0x03 0xAD <Key> <Checksum>
                {FrameType.SetKeyboardPress, 6}, // 0xAB 0x03 0xBB <2-byte key> <Checksum>
                {FrameType.SetKeyboardRelease, 6}, // 0xAB 0x03 0xBC <2-byte key> <Checksum>

                // Get
                {FrameType.GetMouseData, 9}, // 0xAB 0x06 0xCA <8-byte coordinate> <Checksum>
                {FrameType.GetKeyboardData, 12}, // 0xAB 0x03 0xCB <4-byte Value> <Checksum>
            };

        /// <summary>
        /// All valid frame types
        /// </summary>
        public static HashSet<FrameType> ValidFrameTypes
            = new HashSet<FrameType>(FrameLengthLookup.Keys);

        public static byte XorChecksum(Memory<byte> memory)
        {
            if (memory.Length == 0)
            {
                return 0;
            }
            Span<byte> arr = memory.Span;
            byte ret = arr[0];
            for (int i = 1; i < arr.Length; ++i)
            {
                ret ^= arr[i];
            }
            return ret;
        }

        public static bool XorChecker(Memory<byte> memory, byte desired)
        {
            return XorChecksum(memory) == desired;
        }
    }

}
