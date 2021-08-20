<img src="https://cdn.antratek.nl/media/product/68a/usb-host-cable-for-teensy-3-6-and-teensy-4-1-cable-usb-host-t36-dc0.jpg" width="200" align="right" />

# SerialProxy
SerialProxy is a complete MITM solution for **modifying 🖱️ mouse & ⌨️ keyboard input against highly sophisticated anti-cheats (ESEA/Faceit/Vanguard/...)**. 
This .NET Core package provides convenient access to the **Teensy 4.1 interface** to get/set proper input values over the provided USB Host Shield.

```
    --------------------                 --------------                    ------------
    | USB Mouse/Keyboard | --[USB HUB]-- | Teensy 4.1 | --[FAKE HID USB]-- | Computer |
    --------------------                 --------------                    ------------
                                               |                                | [Serial USB]
                                               |              -------------------------
                                               ---[SERIAL]--- | USB To SERIAL Adapter |
                                                              -------------------------
```

Preparation: Check this [Tutorial](https://www.unknowncheats.me/forum/anti-cheat-bypass/439183-mouse-proxy-teensy-4-1-a.html)
1) Override Arduino files (USB Mode Modification + Keyboard Interception)
2) Select "Logitech USB Receiver" in Arduino IDE, Paste *.ino script, flash it
3) Modify input signal using .NET Core Library

# Features
- Set Mouse Cursor Position (Relative X,Y)
- Set Mouse Scroll (Relative Y)
- Set Mouse Press/Release (Left, Right, Middle, Back, Forward)
- Set Keyboard Press/Release ([Keyboard Codes](https://gist.github.com/MightyPork/6da26e382a7ad91b5496ee55fdc73db2))
- Get real Keyboard/Mouse Data (Being pressed on the real HID device)

# Quickstart Guide
```csharp
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

Mouse = new SerialProxy.Mouse(serial);
Keyboard = new SerialProxy.Keyboard(serial);

// Modify fake mouse
Mouse.SetMousePos(0, -10);

```

# Example codes
All examples can be found in the [testing project](https://github.com/earthlion/SerialProxy/blob/main/SerialProxy.Test/Program.cs).
If you have any questions, feel free to open an issue on Github.

# Credits
🧍 [Me](https://github.com/earthlion) - API<br/>
🧍 [mysnphit](https://www.unknowncheats.me/forum/members/165040.html) - Base<br/>

# License
This project is released under MIT License. Please refer the LICENSE.txt for more details.
