﻿using System.ComponentModel;

namespace Common.Devices;

/// <summary>
/// Enum definition, representing everysingle supported device key
/// </summary>
public enum DeviceKeys
{
    /// <summary>
    /// Peripheral Device
    /// <note type="note">Setting this key will make entire peripheral device one color</note>
    /// </summary>
    [Description("All Peripheral Devices")]
    Peripheral = 0,

    /// <summary>
    /// Escape key
    /// </summary>
    [Description("Escape")]
    ESC = 1,

    /// <summary>
    /// F1 key
    /// </summary>
    [Description("F1")]
    F1 = 2,

    /// <summary>
    /// F2 key
    /// </summary>
    [Description("F2")]
    F2 = 3,

    /// <summary>
    /// F3 key
    /// </summary>
    [Description("F3")]
    F3 = 4,

    /// <summary>
    /// F4 key
    /// </summary>
    [Description("F4")]
    F4 = 5,

    /// <summary>
    /// F5 key
    /// </summary>
    [Description("F5")]
    F5 = 6,

    /// <summary>
    /// F6 key
    /// </summary>
    [Description("F6")]
    F6 = 7,

    /// <summary>
    /// F7 key
    /// </summary>
    [Description("F7")]
    F7 = 8,

    /// <summary>
    /// F8 key
    /// </summary>
    [Description("F8")]
    F8 = 9,

    /// <summary>
    /// F9 key
    /// </summary>
    [Description("F9")]
    F9 = 10,

    /// <summary>
    /// F10 key
    /// </summary>
    [Description("F10")]
    F10 = 11,

    /// <summary>
    /// F11 key
    /// </summary>
    [Description("F11")]
    F11 = 12,

    /// <summary>
    /// F12 key
    /// </summary>
    [Description("F12")]
    F12 = 13,

    /// <summary>
    /// Print Screen key
    /// </summary>
    [Description("Print Screen")]
    PRINT_SCREEN = 14,

    /// <summary>
    /// Scroll Lock key
    /// </summary>
    [Description("Scroll Lock")]
    SCROLL_LOCK = 15,

    /// <summary>
    /// Pause/Break key
    /// </summary>
    [Description("Pause")]
    PAUSE_BREAK = 16,

    /// <summary>
    /// Half/Full width (Japanese layout) key
    /// </summary>
    [Description("Half/Full width")]
    JPN_HALFFULLWIDTH = 152,

    /// <summary>
    /// OEM 5 key
    /// </summary>
    [Description("OEM 5")]
    OEM5 = 156,

    /// <summary>
    /// Tilde key
    /// </summary>
    [Description("Tilde")]
    TILDE = 17,

    /// <summary>
    /// One key
    /// </summary>
    [Description("1")]
    ONE = 18,

    /// <summary>
    /// Two key
    /// </summary>
    [Description("2")]
    TWO = 19,

    /// <summary>
    /// Three key
    /// </summary>
    [Description("3")]
    THREE = 20,

    /// <summary>
    /// Four key
    /// </summary>
    [Description("4")]
    FOUR = 21,

    /// <summary>
    /// Five key
    /// </summary>
    [Description("5")]
    FIVE = 22,

    /// <summary>
    /// Six key
    /// </summary>
    [Description("6")]
    SIX = 23,

    /// <summary>
    /// Seven key
    /// </summary>
    [Description("7")]
    SEVEN = 24,

    /// <summary>
    /// Eight key
    /// </summary>
    [Description("8")]
    EIGHT = 25,

    /// <summary>
    /// Nine key
    /// </summary>
    [Description("9")]
    NINE = 26,

    /// <summary>
    /// Zero key
    /// </summary>
    [Description("0")]
    ZERO = 27,

    /// <summary>
    /// Minus key
    /// </summary>
    [Description("-")]
    MINUS = 28,

    /// <summary>
    /// Equals key
    /// </summary>
    [Description("=")]
    EQUALS = 29,

    /// <summary>
    /// Backspace key
    /// </summary>
    [Description("Backspace")]
    BACKSPACE = 30,

    /// <summary>
    /// Insert key
    /// </summary>
    [Description("Insert")]
    INSERT = 31,

    /// <summary>
    /// Home key
    /// </summary>
    [Description("Home")]
    HOME = 32,

    /// <summary>
    /// Page up key
    /// </summary>
    [Description("Page Up")]
    PAGE_UP = 33,

    /// <summary>
    /// Numpad Lock key
    /// </summary>
    [Description("Numpad Lock")]
    NUM_LOCK = 34,

    /// <summary>
    /// Numpad divide key
    /// </summary>
    [Description("Numpad /")]
    NUM_SLASH = 35,

    /// <summary>
    /// Numpad multiply key
    /// </summary>
    [Description("Numpad *")]
    NUM_ASTERISK = 36,

    /// <summary>
    /// Numpad minus key
    /// </summary>
    [Description("Numpad -")]
    NUM_MINUS = 37,


    /// <summary>
    /// Tab key
    /// </summary>
    [Description("Tab")]
    TAB = 38,

    /// <summary>
    /// Q key
    /// </summary>
    [Description("Q")]
    Q = 39,

    /// <summary>
    /// W key
    /// </summary>
    [Description("W")]
    W = 40,

    /// <summary>
    /// E key
    /// </summary>
    [Description("E")]
    E = 41,

    /// <summary>
    /// R key
    /// </summary>
    [Description("R")]
    R = 42,

    /// <summary>
    /// T key
    /// </summary>
    [Description("T")]
    T = 43,

    /// <summary>
    /// Y key
    /// </summary>
    [Description("Y")]
    Y = 44,

    /// <summary>
    /// U key
    /// </summary>
    [Description("U")]
    U = 45,

