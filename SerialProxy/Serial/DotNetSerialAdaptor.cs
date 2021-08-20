using System;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SerialProxy.Serial
{
    public class DotNetSerialAdaptor : ISerialAdaptor
    {
        public readonly RJCP.IO.Ports.SerialPortStream _serialPort;

        public DotNetSerialAdaptor(string portName)
        {
            _serialPort = new RJCP.IO.Ports.SerialPortStream(portName);
            _serialPort.ReadBufferSize = 128;
            _serialPort.WriteBufferSize = 128;
            _serialPort.Open();
            _serialPort.DataReceived += (o, e) => 
            { 
                SerialDataAvailableEvent?.Invoke(this);
                UpdateFrame(this);
            };

        }

        public static void UpdateFrame(ISerialAdaptor serial)
        {
            bool timeout;

            if (serial.ReadByte(out timeout) != SerialSymbols.FrameStart)
                return;

            var length = serial.ReadByte(out timeout);

            if (length > SerialSymbols.MaxDataLength || length == 0)
                return;

            // After frame start and size
            byte[] chunk = new byte[length];
            var mem = new Memory<byte>(chunk);

            serial.Read(mem);
            serial.DiscardReadBuffer();

            byte checksum = chunk[length - 1];
            var data = new Memory<byte>(chunk, 0, length - 1); // includes type

            if (!SerialSymbols.XorChecker(data, checksum))
                return;

            var type = (SerialSymbols.FrameType)chunk[0];
            var frameData = data.Slice(1, data.Length - 1).ToArray();
            var frame = new SerialCommandFrame(type, frameData); // no type, no xor

            switch (type)
            {
                case SerialSymbols.FrameType.GetMouseData:
                    Mouse.State = frame.data[0];
                    Mouse.MouseX = (sbyte)frame.data[1];
                    Mouse.MouseY = (sbyte)frame.data[2];
                    Mouse.Wheel = (sbyte)frame.data[3];
                    Mouse.WheelH = (sbyte)frame.data[4];
                    break;

                case SerialSymbols.FrameType.GetKeyboardData:
                    Keyboard.ModifierKey = frame.data[0];
                    Keyboard.Keys = frame.data.Skip(1).Take(frame.data.Length - 1).ToArray();
                    break;
            }

        }

        public event ISerialAdaptor.SerialDataAvailable SerialDataAvailableEvent;

        public byte ReadByte(out bool timeout)
        {
            byte ret = 0;
            try
            {
                ret = (byte)_serialPort.ReadByte();
            }
            catch (TimeoutException)
            {
                timeout = true;
                return ret;
            }
            timeout = false;
            return ret;
        }

        public void WriteByte(byte b)
        {
            Span<byte> toSend = stackalloc byte[1];
            _serialPort.Write(toSend);
        }

        public int Read(Memory<byte> memory)
        {
            return _serialPort.Read(memory.Span);
        }

        public void Write(Memory<byte> memory)
        {
            _serialPort.Write(memory.Span);
        }

        public int AvailableBytes => (int)_serialPort.Length;

        public ValueTask<int> AsyncRead(Memory<byte> memory, CancellationToken token = default)
        {
            return _serialPort.ReadAsync(memory, token);
        }

        public ValueTask AsyncWrite(Memory<byte> memory, CancellationToken token = default)
        {
            return _serialPort.WriteAsync(memory, token);
        }

        public async ValueTask<byte> AsyncReadByte(CancellationToken token = default)
        {
            byte[] arr = new byte[1];
            await AsyncRead(arr, token).ConfigureAwait(false);
            return arr[0];
        }

        public async ValueTask AsyncWriteByte(byte b, CancellationToken token = default)
        {
            byte[] arr = new byte[1];
            arr[0] = b;
            await AsyncWrite(arr, token).ConfigureAwait(false);
        }

        public void DiscardReadBuffer()
        {
            _serialPort.Flush();
            _serialPort.DiscardInBuffer();
        }

        public void Dispose()
        {
            _serialPort?.Dispose();
        }
    }
}