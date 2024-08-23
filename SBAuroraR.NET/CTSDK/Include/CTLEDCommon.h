/*
################################################################################
##
## File:     CTLEDCommon.h
##
## Purpose:  This declares and defines common data used by the Creative LED Manager library.
##
## Remarks:
##
## ---------------------------------------------------------------------------
## Copyright (c) Creative Technology Ltd., 2016-2017.  All rights reserved.
##
## ---------------------------------------------------------------------------
## THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF
## ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO
## THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
## PARTICULAR PURPOSE.
##
################################################################################
*/

#ifndef __CTLEDCOMMON_H__
#define __CTLEDCOMMON_H__

#include <tchar.h>

//============================================================================
// Definitions of constant values.
//============================================================================

//============================================================================
// Definitions for Vendor ID, Product ID and Product Friendly Name.
//============================================================================
const LPCTSTR DEFINITION_PRODUCT_FRIENDLY_NAME_SoundBlasterXVanguardK08_USEnglish = _T("Sound BlasterX Vanguard K08 (US English)");
const USHORT DEFINITION_VENDOR_ID_SoundBlasterXVanguardK08_USEnglish = 0x041E;
const USHORT DEFINITION_PRODUCT_ID_SoundBlasterXVanguardK08_USEnglish = 0x3126;

const LPCTSTR DEFINITION_PRODUCT_FRIENDLY_NAME_SoundBlasterXVanguardK08_German = _T("Sound BlasterX Vanguard K08 (German)");
const USHORT DEFINITION_VENDOR_ID_SoundBlasterXVanguardK08_German = 0x041E;
const USHORT DEFINITION_PRODUCT_ID_SoundBlasterXVanguardK08_German = 0x3128;

const LPCTSTR DEFINITION_PRODUCT_FRIENDLY_NAME_SoundBlasterXVanguardK08_Nordic = _T("Sound BlasterX Vanguard K08 (Nordic)");
const USHORT DEFINITION_VENDOR_ID_SoundBlasterXVanguardK08_Nordic = 0x041E;
const USHORT DEFINITION_PRODUCT_ID_SoundBlasterXVanguardK08_Nordic = 0x3129;

const LPCTSTR DEFINITION_PRODUCT_FRIENDLY_NAME_SoundBlasterXSiegeM04 = _T("Sound BlasterX Siege M04");
const USHORT DEFINITION_VENDOR_ID_SoundBlasterXSiegeM04 = 0x041E;
const USHORT DEFINITION_PRODUCT_ID_SoundBlasterXSiegeM04 = 0x3127;

const LPCTSTR DEFINITION_PRODUCT_FRIENDLY_NAME_SoundBlasterXKatana = _T("Sound BlasterX Katana");
const USHORT DEFINITION_VENDOR_ID_SoundBlasterXKatana = 0x041E;
const USHORT DEFINITION_PRODUCT_ID_SoundBlasterXKatana = 0x3247;

const LPCTSTR DEFINITION_PRODUCT_FRIENDLY_NAME_SoundBlasterXKratosS5 = _T("Sound BlasterX Kratos S5");
const USHORT DEFINITION_VENDOR_ID_SoundBlasterXKratosS5 = 0x041E;
const USHORT DEFINITION_PRODUCT_ID_SoundBlasterXKratosS5 = 0x30CA;

const LPCTSTR DEFINITION_PRODUCT_FRIENDLY_NAME_SoundBlasterXAE5 = _T("Sound BlasterX AE-5");
const LPCTSTR DEFINITION_PRODUCT_FRIENDLY_NAME_SoundBlasterXAE5_ExternalLighting = _T("Sound BlasterX AE-5 (External Lighting)");
const LPCTSTR DEFINITION_PRODUCT_FRIENDLY_NAME_SoundBlasterXAE5_BuiltInLighting = _T("Sound BlasterX AE-5 (Built-in Lighting)");
const USHORT DEFINITION_VENDOR_ID_SoundBlasterXAE5 = 0x1102;
const USHORT DEFINITION_PRODUCT_ID_SoundBlasterXAE5 = 0x0051;


	#ifndef __ICTHIDRpA_H__
//============================================================================
// This is a special LED Index to indicate that the per LED Index is not applicable in the context. For example, it may be used to denote "All LEDs".
//============================================================================
const DWORD DEFINITION_CTLEDIndex_NotApplicable = 0xFFFFFFFF; 

