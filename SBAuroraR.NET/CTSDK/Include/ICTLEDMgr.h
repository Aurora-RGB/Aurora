/*
################################################################################
##
## File:     ICTLEDMgr.h
##
## Purpose:  This declares the interface class ICTLEDMgr used by 
##           the Creative LED Manager library.
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

#ifndef __ICTLEDMGR_H__
#define __ICTLEDMGR_H__


#include <objbase.h>

#include "CTLEDCommon.h"

//============================================================================
// Definitions of constant values.
//============================================================================

// The filename of the Creative Interface Library.
const LPCTSTR DEFINITION_FILENAME_CreativeInterfaceLibrary = _T("CTIntrfu.dll");

// The filename of the Creative LED Manager Library.
const LPCTSTR DEFINITION_FILENAME_CreativeLedManagerLibrary = _T("CTLEDMgr.dll");

// The version of the interface.
const DWORD DEFINITION_CTLEDMgr_Interface_Version = 0x00000002;


//============================================================================
// Definitions of CTDEVICECATEGORY_BitwiseMask_xxx bitwise mask.
//============================================================================
const DWORD CTDEVICECATEGORY_BitwiseMask_Keyboard = 0x00000001;
const DWORD CTDEVICECATEGORY_BitwiseMask_Mouse = 0x00000002;
const DWORD CTDEVICECATEGORY_BitwiseMask_Speakers = 0x00000004;
const DWORD CTDEVICECATEGORY_BitwiseMask_InternalAudioDevice = 0x00000008;
const DWORD CTDEVICECATEGORY_BitwiseMask_ExternalAudioDevice = 0x00000010;


//============================================================================
// The LPARAM parameter of ICTLEDMgr::PrepareLedGrouping() for the message CTLEDGROUPINGCMD_xxx.
//============================================================================

// For the message CTLEDGROUPINGCMD_ByOneGlobalLedLedGroup.
typedef struct tagCTLEDGROUPINGCMDPARAM_ByOneGlobalLedGroup
{
	/*IN*/ DWORD dwNumColumnsOfLedGroupingArray2D; // Specify the number of columns of the 2 dimensional array for the LED grouping.
	/*OUT*/ PDWORD pdwLedGroupingArray2D; // Contains the 2 dimensional array for the LED grouping.
	/*OUT*/ DWORD dwNumLedGroupsCreated; // Contains the number of LED groups created. This should be equal to 1 if successful.
} CTLEDGROUPINGCMDPARAM_ByOneGlobalLedGroup, *PCTLEDGROUPINGCMDPARAM_ByOneGlobalLedGroup;

// For the messages CTLEDGROUPINGCMD_ByDesiredNumLedGroups_xAxis, CTLEDGROUPINGCMD_ByDesiredNumLedGroups_yAxis, CTLEDGROUPINGCMD_ByDesiredNumLedGroups_xAxis_Mirror and CTLEDGROUPINGCMD_ByDesiredNumLedGroups_yAxis_Mirror.
typedef struct tagCTLEDGROUPINGCMDPARAM_ByDesiredNumLedGroups_Axis
{
	/*IN*/ DWORD dwTotalNumLeds; // Specify the total number of LEDs. Set it to 0 to denote default.
	/*IN*/ DWORD dwDesiredNumLedGroups; // Specify the desired number of LED groups.
	/*IN*/ BOOL fMakeDesiredNumLedGroupsNotMoreThanMaxAllowable; // TRUE ==> If (dwDesiredNumLedGroups > maximum allowable), then assume dwDesiredNumLedGroups is equal to maximum allowable; FALSE ==> If (dwDesiredNumLedGroups > maximum allowable), then operation FAILED.
	/*IN*/ DWORD dwNumColumnsOfLedGroupingArray2D; // Specify the number of columns of the 2 dimensional array for the LED grouping.
	/*OUT*/ PDWORD pdwLedGroupingArray2D; // Contains the 2 dimensional array for the LED grouping.
	/*OUT*/ DWORD dwNumLedGroupsCreated; // Contains the number of LED groups actually created.
} CTLEDGROUPINGCMDPARAM_ByDesiredNumLedGroups_Axis, *PCTLEDGROUPINGCMDPARAM_ByDesiredNumLedGroups_Axis;