    /// <summary>
    /// I key
    /// </summary>
    [Description("I")]
    I = 46,

    /// <summary>
    /// O key
    /// </summary>
    [Description("O")]
    O = 47,

    /// <summary>
    /// P key
    /// </summary>
    [Description("P")]
    P = 48,

    /// <summary>
    /// Open Bracket key
    /// </summary>
    [Description("{")]
    OPEN_BRACKET = 49,

    /// <summary>
    /// Close Bracket key
    /// </summary>
    [Description("}")]
    CLOSE_BRACKET = 50,

    /// <summary>
    /// Backslash key
    /// </summary>
    [Description("\\")]
    BACKSLASH = 51,

    /// <summary>
    /// Delete key
    /// </summary>
    [Description("Delete")]
    DELETE = 52,

    /// <summary>
    /// End key
    /// </summary>
    [Description("End")]
    END = 53,

    /// <summary>
    /// Page down key
    /// </summary>
    [Description("Page Down")]
    PAGE_DOWN = 54,

    /// <summary>
    /// Numpad seven key
    /// </summary>
    [Description("Numpad 7")]
    NUM_SEVEN = 55,

    /// <summary>
    /// Numpad eight key
    /// </summary>
    [Description("Numpad 8")]
    NUM_EIGHT = 56,

    /// <summary>
    /// Numpad nine key
    /// </summary>
    [Description("Numpad 9")]
    NUM_NINE = 57,

    /// <summary>
    /// Numpad add key
    /// </summary>
    [Description("Numpad +")]
    NUM_PLUS = 58,

    /// <summary>
    /// Caps Lock key
    /// </summary>
    [Description("Caps Lock")]
    CAPS_LOCK = 59,

    /// <summary>
    /// A key
    /// </summary>
    [Description("A")]
    A = 60,

    /// <summary>
    /// S key
    /// </summary>
    [Description("S")]
    S = 61,

    /// <summary>
    /// D key
    /// </summary>
    [Description("D")]
    D = 62,

    /// <summary>
    /// F key
    /// </summary>
    [Description("F")]
    F = 63,

    /// <summary>
    /// G key
    /// </summary>
    [Description("G")]
    G = 64,

    /// <summary>
    /// H key
    /// </summary>
    [Description("H")]
    H = 65,

    /// <summary>
    /// J key
    /// </summary>
    [Description("J")]
    J = 66,

    /// <summary>
    /// K key
    /// </summary>
    [Description("K")]
    K = 67,

    /// <summary>
    /// L key
    /// </summary>
    [Description("L")]
    L = 68,

    /// <summary>
    /// OEM Tilde key
    /// </summary>
    [Description("OEM Tilde")]
    OEMTilde = 157,

    /// <summary>
    /// Semicolon key
    /// </summary>
    [Description("Semicolon")]
    SEMICOLON = 69,

    /// <summary>
    /// Apostrophe key
    /// </summary>
    [Description("Apostrophe")]
    APOSTROPHE = 70,

    /// <summary>
    /// Hashtag key
    /// </summary>
    [Description("#")]
    HASHTAG = 71,

    /// <summary>
    /// Enter key
    /// </summary>
    [Description("Enter")]
    ENTER = 72,

    /// <summary>
    /// Numpad four key
    /// </summary>
    [Description("Numpad 4")]
    NUM_FOUR = 73,

    /// <summary>
    /// Numpad five key
    /// </summary>
    [Description("Numpad 5")]
    NUM_FIVE = 74,

    /// <summary>
    /// Numpad six key
    /// </summary>
    [Description("Numpad 6")]
    NUM_SIX = 75,


    /// <summary>
    /// Left Shift key
    /// </summary>
    [Description("Left Shift")]
    LEFT_SHIFT = 76,

    /// <summary>
    /// Non-US Backslash key
    /// </summary>
    [Description("Non-US Backslash")]
    BACKSLASH_UK = 77,

    /// <summary>
    /// Z key
    /// </summary>
    [Description("Z")]
    Z = 78,

    /// <summary>
    /// X key
    /// </summary>
    [Description("X")]
    X = 79,

    /// <summary>
    /// C key
    /// </summary>
    [Description("C")]
    C = 80,

    /// <summary>
    /// V key
    /// </summary>
    [Description("V")]
    V = 81,

    /// <summary>
    /// B key
    /// </summary>
    [Description("B")]
    B = 82,

    /// <summary>
    /// N key
    /// </summary>
    [Description("N")]
    N = 83,

    /// <summary>
    /// M key
    /// </summary>
    [Description("M")]
    M = 84,

    /// <summary>
    /// Comma key
    /// </summary>
    [Description("Comma")]
    COMMA = 85,

    /// <summary>
    /// Period key
    /// </summary>
    [Description("Period")]
    PERIOD = 86,

    /// <summary>
    /// Forward Slash key
    /// </summary>
    [Description("Forward Slash")]
    FORWARD_SLASH = 87,

    /// <summary>
    /// OEM 8 key
    /// </summary>
    [Description("OEM 8")]
    OEM8 = 158,

    /// <summary>
    /// OEM 102 key
    /// </summary>
    [Description("OEM 102")]
    OEM102 = 159,

    /// <summary>
    /// Right Shift key
    /// </summary>
    [Description("Right Shift")]
    RIGHT_SHIFT = 88,

    /// <summary>
    /// Arrow Up key
    /// </summary>
    [Description("Arrow Up")]
    ARROW_UP = 89,

    /// <summary>
    /// Numpad one key
    /// </summary>
    [Description("Numpad 1")]
    NUM_ONE = 90,

    /// <summary>
    /// Numpad two key
    /// </summary>
    [Description("Numpad 2")]
    NUM_TWO = 91,