//============================================================================
// The CTLEDIndex_xxx definitions. 
// These are general purpose LED index values which must start from 0.
// Currently, the maximum LED index value defined here is CTLEDIndex_199.
//============================================================================
typedef enum tagCTLEDIndex
{
	CTLEDIndex_0, CTLEDIndex_1, CTLEDIndex_2, CTLEDIndex_3, CTLEDIndex_4, CTLEDIndex_5, CTLEDIndex_6, CTLEDIndex_7, CTLEDIndex_8, CTLEDIndex_9,
	CTLEDIndex_10, CTLEDIndex_11, CTLEDIndex_12, CTLEDIndex_13, CTLEDIndex_14, CTLEDIndex_15, CTLEDIndex_16, CTLEDIndex_17, CTLEDIndex_18, CTLEDIndex_19,
	CTLEDIndex_20, CTLEDIndex_21, CTLEDIndex_22, CTLEDIndex_23, CTLEDIndex_24, CTLEDIndex_25, CTLEDIndex_26, CTLEDIndex_27, CTLEDIndex_28, CTLEDIndex_29,
	CTLEDIndex_30, CTLEDIndex_31, CTLEDIndex_32, CTLEDIndex_33, CTLEDIndex_34, CTLEDIndex_35, CTLEDIndex_36, CTLEDIndex_37, CTLEDIndex_38, CTLEDIndex_39,
	CTLEDIndex_40, CTLEDIndex_41, CTLEDIndex_42, CTLEDIndex_43, CTLEDIndex_44, CTLEDIndex_45, CTLEDIndex_46, CTLEDIndex_47, CTLEDIndex_48, CTLEDIndex_49,
	CTLEDIndex_50, CTLEDIndex_51, CTLEDIndex_52, CTLEDIndex_53, CTLEDIndex_54, CTLEDIndex_55, CTLEDIndex_56, CTLEDIndex_57, CTLEDIndex_58, CTLEDIndex_59,
	CTLEDIndex_60, CTLEDIndex_61, CTLEDIndex_62, CTLEDIndex_63, CTLEDIndex_64, CTLEDIndex_65, CTLEDIndex_66, CTLEDIndex_67, CTLEDIndex_68, CTLEDIndex_69,
	CTLEDIndex_70, CTLEDIndex_71, CTLEDIndex_72, CTLEDIndex_73, CTLEDIndex_74, CTLEDIndex_75, CTLEDIndex_76, CTLEDIndex_77, CTLEDIndex_78, CTLEDIndex_79,
	CTLEDIndex_80, CTLEDIndex_81, CTLEDIndex_82, CTLEDIndex_83, CTLEDIndex_84, CTLEDIndex_85, CTLEDIndex_86, CTLEDIndex_87, CTLEDIndex_88, CTLEDIndex_89,
	CTLEDIndex_90, CTLEDIndex_91, CTLEDIndex_92, CTLEDIndex_93, CTLEDIndex_94, CTLEDIndex_95, CTLEDIndex_96, CTLEDIndex_97, CTLEDIndex_98, CTLEDIndex_99,
	CTLEDIndex_100, CTLEDIndex_101, CTLEDIndex_102, CTLEDIndex_103, CTLEDIndex_104, CTLEDIndex_105, CTLEDIndex_106, CTLEDIndex_107, CTLEDIndex_108, CTLEDIndex_109,
	CTLEDIndex_110, CTLEDIndex_111, CTLEDIndex_112, CTLEDIndex_113, CTLEDIndex_114, CTLEDIndex_115, CTLEDIndex_116, CTLEDIndex_117, CTLEDIndex_118, CTLEDIndex_119,
	CTLEDIndex_120, CTLEDIndex_121, CTLEDIndex_122, CTLEDIndex_123, CTLEDIndex_124, CTLEDIndex_125, CTLEDIndex_126, CTLEDIndex_127, CTLEDIndex_128, CTLEDIndex_129,
	CTLEDIndex_130, CTLEDIndex_131, CTLEDIndex_132, CTLEDIndex_133, CTLEDIndex_134, CTLEDIndex_135, CTLEDIndex_136, CTLEDIndex_137, CTLEDIndex_138, CTLEDIndex_139,
	CTLEDIndex_140, CTLEDIndex_141, CTLEDIndex_142, CTLEDIndex_143, CTLEDIndex_144, CTLEDIndex_145, CTLEDIndex_146, CTLEDIndex_147, CTLEDIndex_148, CTLEDIndex_149,
	CTLEDIndex_150, CTLEDIndex_151, CTLEDIndex_152, CTLEDIndex_153, CTLEDIndex_154, CTLEDIndex_155, CTLEDIndex_156, CTLEDIndex_157, CTLEDIndex_158, CTLEDIndex_159,
	CTLEDIndex_160, CTLEDIndex_161, CTLEDIndex_162, CTLEDIndex_163, CTLEDIndex_164, CTLEDIndex_165, CTLEDIndex_166, CTLEDIndex_167, CTLEDIndex_168, CTLEDIndex_169,
	CTLEDIndex_170, CTLEDIndex_171, CTLEDIndex_172, CTLEDIndex_173, CTLEDIndex_174, CTLEDIndex_175, CTLEDIndex_176, CTLEDIndex_177, CTLEDIndex_178, CTLEDIndex_179,
	CTLEDIndex_180, CTLEDIndex_181, CTLEDIndex_182, CTLEDIndex_183, CTLEDIndex_184, CTLEDIndex_185, CTLEDIndex_186, CTLEDIndex_187, CTLEDIndex_188, CTLEDIndex_189,
	CTLEDIndex_190, CTLEDIndex_191, CTLEDIndex_192, CTLEDIndex_193, CTLEDIndex_194, CTLEDIndex_195, CTLEDIndex_196, CTLEDIndex_197, CTLEDIndex_198, CTLEDIndex_199,
	CTLEDIndex_NotApplicable = DEFINITION_CTLEDIndex_NotApplicable
} CTLEDIndex;

