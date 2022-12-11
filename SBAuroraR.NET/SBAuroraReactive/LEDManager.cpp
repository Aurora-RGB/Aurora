#include "LEDManager.h"

SBAuroraReactive::LEDManager::LEDManager()
{
	ICTLEDMgr* outMgr = nullptr;
	const auto creationResult = CreateLEDManager(&outMgr);
	if (creationResult == CTINTRFCRESULT_Success)
		m_Instance = outMgr;
	else
		throw gcnew System::Exception("Error creating ICTLEDMgr");
}

array<SBAuroraReactive::EnumeratedDevice>^ SBAuroraReactive::LEDManager::EnumConnectedDevices()
{
	auto tempList = gcnew System::Collections::Generic::List<EnumeratedDevice>();

	msclr::lock l(m_enumLock);
	{
		DWORD currIdx = 0;
		for (;;)
		{
			USHORT usVendorID = 0;
			USHORT usProductID = 0;
			wchar_t serialNoBuf[256] ={ 0 };
			wchar_t deviceInstanceBuf[256] ={ 0 };
			DWORD sizeOfSerialNoBuf = (DWORD)array_countof(serialNoBuf)-1;
			DWORD sizeOfDeviceInstanceBuf = (DWORD)array_countof(deviceInstanceBuf)-1;
			USHORT usLedInfoFlag = 0;
			USHORT usTotalNumLeds = 0;
			wchar_t deviceFriendlyNameBuf[256] ={ 0 };
			DWORD sizeOfDeviceFriendlyNameBuf = (DWORD)array_countof(deviceFriendlyNameBuf)-1;
			DWORD dwFlag = 0;
			HRESULT hr = m_Instance->EnumConnectedDevices(
				currIdx, &usVendorID, &usProductID,
				serialNoBuf, &sizeOfSerialNoBuf,
				deviceInstanceBuf, &sizeOfDeviceInstanceBuf,
				&usLedInfoFlag, &usTotalNumLeds,
				deviceFriendlyNameBuf, &sizeOfDeviceFriendlyNameBuf, dwFlag);

			if (currIdx == 0xFFFFFFFF)
				break;

			if (!SUCCEEDED(hr))
			{
				currIdx = 0xFFFFFFFF;
				continue;
			}
			else
			{
				EnumeratedDevice dev;
				dev.deviceId.vid = usVendorID;
				dev.deviceId.pid = usProductID;
				dev.ledInfoFlag = usLedInfoFlag;
				dev.totalNumLeds = usTotalNumLeds;
				dev.serialNo = gcnew String(serialNoBuf);
				dev.deviceInstance = gcnew String(deviceInstanceBuf);
				dev.friendlyName = gcnew String(deviceFriendlyNameBuf);
				tempList->Add(dev);
				currIdx++;
			}
		}
	}	

	return tempList->ToArray();
}

u32 SBAuroraReactive::LEDManager::OpenDevice(SBAuroraReactive::EnumeratedDevice devInfo, bool reverseLeds)
{
	DWORD detailedErrNo = 0;
	{
		pin_ptr<const wchar_t> serialNoPtr = PtrToStringChars(devInfo.serialNo);
		pin_ptr<const wchar_t> devInstPtr = PtrToStringChars(devInfo.deviceInstance);

		DWORD dwFlag = 0;
		HRESULT hr = m_Instance->Open(devInfo.deviceId.vid, devInfo.deviceId.pid, serialNoPtr, devInstPtr, devInfo.ledInfoFlag, devInfo.totalNumLeds, reverseLeds, &detailedErrNo, dwFlag);
		if (hr != S_FALSE && !SUCCEEDED(hr))
			throw gcnew System::Runtime::InteropServices::ExternalException("Error opening ICTLEDMgr device", hr);
	}
	return detailedErrNo;
}

void SBAuroraReactive::LEDManager::CloseDevice()
{
	DWORD dwFlag = 0;
	HRESULT hr = m_Instance->Close(dwFlag);
	if (!SUCCEEDED(hr))
		throw gcnew System::Runtime::InteropServices::ExternalException("Error closing ICTLEDMgr device", hr);
}