    /// <summary>
    /// Numpad three key
    /// </summary>
    [Description("Numpad 3")]
    NUM_THREE = 92,

    /// <summary>
    /// Numpad enter key
    /// </summary>
    [Description("Numpad Enter")]
    NUM_ENTER = 93,


    /// <summary>
    /// Left Control key
    /// </summary>
    [Description("Left Control")]
    LEFT_CONTROL = 94,

    /// <summary>
    /// Left Windows key
    /// </summary>
    [Description("Left Windows Key")]
    LEFT_WINDOWS = 95,

    /// <summary>
    /// Left Alt key
    /// </summary>
    [Description("Left Alt")]
    LEFT_ALT = 96,

    /// <summary>
    /// Non-conversion (Japanese layout) key
    /// </summary>
    [Description("Non-conversion")]
    JPN_MUHENKAN = 153,

    /// <summary>
    /// Spacebar key
    /// </summary>
    [Description("Spacebar")]
    SPACE = 97,

    /// <summary>
    /// Conversion (Japanese layout) key
    /// </summary>
    [Description("Conversion")]
    JPN_HENKAN = 154,

    /// <summary>
    /// Hiragana/Katakana (Japanese layout) key
    /// </summary>
    [Description("Hiragana/Katakana")]
    JPN_HIRAGANA_KATAKANA = 155,

    /// <summary>
    /// Right Alt key
    /// </summary>
    [Description("Right Alt")]
    RIGHT_ALT = 98,

    /// <summary>
    /// Right Windows key
    /// </summary>
    [Description("Right Windows Key")]
    RIGHT_WINDOWS = 99,

    /// <summary>
    /// Application Select key
    /// </summary>
    [Description("Application Select Key")]
    APPLICATION_SELECT = 100,

    /// <summary>
    /// Right Control key
    /// </summary>
    [Description("Right Control")]
    RIGHT_CONTROL = 101,

    /// <summary>
    /// Arrow Left key
    /// </summary>
    [Description("Arrow Left")]
    ARROW_LEFT = 102,

    /// <summary>
    /// Arrow Down key
    /// </summary>
    [Description("Arrow Down")]
    ARROW_DOWN = 103,

    /// <summary>
    /// Arrow Right key
    /// </summary>
    [Description("Arrow Right")]
    ARROW_RIGHT = 104,

    /// <summary>
    /// Numpad zero key
    /// </summary>
    [Description("Numpad 0")]
    NUM_ZERO = 105,

    /// <summary>
    /// Numpad period key
    /// </summary>
    [Description("Numpad Period")]
    NUM_PERIOD = 106,


    /// <summary>
    /// Function key
    /// </summary>
    [Description("FN Key")]
    FN_Key = 107,


    /// <summary>
    /// Macrokey 1 key
    /// </summary>
    [Description("G1")]
    G1 = 108,

    /// <summary>
    /// Macrokey 2 key
    /// </summary>
    [Description("G2")]
    G2 = 109,

    /// <summary>
    /// Macrokey 3 key
    /// </summary>
    [Description("G3")]
    G3 = 110,

    /// <summary>
    /// Macrokey 4 key
    /// </summary>
    [Description("G4")]
    G4 = 111,

    /// <summary>
    /// Macrokey 5 key
    /// </summary>
    [Description("G5")]
    G5 = 112,

    /// <summary>
    /// Macrokey 6 key
    /// </summary>
    [Description("G6")]
    G6 = 113,

    /// <summary>
    /// Macrokey 7 key
    /// </summary>
    [Description("G7")]
    G7 = 114,

    /// <summary>
    /// Macrokey 8 key
    /// </summary>
    [Description("G8")]
    G8 = 115,

    /// <summary>
    /// Macrokey 9 key
    /// </summary>
    [Description("G9")]
    G9 = 116,

    /// <summary>
    /// Macrokey 10 key
    /// </summary>
    [Description("G10")]
    G10 = 117,

    /// <summary>
    /// Macrokey 11 key
    /// </summary>
    [Description("G11")]
    G11 = 118,

    /// <summary>
    /// Macrokey 12 key
    /// </summary>
    [Description("G12")]
    G12 = 119,

    /// <summary>
    /// Macrokey 13 key
    /// </summary>
    [Description("G13")]
    G13 = 120,

    /// <summary>
    /// Macrokey 14 key
    /// </summary>
    [Description("G14")]
    G14 = 121,

    /// <summary>
    /// Macrokey 15 key
    /// </summary>
    [Description("G15")]
    G15 = 122,

    /// <summary>
    /// Macrokey 16 key
    /// </summary>
    [Description("G16")]
    G16 = 123,

    /// <summary>
    /// Macrokey 17 key
    /// </summary>
    [Description("G17")]
    G17 = 124,

    /// <summary>
    /// Macrokey 18 key
    /// </summary>
    [Description("G18")]
    G18 = 125,

    /// <summary>
    /// Macrokey 19 key
    /// </summary>
    [Description("G19")]
    G19 = 126,

    /// <summary>
    /// Macrokey 20 key
    /// </summary>
    [Description("G20")]
    G20 = 127,

    /// <summary>
    /// Brand Logo
    /// </summary>
    [Description("Brand Logo")]
    LOGO = 128,

    /// <summary>
    /// Brand Logo #2
    /// </summary>
    [Description("Brand Logo #2")]
    LOGO2 = 129,

    /// <summary>
    /// Brand Logo #3
    /// </summary>
    [Description("Brand Logo #3")]
    LOGO3 = 130,

    /// <summary>
    /// Brightness Switch
    /// </summary>
    [Description("Brightness Switch")]
    BRIGHTNESS_SWITCH = 131,

    /// <summary>
    /// Lock Switch
    /// </summary>
    [Description("Lock Switch")]
    LOCK_SWITCH = 132,