//============================================================================
// The CTKEYBOARD_LEDIndex_xxx definitions.
//============================================================================
typedef enum tagCTKEYBOARD_LEDIndex
{
	CTKEYBOARD_LEDIndex_Esc				= 0,	// 0x00
	CTKEYBOARD_LEDIndex_F1				= 1,	// 0x01
	CTKEYBOARD_LEDIndex_F2				= 2,	// 0x02
	CTKEYBOARD_LEDIndex_F3				= 3,	// 0x03
	CTKEYBOARD_LEDIndex_F4 = 4,	// 0x04
	CTKEYBOARD_LEDIndex_F5 = 5,	// 0x05
	CTKEYBOARD_LEDIndex_F6 = 6,	// 0x06
	CTKEYBOARD_LEDIndex_F7 = 7,	// 0x07
	CTKEYBOARD_LEDIndex_F8 = 8,	// 0x08
	CTKEYBOARD_LEDIndex_F9 = 9,	// 0x09
	CTKEYBOARD_LEDIndex_F10 = 10,	// 0x0A
	CTKEYBOARD_LEDIndex_F11 = 11,	// 0x0B
	CTKEYBOARD_LEDIndex_F12 = 12,	// 0x0C
	CTKEYBOARD_LEDIndex_M1 = 13,	// 0x0D
	CTKEYBOARD_LEDIndex_BackQuote = 14,	// 0x0E
	CTKEYBOARD_LEDIndex_1 = 15,	// 0x0F
	CTKEYBOARD_LEDIndex_2 = 16,	// 0x10
	CTKEYBOARD_LEDIndex_3 = 17,	// 0x11
	CTKEYBOARD_LEDIndex_4 = 18,	// 0x12
	CTKEYBOARD_LEDIndex_5 = 19,	// 0x13
	CTKEYBOARD_LEDIndex_6 = 20,	// 0x14
	CTKEYBOARD_LEDIndex_7 = 21,	// 0x15
	CTKEYBOARD_LEDIndex_8 = 22,	// 0x16
	CTKEYBOARD_LEDIndex_9 = 23,	// 0x17
	CTKEYBOARD_LEDIndex_0 = 24,	// 0x18
	CTKEYBOARD_LEDIndex_Minus = 25,	// 0x19
	CTKEYBOARD_LEDIndex_Equal = 26,	// 0x1A
	CTKEYBOARD_LEDIndex_Reserved27 = 27,	// 0x1B
	CTKEYBOARD_LEDIndex_Backspace = 28,	// 0x1C
	CTKEYBOARD_LEDIndex_M2 = 29,	// 0x1D
	CTKEYBOARD_LEDIndex_Tab = 30,	// 0x1E
	CTKEYBOARD_LEDIndex_Q = 31,	// 0x1F
	CTKEYBOARD_LEDIndex_W = 32,	// 0x20
	CTKEYBOARD_LEDIndex_E = 33,	// 0x21
	CTKEYBOARD_LEDIndex_R = 34,	// 0x22
	CTKEYBOARD_LEDIndex_T = 35,	// 0x23
	CTKEYBOARD_LEDIndex_Y = 36,	// 0x24
	CTKEYBOARD_LEDIndex_U = 37,	// 0x25
	CTKEYBOARD_LEDIndex_I = 38,	// 0x26
	CTKEYBOARD_LEDIndex_O = 39,	// 0x27
	CTKEYBOARD_LEDIndex_P = 40,	// 0x28
	CTKEYBOARD_LEDIndex_OpenBracket = 41,	// 0x29
	CTKEYBOARD_LEDIndex_ClosedBracket = 42,	// 0x2A
	CTKEYBOARD_LEDIndex_BackSlash = 43,	// 0x2B // --> Not applicable for German and Nordic (because they do not have this key).
	CTKEYBOARD_LEDIndex_M3 = 44,	// 0x2C
	CTKEYBOARD_LEDIndex_CapsLock = 45,	// 0x2D
	CTKEYBOARD_LEDIndex_A = 46,	// 0x2E
	CTKEYBOARD_LEDIndex_S = 47,	// 0x2F
	CTKEYBOARD_LEDIndex_D = 48,	// 0x30
	CTKEYBOARD_LEDIndex_F = 49,	// 0x31
	CTKEYBOARD_LEDIndex_G = 50,	// 0x32
	CTKEYBOARD_LEDIndex_H = 51,	// 0x33
	CTKEYBOARD_LEDIndex_J = 52,	// 0x34
	CTKEYBOARD_LEDIndex_K = 53,	// 0x35
	CTKEYBOARD_LEDIndex_L = 54,	// 0x36
	CTKEYBOARD_LEDIndex_Semicolon = 55,	// 0x37
	CTKEYBOARD_LEDIndex_Apostrophe = 56,	// 0x38
	CTKEYBOARD_LEDIndex_Reserved57 = 57,	// 0x39
	CTKEYBOARD_LEDIndex_NonUS57	= CTKEYBOARD_LEDIndex_Reserved57, // --> Applicable for German and Nordic keyboards (which is the key on the left of the Enter key).
	CTKEYBOARD_LEDIndex_Enter = 58,	// 0x3A
	CTKEYBOARD_LEDIndex_M4 = 59,	// 0x3B
	CTKEYBOARD_LEDIndex_LeftShift = 60,	// 0x3C
	CTKEYBOARD_LEDIndex_Reserved61 = 61,	// 0x3D
	CTKEYBOARD_LEDIndex_NonUS61	= CTKEYBOARD_LEDIndex_Reserved61, // --> Applicable for German and Nordic keyboards (which is the key on the right of the Left Shift key).
	CTKEYBOARD_LEDIndex_Z = 62,	// 0x3E
	CTKEYBOARD_LEDIndex_X = 63,	// 0x3F
	CTKEYBOARD_LEDIndex_C = 64,	// 0x40
	CTKEYBOARD_LEDIndex_V = 65,	// 0x41
	CTKEYBOARD_LEDIndex_B = 66,	// 0x42
	CTKEYBOARD_LEDIndex_N = 67,	// 0x43
	CTKEYBOARD_LEDIndex_M = 68,	// 0x44
	CTKEYBOARD_LEDIndex_Comma = 69,	// 0x45
	CTKEYBOARD_LEDIndex_Fullstop = 70,	// 0x46
	CTKEYBOARD_LEDIndex_ForwardSlash = 71,	// 0x47
	CTKEYBOARD_LEDIndex_Reserved72 = 72,	// 0x48
	CTKEYBOARD_LEDIndex_RightShift = 73,	// 0x49
	CTKEYBOARD_LEDIndex_M5 = 74,	// 0x4A
	CTKEYBOARD_LEDIndex_LeftCtrl = 75,	// 0x4B
	CTKEYBOARD_LEDIndex_LeftWindows = 76,	// 0x4C
	CTKEYBOARD_LEDIndex_LeftAlt = 77,	// 0x4D
	CTKEYBOARD_LEDIndex_Reserved78 = 78,	// 0x4E
	CTKEYBOARD_LEDIndex_Reserved79 = 79,	// 0x4F
	CTKEYBOARD_LEDIndex_Space = 80,	// 0x50
	CTKEYBOARD_LEDIndex_Reserved81 = 81,	// 0x51
	CTKEYBOARD_LEDIndex_Reserved82 = 82,	// 0x52
	CTKEYBOARD_LEDIndex_Reserved83 = 83,	// 0x53
	CTKEYBOARD_LEDIndex_RightAlt = 84,	// 0x54
	CTKEYBOARD_LEDIndex_Fn = 85,	// 0x55
	CTKEYBOARD_LEDIndex_Menu = 86,	// 0x56
	CTKEYBOARD_LEDIndex_RightCtrl = 87,	// 0x57
	CTKEYBOARD_LEDIndex_Reserved88 = 88,	// 0x58
	CTKEYBOARD_LEDIndex_Reserved89 = 89,	// 0x59
	CTKEYBOARD_LEDIndex_Reserved90 = 90,	// 0x5A
	CTKEYBOARD_LEDIndex_Reserved91 = 91,	// 0x5B
	CTKEYBOARD_LEDIndex_Reserved92 = 92,	// 0x5C
	CTKEYBOARD_LEDIndex_Reserved93 = 93,	// 0x5D
	CTKEYBOARD_LEDIndex_PadMinus = 94,	// 0x5E
	CTKEYBOARD_LEDIndex_PadAsterisk = 95,	// 0x5F
	CTKEYBOARD_LEDIndex_PadForwardSlash = 96,	// 0x60
	CTKEYBOARD_LEDIndex_PadNumLock = 97,	// 0x61
	CTKEYBOARD_LEDIndex_PageUp = 98,	// 0x62
	CTKEYBOARD_LEDIndex_Home = 99,	// 0x63
	CTKEYBOARD_LEDIndex_Insert = 100,	// 0x64
	CTKEYBOARD_LEDIndex_Reserved101 = 101,	// 0x65
	CTKEYBOARD_LEDIndex_PadPlus = 102,	// 0x66
	CTKEYBOARD_LEDIndex_Pad9 = 103,	// 0x67
	CTKEYBOARD_LEDIndex_Pad8 = 104,	// 0x68
	CTKEYBOARD_LEDIndex_Pad7 = 105,	// 0x69
	CTKEYBOARD_LEDIndex_PageDown = 106,	// 0x6A
	CTKEYBOARD_LEDIndex_End = 107,	// 0x6B
	CTKEYBOARD_LEDIndex_Delete = 108,	// 0x6C
	CTKEYBOARD_LEDIndex_PrintScreen = 109,	// 0x6D
	CTKEYBOARD_LEDIndex_Reserved110 = 110,	// 0x6E
	CTKEYBOARD_LEDIndex_Pad6 = 111,	// 0x6F
	CTKEYBOARD_LEDIndex_Pad5 = 112,	// 0x70
	CTKEYBOARD_LEDIndex_Pad4 = 113,	// 0x71
	CTKEYBOARD_LEDIndex_Pad1 = 114,	// 0x72
	CTKEYBOARD_LEDIndex_UpArrow = 115,	// 0x73
	CTKEYBOARD_LEDIndex_LeftArrow = 116,	// 0x74
	CTKEYBOARD_LEDIndex_ScrollLock = 117,	// 0x75
	CTKEYBOARD_LEDIndex_PadEnter = 118,	// 0x76
	CTKEYBOARD_LEDIndex_Pad3 = 119,	// 0x77
	CTKEYBOARD_LEDIndex_Pad2 = 120,	// 0x78
	CTKEYBOARD_LEDIndex_PadFullstop = 121,	// 0x79
	CTKEYBOARD_LEDIndex_Pad0 = 122,	// 0x7A
	CTKEYBOARD_LEDIndex_RightArrow = 123,	// 0x7B
	CTKEYBOARD_LEDIndex_DownArrow = 124,	// 0x7C
	CTKEYBOARD_LEDIndex_Pause = 125,	// 0x7D
	CTKEYBOARD_LEDIndex_Logo = 126,	// 0x7E
	CTKEYBOARD_LEDIndex_NotApplicable = DEFINITION_CTLEDIndex_NotApplicable
} CTKEYBOARD_LEDIndex;

