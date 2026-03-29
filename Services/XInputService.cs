using System.Runtime.InteropServices;

namespace phonic.Services;

public static class XInputService
{
    const int ERROR_SUCCESS = 0;
    const short BUTTON_DPAD_UP = 0x0001;
    const short BUTTON_DPAD_DOWN = 0x0002;
    const short BUTTON_BACK = 0x0020;
    const short BUTTON_A = 0x1000;
    const short BUTTON_B = 0x2000;
    const short THUMB_DEADZONE = 16000;

    static bool _available = true;

    [DllImport("xinput1_4.dll", EntryPoint = "XInputGetState")]
    static extern int XInputGetState(int dwUserIndex, ref XInputState pState);

    [StructLayout(LayoutKind.Explicit)]
    struct XInputState
    {
        [FieldOffset(0)] public int PacketNumber;
        [FieldOffset(4)] public XInputGamepad Gamepad;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct XInputGamepad
    {
        public short Buttons;
        public byte LeftTrigger;
        public byte RightTrigger;
        public short ThumbLX;
        public short ThumbLY;
        public short ThumbRX;
        public short ThumbRY;
    }

    public record ControllerState(
        bool DpadUp, bool DpadDown,
        bool A, bool B, bool Back,
        bool StickUp, bool StickDown
    );

    public static ControllerState? GetStateForAnyController()
    {
        if (!_available) return null;

        for (var i = 0; i < 4; i++)
        {
            var state = ReadState(i);
            if (state != null) return state;
        }

        return null;
    }

    static ControllerState? ReadState(int index)
    {
        try
        {
            var raw = new XInputState();
            if (XInputGetState(index, ref raw) != ERROR_SUCCESS) return null;

            var b = raw.Gamepad.Buttons;
            var ly = raw.Gamepad.ThumbLY;

            return new ControllerState(
                DpadUp: (b & BUTTON_DPAD_UP) != 0,
                DpadDown: (b & BUTTON_DPAD_DOWN) != 0,
                A: (b & BUTTON_A) != 0,
                B: (b & BUTTON_B) != 0,
                Back: (b & BUTTON_BACK) != 0,
                StickUp: ly > THUMB_DEADZONE,
                StickDown: ly < -THUMB_DEADZONE
            );
        }
        catch (DllNotFoundException)
        {
            _available = false;
            return null;
        }
    }
}