    /// <summary>
    /// Multimedia Play/Pause
    /// </summary>
    [Description("Media Play/Pause")]
    MEDIA_PLAY_PAUSE = 133,

    /// <summary>
    /// Multimedia Play
    /// </summary>
    [Description("Media Play")]
    MEDIA_PLAY = 134,

    /// <summary>
    /// Multimedia Pause
    /// </summary>
    [Description("Media Pause")]
    MEDIA_PAUSE = 135,

    /// <summary>
    /// Multimedia Stop
    /// </summary>
    [Description("Media Stop")]
    MEDIA_STOP = 136,

    /// <summary>
    /// Multimedia Previous
    /// </summary>
    [Description("Media Previous")]
    MEDIA_PREVIOUS = 137,

    /// <summary>
    /// Multimedia Next
    /// </summary>
    [Description("Media Next")]
    MEDIA_NEXT = 138,


    /// <summary>
    /// Volume Mute
    /// </summary>
    [Description("Volume Mute")]
    VOLUME_MUTE = 139,

    /// <summary>
    /// Volume Down
    /// </summary>
    [Description("Volume Down")]
    VOLUME_DOWN = 140,

    /// <summary>
    /// Volume Up
    /// </summary>
    [Description("Volume Up")]
    VOLUME_UP = 141,


    /// <summary>
    /// Additional Light 1
    /// </summary>
    [Description("Additional Light 1")]
    ADDITIONALLIGHT1 = 142,

    /// <summary>
    /// Additional Light 2
    /// </summary>
    [Description("Additional Light 2")]
    ADDITIONALLIGHT2 = 143,

    /// <summary>
    /// Additional Light 3
    /// </summary>
    [Description("Additional Light 3")]
    ADDITIONALLIGHT3 = 144,

    /// <summary>
    /// Additional Light 4
    /// </summary>
    [Description("Additional Light 4")]
    ADDITIONALLIGHT4 = 145,

    /// <summary>
    /// Additional Light 5
    /// </summary>
    [Description("Additional Light 5")]
    ADDITIONALLIGHT5 = 146,

    /// <summary>
    /// Additional Light 6
    /// </summary>
    [Description("Additional Light 6")]
    ADDITIONALLIGHT6 = 147,

    /// <summary>
    /// Additional Light 7
    /// </summary>
    [Description("Additional Light 7")]
    ADDITIONALLIGHT7 = 148,

    /// <summary>
    /// Additional Light 8
    /// </summary>
    [Description("Additional Light 8")]
    ADDITIONALLIGHT8 = 149,

    /// <summary>
    /// Additional Light 9
    /// </summary>
    [Description("Additional Light 9")]
    ADDITIONALLIGHT9 = 150,

    /// <summary>
    /// Additional Light 10
    /// </summary>
    [Description("Additional Light 10")]
    ADDITIONALLIGHT10 = 151,

    /// <summary>
    /// Peripheral Logo
    /// </summary>
    [Description("Peripheral Logo")]
    Peripheral_Logo = 160,

    /// <summary>
    /// Peripheral Scroll Wheel
    /// </summary>
    [Description("Peripheral Scroll Wheel")]
    Peripheral_ScrollWheel = 161,

    /// <summary>
    /// Peripheral Front-facing lights
    /// </summary>
    [Description("Peripheral Front Lights")]
    Peripheral_FrontLight = 162,

    /// <summary>
    /// Profile key 1
    /// </summary>
    [Description("Profile Key 1")]
    Profile_Key1 = 163,

    /// <summary>
    /// Profile key 2
    /// </summary>
    [Description("Profile Key 2")]
    Profile_Key2 = 164,

    /// <summary>
    /// Profile key 3
    /// </summary>
    [Description("Profile Key 3")]
    Profile_Key3 = 165,

    /// <summary>
    /// Profile key 4
    /// </summary>
    [Description("Profile Key 4")]
    Profile_Key4 = 166,

    /// <summary>
    /// Profile key 5
    /// </summary>
    [Description("Profile Key 5")]
    Profile_Key5 = 167,

    /// <summary>
    /// Profile key 6
    /// </summary>
    [Description("Profile Key 6")]
    Profile_Key6 = 168,

    /// <summary>
    /// Numpad 00
    /// </summary>
    [Description("Numpad 00")]
    NUM_ZEROZERO = 169,

    /// <summary>
    /// Macrokey 0 key
    /// </summary>
    [Description("G0")]
    G0 = 170,

    /// <summary>
    /// Macrokey 0 key
    /// </summary>
    [Description("Left FN")]
    LEFT_FN = 171,

    /// <summary>
    /// Additional Light 11
    /// </summary>
    [Description("Additional Light 11")]
    ADDITIONALLIGHT11 = 172,

    /// <summary>
    /// Additional Light 12
    /// </summary>
    [Description("Additional Light 12")]
    ADDITIONALLIGHT12 = 173,

    /// <summary>
    /// Additional Light 13
    /// </summary>
    [Description("Additional Light 13")]
    ADDITIONALLIGHT13 = 174,

    /// <summary>
    /// Additional Light 14
    /// </summary>
    [Description("Additional Light 14")]
    ADDITIONALLIGHT14 = 175,

    /// <summary>
    /// Additional Light 15
    /// </summary>
    [Description("Additional Light 15")]
    ADDITIONALLIGHT15 = 176,

    /// <summary>
    /// Additional Light 16
    /// </summary>
    [Description("Additional Light 16")]
    ADDITIONALLIGHT16 = 177,

    /// <summary>
    /// Additional Light 17
    /// </summary>
    [Description("Additional Light 17")]
    ADDITIONALLIGHT17 = 178,