//============================================================================
// The CTMOUSE_LEDIndex_xxx definitions.
//============================================================================
typedef enum tagCTMOUSE_LEDIndex
{
	CTMOUSE_LEDIndex_0 = 0,	// 0x00
	CTMOUSE_LEDIndex_1 = 1,	// 0x01
	CTMOUSE_LEDIndex_2 = 2,	// 0x02
	CTMOUSE_LEDIndex_3 = 3,	// 0x03
	CTMOUSE_LEDIndex_4 = 4,	// 0x04
	CTMOUSE_LEDIndex_5 = 5,	// 0x05
	CTMOUSE_LEDIndex_6 = 6,	// 0x06
	CTMOUSE_LEDIndex_7 = 7,	// 0x07
	CTMOUSE_LEDIndex_8 = 8,	// 0x08
	CTMOUSE_LEDIndex_9 = 9,	// 0x09
	CTMOUSE_LEDIndex_10 = 10,	// 0x0A
	CTMOUSE_LEDIndex_Logo = 0x80000001,
	CTMOUSE_LEDIndex_Wheel = 0x80000002,
	CTMOUSE_LEDIndex_NotApplicable = DEFINITION_CTLEDIndex_NotApplicable
} CTMOUSE_LEDIndex;

const DWORD DEFINITION_ProfileIDMain_Custom = 126;
const DWORD DEFINITION_ProfileIDSub_Custom = 63;
	#endif //#ifndef __ICTHIDRpA_H__

