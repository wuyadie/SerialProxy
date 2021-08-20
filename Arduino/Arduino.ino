#ifdef _DEBUG

#define debug_print(arg) do{}while(0);
#define debug_println(arg) do{ }while(0);

#else

#define debug_print(arg) do{ }while(0);
#define debug_println(arg) do{ }while(0);

#endif

constexpr uint8_t FRAME_START = 0xABu;
constexpr uint8_t MAX_DATA_LENGTH = 10; // Data(max 8-byte) + Checksum(1-byte)
constexpr uint8_t MAX_FRAME_LENGTH = MAX_DATA_LENGTH + 2; // Prefix 0xAB <Length> are 2 bytes
constexpr unsigned int RECEIVE_DATA_BUFFER_SIZE = 128u;
static_assert(RECEIVE_DATA_BUFFER_SIZE >= MAX_FRAME_LENGTH + 2, "Serial receiving buffer must larger than frame size!");
#define ControlSerial Serial5

#include <USBHost_t36.h>

/****************************** Enums ***********************************/

enum FrameType
{
      // Set
      FRAME_TYPE_SET_MOUSE_POS = 0xAAu,
      FRAME_TYPE_SET_MOUSE_SCROLL = 0xABu,
      FRAME_TYPE_SET_MOUSE_PRESS = 0xACu,
      FRAME_TYPE_SET_MOUSE_RELEASE = 0xADu,
      FRAME_TYPE_SET_KEYBOARD_PRESS = 0xBBu,
      FRAME_TYPE_SET_KEYBOARD_RELEASE = 0xBCu,

      // Get
      FRAME_TYPE_GET_MOUSE_DATA = 0xCAu,
      FRAME_TYPE_GET_KEYBOARD_DATA = 0xDAu,

      FRAME_TYPE_UNKNOWN = 0xFF
};

constexpr uint8_t RELEASE_ALL_KEYS = 0x00u;

/*************************** Implementation ***************************/
inline uint8_t xor_checksum(const uint8_t* data, const uint8_t length)
{
    if (length == 0)
    {
        return true;
    }
    const uint8_t* ptr = data;
    uint8_t checksum = *(ptr++);
    for (uint8_t i = 1; i < length; ++i)
    {
        checksum ^= *ptr;
        ++ptr;
    }
    return checksum;
}

/*************************** Script ***************************/

// TODO: fully upgrade to 16-bit integer (set & read)
// TODO: enable fn keys on keyboard passthrough (media key?)

USBHost usb_host;
USBHub hub(usb_host);
USBHIDParser hid(usb_host);
MouseController mouse_in(usb_host);
KeyboardController keyboard_in(usb_host);

void setup() { 
  // Serial to PC
  ControlSerial.begin(115200);

  // USB IN
  usb_host.begin();

  // Listen to keyboard updates
  keyboard_in.attachReportReader(reportReader);

  ControlSerial.println("ControlSerial Initialized!");
}

void loop() {
  usb_host.Task();

  // Listen to mouse updates
  if (mouse_in.available()) 
  { 
    // Proxy Mouse
    report_mouse_update_to_host();

    // Real data to serial
    report_mouse_update_to_serial();
    
    mouse_in.mouseDataClear();
  }

  // Set commands
  handle_host_commands();
}

void report_mouse_update_to_serial()
{
  const int mouse_data_size = 9;
  int8_t mouse_data[mouse_data_size];

  mouse_data[0] = FRAME_START; // Frame Start
  mouse_data[1] = mouse_data_size - 2; // Size
  mouse_data[2] = FRAME_TYPE_GET_MOUSE_DATA; // Type
  mouse_data[3] = mouse_in.getButtons();
  mouse_data[4] = mouse_in.getMouseX();
  mouse_data[5] = mouse_in.getMouseY();
  mouse_data[6] = mouse_in.getWheel();
  mouse_data[7] = mouse_in.getWheelH();

  int8_t data[mouse_data_size - 2];
  memcpy(data, mouse_data + 2, sizeof(data));
  
  mouse_data[8] = xor_checksum((uint8_t*)data, mouse_data[1] - 1); // Checksum

  ControlSerial.write((uint8_t*) mouse_data, mouse_data_size);
}

