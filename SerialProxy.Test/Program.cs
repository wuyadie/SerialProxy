using SerialProxy.Serial;
using System;
using System.Linq;

namespace SerialProxy.Test
{
    class Program
    {
        public static SerialProxy.Mouse Mouse;

        public static SerialProxy.Keyboard Keyboard;

        static void Main(string[] args)
        {
            string serialPort = "COM3";

            // Setup arduino mouse
            DotNetSerialAdaptor serial = null;
            try
            {
                serial = new DotNetSerialAdaptor(serialPort);
                Console.Error.WriteLine($"Arduino connected successfully on port {serialPort}");

            }
            catch (Exception)
            {
                Console.Error.WriteLine($"Cannot connect to port {serialPort}!");
                return;
            }

            if (serial != null)
            {
                Mouse = new SerialProxy.Mouse(serial);
                Keyboard = new SerialProxy.Keyboard(serial);
            }

            while(true)
            {
                //Mouse.SetMousePos(0, -10);
                //Mouse.SetMousePress(SerialSymbols.MouseButton.Right); // works
                //Mouse.SetMouseScroll(10); // works


                var mousePos = Mouse.GetMousePos();
                if (mousePos.X != 0 || mousePos.Y != 0)
                    Console.WriteLine($"Mouse Coords: {mousePos}");

                var mouseScroll = Mouse.GetMouseScroll();
                if(mouseScroll.X != 0 || mouseScroll.Y != 0)
                    Console.WriteLine($"Mouse Scroll: {mouseScroll}");

                var mouseState = Mouse.GetMouseState();

                if(mouseState != 0)
                    Console.WriteLine($"Mouse State: {mouseState}");

                //var keyboardState = Keyboard.GetKeyboardKeys();
                //if(keyboardState != 0)
                //    Console.WriteLine($"Keyboard State: {keyboardState}");

                //var modifier = Keyboard.GetKeyboardModifier()
                //var pressedKeys = Keyboard.GetKeyboardKeys()
                //Keyboard.SetKeyboardPress(0x04)
                //Keyboard.SetKeyboardRelease(0x04)
                //Keyboard.KeyboardReleaseAll()

                System.Threading.Thread.Sleep(100);
            }
        }
    }
}