//============================================================================
// Detail eror code contained in the pdwDetailErrorCode "out" parameter when
// the returned HRESULT of the methods is not S_OK.
//============================================================================
	#ifndef __ICTHIDRpA_H__
const DWORD CTERROR_NewerVersionOfFirmwareIsNeeded = 101;
	#endif //#ifndef __ICTHIDRpA_H__

	#ifndef __ICTHIDRpA_H__
const BYTE DEFINITION_CTHIDGENERALCONTROL_State_LEDV2Master_Off = 0x00;
const BYTE DEFINITION_CTHIDGENERALCONTROL_State_LEDV2Master_On = 0x01;
	#endif //#ifndef __ICTHIDRpA_H__
#define DEFINITION_CTLEDMGR_LedMaster_Off DEFINITION_CTHIDGENERALCONTROL_State_LEDV2Master_Off
#define DEFINITION_CTLEDMGR_LedMaster_On DEFINITION_CTHIDGENERALCONTROL_State_LEDV2Master_On

//const DWORD DEFINITION_FlagForApplySettings_NotApplicable = 0xFFFFFFFF; // This is a special flag to indicate that the flag is not applicable in the context.
const DWORD DEFINITION_FlagForApplySettings_Default = 0x00000003; //DEFINITION_Flag_SetMultipleAndApply

	#ifndef __ICTHIDRpA_H__