static int __stdcall TimerCallbackHandler(PCTLEDMGRTIMERPROCDATA timerStatePtr, LPARAM lparamUserData)
{
	using System::Runtime::InteropServices::GCHandle;
	GCHandle h = GCHandle::FromIntPtr(IntPtr(lparamUserData));
	int funcRes = 0;
	{
		auto delegateFunc = dynamic_cast<SBAuroraReactive::LEDManager::PeriodicTimerDelegate^>(h.Target);
		if (delegateFunc != nullptr)
			funcRes = delegateFunc->Invoke(timerStatePtr->dwTimerPeriodicTimeInMilliseconds, timerStatePtr->llCurrentTimeTickInMilliseconds, timerStatePtr->llTotalTimeElapsedInMilliseconds);
		delegateFunc = nullptr;
	}
	//h.Free();

	return funcRes;
}

SBAuroraReactive::TimerSettings SBAuroraReactive::LEDManager::RegisterTimerCallback(PeriodicTimerDelegate^ timerFunc, TimerSettings timerSettings)
{
	CTTIMERINFOPARAM param;
	param.dwDueTimeInMilliseconds = timerSettings.dueTimeMs;
	param.dwPeriodicTimeInMilliseconds = timerSettings.periodicTimeDesiredMs;

	DWORD dwFlag = 0;
	using System::Runtime::InteropServices::GCHandle;
	HRESULT hr = m_Instance->RegisterTimerCallback(&TimerCallbackHandler, &param, (LPARAM)GCHandle::ToIntPtr(GCHandle::Alloc(timerFunc)).ToPointer(), dwFlag);
	if (!SUCCEEDED(hr))
		throw gcnew System::Runtime::InteropServices::ExternalException("Error registering ICTLEDMgr timer callback", hr);

	timerSettings.dueTimeMs = param.dwDueTimeInMilliseconds;
	timerSettings.periodicTimeDesiredMs = param.dwPeriodicTimeInMilliseconds;

	return timerSettings;
}

void SBAuroraReactive::LEDManager::UnregisterTimerCallback()
{
	DWORD dwFlag = 0;
	HRESULT hr = m_Instance->UnregisterTimerCallback(dwFlag);
	if (!SUCCEEDED(hr))
		throw gcnew System::Runtime::InteropServices::ExternalException("Error unregistering ICTLEDMgr timer callback", hr);
}

u32 SBAuroraReactive::LEDManager::PrepareLedGrouping(LedGroupingType grpType, u32 totalNumLeds, u32 desiredNumGroups, u32 ledGroupingStride, bool capByNumTotalLeds, array<u32>^ groupingArr)
{
	u32 returnVal = 0;
	DWORD dwFlag = 0;
	HRESULT hr = S_OK;

	pin_ptr<u32> arrayPtr = &groupingArr[0];
	if (grpType == LedGroupingType::ByOneGlobalLedGroup)
	{
		CTLEDGROUPINGCMDPARAM_ByOneGlobalLedGroup param;
		param.dwNumColumnsOfLedGroupingArray2D = ledGroupingStride;
		param.dwNumLedGroupsCreated = 0;
		param.pdwLedGroupingArray2D = (DWORD*)arrayPtr;

		hr = m_Instance->PrepareLedGrouping(CTLEDGROUPINGCMD::CTLEDGROUPINGCMD_ByOneGlobalLedGroup, (LPARAM)&param, dwFlag);
		returnVal = param.dwNumLedGroupsCreated;
	}
	else if (grpType != LedGroupingType::Reserved)
	{
		CTLEDGROUPINGCMDPARAM_ByDesiredNumLedGroups_Axis param;
		param.dwTotalNumLeds = totalNumLeds;
		param.dwDesiredNumLedGroups = desiredNumGroups;
		param.fMakeDesiredNumLedGroupsNotMoreThanMaxAllowable = capByNumTotalLeds;
		param.dwNumColumnsOfLedGroupingArray2D = ledGroupingStride;
		param.dwNumLedGroupsCreated = 0;
		param.pdwLedGroupingArray2D = (DWORD*)arrayPtr;
		hr = m_Instance->PrepareLedGrouping((CTLEDGROUPINGCMD)grpType, (LPARAM)&param, dwFlag);
		returnVal = param.dwNumLedGroupsCreated;
	}

	if (!SUCCEEDED(hr))
		throw gcnew System::Runtime::InteropServices::ExternalException("Error in ICTLEDMgr::PrepareLedGrouping", hr);

	return returnVal;
}

