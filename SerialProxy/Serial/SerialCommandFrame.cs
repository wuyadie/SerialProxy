using System;
using System.Buffers;
using System.Collections.Generic;
using System.Numerics;

namespace SerialProxy.Serial
{
    public class SerialCommandFrame
    {
        private static readonly ArrayPool<byte> FrameArrayPool
            = ArrayPool<byte>.Create(SerialSymbols.MaxFrameLength, 1000);

        /// <summary>
        /// Length of raw frame in bytes
        /// </summary>
        public int Length { get; private set; }

        private SerialSymbols.FrameType _type;
        /// <summary>
        /// Type of this serial frame
        /// </summary>
        public SerialSymbols.FrameType Type
        {
            get => _type;
            private set
            {
                if (!SerialSymbols.FrameLengthLookup.TryGetValue(value, out int length))
                {
                    throw new ArgumentException("Invalid type of serial frame!");
                }
                _type = value;
                Length = length;
            }
        }

        public byte[] data { get; set; }


        private byte[] _bytes;

        /// <summary>
        /// Bytes that are ready to send
        /// </summary>
        public Memory<byte> Bytes
        {
            get
            {
                // Begin
                _bytes[0] = SerialSymbols.FrameStart;
                _bytes[1] = (byte)(Length - 2);
                _bytes[2] = (byte)Type;

                // Fill data from _bytes[3]
                for(int i = 0; i < data.Length; i++)
                {
                    _bytes[i + 3] = data[i];
                }

                // Checksum
                _bytes[Length - 1] = SerialSymbols.XorChecksum(new Memory<byte>(_bytes, 2, Length - 3));

                return new Memory<byte>(_bytes, 0, Length);
            }
        }

        public SerialCommandFrame(SerialSymbols.FrameType type, byte[] dataIn)
        {
            Type = type;
            _bytes = FrameArrayPool.Rent(SerialSymbols.MaxFrameLength);
            data = dataIn;
        }


        ~SerialCommandFrame()
        {
            FrameArrayPool.Return(_bytes, true);
        }
    }
}