typedef enum tagCTLED_Pattern
{
	CTLED_Pattern_None = 0,
	CTLED_Pattern_ColourCycle = 1,
	CTLED_Pattern_SpectrumAnalyzer = 2, // Reserved for future.
	CTLED_Pattern_Static = 3,
	CTLED_Pattern_Wave = 4,
	CTLED_Pattern_Pulsate = 5,
	CTLED_Pattern_Blink = 6, // Reserved for future.
	CTLED_Pattern_Morph = 7, // Reserved for future.
	CTLED_Pattern_Aurora = 8,
	CTLED_Pattern_Cosmic = 1000,  // Reserved for future.
	CTLED_Pattern_WaveCycleBounce = 1001,  // Reserved for future.
	CTLED_Pattern_Impact = 1002,  // Reserved for future.
	CTLED_Pattern_FollowGlobal = 0xFFFFFFFE,  // Reserved for future.
	CTLED_Pattern_Unknown = 0xFFFFFFFF
} CTLED_Pattern;

typedef enum tagCTLED_PatternDirection
{
	CTLED_PatternDirection_LeftToRight = 1,
	CTLED_PatternDirection_RightToLeft = 2,
	CTLED_PatternDirection_TopToBottom = 3,
	CTLED_PatternDirection_BottomToTop = 4,
	CTLED_PatternDirection_Clockwise = 5,
	CTLED_PatternDirection_Anticlockwise = 6,
	CTLED_PatternDirection_Unknown = 0xFFFFFFFF,
	CTLED_PatternDirection_NotApplicable = CTLED_PatternDirection_Unknown
} CTLED_PatternDirection;

typedef enum tagCTLED_PatternDirectionFlag
{
	CTLED_PatternDirectionFlag_Looping = 0,
	CTLED_PatternDirectionFlag_Bouncing = 1,
	CTLED_PatternDirectionFlag_Unknown = 0xFFFFFFFF,
	CTLED_PatternDirectionFlag_NotApplicable = CTLED_PatternDirectionFlag_Unknown
} CTLED_PatternDirectionFlag;

typedef enum tagCTLED_ConnectionType
{
	CTLED_ConnectionType_BuiltIn = 0,
	CTLED_ConnectionType_External = 1,
	CTLED_ConnectionType_Unknown = 0xFFFFFFFF
} CTLED_ConnectionType;

