using System.Collections.Generic;

namespace Aurora.Devices.Durgod
{
    static class DurgodRGBMappings
    {
        public const int DurgodID = 0x2f68;
        public static int[] KeyboardIDs = new int[]
        {
            0x0082, // Durgod K310
            0x0081 // Durgod K320
        };

        public static byte[] RgbOff => new byte[] { 0x00, 0x03, 0x06, 0x86, 0x01 };
        public static byte[] RgbOn => new byte[] { 0x00, 0x03, 0x06, 0x86, 0x00 };
        public static byte[] RgbColormapStartMessage => new byte[] { 0x00, 0x03, 0x19, 0x66 };
        public static byte[] RgbColormapRowMessage => new byte[] { 0x00, 0x03, 0x18, 0x08 };
        public static byte[] RgbColormapEndMessage => new byte[] { 0x00, 0x03, 0x19, 0x88 };

        //Keyboard represents as matrix of keys
        public static int HEIGHT = 16;
        public static int WIDTH = 8;

        public static Dictionary<DeviceKeys, int> DurgodColourOffsetMap => new Dictionary<DeviceKeys, int>
        {
            {DeviceKeys.ESC, 0 * WIDTH + 0 },

            {DeviceKeys.F1, 0 * WIDTH + 2 },
            {DeviceKeys.F2, 0 * WIDTH + 3 },
            {DeviceKeys.F3, 0 * WIDTH + 4 },
            {DeviceKeys.F4, 0 * WIDTH + 5 },
            {DeviceKeys.F5, 0 * WIDTH + 6 },
            {DeviceKeys.F6, 0 * WIDTH + 7 },
            {DeviceKeys.F7, 1 * WIDTH + 0 },
            {DeviceKeys.F8, 1 * WIDTH + 1 },
            {DeviceKeys.F9, 1 * WIDTH + 2 },
            {DeviceKeys.F10, 1 * WIDTH + 3 },
            {DeviceKeys.F11, 1 * WIDTH + 4 },
            {DeviceKeys.F12, 1 * WIDTH + 5 },
            {DeviceKeys.PRINT_SCREEN, 1 * WIDTH + 6 },
            {DeviceKeys.SCROLL_LOCK, 1 * WIDTH + 7 },
            {DeviceKeys.PAUSE_BREAK, 2 * WIDTH + 0},

            {DeviceKeys.TILDE, 2 * WIDTH + 5 },
            {DeviceKeys.ONE, 2 * WIDTH + 6 },
            {DeviceKeys.TWO, 2 * WIDTH + 7 },
            {DeviceKeys.THREE, 3 * WIDTH + 0 },
            {DeviceKeys.FOUR, 3 * WIDTH + 1 },
            {DeviceKeys.FIVE, 3 * WIDTH + 2 },
            {DeviceKeys.SIX, 3 * WIDTH + 3 },
            {DeviceKeys.SEVEN, 3 * WIDTH + 4 },
            {DeviceKeys.EIGHT, 3 * WIDTH + 5 },
            {DeviceKeys.NINE, 3 * WIDTH + 6 },
            {DeviceKeys.ZERO, 3 * WIDTH + 7 },
            {DeviceKeys.MINUS,  4 * WIDTH + 0 },
            {DeviceKeys.EQUALS,  4 * WIDTH + 1 },
            {DeviceKeys.BACKSPACE,  4 * WIDTH + 2 },
            {DeviceKeys.INSERT, 4 * WIDTH + 3 },
            {DeviceKeys.HOME, 4 * WIDTH + 4 },
            {DeviceKeys.PAGE_UP, 4 * WIDTH + 5 },

            {DeviceKeys.TAB, 5 * WIDTH + 2 },
            {DeviceKeys.Q, 5 * WIDTH + 3},
            {DeviceKeys.W, 5 * WIDTH + 4},
            {DeviceKeys.E, 5 * WIDTH + 5},
            {DeviceKeys.R, 5 * WIDTH + 6},
            {DeviceKeys.T, 5 * WIDTH + 7},
            {DeviceKeys.Y, 6 * WIDTH + 0},
            {DeviceKeys.U, 6 * WIDTH + 1},
            {DeviceKeys.I, 6 * WIDTH + 2},
            {DeviceKeys.O, 6 * WIDTH + 3},
            {DeviceKeys.P, 6 * WIDTH + 4},
            {DeviceKeys.OPEN_BRACKET, 6 * WIDTH + 5 },
            {DeviceKeys.CLOSE_BRACKET, 6 * WIDTH + 6 },
            {DeviceKeys.BACKSLASH,  6 * WIDTH + 7 },
            {DeviceKeys.BACKSLASH_UK,  6 * WIDTH + 7 },


            {DeviceKeys.CAPS_LOCK, 7 * WIDTH + 7 },
            {DeviceKeys.A, 8 * WIDTH + 0},
            {DeviceKeys.S, 8 * WIDTH + 1},
            {DeviceKeys.D, 8 * WIDTH + 2},
            {DeviceKeys.F, 8 * WIDTH + 3},
            {DeviceKeys.G, 8 * WIDTH + 4},
            {DeviceKeys.H, 8 * WIDTH + 5},
            {DeviceKeys.J, 8 * WIDTH + 6},
            {DeviceKeys.K, 8 * WIDTH + 7},
            {DeviceKeys.L, 9 * WIDTH + 0},
            {DeviceKeys.SEMICOLON, 9 * WIDTH + 1},
            {DeviceKeys.APOSTROPHE, 9 * WIDTH + 2},
            {DeviceKeys.HASHTAG, 9 * WIDTH + 3},
            {DeviceKeys.ENTER, 9 * WIDTH + 4},
            {DeviceKeys.DELETE, 7 * WIDTH + 0 },
            {DeviceKeys.END, 7 * WIDTH + 1 },
            {DeviceKeys.PAGE_DOWN, 7 * WIDTH + 2 },


            {DeviceKeys.LEFT_SHIFT, 10 * WIDTH + 4 },
            {DeviceKeys.Z, 10 * WIDTH + 6},
            {DeviceKeys.X, 10 * WIDTH + 7},
            {DeviceKeys.C, 11 * WIDTH + 0},
            {DeviceKeys.V, 11 * WIDTH + 1},
            {DeviceKeys.B, 11 * WIDTH + 2},
            {DeviceKeys.N, 11 * WIDTH + 3},
            {DeviceKeys.M, 11 * WIDTH + 4},
            {DeviceKeys.COMMA, 11 * WIDTH + 5},
            {DeviceKeys.PERIOD, 11 * WIDTH + 6 },
            {DeviceKeys.FORWARD_SLASH, 11 * WIDTH + 7 },
            {DeviceKeys.RIGHT_SHIFT, 12 * WIDTH + 1 },

            {DeviceKeys.LEFT_CONTROL, 13 * WIDTH + 1 },
            {DeviceKeys.LEFT_WINDOWS, 13 * WIDTH + 2 },
            {DeviceKeys.LEFT_ALT, 13 * WIDTH + 3 },
            {DeviceKeys.SPACE, 13 * WIDTH + 7 },
            {DeviceKeys.RIGHT_ALT, 14 * WIDTH + 3 },
            {DeviceKeys.FN_Key, 14 * WIDTH + 4 },
            {DeviceKeys.RIGHT_WINDOWS, 14 * WIDTH + 5 }, //Context menu (???)
            {DeviceKeys.APPLICATION_SELECT, 14 * WIDTH + 5 }, //Context menu (???)
            {DeviceKeys.RIGHT_CONTROL, 14 * WIDTH + 6 },

            {DeviceKeys.ARROW_UP, 12 * WIDTH + 3 },
            {DeviceKeys.ARROW_DOWN, 15 * WIDTH + 0 },
            {DeviceKeys.ARROW_LEFT, 14 * WIDTH + 7 },
            {DeviceKeys.ARROW_RIGHT, 15 * WIDTH + 1 },
           
        };


    }
}