//============================================================================
// The LPARAM parameter of ICTLEDMgr::PrepareLedInfo() for the message CTLEDINFOCMD_xxx.
//============================================================================

// For the message CTLEDINFOCMD_Initialize.
typedef struct tagCTLEDINFOCMDPARAM_Initialize
{
	/*IN*/ DWORD dwMaxNumLedGroups;
	/*IN*/ DWORD dwMaxNumLedsInEachGroup;
	/*IN*/ DWORD dwMaxNumColourLayersInEachGroup;
	/*OUT*/ PCTColourLayerForMultipleLedGroups pLedInfo;
} CTLEDINFOCMDPARAM_Initialize, *PCTLEDINFOCMDPARAM_Initialize;

// For the message CTLEDINFOCMD_FillupAll.
typedef struct tagCTLEDINFOCMDPARAM_FillupAll
{
	/*IN*/ DWORD dwNumLedGroups;
	/*IN*/ CTLED_Pattern *pPatternIndividualLedGroupArray1D;
	/*IN*/ DWORD dwNumColumnsOfLedGroupArray2D;
	/*IN*/ PDWORD pdwLedGroupingArray2D;
	/*IN*/ DWORD dwNumColourLayers;
	/*IN*/ DWORD dwNumColumnsOfColourIndividualLedGroupArray2D;
	/*IN*/ PCTColour pColourIndividualLedGroupArray2D;
	/*OUT*/ PCTColourLayerForMultipleLedGroups pLedInfo;
} CTLEDINFOCMDPARAM_FillupAll, *PCTLEDINFOCMDPARAM_FillupAll;

// For the message CTLEDINFOCMD_FillupNumLedGroups.
typedef struct tagCTLEDINFOCMDPARAM_FillupNumLedGroups
{
	/*IN*/ DWORD dwNumLedGroups;
	/*OUT*/ PCTColourLayerForMultipleLedGroups pLedInfo;
} CTLEDINFOCMDPARAM_FillupNumLedGroups, *PCTLEDINFOCMDPARAM_FillupNumLedGroups;

// For the message CTLEDINFOCMD_FillupGroupLedPattern.
typedef struct tagCTLEDINFOCMDPARAM_FillupGroupLedPattern
{
	/*IN*/ DWORD dwLedGroupIndex;
	/*IN*/ CTLED_Pattern patternLed;
	/*OUT*/ PCTColourLayerForMultipleLedGroups pLedInfo;
} CTLEDINFOCMDPARAM_FillupGroupLedPattern, *PCTLEDINFOCMDPARAM_FillupGroupLedPattern;

// For the message CTLEDINFOCMD_FillupNumLedsInGroup.
typedef struct tagCTLEDINFOCMDPARAM_FillupNumLedsInGroup
{
	/*IN*/ DWORD dwLedGroupIndex;
	/*IN*/ DWORD dwNumLedsInGroup;
	/*OUT*/ PCTColourLayerForMultipleLedGroups pLedInfo;
} CTLEDINFOCMDPARAM_FillupNumLedsInGroup, *PCTLEDINFOCMDPARAM_FillupNumLedsInGroup;

// For the message CTLEDINFOCMD_FillupLedID.
typedef struct tagCTLEDINFOCMDPARAM_FillupLedID
{
	/*IN*/ DWORD dwLedGroupIndex;
	/*IN*/ DWORD dwLedIndex;
	/*IN*/ DWORD dwLedID;
	/*OUT*/ PCTColourLayerForMultipleLedGroups pLedInfo;
} CTLEDINFOCMDPARAM_FillupLedID, *PCTLEDINFOCMDPARAM_FillupLedID;