// CTLED_HardwareType is only applicable for devices with CTLED_ConnectionType_External as its connection type.
// It describes whether the LED hardware has separate clock and data signal paths or the clock is encoded together with the data.
typedef enum tagCTLED_HardwareType
{
	CTLED_HardwareType_NotApplicable = 0, // This may be used to denote the hardware type for connection type CTLED_ConnectionType_BuiltIn.
	CTLED_HardwareType_SeparateClockAndData = 0, // One clock input and output, one data input and output.
	CTLED_HardwareType_ClockEncodedWithData = 1, // One signal input and output. Data is encoded in pulse width of clock signal.
	CTLED_HardwareType_Unknown = 0xFFFFFFFF
} CTLED_HardwareType;
	#endif //#ifndef __ICTHIDRpA_H__


//============================================================================
// Data Structures.
//============================================================================

	#ifndef __ICTHIDRpA_H__
typedef struct tagCTColour
{
	BYTE bRed;
	BYTE bGreen;
	BYTE bBlue;
	BYTE bAlpha;
} CTColour, *PCTColour;

typedef struct tagCTColourLayerForMultipleLedGroups
{
	DWORD dwNumLedGroups;
	HANDLE hColourLayerForLedGroupArray;
} CTColourLayerForMultipleLedGroups, *PCTColourLayerForMultipleLedGroups;
	#endif //#ifndef __ICTHIDRpA_H__


//============================================================================
// The LPARAM parameter of ICTLEDMgr::ExecuteCommand() for the message CTLEDMGRCMD_SetLedSettings.
//============================================================================
typedef struct tagCTLEDMGRCMDPARAM_SetLedSettings
{
	/*IN*/ DWORD dwFlagForApplySettings; // DEFINITION_FlagForApplySettings_xxx

	/*IN*/ BOOL fIgnoreProfileID; // Flag to specify whether to ignore the parameters dwProfileIDMain and dwProfileIDSub.
	/*IN*/ DWORD dwProfileIDMain; // Profile Main ID
	/*IN*/ DWORD dwProfileIDSub; // Profile Sub ID

	/*IN*/ BOOL fPersistent; // Flag to specify whether the settings are to be persistently saved into the device.

	/*IN*/ BOOL fIgnoreMasterLedState; // Flag to specify whether to ignore the parameter dwMasterLedState.
	/*IN*/ DWORD dwMasterLedState; // DEFINITION_CTHIDGENERALCONTROL_State_LEDV2Master_xxx.

	/*IN*/ BOOL fIgnoreGlobalPatternMode; // Flag to specify whether to ignore the parameter fGlobalPatternMode.
	/*IN*/ BOOL fGlobalPatternMode; // Flag to specify whether to set the LEDs into global pattern mode or local pattern mode.

	/*IN*/ DWORD dwLedIndex; // DEFINITION_CTLEDIndex_NotApplicable

	/*IN*/ BOOL fIgnorePattern; // Flag to specify whether to ignore the parameter patternLed.
	/*IN*/ CTLED_Pattern patternLed; // CTLED_Pattern_xxx
	/*IN*/ BOOL fIgnorePatternDirection; // Flag to specify whether to ignore the parameter directionLed.
	/*IN*/ CTLED_PatternDirection directionLedPattern; // CTLED_PatternDirection_xxx
	/*IN*/ BOOL fIgnorePatternDirectionFlag; // Flag to specify whether to ignore the parameter flagLedPatternDirection.
	/*IN*/ CTLED_PatternDirectionFlag flagLedPatternDirection; // CTLED_PatternDirectionFlag_xxx

	/*IN*/ BOOL fIgnorePeriodicTime; // Flag to specify whether to ignore the parameter dwPeriodicTimeInMilliseconds and dwPeriodicTimeInCyclesPerMinute.
	/*IN*/ CTLED_Pattern patternLedThePeriodicTimeIsFor; // CTLED_Pattern_xxx
	/*IN*/ CTLED_PatternDirection directionLedPatternThePeriodicTimeIsFor; // Reserved for future use. For now, always set this to CTLED_PatternDirection_NotApplicable.
	/*IN*/ CTLED_PatternDirectionFlag flagLedPatternDirectionThePeriodicTimeIsFor; // Reserved for future use. For now, always set this to CTLED_PatternDirectionFlag_NotApplicable.
	/*IN*/ DWORD dwPeriodicTimeInMilliseconds; // The periodic time of the LED pattern in milliseconds. If this is zero, then dwPeriodicTimeInCyclesPerMinute will be used.
	/*IN*/ DWORD dwPeriodicTimeInCyclesPerMinute; // The periodic time of the LED pattern in cycles per minute. If this is zero, then dwPeriodicTimeInMilliseconds will be used.

	/*IN*/ BOOL fIgnoreColour; // Flag to specify whether to ignore the parameter colourLed.
	/*IN*/ CTColourLayerForMultipleLedGroups colourLed;
} CTLEDMGRCMDPARAM_SetLedSettings, *PCTLEDMGRCMDPARAM_SetLedSettings;

