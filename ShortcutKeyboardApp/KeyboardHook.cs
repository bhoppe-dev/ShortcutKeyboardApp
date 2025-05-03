using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ShortcutKeyboardApp
{
    public class KeyboardHook
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private const int WM_SYSKEYDOWN = 0x0104;
        private const int WM_SYSKEYUP = 0x0105;

        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;

        private static bool _isLeftShiftPressed = false;
        private static bool _isLeftAltPressed = false;
        private static bool _isLeftControlPressed = false;

        public static event EventHandler<MacroKeyPressedEventArgs> MacroKeyPressed;

        public static void SetHook()
        {
            Debug.WriteLine("Setting up keyboard hook");
            _hookID = SetWindowsHookEx(WH_KEYBOARD_LL, _proc, GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName), 0);
            Debug.WriteLine($"Hook ID: {_hookID}");
        }

        public static void UnhookWindowsHookEx()
        {
            UnhookWindowsHookEx(_hookID);
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                Debug.WriteLine($"Key pressed: VK code = {vkCode}, Key = {(Keys)vkCode}");

                if (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN)
                {
                    if (_isLeftShiftPressed && _isLeftAltPressed && _isLeftControlPressed)
                    {
                        int macroKeyIndex = GetMacroKeyIndex(vkCode);
                        Debug.WriteLine($"Macro key detected: VK code = {vkCode}, Mapped index = {macroKeyIndex}");
                        if (macroKeyIndex != -1)
                        {
                            MacroKeyPressed?.Invoke(null, new MacroKeyPressedEventArgs(macroKeyIndex));
                            return (IntPtr)1;
                        }
                    }
                    else if (vkCode == 0xA0) // Left Shift
                    {
                        _isLeftShiftPressed = true;
                        Debug.WriteLine("Left Shift pressed");
                    }
                    else if (vkCode == 0xA4) // Left Alt
                    {
                        _isLeftAltPressed = true;
                        Debug.WriteLine("Left Alt pressed");
                    }
                    else if (vkCode == 0xA2) // Left Control
                    {
                        _isLeftControlPressed = true;
                        Debug.WriteLine("Left Control pressed");
                    }
                }
                else if (wParam == (IntPtr)WM_KEYUP || wParam == (IntPtr)WM_SYSKEYUP)
                {
                    if (vkCode == 0xA0) // Left Shift
                    {
                        _isLeftShiftPressed = false;
                        Debug.WriteLine("Left Shift released");
                    }
                    else if (vkCode == 0xA4) // Left Alt
                    {
                        _isLeftAltPressed = false;
                        Debug.WriteLine("Left Alt released");
                    }
                    else if (vkCode == 0xA2) // Left Control
                    {
                        _isLeftControlPressed = false;
                        Debug.WriteLine("Left Control released");
                    }
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private static int GetMacroKeyIndex(int vkCode)
        {
            switch (vkCode)
            {
                case 0x41: return 0; // A - Key 1
                case 0x43: return 1; // C - Key 2
                case 0x45: return 2; // E - Key 3
                case 0x47: return 3; // G - Key 4
                case 0x42: return 4; // B - Key 5
                case 0x44: return 5; // D - Key 6
                case 0x46: return 6; // F - Key 7
                case 0x48: return 7; // H - Key 8
                default: return -1;
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
    }

    public class MacroKeyPressedEventArgs : EventArgs
    {
        public int KeyIndex { get; }

        public MacroKeyPressedEventArgs(int keyIndex)
        {
            KeyIndex = keyIndex;
        }
    }
}