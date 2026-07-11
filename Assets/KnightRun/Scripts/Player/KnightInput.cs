using UnityEngine;
using UnityEngine.InputSystem;

namespace KnightRun.Player
{
    public static class KnightInput
    {
        public static bool GetKey(KeyCode keyCode)
        {
            if (Keyboard.current != null && TryMap(keyCode, out Key key))
                return Keyboard.current[key].isPressed;

            return Input.GetKey(keyCode);
        }

        public static bool GetKeyDown(KeyCode keyCode)
        {
            if (Keyboard.current != null && TryMap(keyCode, out Key key))
                return Keyboard.current[key].wasPressedThisFrame;

            return Input.GetKeyDown(keyCode);
        }

        static bool TryMap(KeyCode keyCode, out Key key)
        {
            switch (keyCode)
            {
                case KeyCode.A: key = Key.A; return true;
                case KeyCode.D: key = Key.D; return true;
                case KeyCode.W: key = Key.W; return true;
                case KeyCode.S: key = Key.S; return true;
                case KeyCode.Space: key = Key.Space; return true;
                case KeyCode.LeftArrow: key = Key.LeftArrow; return true;
                case KeyCode.RightArrow: key = Key.RightArrow; return true;
                case KeyCode.UpArrow: key = Key.UpArrow; return true;
                case KeyCode.DownArrow: key = Key.DownArrow; return true;
                case KeyCode.Return: key = Key.Enter; return true;
                case KeyCode.P: key = Key.P; return true;
                case KeyCode.Escape: key = Key.Escape; return true;
                case KeyCode.M: key = Key.M; return true;
                case KeyCode.R: key = Key.R; return true;
                case KeyCode.Alpha1: key = Key.Digit1; return true;
                case KeyCode.Alpha2: key = Key.Digit2; return true;
                case KeyCode.Alpha3: key = Key.Digit3; return true;
                case KeyCode.Keypad1: key = Key.Numpad1; return true;
                case KeyCode.Keypad2: key = Key.Numpad2; return true;
                case KeyCode.Keypad3: key = Key.Numpad3; return true;
                default:
                    key = default;
                    return false;
            }
        }
    }
}