void __inline CTInit_CTLEDMGRCMDPARAM_SetLedSettings(OUT PCTLEDMGRCMDPARAM_SetLedSettings pParam)
{
	pParam->dwFlagForApplySettings = DEFINITION_FlagForApplySettings_Default;
	pParam->fIgnoreProfileID = TRUE;
  #if 1 // Not applicable if fIgnoreProfileID is TRUE. Nevertheless, just proceed to initialize the parameters to defaults.
	pParam->dwProfileIDMain = DEFINITION_ProfileIDMain_Custom;
	pParam->dwProfileIDSub = DEFINITION_ProfileIDSub_Custom;
  #endif
	pParam->fPersistent = FALSE;
	pParam->fIgnoreMasterLedState = FALSE;
  #if 1 // Not applicable if fIgnoreMasterLedState is TRUE.
	pParam->dwMasterLedState = DEFINITION_CTLEDMGR_LedMaster_On;
  #endif
	pParam->fIgnoreGlobalPatternMode = FALSE;
  #if 1 // Not applicable if fIgnoreGlobalPatternMode is TRUE.
	pParam->fGlobalPatternMode = FALSE;
  #endif
	pParam->dwLedIndex = DEFINITION_CTLEDIndex_NotApplicable;
	pParam->fIgnorePattern = TRUE;
  #if 1 // Not applicable if fIgnorePattern is TRUE. Nevertheless, just proceed to initialize the parameters to defaults.
	pParam->patternLed = CTLED_Pattern_Unknown;
  #endif
	pParam->fIgnorePatternDirection = TRUE;
  #if 1 // Not applicable if fIgnorePatternDirection is TRUE. Nevertheless, just proceed to initialize the parameters to defaults.
	pParam->directionLedPattern = CTLED_PatternDirection_Unknown;
  #endif
	pParam->fIgnorePatternDirectionFlag = TRUE;
  #if 1 // Not applicable if fIgnorePatternDirectionFlag is TRUE. Nevertheless, just proceed to initialize the parameters to defaults.
	pParam->flagLedPatternDirection = CTLED_PatternDirectionFlag_Unknown;
  #endif
	pParam->fIgnorePeriodicTime = TRUE;
  #if 1 // Not applicable if fIgnorePeriodicTime is TRUE. Nevertheless, just proceed to initialize the parameters to defaults.
	pParam->patternLedThePeriodicTimeIsFor = CTLED_Pattern_Unknown;
	pParam->directionLedPatternThePeriodicTimeIsFor = CTLED_PatternDirection_NotApplicable;
	pParam->flagLedPatternDirectionThePeriodicTimeIsFor = CTLED_PatternDirectionFlag_NotApplicable;
	pParam->dwPeriodicTimeInMilliseconds = 0;
	pParam->dwPeriodicTimeInCyclesPerMinute = 0;
  #endif
	pParam->fIgnoreColour = TRUE;
  #if 0 // Not applicable if fIgnoreColour is TRUE.
	pParam->colourLed.dwNumLedGroups = 0;
	pParam->colourLed.pColourLayerForLedGroupArray = NULL;
  #endif
}

//============================================================================
// Helper functions.
//============================================================================
BOOL __inline CTValidatePeriodicTime(DWORD dwPeriodicTimeInMilliseconds, DWORD dwPeriodicTimeInCyclesPerMinute)
{
	if ((dwPeriodicTimeInMilliseconds == 0) && (dwPeriodicTimeInCyclesPerMinute == 0))
		return FALSE;
	else
		return TRUE;
}

DWORD __inline CTRetrievePeriodicTimeInMilliseconds(DWORD dwPeriodicTimeInMilliseconds, DWORD dwPeriodicTimeInCyclesPerMinute)
{
	DWORD dwPeriodicTime;
	if (CTValidatePeriodicTime(dwPeriodicTimeInMilliseconds, dwPeriodicTimeInCyclesPerMinute))
	{
		dwPeriodicTime = (dwPeriodicTimeInMilliseconds != 0) ? dwPeriodicTimeInMilliseconds : (60000 / dwPeriodicTimeInCyclesPerMinute);
	}
	else
	{
		dwPeriodicTime = 0;
	}
	return dwPeriodicTime;
}


#endif // #ifndef __CTLEDCOMMON_H__