// For the message CTLEDINFOCMD_FillupNumLedColourLayers.
typedef struct tagCTLEDINFOCMDPARAM_FillupNumLedColourLayers
{
	/*IN*/ DWORD dwLedGroupIndex;
	/*IN*/ DWORD dwNumColourLayers;
	/*OUT*/ PCTColourLayerForMultipleLedGroups pLedInfo;
} CTLEDINFOCMDPARAM_FillupNumLedColourLayers, *PCTLEDINFOCMDPARAM_FillupNumLedColourLayers;

typedef struct tagCTLEDINFOCMDPARAM_FillupLedColour
{
	/*IN*/ DWORD dwLedGroupIndex;
	/*IN*/ DWORD dwColourLayerIndex;
	/*IN*/ CTColour colourLayer;
	/*OUT*/ PCTColourLayerForMultipleLedGroups pLedInfo;
} CTLEDINFOCMDPARAM_FillupLedColour, *PCTLEDINFOCMDPARAM_FillupLedColour;

// For the message CTLEDINFOCMD_Cleanup.
typedef struct tagCTLEDINFOCMDPARAM_Cleanup
{
	/*IN*/ DWORD dwMaxNumLedGroups;
	/*OUT*/ PCTColourLayerForMultipleLedGroups pLedInfo;
} CTLEDINFOCMDPARAM_Cleanup, *PCTLEDINFOCMDPARAM_Cleanup;


//============================================================================
// Definitions of Commands and Notification Messages.
//============================================================================

// The CTLEDMGRCMD parameter of ICTLEDMgr::ExecuteCommand().
typedef enum tagCTLEDMGRCMD
{
	CTLEDMGRCMD_SetLedSettings = 2, // LPARAM is PCTLEDMGRCMDPARAM_SetLedSettings.
	CTLEDMGRCMD_RestoreDefaultLedSettings = 4, // LPARAM is NULL.

	CTLEDMGRCMD_Reserved = 0xFFFFFFFF
} CTLEDMGRCMD;

// The CTLEDMGRNOTIFYPROCMSG parameter of the PFNCTLEDMGRNOTIFYPROC callback function.
// NOTE: Currently, no notification message is being supported yet. In future, notififations messages such as device arrival may be supported.
typedef enum tagCTLEDMGRNOTIFYPROCMSG
{
	CTLEDMGRNOTIFYPROCMSG_Unknown = 0xFFFFFFFF
} CTLEDMGRNOTIFYPROCMSG;

// The CTLEDGROUPINGCMD parameter of ICTLEDMgr::PrepareLedGrouping().
typedef enum tagCTLEDGROUPINGCMD
{
	CTLEDGROUPINGCMD_ByOneGlobalLedGroup = 3, // LPARAM is PCTLEDGROUPINGCMDPARAM_ByOneGlobalLedGroup.
	CTLEDGROUPINGCMD_ByDesiredNumLedGroups_xAxis = 4, // LPARAM is PCTLEDGROUPINGCMDPARAM_ByDesiredNumLedGroups_Axis.
	CTLEDGROUPINGCMD_ByDesiredNumLedGroups_yAxis = 5, // LPARAM is PCTLEDGROUPINGCMDPARAM_ByDesiredNumLedGroups_Axis.
	CTLEDGROUPINGCMD_ByDesiredNumLedGroups_xAxis_Mirror = 6, // LPARAM is PCTLEDGROUPINGCMDPARAM_ByDesiredNumLedGroups_Axis.
	CTLEDGROUPINGCMD_ByDesiredNumLedGroups_yAxis_Mirror = 7, // LPARAM is PCTLEDGROUPINGCMDPARAM_ByDesiredNumLedGroups_Axis.

	CTLEDGROUPINGCMD_Reserved = 0xFFFFFFFF
} CTLEDGROUPINGCMD;