template<typename StructType>
static inline SBAuroraReactive::LedPayloadData RunPrepareLedInfo(ICTLEDMgr* ledManager, CTLEDINFOCMD cmd, StructType* paramStruct)
{
	DWORD dwFlag = 0;
	HRESULT hr = ledManager->PrepareLedInfo(cmd, (LPARAM)paramStruct, dwFlag);
	if (!SUCCEEDED(hr))
		throw gcnew System::Runtime::InteropServices::ExternalException("Error in ICTLEDMgr::PrepareLedInfo", hr);

	SBAuroraReactive::LedPayloadData outPayload;
	outPayload.opaqueSize = paramStruct->pLedInfo->dwNumLedGroups;
	outPayload.opaqueData = (uintptr_t)paramStruct->pLedInfo->hColourLayerForLedGroupArray;
	return outPayload;
}

SBAuroraReactive::LedPayloadData SBAuroraReactive::LEDManager::LedPayloadInitialize(LedPayloadData srcPayload, u32 maxNumLedGroups, u32 maxNumLedsInEachGroup, u32 maxNumColourLayersInEachGroup)
{
	CTLEDINFOCMDPARAM_Initialize param;
	param.dwMaxNumLedGroups = maxNumLedGroups;
	param.dwMaxNumLedsInEachGroup = maxNumLedsInEachGroup;
	param.dwMaxNumColourLayersInEachGroup = maxNumColourLayersInEachGroup;
	param.pLedInfo = (PCTColourLayerForMultipleLedGroups)&srcPayload;

	return RunPrepareLedInfo(m_Instance, CTLEDINFOCMD_Initialize, &param);
}

SBAuroraReactive::LedPayloadData SBAuroraReactive::LEDManager::LedPayloadFillupAll(
	LedPayloadData srcPayload, 
	u32 numLedGroups, array<LedPattern>^ patternArr, 
	u32 ledGroupingStride, array<u32>^ groupingArr, 
	u32 numColourLayers, u32 colourLayersStride, array<LedColour>^ ledColoursArr)
{
	pin_ptr<LedPattern> patternArrPinned = &patternArr[0];
	pin_ptr<u32> groupingArrPinned = &groupingArr[0];
	pin_ptr<LedColour> coloursArrPinned = &ledColoursArr[0];

	CTLEDINFOCMDPARAM_FillupAll param;
	param.dwNumLedGroups = numLedGroups;
	param.pPatternIndividualLedGroupArray1D = (CTLED_Pattern*)patternArrPinned;
	param.dwNumColumnsOfLedGroupArray2D = ledGroupingStride;
	param.pdwLedGroupingArray2D = (DWORD*)groupingArrPinned;
	param.dwNumColourLayers = numColourLayers;
	param.dwNumColumnsOfColourIndividualLedGroupArray2D = colourLayersStride;
	param.pColourIndividualLedGroupArray2D = (PCTColour)coloursArrPinned;
	param.pLedInfo = (PCTColourLayerForMultipleLedGroups)&srcPayload;

	return RunPrepareLedInfo(m_Instance, CTLEDINFOCMD_FillupAll, &param);
}

SBAuroraReactive::LedPayloadData SBAuroraReactive::LEDManager::LedPayloadFillupNumLedGroups(LedPayloadData srcPayload, u32 numLedGroups)
{
	CTLEDINFOCMDPARAM_FillupNumLedGroups param;
	param.dwNumLedGroups = numLedGroups;
	param.pLedInfo = (PCTColourLayerForMultipleLedGroups)&srcPayload;

	return RunPrepareLedInfo(m_Instance, CTLEDINFOCMD_FillupNumLedGroups, &param);
}

SBAuroraReactive::LedPayloadData SBAuroraReactive::LEDManager::LedPayloadFillupGroupLedPattern(LedPayloadData srcPayload, u32 ledGroupIndex, LedPattern patternType)
{
	CTLEDINFOCMDPARAM_FillupGroupLedPattern param;
	param.dwLedGroupIndex = ledGroupIndex;
	param.patternLed = (CTLED_Pattern)patternType;
	param.pLedInfo = (PCTColourLayerForMultipleLedGroups)&srcPayload;

	return RunPrepareLedInfo(m_Instance, CTLEDINFOCMD_FillupGroupLedPattern, &param);
}