    /// <summary>
    /// Additional Light 18
    /// </summary>
    [Description("Additional Light 18")]
    ADDITIONALLIGHT18 = 179,

    /// <summary>
    /// Additional Light 19
    /// </summary>
    [Description("Additional Light 19")]
    ADDITIONALLIGHT19 = 180,

    /// <summary>
    /// Additional Light 20
    /// </summary>
    [Description("Additional Light 20")]
    ADDITIONALLIGHT20 = 181,

    /// <summary>
    /// Additional Light 21
    /// </summary>
    [Description("Additional Light 21")]
    ADDITIONALLIGHT21 = 182,

    /// <summary>
    /// Additional Light 22
    /// </summary>
    [Description("Additional Light 22")]
    ADDITIONALLIGHT22 = 183,

    /// <summary>
    /// Additional Light 23
    /// </summary>
    [Description("Additional Light 23")]
    ADDITIONALLIGHT23 = 184,

    /// <summary>
    /// Additional Light 24
    /// </summary>
    [Description("Additional Light 24")]
    ADDITIONALLIGHT24 = 185,

    /// <summary>
    /// Additional Light 25
    /// </summary>
    [Description("Additional Light 25")]
    ADDITIONALLIGHT25 = 186,

    /// <summary>
    /// Additional Light 26
    /// </summary>
    [Description("Additional Light 26")]
    ADDITIONALLIGHT26 = 187,

    /// <summary>
    /// Additional Light 27
    /// </summary>
    [Description("Additional Light 27")]
    ADDITIONALLIGHT27 = 188,

    /// <summary>
    /// Additional Light 28
    /// </summary>
    [Description("Additional Light 28")]
    ADDITIONALLIGHT28 = 189,

    /// <summary>
    /// Additional Light 29
    /// </summary>
    [Description("Additional Light 29")]
    ADDITIONALLIGHT29 = 190,

    /// <summary>
    /// Additional Light 30
    /// </summary>
    [Description("Additional Light 30")]
    ADDITIONALLIGHT30 = 191,

    /// <summary>
    /// Additional Light 31
    /// </summary>
    [Description("Additional Light 31")]
    ADDITIONALLIGHT31 = 192,

    /// <summary>
    /// Additional Light 32
    /// </summary>
    [Description("Additional Light 32")]
    ADDITIONALLIGHT32 = 193,

    /// <summary>
    /// Mousepad Light 1
    /// </summary>
    [Description("Mousepad Light 1")]
    MOUSEPADLIGHT1 = 201,

    /// <summary>
    /// Mousepad Light 2
    /// </summary>
    [Description("Mousepad Light 2")]
    MOUSEPADLIGHT2 = 202,

    /// <summary>
    /// Mousepad Light 1
    /// </summary>
    [Description("Mousepad Light 3")]
    MOUSEPADLIGHT3 = 203,

    /// <summary>
    /// Mousepad Light 2
    /// </summary>
    [Description("Mousepad Light 4")]
    MOUSEPADLIGHT4 = 204,

    /// <summary>
    /// Mousepad Light 1
    /// </summary>
    [Description("Mousepad Light 5")]
    MOUSEPADLIGHT5 = 205,

    /// <summary>
    /// Mousepad Light 2
    /// </summary>
    [Description("Mousepad Light 6")]
    MOUSEPADLIGHT6 = 206,

    /// <summary>
    /// Mousepad Light 1
    /// </summary>
    [Description("Mousepad Light 7")]
    MOUSEPADLIGHT7 = 207,

    /// <summary>
    /// Mousepad Light 2
    /// </summary>
    [Description("Mousepad Light 8")]
    MOUSEPADLIGHT8 = 208,

    /// <summary>
    /// Mousepad Light 1
    /// </summary>
    [Description("Mousepad Light 9")]
    MOUSEPADLIGHT9 = 209,

    /// <summary>
    /// Mousepad Light 2
    /// </summary>
    [Description("Mousepad Light 10")]
    MOUSEPADLIGHT10 = 210,

    /// <summary>
    /// Mousepad Light 2
    /// </summary>
    [Description("Mousepad Light 11")]
    MOUSEPADLIGHT11 = 211,

    /// <summary>
    /// Mousepad Light 1
    /// </summary>
    [Description("Mousepad Light 12")]
    MOUSEPADLIGHT12 = 212,

    /// <summary>
    /// Mousepad Light 2
    /// </summary>
    [Description("Mousepad Light 13")]
    MOUSEPADLIGHT13 = 213,

    /// <summary>
    /// Mousepad Light 1
    /// </summary>
    [Description("Mousepad Light 14")]
    MOUSEPADLIGHT14 = 214,

    /// <summary>
    /// Mousepad Light 2
    /// </summary>
    [Description("Mousepad Light 15")]
    MOUSEPADLIGHT15 = 215,

    ///<summary>
    /// Calculator Key
    /// </summary>
    [Description("Calculator")]
    CALC = 216,

    /// <summary>
    /// Peripheral Light 1
    /// </summary>
    [Description("Peripheral Light 1")]
    PERIPHERAL_LIGHT1 = 217,

    /// <summary>
    /// Peripheral Light 2
    /// </summary>
    [Description("Peripheral Light 2")]
    PERIPHERAL_LIGHT2 = 218,

    /// <summary>
    /// Peripheral Light 3
    /// </summary>
    [Description("Peripheral Light 3")]
    PERIPHERAL_LIGHT3 = 219,

    /// <summary>
    /// Peripheral Light 4
    /// </summary>
    [Description("Peripheral Light 4")]
    PERIPHERAL_LIGHT4 = 220,

    /// <summary>
    /// Peripheral Light 5
    /// </summary>
    [Description("Peripheral Light 5")]
    PERIPHERAL_LIGHT5 = 221,