// The CTLEDINFOCMD parameter of ICTLEDMgr::PrepareLedInfo().
typedef enum tagCTLEDINFOCMD
{
	CTLEDINFOCMD_Initialize = 1, // LPARAM is PCTLEDINFOCMDPARAM_Initialize
	CTLEDINFOCMD_FillupAll = 2, // LPARAM is PCTLEDINFOCMDPARAM_FillupAll
	CTLEDINFOCMD_FillupNumLedGroups = 3, // LPARAM is PCTLEDINFOCMDPARAM_FillupNumLedGroups
	CTLEDINFOCMD_FillupGroupLedPattern = 4, // LPARAM is PCTLEDINFOCMDPARAM_FillupGroupLedPattern
	CTLEDINFOCMD_FillupNumLedsInGroup = 5, // LPARAM is PCTLEDINFOCMDPARAM_FillupNumLedsInGroup
	CTLEDINFOCMD_FillupLedID = 6, // LPARAM is PCTLEDINFOCMDPARAM_FillupLedID
	CTLEDINFOCMD_FillupNumLedColourLayers = 7, // LPARAM is PCTLEDINFOCMDPARAM_FillupNumLedColourLayers
	CTLEDINFOCMD_FillupLedColour = 8, // LPARAM is PCTLEDINFOCMDPARAM_FillupLedColour
	CTLEDINFOCMD_Cleanup = 9, // LPARAM is PCTLEDINFOCMDPARAM_Cleanup
	CTLEDINFOCMD_FillupLedColourInReversedDirection = 10, // LPARAM is PCTLEDINFOCMDPARAM_FillupLedColour

	CTLEDINFOCMD_Reserved = 0xFFFFFFFF
} CTLEDINFOCMD;


//============================================================================
// Declaration of notification callback function.
//============================================================================

typedef int(/*CALLBACK*/ __stdcall *PFNCTLEDMGRNOTIFYPROC)(
	/*IN*/ CTLEDMGRNOTIFYPROCMSG msgCTLEDMgrNotifyProc,
	/*IN*/ LPARAM lparamCTLEDMgrNotifyProcData,
	/*IN*/ LPARAM lparamUserData
	);

//============================================================================
// Parameter of timer callback function.
//============================================================================
typedef struct tagCTLEDMGRTIMERPROCDATA
{
	/*IN*/ DWORD dwTimerPeriodicTimeInMilliseconds; // Contains the periodic time in milliseconds of the timer callback that the timer is configured to. However, there is no guarantee that the timer callback will always be triggered precisely at this configured periodic timing.
	/*IN*/ LONGLONG llCurrentTimeTickInMilliseconds; // Contains the current time stamp of the computer system in terms of time ticks in milliseconds.
	/*IN*/ LONGLONG llTotalTimeElapsedInMilliseconds; // Contains the total time that has elapsed in milliseconds since the registered timer callback started. Note: The first timer callback of a newly registered timer callback will always start with llTotalTimeElapsedInMilliseconds equals to zero.
} CTLEDMGRTIMERPROCDATA, *PCTLEDMGRTIMERPROCDATA;

//============================================================================
// Declaration of timer callback function.
//============================================================================
typedef int(/*CALLBACK*/ __stdcall *PFNCTLEDMGRTIMERPROC)(
	/*IN*/ PCTLEDMGRTIMERPROCDATA lparamCTLEDMgrTimerProcData,
	/*IN*/ LPARAM lparamUserData
	);