SBAuroraReactive::LedPayloadData SBAuroraReactive::LEDManager::LedPayloadFillupNumLedsInGroup(LedPayloadData srcPayload, u32 ledGroupIndex, u32 numLedsInGroup)
{
	CTLEDINFOCMDPARAM_FillupNumLedsInGroup param;
	param.dwLedGroupIndex = ledGroupIndex;
	param.dwNumLedsInGroup = numLedsInGroup;
	param.pLedInfo = (PCTColourLayerForMultipleLedGroups)&srcPayload;

	return RunPrepareLedInfo(m_Instance, CTLEDINFOCMD_FillupNumLedsInGroup, &param);
}

SBAuroraReactive::LedPayloadData SBAuroraReactive::LEDManager::LedPayloadFillupLedID(LedPayloadData srcPayload, u32 ledGroupIndex, u32 ledIndex, unsigned int ledID)
{
	CTLEDINFOCMDPARAM_FillupLedID param;
	param.dwLedGroupIndex = ledGroupIndex;
	param.dwLedIndex = ledIndex;
	param.dwLedID = ledID;
	param.pLedInfo = (PCTColourLayerForMultipleLedGroups)&srcPayload;

	return RunPrepareLedInfo(m_Instance, CTLEDINFOCMD_FillupLedID, &param);
}

SBAuroraReactive::LedPayloadData SBAuroraReactive::LEDManager::LedPayloadFillupNumLedColourLayers(LedPayloadData srcPayload, u32 ledGroupIndex, u32 numColourLayers)
{
	CTLEDINFOCMDPARAM_FillupNumLedColourLayers param;
	param.dwLedGroupIndex = ledGroupIndex;
	param.dwNumColourLayers = numColourLayers;
	param.pLedInfo = (PCTColourLayerForMultipleLedGroups)&srcPayload;

	return RunPrepareLedInfo(m_Instance, CTLEDINFOCMD_FillupNumLedColourLayers, &param);
}

SBAuroraReactive::LedPayloadData SBAuroraReactive::LEDManager::LedPayloadFillupLedColour(LedPayloadData srcPayload, u32 ledGroupIndex, u32 colourLayerIndex, LedColour colourLayer, bool reversedDirection)
{
	CTLEDINFOCMDPARAM_FillupLedColour param;
	param.dwLedGroupIndex = ledGroupIndex;
	param.dwColourLayerIndex = colourLayerIndex;
	param.colourLayer.bRed = colourLayer.r;
	param.colourLayer.bGreen = colourLayer.g;
	param.colourLayer.bBlue = colourLayer.b;
	param.colourLayer.bAlpha = colourLayer.a;	
	param.pLedInfo = (PCTColourLayerForMultipleLedGroups)&srcPayload;

	return RunPrepareLedInfo(m_Instance, reversedDirection ? CTLEDINFOCMD_FillupLedColourInReversedDirection : CTLEDINFOCMD_FillupLedColour, &param);
}

SBAuroraReactive::LedPayloadData SBAuroraReactive::LEDManager::LedPayloadCleanup(LedPayloadData srcPayload, u32 maxNumLedGroups)
{
	CTLEDINFOCMDPARAM_Cleanup param;
	param.dwMaxNumLedGroups = maxNumLedGroups;
	param.pLedInfo = (PCTColourLayerForMultipleLedGroups)&srcPayload;

	return RunPrepareLedInfo(m_Instance, CTLEDINFOCMD_Cleanup, &param);
}

