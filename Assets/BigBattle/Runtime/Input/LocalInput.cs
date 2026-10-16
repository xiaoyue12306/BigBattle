using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace BigBattle
{
    public enum InputKind { Keyboard, Xbox }

    [Serializable]
    public struct InputBinding
    {
        public InputKind Kind;
        public int DeviceIndex;
        public int KeyboardLayout;
    }

    public struct CombatInput
    {
        public Vector2 Move;
        public bool Attack, Thrust, Peel, Dodge, Ultimate, Pause, Session, Confirm;
        public int MenuStep;
    }

    // Controllers are addressed by XInput slot, never by one shared global axis.
    public sealed class LocalInput
    {
        public InputBinding Binding { get; private set; }
        public string DeviceLabel { get; private set; } = "键盘";
        public bool ControllerConnected { get; private set; }
        public bool Injected;
        public CombatInput InjectedFrame;
        ushort previousButtons;
        float previousMenuAxis;
        float rumbleEnd;
        bool vibrationActive;
        readonly bool autoSwitch;

        public LocalInput(InputBinding binding, bool allowSinglePlayerDeviceSwitch = false)
        {
            Binding = binding; autoSwitch = allowSinglePlayerDeviceSwitch;
            DeviceLabel = binding.Kind == InputKind.Xbox ? "Xbox" : "键盘";
        }

        public CombatInput Read()
        {
            if (Injected) return InjectedFrame;
            var frame = Binding.Kind == InputKind.Keyboard || autoSwitch ? ReadKeyboard(Binding.KeyboardLayout) : default(CombatInput);
            bool keyboardActive = frame.Move.sqrMagnitude > .01f || frame.Attack || frame.Thrust || frame.Peel || frame.Dodge || frame.Ultimate || frame.Pause || frame.Session || frame.Confirm;
            if (keyboardActive) DeviceLabel = "键盘";
            XInput.State state;
            ControllerConnected = false;state = default(XInput.State);
            if (Binding.Kind == InputKind.Xbox || autoSwitch) ControllerConnected = XInput.TryRead(Binding.DeviceIndex, out state);
            if (!ControllerConnected && autoSwitch)
            {
                for(int index=0;index<4;index++)
                {
                    if(!XInput.TryRead(index,out state))continue;
                    var binding=Binding;binding.DeviceIndex=index;Binding=binding;
                    ControllerConnected=true;break;
                }
            }
            if (ControllerConnected)
            {
                ushort buttons = state.Gamepad.Buttons;
                Func<ushort, bool> pressed = mask => (buttons & mask) != 0 && (previousButtons & mask) == 0;
                Vector2 move = Deadzone(new Vector2(state.Gamepad.ThumbLX / 32767f, state.Gamepad.ThumbLY / 32767f));
                int menuStep = pressed(XInput.Up) ? -1 : pressed(XInput.Down) ? 1 : 0;
                if (Mathf.Abs(previousMenuAxis) < .5f && Mathf.Abs(move.y) > .5f) menuStep = move.y > 0 ? -1 : 1;
                previousMenuAxis = move.y;
                if (move.sqrMagnitude > .01f || buttons != 0)
                {
                    DeviceLabel = "Xbox";
                    if (!keyboardActive) frame.Move = move;
                }
                frame.Attack |= (buttons & XInput.X) != 0;
                frame.Thrust |= pressed(XInput.A);
                frame.Peel |= pressed(XInput.B);
                frame.Dodge |= pressed(XInput.LB);
                frame.Ultimate |= pressed(XInput.Y);
                frame.Pause |= pressed(XInput.Start);
                frame.Session |= pressed(XInput.Back);
                frame.Confirm |= pressed(XInput.A);
                if (menuStep != 0) frame.MenuStep = menuStep;
                previousButtons = buttons;
            }
            else { previousButtons = 0; previousMenuAxis = 0; }
            if (vibrationActive && Time.unscaledTime >= rumbleEnd) StopRumble();
            return frame;
        }

        public static Vector2 Deadzone(Vector2 value)
        {
            float length = value.magnitude;
            if (length < .2f) return Vector2.zero;
            return value.normalized * Mathf.Clamp01((length - .2f) / .8f);
        }

        static CombatInput ReadKeyboard(int layout)
        {
            // Layout 1 is reserved for the future second keyboard slot. These are
            // key ranges, not physical keyboards; Windows merges physical devices.
            bool second = layout == 1;
            KeyCode up = second ? KeyCode.UpArrow : KeyCode.W;
            KeyCode down = second ? KeyCode.DownArrow : KeyCode.S;
            KeyCode left = second ? KeyCode.LeftArrow : KeyCode.A;
            KeyCode right = second ? KeyCode.RightArrow : KeyCode.D;
            Vector2 move = new Vector2((Input.GetKey(right) ? 1 : 0) - (Input.GetKey(left) ? 1 : 0), (Input.GetKey(up) ? 1 : 0) - (Input.GetKey(down) ? 1 : 0));
            return new CombatInput
            {
                Move = Vector2.ClampMagnitude(move, 1),
                Attack = Input.GetKey(second ? KeyCode.Keypad1 : KeyCode.J),
                Thrust = Input.GetKeyDown(second ? KeyCode.Keypad2 : KeyCode.K),
                Peel = Input.GetKeyDown(second ? KeyCode.Keypad3 : KeyCode.L),
                Dodge = Input.GetKeyDown(second ? KeyCode.Keypad0 : KeyCode.Space),
                Ultimate = Input.GetKeyDown(second ? KeyCode.Keypad5 : KeyCode.I),
                Pause = Input.GetKeyDown(KeyCode.Escape),
                Session = Input.GetKeyDown(KeyCode.F),
                Confirm = Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.J),
                MenuStep = Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow) ? -1 : Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow) ? 1 : 0
            };
        }

        public void Rumble(float amount, float duration)
        {
            if (DeviceLabel != "Xbox") return;
            XInput.Vibrate(Binding.DeviceIndex, amount);
            rumbleEnd = Time.unscaledTime + duration;
            vibrationActive = true;
        }

        public void StopRumble() { XInput.Vibrate(Binding.DeviceIndex, 0); vibrationActive = false; }
    }

    public static class XInput
    {
        public const ushort Up = 0x0001, Down = 0x0002, Start = 0x0010, Back = 0x0020, LB = 0x0100, A = 0x1000, B = 0x2000, X = 0x4000, Y = 0x8000;
        [StructLayout(LayoutKind.Sequential)] public struct Pad { public ushort Buttons; public byte LeftTrigger, RightTrigger; public short ThumbLX, ThumbLY, ThumbRX, ThumbRY; }
        [StructLayout(LayoutKind.Sequential)] public struct State { public uint Packet; public Pad Gamepad; }
        [StructLayout(LayoutKind.Sequential)] struct Vibration { public ushort Left, Right; }
        static bool available = true;
        [DllImport("xinput1_4.dll", EntryPoint = "XInputGetState")] static extern uint GetState(uint index, out State state);
        [DllImport("xinput1_4.dll", EntryPoint = "XInputSetState")] static extern uint SetState(uint index, ref Vibration vibration);
        public static bool TryRead(int index, out State state)
        {
            state = default(State);
            if (!available || index < 0 || index > 3) return false;
            try { return GetState((uint)index, out state) == 0; }
            catch (DllNotFoundException) { available = false; return false; }
            catch (EntryPointNotFoundException) { available = false; return false; }
        }
        public static void Vibrate(int index, float amount)
        {
            if (!available || index < 0 || index > 3) return;
            var v = new Vibration { Left = (ushort)(Mathf.Clamp01(amount) * 30000), Right = (ushort)(Mathf.Clamp01(amount) * 18000) };
            try { SetState((uint)index, ref v); }
            catch (DllNotFoundException) { available = false; }
            catch (EntryPointNotFoundException) { available = false; }
        }
    }

    public static class SplitScreenLayout
    {
        public const int MaxPlayers = 5;
        public static Rect[] ForPlayers(int count)
        {
            if (count < 1 || count > MaxPlayers) throw new ArgumentOutOfRangeException(nameof(count));
            if (count == 1) return new[] { new Rect(0, 0, 1, 1) };
            if (count == 2) return new[] { new Rect(0, 0, .5f, 1), new Rect(.5f, 0, .5f, 1) };
            var result = new Rect[count];
            int bottom = count == 5 ? 3 : 2;
            for (int i = 0; i < count; i++)
                result[i] = i < 2 ? new Rect(i * .5f, .5f, .5f, .5f) : new Rect((i - 2) / (float)bottom, 0, 1f / bottom, .5f);
            // Three players use the full lower half for the third camera.
            if (count == 3) result[2] = new Rect(0, 0, 1, .5f);
            return result;
        }
    }
}