//============================================================================
// Parameter of ICTLEDMgr::RegisterTimerCallback().
// - dwDueTimeInMilliseconds
//       Specifies the amount of time in milliseconds relative to the current time that must elapse before the timer callback is triggered for the first time.
// - dwPeriodicTimeInMilliseconds
//       On entry, this specifies the desired period of the timer, in milliseconds.
//       On exit, this contains the actual period of the timer used, in milliseconds. This will be equal to or greater than the desired value.
// Remarks:
//     For the best lighting performance, it is recommended to set dwPeriodicTimeInMilliseconds to 1 on entry. 
//     On exit, the method will set dwPeriodicTimeInMilliseconds to the value that is actually used. 
//     For example, if the method sets dwPeriodicTimeInMilliseconds to 5 on exit, then it means that the timer callback function shall be triggerred every 5 milliseconds.
//============================================================================
typedef struct tagCTTIMERINFOPARAM
{
	/*IN*/ DWORD dwDueTimeInMilliseconds;
	/*IN OUT*/ DWORD dwPeriodicTimeInMilliseconds;
} CTTIMERINFOPARAM, *PCTTIMERINFOPARAM;


//============================================================================
// Definitions of CTLED_PatternMode_xxx.
//============================================================================
typedef enum tagCTLED_PatternMode
{
	CTLED_PatternMode_Wave_Default = 0, // Wave continuous flow.
	CTLED_PatternMode_Wave_DiscreteFlow = 1, // Wave discrete flow.

	CTLED_PatternMode_Pulsate_Default = 1000, // Pulsate from low to high.
	CTLED_PatternMode_Pulsate_AlternateIntensityDirection = 1001, // Pulsate from high to low (instead of the default low to high) for odd-numbered LED groups.
	CTLED_PatternMode_Pulsate_IntensityGradient = 1002, // Pulsate with intensity gradient across all the LED groups.

	CTLED_PatternMode_ColourCycle_Default = 2000,

	CTLED_PatternMode_Aurora_Default = 3000,

	CTLED_PatternMode_Static_Default = 4000,

	CTLED_PatternMode_NotApplicable = 0xFFFFFFFF
} CTLED_PatternMode;


//============================================================================
// The LPARAM parameter of ICTLEDMgr::GetColourInfoOfPattern() for the pattern CTLED_Pattern_xxx.
//============================================================================

// For the pattern CTLED_Pattern_Static.
typedef struct tagPCTLEDINFOPARAM_GetStaticColourInfo
{
	/*IN*/ CTLED_PatternMode modePattern; // CTLED_PatternMode_Static_xxx
	/*IN*/ CTLEDGROUPINGCMD cmdLedGrouping; // CTLEDGROUPINGCMD_xxx
	/*IN*/ DWORD dwLedGroupIndex;
	/*IN*/ DWORD dwTotalNumLedGroups;
	/*IN*/ DWORD dwPatternPeriodicTimeInMilliseconds; // Reserved for future use. For now, always set this to 0.
	/*IN*/ LONGLONG llPatternTimeElapsedInMilliseconds;
	/*IN*/ DWORD dwNumLedColourLayers; // Set this to the numer of colour layers in pCTColourOriginalArray.
	/*IN*/ PCTColour pCTColourOriginalArray;
	/*OUT*/ CTColour colourCurrent;
} CTLEDINFOPARAM_GetStaticColourInfo, *PCTLEDINFOPARAM_GetStaticColourInfo;

// For the pattern CTLED_Pattern_Pulsate.
typedef struct tagPCTLEDINFOPARAM_GetPulsateColourInfo
{
	/*IN*/ CTLED_PatternMode modePattern; // CTLED_PatternMode_Pulsate_xxx
	/*IN*/ CTLEDGROUPINGCMD cmdLedGrouping; // CTLEDGROUPINGCMD_xxx
	/*IN*/ DWORD dwLedGroupIndex;
	/*IN*/ DWORD dwTotalNumLedGroups;
	/*IN*/ DWORD dwPatternPeriodicTimeInMilliseconds;
	/*IN*/ LONGLONG llPatternTimeElapsedInMilliseconds;
	/*IN*/ DWORD dwNumLedColourLayers; // Set this to the numer of colour layers in pCTColourOriginalArray.
	/*IN*/ PCTColour pCTColourOriginalArray;
	/*OUT*/ CTColour colourCurrent;
} CTLEDINFOPARAM_GetPulsateColourInfo, *PCTLEDINFOPARAM_GetPulsateColourInfo;