    /// <summary>
    /// Peripheral Light 6
    /// </summary>
    [Description("Peripheral Light 6")]
    PERIPHERAL_LIGHT6 = 222,

    /// <summary>
    /// Peripheral Light 7
    /// </summary>
    [Description("Peripheral Light 7")]
    PERIPHERAL_LIGHT7 = 223,

    /// <summary>
    /// Peripheral Light 8
    /// </summary>
    [Description("Peripheral Light 8")]
    PERIPHERAL_LIGHT8 = 224,

    /// <summary>
    /// Peripheral Light 9
    /// </summary>
    [Description("Peripheral Light 9")]
    PERIPHERAL_LIGHT9 = 225,

    /// <summary>
    /// Peripheral Light 10
    /// </summary>
    [Description("Peripheral Light 10")]
    PERIPHERAL_LIGHT10 = 226,

    /// <summary>
    /// Peripheral Light 11
    /// </summary>
    [Description("Peripheral Light 11")]
    PERIPHERAL_LIGHT11 = 227,

    /// <summary>
    /// Peripheral Light 12
    /// </summary>
    [Description("Peripheral Light 12")]
    PERIPHERAL_LIGHT12 = 228,

    /// <summary>
    /// Peripheral Light 13
    /// </summary>
    [Description("Peripheral Light 13")]
    PERIPHERAL_LIGHT13 = 229,

    /// <summary>
    /// Peripheral Light 14
    /// </summary>
    [Description("Peripheral Light 14")]
    PERIPHERAL_LIGHT14 = 230,

    /// <summary>
    /// Peripheral Light 15
    /// </summary>
    [Description("Peripheral Light 15")]
    PERIPHERAL_LIGHT15 = 231,

    /// <summary>
    /// Peripheral Light 16
    /// </summary>
    [Description("Peripheral Light 16")]
    PERIPHERAL_LIGHT16 = 232,

    /// <summary>
    /// Peripheral Light 17
    /// </summary>
    [Description("Peripheral Light 17")]
    PERIPHERAL_LIGHT17 = 233,

    /// <summary>
    /// Peripheral Light 18
    /// </summary>
    [Description("Peripheral Light 18")]
    PERIPHERAL_LIGHT18 = 234,

    /// <summary>
    /// Peripheral Light 19
    /// </summary>
    [Description("Peripheral Light 19")]
    PERIPHERAL_LIGHT19 = 235,

    /// <summary>
    /// Peripheral Light 20
    /// </summary>
    [Description("Peripheral Light 20")]
    PERIPHERAL_LIGHT20 = 236,

    /// <summary>
    /// Peripheral DPI
    /// </summary>
    [Description("PERIPHERAL_DPI")]
    PERIPHERAL_DPI = 237,

    /// <summary>
    /// Mousepad Light 2
    /// </summary>
    [Description("Mousepad Light 16")]
    MOUSEPADLIGHT16 = 238,

    /// <summary>
    /// Mousepad Light 2
    /// </summary>
    [Description("Mousepad Light 17")]
    MOUSEPADLIGHT17 = 239,

    /// <summary>
    /// Mousepad Light 1
    /// </summary>
    [Description("Mousepad Light 18")]
    MOUSEPADLIGHT18 = 240,

    /// <summary>
    /// Mousepad Light 2
    /// </summary>
    [Description("Mousepad Light 19")]
    MOUSEPADLIGHT19 = 241,

    /// <summary>
    /// Mousepad Light 1
    /// </summary>
    [Description("Mousepad Light 20")]
    MOUSEPADLIGHT20 = 242,

    /// <summary>
    /// Mousepad Light 2
    /// </summary>
    [Description("Mousepad Light 21")]
    MOUSEPADLIGHT21 = 243,

    /// <summary>
    /// Additional Light 33
    /// </summary>
    [Description("Additional Light 33")]
    ADDITIONALLIGHT33 = 250,


    /// <summary>
    /// Additional Light 34
    /// </summary>
    [Description("Additional Light 34")]
    ADDITIONALLIGHT34 = 251,

    /// <summary>
    /// Additional Light 35
    /// </summary>
    [Description("Additional Light 35")]
    ADDITIONALLIGHT35 = 252,

    /// <summary>
    /// Additional Light 36
    /// </summary>
    [Description("Additional Light 36")]
    ADDITIONALLIGHT36 = 253,

    /// <summary>
    /// Additional Light 37
    /// </summary>
    [Description("Additional Light 37")]
    ADDITIONALLIGHT37 = 254,

    /// <summary>
    /// Additional Light 38
    /// </summary>
    [Description("Additional Light 38")]
    ADDITIONALLIGHT38 = 255,

    /// <summary>
    /// Additional Light 39
    /// </summary>
    [Description("Additional Light 39")]
    ADDITIONALLIGHT39 = 256,

    /// <summary>
    /// Additional Light 40
    /// </summary>
    [Description("Additional Light 40")]
    ADDITIONALLIGHT40 = 257,

    /// <summary>
    /// Additional Light 41
    /// </summary>
    [Description("Additional Light 41")]
    ADDITIONALLIGHT41 = 258,

    /// <summary>
    /// Additional Light 42
    /// </summary>
    [Description("Additional Light 42")]
    ADDITIONALLIGHT42 = 259,

    /// <summary>
    /// Additional Light 43
    /// </summary>
    [Description("Additional Light 43")]
    ADDITIONALLIGHT43 = 260,

    /// <summary>
    /// Additional Light 44
    /// </summary>
    [Description("Additional Light 44")]
    ADDITIONALLIGHT44 = 261,

    /// <summary>
    /// Additional Light 45
    /// </summary>
    [Description("Additional Light 45")]
    ADDITIONALLIGHT45 = 262,