void SBAuroraReactive::LEDManager::SetLedSettings(LedSettings^ settingsData)
{
	DWORD dwFlag = 0;
	HRESULT hr = S_OK;
	if (settingsData == nullptr)
		hr = m_Instance->ExecuteCommand(CTLEDMGRCMD_RestoreDefaultLedSettings, NULL, dwFlag);
	else
	{
		CTLEDMGRCMDPARAM_SetLedSettings param;
		param.dwFlagForApplySettings = settingsData->applySettingsFlag;
		if (settingsData->profileId.HasValue)
		{
			param.fIgnoreProfileID = FALSE;
			param.dwProfileIDMain = settingsData->profileId.Value.main;
			param.dwProfileIDSub = settingsData->profileId.Value.sub;
		}
		else
		{
			param.fIgnoreProfileID = TRUE;
			param.dwProfileIDMain = CustomProfileID.main;
			param.dwProfileIDSub = CustomProfileID.sub;
		}
		param.fPersistent = settingsData->persistentInDevice;
		if (settingsData->masterLedState.HasValue)
		{
			param.fIgnoreMasterLedState = FALSE;
			param.dwMasterLedState = (u32)settingsData->masterLedState.Value;
		}
		else
		{
			param.fIgnoreMasterLedState = TRUE;
			param.dwMasterLedState = DEFINITION_CTLEDMGR_LedMaster_On;
		}
		if (settingsData->globalPatternMode.HasValue)
		{
			param.fIgnoreGlobalPatternMode = FALSE;
			param.fGlobalPatternMode = settingsData->globalPatternMode.Value;
		}
		else
		{
			param.fIgnoreGlobalPatternMode = TRUE;
			param.fGlobalPatternMode = FALSE;
		}
		param.dwLedIndex = settingsData->ledIndex;
		if (settingsData->pattern.HasValue)
		{
			param.fIgnorePattern = FALSE;
			param.patternLed = (CTLED_Pattern)settingsData->pattern.Value;
		}
		else
		{
			param.fIgnorePattern = TRUE;
			param.patternLed = CTLED_Pattern_Unknown;
		}
		if (settingsData->patternDirection.HasValue)
		{
			param.fIgnorePatternDirection = FALSE;
			param.directionLedPattern = (CTLED_PatternDirection)settingsData->patternDirection.Value;
		}
		else
		{
			param.fIgnorePatternDirection = TRUE;
			param.directionLedPattern = CTLED_PatternDirection_Unknown;
		}
		if (settingsData->patternDirectionFlag.HasValue)
		{
			param.fIgnorePatternDirectionFlag = FALSE;
			param.flagLedPatternDirection = (CTLED_PatternDirectionFlag)settingsData->patternDirectionFlag.Value;
		}
		else
		{
			param.fIgnorePatternDirectionFlag = TRUE;
			param.flagLedPatternDirection = CTLED_PatternDirectionFlag_Unknown;
		}
		if (settingsData->periodicTime.HasValue)
		{
			param.fIgnorePeriodicTime = FALSE;
			param.patternLedThePeriodicTimeIsFor = (CTLED_Pattern)settingsData->periodicTime.Value.pattern;
			param.directionLedPatternThePeriodicTimeIsFor = (CTLED_PatternDirection)settingsData->periodicTime.Value.direction;
			param.flagLedPatternDirectionThePeriodicTimeIsFor = (CTLED_PatternDirectionFlag)settingsData->periodicTime.Value.directionFlag;
			param.dwPeriodicTimeInMilliseconds = settingsData->periodicTime.Value.periodicTimeInMs;
			param.dwPeriodicTimeInCyclesPerMinute = settingsData->periodicTime.Value.periodicTimeInCyclesPerMinute;
		}
		else
		{
			param.fIgnorePeriodicTime = TRUE;
			param.patternLedThePeriodicTimeIsFor = CTLED_Pattern_Unknown;
			param.directionLedPatternThePeriodicTimeIsFor = CTLED_PatternDirection_NotApplicable;
			param.flagLedPatternDirectionThePeriodicTimeIsFor = CTLED_PatternDirectionFlag_NotApplicable;
			param.dwPeriodicTimeInMilliseconds = 0;
			param.dwPeriodicTimeInCyclesPerMinute = 0;
		}
		if (settingsData->payloadData.HasValue)
		{
			param.fIgnoreColour = FALSE;
			param.colourLed.dwNumLedGroups = settingsData->payloadData.Value.opaqueSize;
			param.colourLed.hColourLayerForLedGroupArray = (HANDLE)settingsData->payloadData.Value.opaqueData;
		}
		else
		{
			param.fIgnoreColour = TRUE;
			param.colourLed.dwNumLedGroups = 0;
			param.colourLed.hColourLayerForLedGroupArray = NULL;
		}

		hr = m_Instance->ExecuteCommand(CTLEDMGRCMD_SetLedSettings, (LPARAM)&param, dwFlag);
	}
	
	if (!SUCCEEDED(hr))
		throw gcnew System::Runtime::InteropServices::ExternalException("Error in ICTLEDMgr::SetLedSettings", hr);
}