// For the pattern CTLED_Pattern_ColourCycle.
typedef struct tagPCTLEDINFOPARAM_GetColourCycleColourInfo
{
	/*IN*/ CTLED_PatternMode modePattern; // CTLED_PatternMode_ColourCycle_xxx
	/*IN*/ CTLEDGROUPINGCMD cmdLedGrouping; // CTLEDGROUPINGCMD_xxx
	/*IN*/ DWORD dwLedGroupIndex;
	/*IN*/ DWORD dwTotalNumLedGroups;
	/*IN*/ DWORD dwPatternPeriodicTimeInMilliseconds;
	/*IN*/ LONGLONG llPatternTimeElapsedInMilliseconds;
	/*IN*/ DWORD dwNumLedColourLayers; // Reserved for future use. For now, always set this to 0.
	/*IN*/ PCTColour pCTColourOriginalArray; // Reserved for future use. For now, always set this to NULL.
	/*OUT*/ CTColour colourCurrent;
} CTLEDINFOPARAM_GetColourCycleColourInfo, *PCTLEDINFOPARAM_GetColourCycleColourInfo;

// For the pattern CTLED_Pattern_Aurora.
typedef struct tagPCTLEDINFOPARAM_GetAuroraColourInfo
{
	/*IN*/ CTLED_PatternMode modePattern; // CTLED_PatternMode_Aurora_xxx
	/*IN*/ CTLEDGROUPINGCMD cmdLedGrouping; // CTLEDGROUPINGCMD_xxx
	/*IN*/ DWORD dwLedGroupIndex;
	/*IN*/ DWORD dwTotalNumLedGroups;
	/*IN*/ DWORD dwPatternPeriodicTimeInMilliseconds;
	/*IN*/ LONGLONG llPatternTimeElapsedInMilliseconds;
	/*IN*/ DWORD dwNumLedColourLayers; // Reserved for future use. For now, always set this to 0.
	/*IN*/ PCTColour pCTColourOriginalArray; // Reserved for future use. For now, always set this to NULL.
	/*OUT*/ CTColour colourCurrent;
} CTLEDINFOPARAM_GetAuroraColourInfo, *PCTLEDINFOPARAM_GetAuroraColourInfo;

// For the pattern CTLED_Pattern_Wave.
typedef struct tagPCTLEDINFOPARAM_GetWaveColourInfo
{
	/*IN*/ CTLED_PatternMode modePattern; // CTLED_PatternMode_Wave_xxx
	/*IN*/ CTLEDGROUPINGCMD cmdLedGrouping; // CTLEDGROUPINGCMD_xxx
	/*IN*/ DWORD dwLedGroupIndex;
	/*IN*/ DWORD dwTotalNumLedGroups;
	/*IN*/ DWORD dwPatternPeriodicTimeInMilliseconds;
	/*IN*/ LONGLONG llPatternTimeElapsedInMilliseconds;
	/*IN*/ DWORD dwNumLedColourLayers; // Set this to the numer of colour layers in pCTColourOriginalArray.
	/*IN*/ PCTColour pCTColourOriginalArray;
	/*OUT*/ CTColour colourCurrent;
} CTLEDINFOPARAM_GetWaveColourInfo, *PCTLEDINFOPARAM_GetWaveColourInfo;