    /// <summary>
    /// Additional Light 46
    /// </summary>
    [Description("Additional Light 46")]
    ADDITIONALLIGHT46 = 263,

    /// <summary>
    /// Additional Light 47
    /// </summary>
    [Description("Additional Light 47")]
    ADDITIONALLIGHT47 = 264,

    /// <summary>
    /// Additional Light 48
    /// </summary>
    [Description("Additional Light 48")]
    ADDITIONALLIGHT48 = 265,

    /// <summary>
    /// Additional Light 49
    /// </summary>
    [Description("Additional Light 49")]
    ADDITIONALLIGHT49 = 266,

    /// <summary>
    /// Additional Light 50
    /// </summary>
    [Description("Additional Light 50")]
    ADDITIONALLIGHT50 = 267,

    /// <summary>
    /// Additional Light 51
    /// </summary>
    [Description("Additional Light 51")]
    ADDITIONALLIGHT51 = 268,

    /// <summary>
    /// Additional Light 52
    /// </summary>
    [Description("Additional Light 52")]
    ADDITIONALLIGHT52 = 269,

    /// <summary>
    /// Additional Light 53
    /// </summary>
    [Description("Additional Light 53")]
    ADDITIONALLIGHT53 = 270,

    /// <summary>
    /// Additional Light 54
    /// </summary>
    [Description("Additional Light 54")]
    ADDITIONALLIGHT54 = 271,

    /// <summary>
    /// Additional Light 55
    /// </summary>
    [Description("Additional Light 55")]
    ADDITIONALLIGHT55 = 272,

    /// <summary>
    /// Additional Light 56
    /// </summary>
    [Description("Additional Light 56")]
    ADDITIONALLIGHT56 = 273,

    /// <summary>
    /// Additional Light 57
    /// </summary>
    [Description("Additional Light 57")]
    ADDITIONALLIGHT57 = 274,

    /// <summary>
    /// Additional Light 58
    /// </summary>
    [Description("Additional Light 58")]
    ADDITIONALLIGHT58 = 275,

    /// <summary>
    /// Additional Light 59
    /// </summary>
    [Description("Additional Light 59")]
    ADDITIONALLIGHT59 = 276,

    /// <summary>
    /// Additional Light 60
    /// </summary>
    [Description("Additional Light 60")]
    ADDITIONALLIGHT60 = 277,

    /// <summary>
    /// Left Part of Logo
    /// </summary>
    [Description("Left Part of Logo")]
    LOGOLEFT = 278,

    /// <summary>
    /// Middle Part of Logo
    /// </summary>
    [Description("Middle Part of Logo")]
    LOGOMIDDLE = 279,

    /// <summary>
    /// Right Part of Logo
    /// </summary>
    [Description("Right Part of Logo")]
    LOGORIGHT = 280,

    /// <summary>
    /// Wheel Center
    /// </summary>
    [Description("Wheel Center")]
    WHEELCENTER = 281,

    /// <summary>
    /// Wheel 1
    /// </summary>
    [Description("Wheel 1")]
    WHEEL1 = 282,

    /// <summary>
    /// Wheel 2
    /// </summary>
    [Description("Wheel 2")]
    WHEEL2 = 283,

    /// <summary>
    /// Wheel 3
    /// </summary>
    [Description("Wheel 3")]
    WHEEL3 = 284,

    /// <summary>
    /// Wheel4
    /// </summary>
    [Description("Wheel 4")]
    WHEEL4 = 285,

    /// <summary>
    /// Wheel 5
    /// </summary>
    [Description("Wheel 5")]
    WHEEL5 = 286,

    /// <summary>
    /// Wheel 6
    /// </summary>
    [Description("Wheel 6")]
    WHEEL6 = 287,

    /// <summary>
    /// Wheel 7
    /// </summary>
    [Description("Wheel 7")]
    WHEEL7 = 288,

    /// <summary>
    /// Wheel 8
    /// </summary>
    [Description("Wheel 8")]
    WHEEL8 = 289,

    /// <summary>
    /// Profile Switch
    /// </summary>
    [Description("Profile Switch")]
    PROFILESWITCH = 290,

    /// <summary>
    /// Chroma Link 1
    /// </summary>
    [Description("Chroma Link 1")]
    CL1 = 291,

    /// <summary>
    /// Chroma Link 2
    /// </summary>
    [Description("Chroma Link 2")]
    CL2 = 292,

    /// <summary>
    /// Chroma Link 3
    /// </summary>
    [Description("Chroma Link 3")]
    CL3 = 293,

    /// <summary>
    /// Chroma Link 4
    /// </summary>
    [Description("Chroma Link 4")]
    CL4 = 294,

    /// <summary>
    /// Chroma Link 5
    /// </summary>
    [Description("Chroma Link 5")]
    CL5 = 295,

    /// <summary>
    /// Headset 1
    /// </summary>
    [Description("Headset 1")]
    HEADSET1 = 296,

    /// <summary>
    /// Headset 2
    /// </summary>
    [Description("Headset 2")]
    HEADSET2 = 297,

    /// <summary>
    /// Headset 1
    /// </summary>
    [Description("Headset 3")]
    HEADSET3 = 298,

    /// <summary>
    /// Headset 2
    /// </summary>
    [Description("Headset 4")]
    HEADSET4 = 299,

    /// <summary>
    /// Headset 2
    /// </summary>
    [Description("Headset 5")]
    HEADSET5 = 300,

    /// <summary>
    /// Num Lock Led
    /// </summary>
    [Description("Num Lock Led")]
    NUM_LOCK_LED = 301,

    /// <summary>
    /// Caps Lock Led
    /// </summary>
    [Description("Caps Lock Led")]
    CAPS_LOCK_LED = 302,

