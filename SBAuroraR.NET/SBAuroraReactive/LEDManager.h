#pragma once

#include "DllMain.h"
#include "Types.h"
#include "LEDIndex.h"
#include <msclr/lock.h>

using namespace System;

namespace SBAuroraReactive 
{
	public value struct DeviceId
	{
		u16 vid, pid;
	};

	public value struct EnumeratedDevice
	{
		DeviceId deviceId;
		u16 ledInfoFlag, totalNumLeds;
		String^ serialNo;
		String^ deviceInstance;
		String^ friendlyName;

		static const DeviceId SoundBlasterXVanguardK08_USEnglish = { 0x041E, 0x3126 };
		static const DeviceId SoundBlasterXVanguardK08_German = { 0x041E, 0x3128 };
		static const DeviceId SoundBlasterXVanguardK08_Nordic = { 0x041E, 0x3129 };

		static const DeviceId SoundBlasterXSiegeM04 = { 0x041E, 0x3127 };

		static const DeviceId SoundBlasterXKatana = { 0x041E, 0x3247 };
		static const DeviceId SoundBlasterXKratosS5 = { 0x041E, 0x30CA };
		static const DeviceId SoundBlasterXAE5 = { 0x1102, 0x0051 };
	};

	public value struct TimerSettings
	{
		u32 dueTimeMs;
		u32 periodicTimeDesiredMs;
	};

	public value struct LedColour
	{
		u8 r;
		u8 g;
		u8 b;
		u8 a;
	};

	public enum class LedPattern : unsigned int
	{
		None = 0,
		ColourCycle = 1,
		SpectrumAnalyzer = 2, // Reserved for future.
		Static = 3,
		Wave = 4,
		Pulsate = 5,
		Blink = 6, // Reserved for future.
		Morph = 7, // Reserved for future.
		Aurora = 8,
		Cosmic = 1000,  // Reserved for future.
		WaveCycleBounce = 1001,  // Reserved for future.
		Impact = 1002,  // Reserved for future.
		FollowGlobal = 0xFFFFFFFE,  // Reserved for future.
		Unknown = 0xFFFFFFFF
	};

	public enum class LedPatternDirection : unsigned int
	{
		LeftToRight = 1,
		RightToLeft = 2,
		TopToBottom = 3,
		BottomToTop = 4,
		Clockwise = 5,
		Anticlockwise = 6,
		NotApplicable = 0xFFFFFFFF
	};

	public enum class LedPatternDirectionFlag : unsigned int
	{
		Looping = 0,
		Bouncing = 1,
		NotApplicable = 0xFFFFFFFF
	};

	public value struct LedPayloadData
	{
		u32			opaqueSize;
		uintptr_t	opaqueData;
	};

	public value struct ProfileID
	{
		u32 main, sub;
	};
	static const ProfileID CustomProfileID = { 126, 63 };

	public enum class MasterLedState : unsigned char
	{
		Off = 0x00,
		On = 0x01
	};

	public value struct PatternTiming
	{
		LedPattern pattern; // Pattern we are setting the timing for
		LedPatternDirection direction; // Reserved for future use. For now, always set this to LedPatternDirection::NotApplicable.
		LedPatternDirectionFlag directionFlag; // Reserved for future use. For now, always set this to LedPatternDirectionFlag::NotApplicable.
		u32 periodicTimeInMs; // The periodic time of the LED pattern in milliseconds. If this is zero, then periodicTimeInCyclesPerMinute will be used.
		u32 periodicTimeInCyclesPerMinute; // The periodic time of the LED pattern in cycles per minute. If this is zero, then periodicTimeInMs will be used.
	};

	public ref struct LedSettings
	{
		u32 applySettingsFlag = 0x03; //Flag_SetMultipleAndApply

		Nullable<ProfileID> profileId;
		bool persistentInDevice = false;

		Nullable<MasterLedState> masterLedState;
		Nullable<bool> globalPatternMode;

		u32 ledIndex = (u32)Mouse_LEDIndex::NotApplicable;
		Nullable<LedPattern> pattern;
		Nullable<LedPatternDirection> patternDirection;
		Nullable<LedPatternDirectionFlag> patternDirectionFlag;
		Nullable<PatternTiming> periodicTime;
		Nullable<LedPayloadData> payloadData;
	};

