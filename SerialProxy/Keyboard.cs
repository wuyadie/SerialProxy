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
    public class Keyboard : IDisposable
    {
        private readonly FrameSender _sender;
        private bool _disposedValue;

        public static byte ModifierKey { get; set; }

        public static byte[] Keys { get; set; }

        public Keyboard(DotNetSerialAdaptor serial)
        {
            _sender = new FrameSender(serial);
            Keys = new byte[6];
        }

        #region Set

        public void SetKeyboardPress(ushort key)
        {
            SerialCommandFrame frame = new SerialCommandFrame(SerialSymbols.FrameType.SetKeyboardPress, BitConverter.GetBytes(key));
            _sender.SendFrame(frame);
        }

        public void SetKeyboardRelease(ushort key)
        {
            SerialCommandFrame frame = new SerialCommandFrame(SerialSymbols.FrameType.SetKeyboardRelease, BitConverter.GetBytes(key));
            _sender.SendFrame(frame);
        }

        public void KeyboardReleaseAll()
        {
            SerialCommandFrame frame = new SerialCommandFrame(SerialSymbols.FrameType.SetKeyboardRelease, BitConverter.GetBytes((ushort)SerialSymbols.ReleaseAllKeys));
            _sender.SendFrame(frame);
        }


        #endregion

        #region Get

        // Mouse States: 
        // MOUSE_LEFT 1
        // MOUSE_MIDDLE 4
        // MOUSE_RIGHT 2
        // MOUSE_BACK 8
        // MOUSE_FORWARD 16

        // Keyboard scancodes: https://gist.github.com/MightyPork/6da26e382a7ad91b5496ee55fdc73db2

        /// <summary>
        /// Modified keys pressed
        /// </summary>
        /// <returns></returns>
        public byte GetKeyboardModifier()
        {
            return ModifierKey;
        }

        /// <summary>
        /// Pressed keys
        /// </summary>
        /// <returns></returns>
        public byte[] GetKeyboardKeys()
        {
            return Keys;
        }

        #endregion


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