    /// <summary>
    /// Scroll Lock Led
    /// </summary>
    [Description("Scroll Lock Led")]
    SCROLL_LOCK_LED = 303,

    [Description("Scroll Lock Led")]
    MACRO_INDICATOR = 304,

    [Description("Scroll Lock Led")]
    WIN_LOCK_INDICATOR = 305,

    [Description("Additional Light 61")]
    ADDITIONALLIGHT61 = 306,

    [Description("Additional Light 62")]
    ADDITIONALLIGHT62 = 307,

    [Description("Additional Light 63")]
    ADDITIONALLIGHT63 = 308,

    [Description("Additional Light 64")]
    ADDITIONALLIGHT64 = 309,

    [Description("Additional Light 65")]
    ADDITIONALLIGHT65 = 310,

    [Description("Additional Light 66")]
    ADDITIONALLIGHT66 = 311,

    [Description("Additional Light 67")]
    ADDITIONALLIGHT67 = 312,

    [Description("Additional Light 68")]
    ADDITIONALLIGHT68 = 313,

    [Description("Additional Light 69")]
    ADDITIONALLIGHT69 = 314,

    [Description("Additional Light 70")]
    ADDITIONALLIGHT70 = 315,

    [Description("Additional Light 71")]
    ADDITIONALLIGHT71 = 316,

    [Description("Additional Light 72")]
    ADDITIONALLIGHT72 = 317,

    [Description("Additional Light 73")]
    ADDITIONALLIGHT73 = 318,

    [Description("Additional Light 74")]
    ADDITIONALLIGHT74 = 319,

    [Description("Additional Light 75")]
    ADDITIONALLIGHT75 = 320,

    [Description("Additional Light 76")]
    ADDITIONALLIGHT76 = 321,

    [Description("Additional Light 77")]
    ADDITIONALLIGHT77 = 322,

    [Description("Additional Light 78")]
    ADDITIONALLIGHT78 = 323,

    [Description("Additional Light 79")]
    ADDITIONALLIGHT79 = 324,

    [Description("Additional Light 80")]
    ADDITIONALLIGHT80 = 325,

    [Description("Additional Light 81")]
    ADDITIONALLIGHT81 = 326,

    [Description("Additional Light 82")]
    ADDITIONALLIGHT82 = 327,

    [Description("Additional Light 83")]
    ADDITIONALLIGHT83 = 328,

    [Description("Additional Light 84")]
    ADDITIONALLIGHT84 = 329,

    [Description("Additional Light 85")]
    ADDITIONALLIGHT85 = 330,

    [Description("Additional Light 86")]
    ADDITIONALLIGHT86 = 331,

    [Description("Additional Light 87")]
    ADDITIONALLIGHT87 = 332,

    [Description("Additional Light 88")]
    ADDITIONALLIGHT88 = 333,

    [Description("Additional Light 89")]
    ADDITIONALLIGHT89 = 334,

    [Description("Additional Light 90")]
    ADDITIONALLIGHT90 = 335,

    [Description("Additional Light 91")]
    ADDITIONALLIGHT91 = 336,

    [Description("Additional Light 92")]
    ADDITIONALLIGHT92 = 337,

    [Description("Additional Light 93")]
    ADDITIONALLIGHT93 = 338,

    [Description("Additional Light 94")]
    ADDITIONALLIGHT94 = 339,

    [Description("Additional Light 95")]
    ADDITIONALLIGHT95 = 340,

    [Description("Additional Light 96")]
    ADDITIONALLIGHT96 = 341,

    [Description("Additional Light 97")]
    ADDITIONALLIGHT97 = 342,

    [Description("Additional Light 98")]
    ADDITIONALLIGHT98 = 343,

    [Description("Additional Light 99")]
    ADDITIONALLIGHT99 = 344,

    [Description("Additional Light 100")]
    ADDITIONALLIGHT100 = 345,

    [Description("Additional Light 101")]
    ADDITIONALLIGHT101 = 346,

    [Description("Additional Light 102")]
    ADDITIONALLIGHT102 = 347,

    [Description("Additional Light 103")]
    ADDITIONALLIGHT103 = 348,

    [Description("Additional Light 104")]
    ADDITIONALLIGHT104 = 349,

    [Description("Additional Light 105")]
    ADDITIONALLIGHT105 = 350,

    [Description("Additional Light 106")]
    ADDITIONALLIGHT106 = 351,

    [Description("Additional Light 107")]
    ADDITIONALLIGHT107 = 352,

    [Description("Additional Light 108")]
    ADDITIONALLIGHT108 = 353,

    [Description("Additional Light 109")]
    ADDITIONALLIGHT109 = 354,

    [Description("Additional Light 110")]
    ADDITIONALLIGHT110 = 355,

    [Description("Additional Light 111")]
    ADDITIONALLIGHT111 = 356,

    [Description("Additional Light 112")]
    ADDITIONALLIGHT112 = 357,

    [Description("Additional Light 113")]
    ADDITIONALLIGHT113 = 358,

    [Description("Additional Light 114")]
    ADDITIONALLIGHT114 = 359,

    [Description("Additional Light 115")]
    ADDITIONALLIGHT115 = 360,

    [Description("Additional Light 116")]
    ADDITIONALLIGHT116 = 361,

    [Description("Additional Light 117")]
    ADDITIONALLIGHT117 = 362,

    [Description("Additional Light 118")]
    ADDITIONALLIGHT118 = 363,

    [Description("Additional Light 119")]
    ADDITIONALLIGHT119 = 364,

    [Description("Additional Light 120")]
    ADDITIONALLIGHT120 = 365,

    /// <summary>
    /// None
    /// </summary>
    [Description("None")]
    NONE = -1,
};