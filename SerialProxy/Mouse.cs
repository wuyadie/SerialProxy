using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using SerialProxy.Serial;

namespace SerialProxy
{
    public class Mouse : IDisposable
    {
        private readonly FrameSender _sender;
        private bool _disposedValue;

        public static byte State { get; set; }
        public static int MouseX { get; set; }

        public static int MouseY { get; set; }

        public static int Wheel { get; set; }

        public static int WheelH { get; set; }

        public Mouse(DotNetSerialAdaptor serial)
        {
            _sender = new FrameSender(serial);
        }

        #region Set

        /// <summary>
        /// Set relative mouse position
        /// Range: -128 to 127
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void SetMousePos(sbyte x, sbyte y)
        {
            SerialCommandFrame frame = new SerialCommandFrame(SerialSymbols.FrameType.SetMousePos, new byte[] { (byte)x, (byte)y });
            _sender.SendFrame(frame);
        }

        public void SetMouseScroll(sbyte value)
        {
            SerialCommandFrame frame = new SerialCommandFrame(SerialSymbols.FrameType.SetMouseScroll, new byte[] { (byte)value });
            _sender.SendFrame(frame);
        }

        public void SetMousePress(SerialSymbols.MouseButton button)
        {
            SerialCommandFrame frame = new SerialCommandFrame(SerialSymbols.FrameType.SetMousePress, new byte[] { (byte)button });
            _sender.SendFrame(frame);
        }


        public void SetMouseRelease(SerialSymbols.MouseButton button)
        {
            SerialCommandFrame frame = new SerialCommandFrame(SerialSymbols.FrameType.SetMouseRelease, new byte[] { (byte)button });
            _sender.SendFrame(frame);
        }

        public void SetMouseReleaseAll()
        {
            SerialCommandFrame frame = new SerialCommandFrame(SerialSymbols.FrameType.SetMouseRelease, new byte[] { (byte)SerialSymbols.ReleaseAllKeys });
            _sender.SendFrame(frame);
        }

        #endregion

        #region Get


        #endregion

        /// <summary>
        /// Relative mouse movement
        /// </summary>
        /// <returns></returns>
        public Point GetMousePos()
        {
            var savedPoint = new Point(MouseX, MouseY);

            MouseX = 0;
            MouseY = 0;

            return savedPoint;
        }


        /// <summary>
        /// Relative wheel movement
        /// </summary>
        /// <returns></returns>
        public Point GetMouseScroll()
        {
            var savedPoint = new Point(Wheel, WheelH);

            Wheel = 0;
            WheelH = 0;

            return savedPoint;
        }

        public byte GetMouseState()
        {
            var savedState = State;

            return savedState;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _sender.Dispose();
                }
                _disposedValue = true;
            }
        }
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
