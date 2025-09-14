using System.Collections.Frozen;
using System.Collections.Generic;
using Common.Devices;
using iCUE_ReverseEngineer;

namespace AuroraRgb.Modules.Icue;

public static class IcueAuroraKeyMapping
{
    public static FrozenDictionary<IcueLedId, DeviceKeys> KeyMapping { get; } = new Dictionary<IcueLedId, DeviceKeys>
    {
        { IcueLedId.Escape, DeviceKeys.ESC },
        { IcueLedId.F1, DeviceKeys.F1 },
        { IcueLedId.F2, DeviceKeys.F2 },
        { IcueLedId.F3, DeviceKeys.F3 },
        { IcueLedId.F4, DeviceKeys.F4 },
        { IcueLedId.F5, DeviceKeys.F5 },
        { IcueLedId.F6, DeviceKeys.F6 },
        { IcueLedId.F7, DeviceKeys.F7 },
        { IcueLedId.F8, DeviceKeys.F8 },
        { IcueLedId.F9, DeviceKeys.F9 },
        { IcueLedId.F10, DeviceKeys.F10 },
        { IcueLedId.F11, DeviceKeys.F11 },
        { IcueLedId.F12, DeviceKeys.F12 },

        { IcueLedId.PrintScreen, DeviceKeys.PRINT_SCREEN },
        { IcueLedId.ScrollLock, DeviceKeys.SCROLL_LOCK },
        { IcueLedId.PauseBreak, DeviceKeys.PAUSE_BREAK },

        { IcueLedId.Tilde, DeviceKeys.HASHTAG },

        { IcueLedId.One, DeviceKeys.ONE },
        { IcueLedId.Two, DeviceKeys.TWO },
        { IcueLedId.Three, DeviceKeys.THREE },
        { IcueLedId.Four, DeviceKeys.FOUR },
        { IcueLedId.Five, DeviceKeys.FIVE },
        { IcueLedId.Six, DeviceKeys.SIX },
        { IcueLedId.Seven, DeviceKeys.SEVEN },
        { IcueLedId.Eight, DeviceKeys.EIGHT },
        { IcueLedId.Nine, DeviceKeys.NINE },
        { IcueLedId.Zero, DeviceKeys.ZERO },

        { IcueLedId.Minus, DeviceKeys.MINUS },
        { IcueLedId.Equals, DeviceKeys.EQUALS },
        { IcueLedId.Backspace, DeviceKeys.BACKSPACE },

        { IcueLedId.Insert, DeviceKeys.INSERT },
        { IcueLedId.Home, DeviceKeys.HOME },
        { IcueLedId.PageUp, DeviceKeys.PAGE_UP },

        { IcueLedId.NumLock, DeviceKeys.NUM_LOCK },
        { IcueLedId.NumpadSlash, DeviceKeys.NUM_SLASH },
        { IcueLedId.NumpadAsterisk, DeviceKeys.NUM_ASTERISK },
        { IcueLedId.NumpadMinus, DeviceKeys.NUM_MINUS },

        { IcueLedId.Tab, DeviceKeys.TAB },

        { IcueLedId.Q, DeviceKeys.Q },
        { IcueLedId.W, DeviceKeys.W },
        { IcueLedId.E, DeviceKeys.E },
        { IcueLedId.R, DeviceKeys.R },
        { IcueLedId.T, DeviceKeys.T },
        { IcueLedId.Y, DeviceKeys.Y },
        { IcueLedId.U, DeviceKeys.U },
        { IcueLedId.I, DeviceKeys.I },
        { IcueLedId.O, DeviceKeys.O },
        { IcueLedId.P, DeviceKeys.P },

        { IcueLedId.OpenBracket, DeviceKeys.OPEN_BRACKET },
        { IcueLedId.CloseBracket, DeviceKeys.CLOSE_BRACKET },
        { IcueLedId.Enter, DeviceKeys.ENTER },

        { IcueLedId.Delete, DeviceKeys.DELETE },
        { IcueLedId.End, DeviceKeys.END },
        { IcueLedId.PageDown, DeviceKeys.PAGE_DOWN },

        { IcueLedId.CapsLock, DeviceKeys.CAPS_LOCK },

        { IcueLedId.A, DeviceKeys.A },
        { IcueLedId.S, DeviceKeys.S },
        { IcueLedId.D, DeviceKeys.D },
        { IcueLedId.F, DeviceKeys.F },
        { IcueLedId.G, DeviceKeys.G },
        { IcueLedId.H, DeviceKeys.H },
        { IcueLedId.J, DeviceKeys.J },
        { IcueLedId.K, DeviceKeys.K },
        { IcueLedId.L, DeviceKeys.L },

        { IcueLedId.Semicolon, DeviceKeys.SEMICOLON },
        { IcueLedId.SingleQuote, DeviceKeys.APOSTROPHE },
        // nonustilde
        //{ IcueLedId.NonUsTilde, DeviceKeys.NON_US_TILDE },

        { IcueLedId.ShiftLeft, DeviceKeys.LEFT_SHIFT },
        { IcueLedId.Backslash, DeviceKeys.BACKSLASH_UK },
        { IcueLedId.Z, DeviceKeys.Z },
        { IcueLedId.X, DeviceKeys.X },
        { IcueLedId.C, DeviceKeys.C },
        { IcueLedId.V, DeviceKeys.V },
        { IcueLedId.B, DeviceKeys.B },
        { IcueLedId.N, DeviceKeys.N },
        { IcueLedId.M, DeviceKeys.M },

        { IcueLedId.Comma, DeviceKeys.COMMA },
        { IcueLedId.Period, DeviceKeys.PERIOD },
        { IcueLedId.ForwardSlash, DeviceKeys.FORWARD_SLASH },

        { IcueLedId.RightShift, DeviceKeys.RIGHT_SHIFT },

        { IcueLedId.UpArrow, DeviceKeys.ARROW_UP },

        { IcueLedId.LeftControl, DeviceKeys.LEFT_CONTROL },
        { IcueLedId.LeftWindows, DeviceKeys.LEFT_WINDOWS },
        { IcueLedId.LeftAlt, DeviceKeys.LEFT_ALT },

        { IcueLedId.Space, DeviceKeys.SPACE },

        { IcueLedId.RightAlt, DeviceKeys.RIGHT_ALT },
        { IcueLedId.ContextMenu, DeviceKeys.APPLICATION_SELECT },
        { IcueLedId.Fn, DeviceKeys.FN_Key },
        { IcueLedId.RightControl, DeviceKeys.RIGHT_CONTROL },

        { IcueLedId.LeftArrow, DeviceKeys.ARROW_LEFT },
        { IcueLedId.DownArrow, DeviceKeys.ARROW_DOWN },
        { IcueLedId.RightArrow, DeviceKeys.ARROW_RIGHT },

        { IcueLedId.NumpadSeven, DeviceKeys.NUM_SEVEN },
        { IcueLedId.NumpadEight, DeviceKeys.NUM_EIGHT },
        { IcueLedId.NumpadNine, DeviceKeys.NUM_NINE },
        { IcueLedId.NumpadPlus, DeviceKeys.NUM_PLUS },

        { IcueLedId.NumpadFour, DeviceKeys.NUM_FOUR },
        { IcueLedId.NumpadFive, DeviceKeys.NUM_FIVE },
        { IcueLedId.NumpadSix, DeviceKeys.NUM_SIX },

        { IcueLedId.NumpadOne, DeviceKeys.NUM_ONE },
        { IcueLedId.NumpadTwo, DeviceKeys.NUM_TWO },
        { IcueLedId.NumpadThree, DeviceKeys.NUM_THREE },
        { IcueLedId.NumpadZero, DeviceKeys.NUM_ZERO },

        { IcueLedId.NumpadPeriod, DeviceKeys.NUM_PERIOD },
        { IcueLedId.NumpadEnter, DeviceKeys.NUM_ENTER },
    }.ToFrozenDictionary();
}