//============================================================================
// Purpose:
//     This declares the ICTLEDMgr interface.
// Member description:
//     ExecuteCommand
//         This function does this and does that.
// Remarks:
//============================================================================
interface ICTLEDMgr : IUnknown
{
	virtual HRESULT __stdcall Initialize(IN DWORD dwInterfaceVersion, IN DWORD dwFlag) = 0;
	virtual HRESULT __stdcall RegisterNotificationCallback(IN PFNCTLEDMGRNOTIFYPROC pfnCTLEDMgrNotifyProc, IN LPARAM lparamUserData, IN DWORD dwFlag) = 0;
	virtual HRESULT __stdcall Open(IN USHORT usVendorID, IN USHORT usProductID, IN LPCWSTR lpcwszSerialNumber, IN LPCWSTR lpcwszDeviceInstance, IN USHORT usLedInfoFlag, IN USHORT usTotalNumLeds, IN BOOL fPhysicalLedOrderingIsReversed, OUT PDWORD pdwDetailErrorCode, IN DWORD dwFlag) = 0;
	virtual HRESULT __stdcall PrepareLedGrouping(IN CTLEDGROUPINGCMD cmd, IN OUT LPARAM lparam, IN DWORD dwFlag) = 0;
	virtual HRESULT __stdcall PrepareLedInfo(IN CTLEDINFOCMD cmd, IN OUT LPARAM lparam, IN DWORD dwFlag) = 0;
	virtual HRESULT __stdcall ExecuteCommand(IN CTLEDMGRCMD cmd, IN OUT LPARAM lparam, IN DWORD dwFlag) = 0;
	virtual HRESULT __stdcall Close(IN DWORD dwFlag) = 0;
	virtual HRESULT __stdcall UnregisterNotificationCallback(IN DWORD dwFlag) = 0;
	virtual HRESULT __stdcall Shutdown(IN DWORD dwFlag) = 0;
	virtual HRESULT __stdcall EnumSupportedDevices(IN DWORD dwEnumIndex, OUT PUSHORT pusVendorID, OUT PUSHORT pusProductID, OUT LPWSTR wszDeviceFriendlyNameBuf, IN OUT PDWORD pdwSizeOfDeviceFriendlyNameBuf, OUT PDWORD pdwDeviceCategoryBitwiseMask, OUT PDWORD pdwReserved1, OUT PDWORD pdwReserved2, OUT PDWORD pdwReserved3, IN DWORD dwFlag) = 0;
	virtual HRESULT __stdcall EnumConnectedDevices(IN DWORD dwEnumIndex, OUT PUSHORT pusVendorID, OUT PUSHORT pusProductID, OUT LPWSTR wszSerialNumberBuf, IN OUT PDWORD pdwSizeOfSerialNumberBuf, OUT LPWSTR wszDeviceInstanceBuf, IN OUT PDWORD pdwSizeOfDeviceInstanceBuf, OUT PUSHORT pusLedInfoFlag, OUT PUSHORT pusTotalNumLeds, OUT LPWSTR wszDeviceFriendlyNameBuf, IN OUT PDWORD pdwSizeOfDeviceFriendlyNameBuf, IN DWORD dwFlag) = 0;
	virtual HRESULT __stdcall RegisterTimerCallback(IN PFNCTLEDMGRTIMERPROC pfnCTLEDMgrTimerProc, IN OUT PCTTIMERINFOPARAM pCTTimerInfoParam, IN LPARAM lparamUserData, IN DWORD dwFlag) = 0;
	virtual HRESULT __stdcall UnregisterTimerCallback(IN DWORD dwFlag) = 0;
	virtual HRESULT __stdcall GetColourInfoOfPattern(IN CTLED_Pattern patternLed, IN OUT LPARAM lparam, IN DWORD dwFlag) = 0;
};

//
// Declaration of GUIDs for interfaces.
//
// {1185BF2F-BEF2-4477-8B6F-A40D529D22EB}
DEFINE_GUID(IID_ICTLEDMgr,
	0x1185bf2f, 0xbef2, 0x4477, 0x8b, 0x6f, 0xa4, 0xd, 0x52, 0x9d, 0x22, 0xeb);


#endif // #ifndef __ICTLEDMGR_H__