void handle_host_commands()
{
  static uint8_t data_buffer[RECEIVE_DATA_BUFFER_SIZE];
  static uint8_t* const ptr_data = data_buffer + 2; // reserve 2 bytes for loop-back frame
  
  if(!ControlSerial.available())
    return;

  if (ControlSerial.read() != FRAME_START)
    return;
  
  // Read length
  uint8_t length = 0xFFu;
  ControlSerial.readBytes(&length, 1);
  if (length > MAX_DATA_LENGTH || length == 0)
  {
      debug_println("Incorrect data length!");
      return;
  }
  // Read data
  if (ControlSerial.readBytes(ptr_data, length) != length)
  {
      debug_println("Reading data timeout!");
      return;
  }
  
  // Integrity check
  if (xor_checksum(ptr_data, length - 1) != ptr_data[length - 1])
  {
      debug_println("Corrupted data!");
      return;
  }

  // Construct reply of the same frame to indicate host that we've complete the frame
  data_buffer[0] = FRAME_START;
  data_buffer[1] = length;

    // Execute the frame
  const uint8_t type = ptr_data[0];
  switch (type)
  {
    // Mouse set
    case FRAME_TYPE_SET_MOUSE_POS:
    {
          // -128 to 127
          const int8_t x = static_cast<int8_t>(ptr_data[1]);
          const int8_t y = static_cast<int8_t>(ptr_data[2]);

          // Set relative position
          Mouse.move(x, y);
          break;
    }

    case FRAME_TYPE_SET_MOUSE_SCROLL:
    {
          // -1 to +1
          const int8_t step = static_cast<int8_t>(ptr_data[1]);
          Mouse.scroll(step);
          break;
    }

    case FRAME_TYPE_SET_MOUSE_PRESS:
    {
          const uint8_t key = ptr_data[1];
          Mouse.press(key);
          break;
    }

    case FRAME_TYPE_SET_MOUSE_RELEASE:
    {
          const uint8_t key = ptr_data[1];
          if (key == RELEASE_ALL_KEYS) 
          {
              Mouse.release(MOUSE_LEFT | MOUSE_RIGHT | MOUSE_MIDDLE);
          } 
          else 
          {
              Mouse.release(key);
          }
          break;
    }

    // Keyboard set
    case FRAME_TYPE_SET_KEYBOARD_PRESS:
    {
          uint16_t key = 0;
          memcpy(&key, ptr_data + 1, 2);
          
          Keyboard.press(key);
          break;
    }

    case FRAME_TYPE_SET_KEYBOARD_RELEASE:
    {
          uint16_t key = 0;
          memcpy(&key, ptr_data + 1, 2);
          
          if (key == RELEASE_ALL_KEYS)
          {
              Keyboard.releaseAll();
          }
          else
          {
              Keyboard.release(key);
          }
          break;
    }

   default:
   {
      return;
   }

  }
        
}

// Proxy mouse data to host
void report_mouse_update_to_host()
{
  // Inside teensy4/usb_mouse.h
  usb_mouse_buttons_state = mouse_in.getButtons();
  usb_mouse_move(mouse_in.getMouseX(), mouse_in.getMouseY(), mouse_in.getWheel(), mouse_in.getWheelH());
}

// https://forum.pjrc.com/threads/51869-USB-keyboard-hardware-proxy
void reportReader(const uint8_t report[8])
{
  // Set emulated keyboard
  Keyboard.set_modifier(report[0]);
  Keyboard.set_key1(report[2]);
  Keyboard.set_key2(report[3]);
  Keyboard.set_key3(report[4]);
  Keyboard.set_key4(report[5]);
  Keyboard.set_key5(report[6]);
  Keyboard.set_key6(report[7]);
  Keyboard.send_now();

  // Report to host
  const int keyboard_data_size = 12;
  int8_t keyboard_data[keyboard_data_size];

  keyboard_data[0] = FRAME_START; // Frame Start
  keyboard_data[1] = keyboard_data_size - 2; // Size
  keyboard_data[2] = FRAME_TYPE_GET_KEYBOARD_DATA; // Type
  keyboard_data[3] = report[0];
  keyboard_data[4] = report[1];
  keyboard_data[5] = report[2];
  keyboard_data[6] = report[3];
  keyboard_data[7] = report[4];
  keyboard_data[8] = report[5];
  keyboard_data[9] = report[6];
  keyboard_data[10] = report[7];

  int8_t data[keyboard_data_size - 2];
  memcpy(data, keyboard_data + 2, sizeof(data));
  
  keyboard_data[11] = xor_checksum((uint8_t*)data, keyboard_data[1] - 1); // Checksum

  ControlSerial.write((uint8_t*) keyboard_data, keyboard_data_size);
  
}