using System;

namespace SerialProxy.Serial
{
    /// <summary>
    /// A reliable serial communication utility class to make sure all
    /// commands are sent correctly and in-order. Otherwise, throw exception
    /// <see cref="SerialDeviceException"/>. If a frame was received by the device,
    /// device will loop it back.
    /// </summary>
    internal class FrameSender : IDisposable
    {
        private readonly DotNetSerialAdaptor _serial;

        public FrameSender(DotNetSerialAdaptor serial)
        {
            _serial = serial ?? throw new ArgumentNullException(nameof(serial));
        }

        /// <summary>
        /// Send frame bytes to serial, and wait respond. 
        /// </summary>
        /// <param name="bytes">Bytes to sent</param>
        /// <exception cref="SerialDeviceException"> If timeout or exceed maximum number of retries.</exception>
        public void SendFrame(SerialCommandFrame frame)
        {
            if (!ValidFrameBytes(frame.Bytes))
            {
                throw new ArgumentException("Invalid frame bytes!");
            }

            // It only blocks if the output buffer is already full or becomes full.
            _serial.Write(frame.Bytes);
        }

        private static bool ValidFrameBytes(Memory<byte> memory)
        {
            Span<byte> span = memory.Span;
            int length = span.Length;
            if (length > SerialSymbols.MaxFrameLength || length < SerialSymbols.MinFrameLength)
            {
                return false;
            }

            if (span[0] != SerialSymbols.FrameStart)
            {
                return false;
            }

            SerialSymbols.FrameType type = (SerialSymbols.FrameType)span[2];
            if (!SerialSymbols.ValidFrameTypes.Contains(type))
            {
                return false;
            }

            byte checksum = span[length - 1];
            if (!SerialSymbols.XorChecker(memory.Slice(2, length - 3), checksum))
            {
                return false;
            }

            return true;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