	public ref class LEDManager
	{
	public:
		LEDManager();
		virtual ~LEDManager() { invalidate(); }
	protected:
		!LEDManager() { invalidate(); }

	public:
		array<EnumeratedDevice>^ EnumConnectedDevices();
		
		u32 OpenDevice(EnumeratedDevice devInfo, bool reverseLeds);
		void CloseDevice();

		delegate int PeriodicTimerDelegate(u32 timerPeriodMs, u64 currentTimeTickMs, u64 totalTimeElapsedMs);

		TimerSettings RegisterTimerCallback(PeriodicTimerDelegate^ timerFunc, TimerSettings timerSettings);
		void UnregisterTimerCallback();

		// Commonly used definitons for LED groupings and colour layers.
		static const int MaxNumLedGroups_SufficientlyLarge = 128; // Maximum number of LED groups.
		static const int MaxNumLedsPerLedGroup_SufficientlyLarge = 128; // Maximum number of LEDs per LED group.
		static const int MaxNumColourLayersPerLedGroup_SufficientlyLarge = 21; // Maximum number of colour layers for each of the LED group.

		static const int MaxNumRows_LedGroupingArray = MaxNumLedGroups_SufficientlyLarge; // Maximum number of rows of the 2 dimensional array for the LED grouping.
		static const int MaxNumColumns_LedGroupingArray = MaxNumLedsPerLedGroup_SufficientlyLarge + 1; // Maximum number of columns of the 2 dimensional array for the LED grouping. Note: There is a "plus one" for the number of columns of the 2 dimensional array because column 0 of each row specifies the number of LEDs in the row.
		static const int MaxNumPatterns_PatternArray = MaxNumRows_LedGroupingArray; // Maximum number of patterns of the 1 dimensional array for the pattern.
		static const int MaxNumRows_ColourLayerArray = MaxNumRows_LedGroupingArray; // Maximum number of rows of the 2 dimensional array for the colour layer.
		static const int MaxNumColumns_ColourLayerArray = MaxNumColourLayersPerLedGroup_SufficientlyLarge; // Maximum number of columns of the 2 dimensional array for the colour layer.

		enum class LedGroupingType : unsigned int
		{
			ByOneGlobalLedGroup = 3,
			ByDesiredNumLedGroups_xAxis = 4,
			ByDesiredNumLedGroups_yAxis = 5,
			ByDesiredNumLedGroups_xAxis_Mirror = 6,
			ByDesiredNumLedGroups_yAxis_Mirror = 7,

			Reserved = 0xFFFFFFFF
		};
		u32 PrepareLedGrouping(LedGroupingType grpType, u32 totalNumLeds, u32 desiredNumGroups, u32 ledGroupingStride, bool capByNumTotalLeds, array<u32>^ groupingArr);

		LedPayloadData LedPayloadInitialize(LedPayloadData srcPayload, u32 maxNumLedGroups, u32 maxNumLedsInEachGroup, u32 maxNumColourLayersInEachGroup);
		LedPayloadData LedPayloadFillupAll(LedPayloadData srcPayload,
			u32 numLedGroups, array<LedPattern>^ patternArr,
			u32 ledGroupingStride, array<u32>^ groupingArr,
			u32 numColourLayers, u32 colourLayersStride, array<LedColour>^ ledColoursArr);
		LedPayloadData LedPayloadFillupNumLedGroups(LedPayloadData srcPayload, u32 numLedGroups);
		LedPayloadData LedPayloadFillupGroupLedPattern(LedPayloadData srcPayload, u32 ledGroupIndex, LedPattern patternType);
		LedPayloadData LedPayloadFillupNumLedsInGroup(LedPayloadData srcPayload, u32 ledGroupIndex, u32 numLedsInGroup);
		LedPayloadData LedPayloadFillupLedID(LedPayloadData srcPayload, u32 ledGroupIndex, u32 ledIndex, unsigned int ledID);
		LedPayloadData LedPayloadFillupNumLedColourLayers(LedPayloadData srcPayload, u32 ledGroupIndex, u32 numColourLayers);
		LedPayloadData LedPayloadFillupLedColour(LedPayloadData srcPayload, u32 ledGroupIndex, u32 colourLayerIndex, LedColour colourLayer, bool reversedDirection);
		LedPayloadData LedPayloadCleanup(LedPayloadData srcPayload, u32 maxNumLedGroups);

		void SetLedSettings(LedSettings^ settingsData);
	
	private:
		void invalidate()
		{
			if (m_Instance != nullptr)
			{
				m_Instance->Shutdown(0);
				m_Instance->Release();
				m_Instance = nullptr;
			}
		}
		ICTLEDMgr* m_Instance = nullptr;
		Object^ m_enumLock = gcnew Object();
	};
}
