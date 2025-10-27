//
// Voron Scripts - PerformanceEffect
// v1.0-beta.8
// https://github.com/VoronFX/Aurora
// Copyright (C) 2016 Voronin Igor <Voron.exe@gmail.com>
// 

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using AuroraRgb.EffectsEngine;
using AuroraRgb.Profiles;
using AuroraRgb.Settings;
using AuroraRgb.Utils;
using AuroraRgb.Vorons.Mathos.Parser;
using AuroraRgb.Vorons.OpenHardwareMonitor.Collections;
using AuroraRgb.Vorons.OpenHardwareMonitor.Hardware.ATI;
using AuroraRgb.Vorons.OpenHardwareMonitor.Hardware.Nvidia;
using Common.Devices;
using CircleGradientStateKey =
	System.Tuple<AuroraRgb.EffectsEngine.ColorSpectrum, AuroraRgb.Vorons.PerformanceCounterManager.IntervalPerformanceCounter, float, float>;
using CircleGradientState = System.Collections.Generic.KeyValuePair<float, long>;

namespace AuroraRgb.Vorons
{
	public class PerformanceEffect : IEffectScript
	{
		public string ID => "Voron Scripts - PerfEffect - v1.0-beta.6";

		public VariableRegistry Properties { get; }

		internal enum EffectTypes
		{
			/// <summary>
			/// All at once
			/// </summary>
			[Description("All at once")]
			AllAtOnce = 0,

			/// <summary>
			/// Progressive
			/// </summary>
			[Description("Progressive")]
			Progressive = 1,

			/// <summary>
			/// Progressive (Gradual)
			/// </summary>
			[Description("Progressive (Gradual)")]
			ProgressiveGradual = 2,

			/// <summary>
			/// Progressive (Gradual)
			/// </summary>
			[Description("Cycled Gradient Shift")]
			CycledGradientShift = 3
		}

		public PerformanceEffect()
		{
			AuroraInternal.Disk.Register();
			AuroraInternal.Network.Register();
			AuroraInternal.Gpu.Register();

			Properties = new VariableRegistry();

			Properties.RegProp("Keys or Freestyle",
				new KeySequence(new[] { DeviceKeys.G6, DeviceKeys.G7, DeviceKeys.G8, DeviceKeys.G9 }));

			Properties.RegProp("Effect type", (long)EffectTypes.ProgressiveGradual,
				String.Join(Environment.NewLine,
				Enum.GetValues(typeof(EffectTypes)).Cast<EffectTypes>().Select(x => $"{(int)x} - {x}")),
				(long)Enum.GetValues(typeof(EffectTypes)).Cast<EffectTypes>().Min(),
				(long)Enum.GetValues(typeof(EffectTypes)).Cast<EffectTypes>().Max());

			Properties.RegProp("Value source", "Processor | % Processor Time | _Total",
				String.Join(Environment.NewLine, new[]
				{
					"Enter any valid Aurora Internal or Windows performance counter for your system in format \"Category | Name | Instance\".",
					"List of all available Windows counters can be found in Performance Monitor.",
					"All Aurora Internal counters are listed below:"
				}.Concat(PerformanceCounterManager.InternalRegisteredCounters
				.Select(x => $"{x.Item1} | {x.Item2} | {x.Item3}").OrderBy(x => x)))
			);

			Properties.RegProp("Value expression", "x",
				"Math expression that is used to normalize raw value. \"x\" is the raw value. Normalized value should be in 0-100 range.");

			Properties.RegProp("Gradient", "#FF00FF00 | #FFFFA500 | #FFFF0000",
				String.Join(Environment.NewLine,
				"Gradient that is used for effect. Separate color points with \"|\".",
				"Optionally set point position with \"@\" symbol."));

			Properties.RegProp("Enable Overload Blinking", true, "Last gradient color will blink when value reaches overload threshold level");

			Properties.RegProp("Overload Start Threshold", 95L, "Blinking start be visible when value reaches this level", 0L, 100L);

			Properties.RegProp("Overload Full Threshold", 100L, "Blinking will be fully visible when value reaches this level", 0L, 100L);

			Properties.RegProp("Overload Blinking Speed", 1000L, "Speed of CoreOverload blinking in (ms)", 10L, 10000L);

			Properties.RegProp("Overload Blinking Color", new RealColor(Color.Black));

			Properties.RegProp("Cycled Gradient Shift Base Speed", 0L, "Cycled gradient shifting speed at 0%", -1000L, 1000L);

			Properties.RegProp("Cycled Gradient Shift Full Speed", 100L, "Cycled gradient shifting speed at 100%", -1000L, 1000L);

			_effectLayer = new BitmapEffectLayer(ID, true);
		}

		private static readonly MathParser MathParser = new();

		private static readonly ConcurrentDictionary<string, PerformanceCounterManager.IntervalPerformanceCounter> ValueSources
			= new();

		private static readonly ConcurrentDictionary<string, ColorSpectrum> Gradients = new();

		private static readonly ConcurrentDictionary<CircleGradientStateKey, CircleGradientState>
			CircleShidtStates = new();

		private readonly BitmapEffectLayer _effectLayer;

		private KeySequence Keys { get; set; }
		private EffectTypes EffectType { get; set; }
		private PerformanceCounterManager.IntervalPerformanceCounter ValueSource { get; set; }
		private string ValueExpression { get; set; }
		private ColorSpectrum Gradient { get; set; }

		private bool EnableOverloadBlinking { get; set; }
		private float OverloadStartThreshold { get; set; }
		private float OverloadFullThreshold { get; set; }
		private int OverloadBlinkingSpeed { get; set; }
		private Color OverloadBlinkingColor { get; set; }
		private float CycledGradientShiftBaseSpeed { get; set; }
		private float CycledGradientShiftFullSpeed { get; set; }

		private void ReadProperties(VariableRegistry properties)
		{
			Keys = properties.GetVariable<KeySequence>("Keys or Freestyle");
			EffectType = (EffectTypes)properties.GetVariable<long>("Effect type");

			ValueSource = ValueSources.GetOrAdd(properties.GetString("Value source"), key =>
			{
				var parsedValueSource = key.Split('|').Select(x => x.Trim()).ToArray();
				return PerformanceCounterManager.GetCounter(parsedValueSource[0], parsedValueSource[1], parsedValueSource[2],
					1000);
			});
			ValueExpression = properties.GetString("Value expression");
			Gradient = Gradients.GetOrAdd(properties.GetString("Gradient"), ScriptHelper.StringToSpectrum);

			EnableOverloadBlinking = properties.GetVariable<bool>("Enable Overload Blinking");
			OverloadStartThreshold = properties.GetVariable<long>("Overload Start Threshold") / 100f;
			OverloadFullThreshold = properties.GetVariable<long>("Overload Full Threshold") / 100f;
			OverloadBlinkingSpeed = (int)properties.GetVariable<long>("Overload Blinking Speed");
			OverloadBlinkingColor = properties.GetVariable<RealColor>("Overload Blinking Color")
				.GetDrawingColor();

			CycledGradientShiftBaseSpeed = properties.GetVariable<long>("Cycled Gradient Shift Base Speed") / 100f;
			CycledGradientShiftFullSpeed = properties.GetVariable<long>("Cycled Gradient Shift Full Speed") / 100f;
		}

		public object UpdateLights(VariableRegistry properties, IGameState state = null)
		{
			ReadProperties(properties);

			_effectLayer.Clear();
			var value = ValueSource.GetValue() / 100f;
			if (ValueExpression != "x")
			{
				MathParser.LocalVariables["x"] = (decimal)value;
				value = (float)MathParser.Parse(ValueExpression);
			}
			value = Math.Max(0, Math.Min(1, value));

			var time = Time.GetMillisecondsSinceEpoch();

			var gradient = new ColorSpectrum();
			foreach (var keyValuePair in Gradient.GetSpectrumColors())
			{
				gradient.SetColorAt(keyValuePair.Key, keyValuePair.Value);
			}

			if (EnableOverloadBlinking)
			{
				var blinkingLevel = (value - OverloadStartThreshold) / (OverloadFullThreshold - OverloadStartThreshold);
				blinkingLevel = Math.Max(0f, Math.Min(1f, blinkingLevel))
								* Math.Abs(1f - (time % OverloadBlinkingSpeed) / (OverloadBlinkingSpeed / 2f));

				gradient.SetColorAt(1, ColorUtils.BlendColors(gradient.GetColorAt(1f), OverloadBlinkingColor, blinkingLevel));
			}

			if (EffectType == EffectTypes.CycledGradientShift)
			{
				var currSpeed = value * Math.Abs(CycledGradientShiftBaseSpeed - CycledGradientShiftFullSpeed) +
								CycledGradientShiftBaseSpeed;

				var gradState = CircleShidtStates.AddOrUpdate(
					new CircleGradientStateKey(
						Gradient, ValueSource, CycledGradientShiftBaseSpeed, CycledGradientShiftFullSpeed), (key) =>
					new CircleGradientState(0, time), (keys, prevState) =>
					new CircleGradientState((prevState.Key + (time - prevState.Value) * currSpeed / 1000f) % 1f, time));

				var grad = gradient.Shift(gradState.Key);

				_effectLayer.PercentEffect(grad, Keys, 1, 1, PercentEffectType.Progressive);

			}
			else
			{
				PercentEffectType effectType;
				switch (EffectType)
				{
					case EffectTypes.AllAtOnce:
						effectType = PercentEffectType.AllAtOnce;
						break;
					case EffectTypes.Progressive:
						effectType = PercentEffectType.Progressive;
						break;
					case EffectTypes.ProgressiveGradual:
						effectType = PercentEffectType.Progressive_Gradual;
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
				_effectLayer.PercentEffect(gradient, Keys, value, 1, effectType);
			}

			return _effectLayer;
		}

	}

	#region EasedPerformanceCounter

	internal static class PerformanceCounterManager
	{
		private static readonly ConcurrentDictionary<Tuple<string, string, string>, Func<float>> InternalPerformanceCounters =
			new();

		private static readonly ConcurrentDictionary<Tuple<string, string, string>, Func<float>>
			CreatedSystemPerformanceCounters = new();

		private static readonly ConcurrentDictionary<Tuple<string, string, string, long>, IntervalPerformanceCounter>
			CountersInstances = new();

		private static readonly ConcurrentQueue<IntervalPerformanceCounter> NewCounters = new();

		public static IEnumerable<Tuple<string, string, string>> InternalRegisteredCounters
		{
			get
			{
				return InternalPerformanceCounters.Select(x => x.Key).ToArray();
			}
		}

		public static void RegisterInternal(string categoryName, string counterName, string instanceName, Func<float> newSample)
		{
			InternalPerformanceCounters.AddOrUpdate(new Tuple<string, string, string>(categoryName, counterName, instanceName),
				newSample, (tuple, func) => newSample);
		}

		public static Func<float> GetSystemPerformanceCounter(string categoryName, string counterName, string instanceName)
		{
			return GetSystemPerformanceCounter(new Tuple<string, string, string>(categoryName, counterName, instanceName));
		}

		public static Func<float> GetSystemPerformanceCounter(Tuple<string, string, string> key)
		{
			return CreatedSystemPerformanceCounters.GetOrAdd(key, tuple2 =>
			{
				var performanceCounter = new PerformanceCounter(key.Item1, key.Item2, key.Item3);
				return () => performanceCounter.NextValue();
			});
		}

		public static IntervalPerformanceCounter GetCounter(string categoryName, string counterName, string instanceName,
			long updateInterval)
		{
			return CountersInstances.GetOrAdd(
				new Tuple<string, string, string, long>(categoryName, counterName, instanceName, updateInterval),
				tuple =>
				{
					var key = new Tuple<string, string, string>(categoryName, counterName, instanceName);
					Func<float> value;
					if (!InternalPerformanceCounters.TryGetValue(key, out value))
					{
						value = GetSystemPerformanceCounter(key);
					}
					var newCounter = new IntervalPerformanceCounter(tuple, (int)Math.Ceiling(3000f / updateInterval), value);
					NewCounters.Enqueue(newCounter);
					return newCounter;
				});
		}

		public sealed partial class IntervalPerformanceCounter
		{
			private sealed class IntervalCounterList : List<IntervalPerformanceCounter>
			{
				public readonly long Interval;
				public DateTime NextUpdate;

				public IntervalCounterList(long interval)
				{
					Interval = interval;
				}
			}

			private static readonly List<IntervalCounterList> Intervals = new();
			private static readonly Timer Timer = new(UpdateTick, null, Timeout.Infinite, Timeout.Infinite);
			private static int sleeping = 1;

			private static void UpdateTick(object state)
			{
				IntervalPerformanceCounter newCounter;
				if (!NewCounters.IsEmpty)
				{
					while (NewCounters.TryDequeue(out newCounter))
					{
						var interval = Intervals.FirstOrDefault(x => x.Interval == newCounter.UpdateInterval);
						if (interval == null)
						{
							interval = new IntervalCounterList(newCounter.UpdateInterval);
							Intervals.Add(interval);
						}
						interval.Add(newCounter);
					}
				}

				var time = DateTime.UtcNow;
				var nextUpdate = DateTime.MaxValue;
				var activeCounters = false;
				foreach (var interval in Intervals)
				{
					if (interval.NextUpdate <= time)
					{
						interval.NextUpdate = time.AddMilliseconds(interval.Interval);
						foreach (var counter in interval)
						{
							if (Volatile.Read(ref counter._counterUsage) > 0)
							{
								try
								{
									Volatile.Write(ref counter._lastFrame,
										new CounterFrame(counter._lastFrame.CurrentValue, counter._newSample()));
								}
								catch (Exception exc)
								{
									Global.logger.Error(exc, "IntervalPerformanceCounter exception in {0}/{1}/{2}/{3}",
										counter.CategoryName, counter.CounterName, counter.InstanceName, counter.UpdateInterval);
								}
								counter._counterUsage--;
								activeCounters = true;
							}
						}
					}
					nextUpdate = nextUpdate > interval.NextUpdate ? interval.NextUpdate : nextUpdate;
				}
				if (activeCounters)
				{
					var nextDelay = nextUpdate - DateTime.UtcNow;
					Timer.Change(nextDelay.Milliseconds > 0 ? nextDelay : TimeSpan.Zero, Timeout.InfiniteTimeSpan);
				}
				else
				{
					Volatile.Write(ref sleeping, 1);
				}
			}
		}

		public sealed partial class IntervalPerformanceCounter
		{
			public Tuple<string, string, string, long> Key { get; private set; }
			public string CategoryName => Key.Item1;

			public string CounterName => Key.Item2;

			public string InstanceName => Key.Item3;

			public long UpdateInterval => Key.Item4;

			public int IdleTimeout { get; private set; }

			private sealed class CounterFrame
			{
				public readonly float PreviousValue;
				public readonly float CurrentValue;
				public readonly long Timestamp;

				public CounterFrame(float previousValue, float currentValue)
				{
					PreviousValue = previousValue;
					CurrentValue = currentValue;
					Timestamp = Time.GetMillisecondsSinceEpoch();
				}
			}

			private CounterFrame _lastFrame = new(0, 0);
			private readonly Func<float> _newSample;

			private int _counterUsage;

			public float GetValue(bool easing = true)
			{
				Volatile.Write(ref _counterUsage, IdleTimeout);

				if (Volatile.Read(ref sleeping) == 1)
				{
					if (Interlocked.CompareExchange(ref sleeping, 0, 1) == 1)
					{
						Timer.Change(0, Timeout.Infinite);
					}
				}

				var frame = Volatile.Read(ref _lastFrame);
				if (!easing)
					return frame.CurrentValue;

				return frame.PreviousValue + (frame.CurrentValue - frame.PreviousValue) *
					   Math.Min(Time.GetMillisecondsSinceEpoch() - frame.Timestamp, UpdateInterval) / UpdateInterval;
			}

			public IntervalPerformanceCounter(Tuple<string, string, string, long> key, int idleTimeout, Func<float> newSample)
			{
				_newSample = newSample;
				Key = key;
				IdleTimeout = idleTimeout;
			}
		}
	}

	#endregion

	#region PerformanceCounters

	internal class AuroraInternal
	{
		public const string CategoryName = "Aurora Internal";

		public class Disk
		{
			public static void Register()
			{
				var performanceCounter = new Lazy<Func<float>>(() =>
						PerformanceCounterManager.GetSystemPerformanceCounter("LogicalDisk", "% Disk Time",
							Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.System)).Substring(0, 2)),
					LazyThreadSafetyMode.PublicationOnly);

				PerformanceCounterManager.RegisterInternal(CategoryName, "System Disk",
					"% Usage", () => performanceCounter.Value());
			}
		}

		public class Network
		{
			public static void Register()
			{
				var propNames = new[] { "Bytes Received/sec", "Bytes Sent/sec",
					"Bytes Total/sec", "Current Bandwidth", "% Network Total Usage" };

				var defaultAdapterName = new Lazy<string>(() =>
				{
					UdpClient u = new UdpClient(Dns.GetHostName(), 1);
					string localAddr = ((IPEndPoint)u.Client.LocalEndPoint).Address.ToString();
					u.Dispose();

					var defInt = NetworkInterface.GetAllNetworkInterfaces()
						.Where(netInt => netInt.OperationalStatus == OperationalStatus.Up)
						.FirstOrDefault(netInt =>
							netInt.GetIPProperties().UnicastAddresses.Any(
								uni => uni.Address.ToString() == localAddr));
					return defInt != null ? defInt.Name : null;

				}, LazyThreadSafetyMode.PublicationOnly);

				var counters = new[] { "Bytes Received/sec", "Bytes Sent/sec",
					"Bytes Total/sec", "Current Bandwidth" }.Select(x =>
					new Lazy<Func<float>>(() => PerformanceCounterManager.GetSystemPerformanceCounter(
						"Network Adapter", x, defaultAdapterName.Value))).ToArray();

				for (var i = 0; i < propNames.Length; i++)
				{
					var indexCopy = i;

					PerformanceCounterManager.RegisterInternal(CategoryName, "Default Network",
						propNames[i], () =>
						{
							if (indexCopy == counters.Length)
							{
								return counters[indexCopy - 2].Value() * 100 / counters[indexCopy - 1].Value();
							}

							return counters[indexCopy].Value();
						});
				}
			}
		}

		public class Gpu
		{
			private static readonly KeyValuePair<NvPhysicalGpuHandle, NvDisplayHandle>[] NvidiaGpus;
			private static readonly int[] AtiGpus;
			public static string InitLog;

			static Gpu()
			{
				NvidiaGpus = GetNvidiaGpus();
				AtiGpus = GetAtiGpus();
			}

			public static void Register()
			{
				bool registerGeneral = true;

				for (var i = 0; i < NvidiaGpus.Length; i++)
				{
					RegisterNvidiaGpuCounters(NvidiaGpus[i], $"NvidiaGpu #{i}", false);

					if (registerGeneral)
					{
						RegisterNvidiaGpuCounters(NvidiaGpus[i], "GPU", true);
						registerGeneral = false;
					}
				}

				for (var i = 0; i < AtiGpus.Length; i++)
				{
					RegisterAtiGpuCounters(AtiGpus[i], $"AtiGpu #{i}", false);

					if (registerGeneral)
					{
						RegisterAtiGpuCounters(AtiGpus[i], "GPU", true);
						registerGeneral = false;
					}
				}
			}

			private static void RegisterAtiGpuCounters(int gpu, string counterName, bool general)
			{
				Action<string, Func<float>> register = (name, newSample) =>
					PerformanceCounterManager.RegisterInternal(CategoryName, counterName, name, newSample);

				register("FanRpm", () => GetAtiFanSpeed(gpu, FanSpeedType.Rpm));
				register("% FanUsage", () => GetAtiFanSpeed(gpu, FanSpeedType.Percent));

				register("Temperature", () => GetAtiTemperature(gpu));

				register("Core Clock", () => GetAtiActivity(gpu, AtiActivityType.CoreClock));
				register("Memory Clock", () => GetAtiActivity(gpu, AtiActivityType.MemoryClock));

				if (general)
				{
					register("% Load", () => GetAtiActivity(gpu, AtiActivityType.LoadCorePercent));
				}
				else
				{
					register("Core Voltage", () => GetAtiActivity(gpu, AtiActivityType.CoreVoltage));
					register("% Load Core", () => GetAtiActivity(gpu, AtiActivityType.LoadCorePercent));
				}
			}

			private static void RegisterNvidiaGpuCounters(KeyValuePair<NvPhysicalGpuHandle, NvDisplayHandle> gpu,
				string counterName, bool general)
			{
				Action<string, Func<float>> register = (name, newSample) =>
					PerformanceCounterManager.RegisterInternal(CategoryName, counterName, name, newSample);

				register("FanRpm", () => GetNvidiaFanSpeed(gpu.Key, FanSpeedType.Rpm));
				register("% FanUsage", () => GetNvidiaFanSpeed(gpu.Key, FanSpeedType.Percent));

				if (general)
				{
					register("Temperature", () => GetNvidiaTemperature(gpu.Key, NvThermalTarget.GPU));

					register("Core Clock", () => GetNvidiaClock(gpu.Key, NvidiaClockType.Core));
					register("Memory Clock", () => GetNvidiaClock(gpu.Key, NvidiaClockType.Memory));

					register("% Load", () => GetNvidiaUsage(gpu.Key, NvidiaUsageType.Core));
				}
				else
				{
					register("Temperature Board", () => GetNvidiaTemperature(gpu.Key, NvThermalTarget.BOARD));
					register("Temperature GPU", () => GetNvidiaTemperature(gpu.Key, NvThermalTarget.GPU));
					register("Temperature Memory", () => GetNvidiaTemperature(gpu.Key, NvThermalTarget.MEMORY));
					register("Temperature Power Supply", () => GetNvidiaTemperature(gpu.Key, NvThermalTarget.POWER_SUPPLY));

					register("Clock Core", () => GetNvidiaClock(gpu.Key, NvidiaClockType.Core));
					register("Clock Memory", () => GetNvidiaClock(gpu.Key, NvidiaClockType.Memory));
					register("Clock Shader", () => GetNvidiaClock(gpu.Key, NvidiaClockType.Shader));

					register("% Load Core", () => GetNvidiaUsage(gpu.Key, NvidiaUsageType.Core));
					register("% Load Memory Controller", () => GetNvidiaUsage(gpu.Key, NvidiaUsageType.MemoryController));
					register("% Load Video Engine", () => GetNvidiaUsage(gpu.Key, NvidiaUsageType.VideoEngine));

					register("Memory Free", () => GetNvidiaMemory(gpu.Value, NvidiaMemory.Free));
					register("Memory Used", () => GetNvidiaMemory(gpu.Value, NvidiaMemory.Used));
					register("Memory Total", () => GetNvidiaMemory(gpu.Value, NvidiaMemory.Total));
					register("% Memory Usage", () => GetNvidiaMemory(gpu.Value, NvidiaMemory.Usage));
				}
			}

			private enum FanSpeedType { Rpm, Percent }

			private static float GetNvidiaFanSpeed(NvPhysicalGpuHandle gpu, FanSpeedType speedType)
			{
				switch (speedType)
				{
					case FanSpeedType.Rpm:
						int value;
						NVAPI.NvAPI_GPU_GetTachReading(gpu, out value);
						return value;
					case FanSpeedType.Percent:
						NvGPUCoolerSettings settings = new NvGPUCoolerSettings
						{
							Version = NVAPI.GPU_COOLER_SETTINGS_VER,
							Cooler = new NvCooler[NVAPI.MAX_COOLER_PER_GPU]
						};
						if (NVAPI.NvAPI_GPU_GetCoolerSettings != null &&
							NVAPI.NvAPI_GPU_GetCoolerSettings(gpu, 0, ref settings) == NvStatus.OK)
							return settings.Cooler[0].CurrentLevel;
						break;
					default:
						throw new ArgumentOutOfRangeException("speedType", speedType, null);
				}
				return 0;
			}

			private static float GetNvidiaTemperature(NvPhysicalGpuHandle gpu, NvThermalTarget sensorTarget)
			{
				NvGPUThermalSettings settings = new NvGPUThermalSettings
				{
					Version = NVAPI.GPU_THERMAL_SETTINGS_VER,
					Count = NVAPI.MAX_THERMAL_SENSORS_PER_GPU,
					Sensor = new NvSensor[NVAPI.MAX_THERMAL_SENSORS_PER_GPU]
				};
				if (NVAPI.NvAPI_GPU_GetThermalSettings != null &&
					NVAPI.NvAPI_GPU_GetThermalSettings(gpu, (int)NvThermalTarget.ALL,
						ref settings) == NvStatus.OK)
				{
					return settings.Sensor.FirstOrDefault(s => s.Target == sensorTarget).CurrentTemp;
				}
				return 0;
			}

			private enum NvidiaUsageType
			{
				Core = 0,
				MemoryController = 1,
				VideoEngine = 2
			}

			private static float GetNvidiaUsage(NvPhysicalGpuHandle gpu, NvidiaUsageType usageType)
			{
				NvPStates states = new NvPStates
				{
					Version = NVAPI.GPU_PSTATES_VER,
					PStates = new NvPState[NVAPI.MAX_PSTATES_PER_GPU]
				};
				if (NVAPI.NvAPI_GPU_GetPStates != null &&
					NVAPI.NvAPI_GPU_GetPStates(gpu, ref states) == NvStatus.OK)
				{
					return states.PStates[(int)usageType].Present ? (float)states.PStates[(int)usageType].Percentage : 0;
				}

				NvUsages usages = new NvUsages
				{
					Version = NVAPI.GPU_USAGES_VER,
					Usage = new uint[NVAPI.MAX_USAGES_PER_GPU]
				};
				if (NVAPI.NvAPI_GPU_GetUsages != null &&
					NVAPI.NvAPI_GPU_GetUsages(gpu, ref usages) == NvStatus.OK)
				{
					switch (usageType)
					{
						case NvidiaUsageType.Core:
							return usages.Usage[2];
						case NvidiaUsageType.MemoryController:
							return usages.Usage[6];
						case NvidiaUsageType.VideoEngine:
							return usages.Usage[10];
					}
				}
				return 0;
			}

			private enum NvidiaClockType
			{
				Core = 0,
				Memory = 1,
				Shader = 2
			}

			private static float GetNvidiaClock(NvPhysicalGpuHandle gpu, NvidiaClockType clockType)
			{
				NvClocks allClocks = new NvClocks
				{
					Version = NVAPI.GPU_CLOCKS_VER,
					Clock = new uint[NVAPI.MAX_CLOCKS_PER_GPU]
				};
				if (NVAPI.NvAPI_GPU_GetAllClocks == null ||
					NVAPI.NvAPI_GPU_GetAllClocks(gpu, ref allClocks) != NvStatus.OK)
					return 0;

				var values = allClocks.Clock;
				var clocks = new float[3];
				clocks[1] = 0.001f * values[8];
				if (values[30] != 0)
				{
					clocks[0] = 0.0005f * values[30];
					clocks[2] = 0.001f * values[30];
				}
				else
				{
					clocks[0] = 0.001f * values[0];
					clocks[2] = 0.001f * values[14];
				}
				return clocks[(int)clockType];
			}

			private enum NvidiaMemory { Free, Used, Total, Usage }

			private static float GetNvidiaMemory(NvDisplayHandle gpu, NvidiaMemory type)
			{
				NvMemoryInfo memoryInfo = new NvMemoryInfo
				{
					Version = NVAPI.GPU_MEMORY_INFO_VER,
					Values = new uint[NVAPI.MAX_MEMORY_VALUES_PER_GPU]
				};
				if (NVAPI.NvAPI_GPU_GetMemoryInfo != null &&
					NVAPI.NvAPI_GPU_GetMemoryInfo(gpu, ref memoryInfo) ==
					NvStatus.OK)
				{
					uint totalMemory = memoryInfo.Values[0];
					uint freeMemory = memoryInfo.Values[4];
					float usedMemory = Math.Max(totalMemory - freeMemory, 0);
					switch (type)
					{
						case NvidiaMemory.Free:
							return freeMemory / 1024f;
						case NvidiaMemory.Used:
							return usedMemory / 1024;
						case NvidiaMemory.Total:
							return totalMemory / 1024f;
						case NvidiaMemory.Usage:
							return 100f * usedMemory / totalMemory;
					}
				}
				return 0;
			}

			private static float GetAtiFanSpeed(int gpu, FanSpeedType speedType)
			{
				ADLFanSpeedValue adlf = new ADLFanSpeedValue
				{
					SpeedType = speedType == FanSpeedType.Rpm ?
						ADL.ADL_DL_FANCTRL_SPEED_TYPE_RPM : ADL.ADL_DL_FANCTRL_SPEED_TYPE_PERCENT
				};
				if (ADL.ADL_Overdrive5_FanSpeed_Get(gpu, 0, ref adlf)
					== ADL.ADL_OK)
				{
					return adlf.FanSpeed;
				}
				return 0;
			}

			private static float GetAtiTemperature(int gpu)
			{
				ADLTemperature adlt = new ADLTemperature();
				if (ADL.ADL_Overdrive5_Temperature_Get(gpu, 0, ref adlt)
					== ADL.ADL_OK)
				{
					return 0.001f * adlt.Temperature;
				}
				return 0;
			}

			private enum AtiActivityType { CoreClock, MemoryClock, CoreVoltage, LoadCorePercent }

			private static float GetAtiActivity(int gpu, AtiActivityType activityType)
			{
				ADLPMActivity adlp = new ADLPMActivity();
				if (ADL.ADL_Overdrive5_CurrentActivity_Get(gpu, ref adlp)
					== ADL.ADL_OK)
				{
					switch (activityType)
					{
						case AtiActivityType.CoreClock:
							if (adlp.EngineClock > 0)
								return 0.01f * adlp.EngineClock;
							break;
						case AtiActivityType.MemoryClock:
							if (adlp.MemoryClock > 0)
								return 0.01f * adlp.MemoryClock;
							break;
						case AtiActivityType.CoreVoltage:
							if (adlp.Vddc > 0)
								return 0.001f * adlp.Vddc;
							break;
						case AtiActivityType.LoadCorePercent:
							return Math.Min(adlp.ActivityPercent, 100);
					}
				}
				return 0;
			}

			private static int[] GetAtiGpus()
			{
				var agpus = new List<ADLAdapterInfo>();
				try
				{
					int status = ADL.ADL_Main_Control_Create(1);

					if (status == ADL.ADL_OK)
					{
						int numberOfAdapters = 0;
						ADL.ADL_Adapter_NumberOfAdapters_Get(ref numberOfAdapters);

						Global.logger.Information("Number of adapters: {NumberOfAdapters}", numberOfAdapters);

						if (numberOfAdapters > 0)
						{
							ADLAdapterInfo[] adapterInfo = new ADLAdapterInfo[numberOfAdapters];
							if (ADL.ADL_Adapter_AdapterInfo_Get(adapterInfo) == ADL.ADL_OK)
							{
								for (int i = 0; i < numberOfAdapters; i++)
								{
									int isActive;
									ADL.ADL_Adapter_Active_Get(adapterInfo[i].AdapterIndex,
										out isActive);
									int adapterID;
									ADL.ADL_Adapter_ID_Get(adapterInfo[i].AdapterIndex,
										out adapterID);
									Global.logger.Information("AdapterIndex: {I}", i);
									Global.logger.Information("isActive: {IsActive}", isActive);
									Global.logger.Information("AdapterName: {AdapterName}", adapterInfo[i].AdapterName);
									Global.logger.Information("UDID: {Udid}", adapterInfo[i].UDID);
									Global.logger.Information("Present: {Present}", adapterInfo[i].Present);
									Global.logger.Information("VendorID: 0x{VendorId}", adapterInfo[i].VendorID);
									Global.logger.Information("BusNumber: {BusNumber}", adapterInfo[i].BusNumber);
									Global.logger.Information("DeviceNumber: {DeviceNumber}", adapterInfo[i].DeviceNumber);
									Global.logger.Information("FunctionNumber: {FunctionNumber}", adapterInfo[i].FunctionNumber);
									Global.logger.Information("AdapterID: 0x{AdapterId}", adapterID);

									if (!string.IsNullOrEmpty(adapterInfo[i].UDID) &&
										adapterInfo[i].VendorID == ADL.ATI_VENDOR_ID)
									{
										bool found = false;
										foreach (var gpu in agpus)
											if (gpu.BusNumber == adapterInfo[i].BusNumber &&
												gpu.DeviceNumber == adapterInfo[i].DeviceNumber)
											{
												found = true;
												break;
											}
										if (!found)
											agpus.Add(adapterInfo[i]);
									}
								}
							}
						}
					}

				}
				catch (DllNotFoundException) { }
				catch (EntryPointNotFoundException e)
				{
					Global.logger.Error(e, $"Error: ");
				}
				return agpus.Select(x => x.AdapterIndex).ToArray();
			}

			private static KeyValuePair<NvPhysicalGpuHandle, NvDisplayHandle>[] GetNvidiaGpus()
			{
				var ngpus = new List<KeyValuePair<NvPhysicalGpuHandle, NvDisplayHandle>>();
				if (NVAPI.IsAvailable)
				{
					Global.logger.Information("NVAPI");

					string version;
					if (NVAPI.NvAPI_GetInterfaceVersionString(out version) == NvStatus.OK)
					{
						Global.logger.Information("Version: {Version}", version);
					}

					NvPhysicalGpuHandle[] handles =
						new NvPhysicalGpuHandle[NVAPI.MAX_PHYSICAL_GPUS];
					int count;
					if (NVAPI.NvAPI_EnumPhysicalGPUs == null)
					{
						Global.logger.Error("Error: NvAPI_EnumPhysicalGPUs not available");
						return ngpus.ToArray();
					}
					else
					{
						NvStatus status = NVAPI.NvAPI_EnumPhysicalGPUs(handles, out count);
						if (status != NvStatus.OK)
						{
							Global.logger.Information("Status: {Status}", status);
							return ngpus.ToArray();
						}
					}

					var displayHandles = new Dictionary<NvPhysicalGpuHandle, NvDisplayHandle>();

					if (NVAPI.NvAPI_EnumNvidiaDisplayHandle != null &&
						NVAPI.NvAPI_GetPhysicalGPUsFromDisplay != null)
					{
						NvStatus status = NvStatus.OK;
						int i = 0;
						while (status == NvStatus.OK)
						{
							NvDisplayHandle displayHandle = new NvDisplayHandle();
							status = NVAPI.NvAPI_EnumNvidiaDisplayHandle(i, ref displayHandle);
							i++;

							if (status == NvStatus.OK)
							{
								NvPhysicalGpuHandle[] handlesFromDisplay =
									new NvPhysicalGpuHandle[NVAPI.MAX_PHYSICAL_GPUS];
								uint countFromDisplay;
								if (NVAPI.NvAPI_GetPhysicalGPUsFromDisplay(displayHandle,
										handlesFromDisplay, out countFromDisplay) == NvStatus.OK)
								{
									for (int j = 0; j < countFromDisplay; j++)
									{
										if (!displayHandles.ContainsKey(handlesFromDisplay[j]))
											displayHandles.Add(handlesFromDisplay[j], displayHandle);
									}
								}
							}
						}
					}

					Global.logger.Information("Number of GPUs: {Count}", count);

					for (int i = 0; i < count; i++)
					{
						NvDisplayHandle displayHandle;
						displayHandles.TryGetValue(handles[i], out displayHandle);
						ngpus.Add(new KeyValuePair<NvPhysicalGpuHandle, NvDisplayHandle>(handles[i], displayHandle));
					}
				}
				return ngpus.ToArray();
			}

		}
	}

	#endregion

	#region OpenHardwareMonitor

	#region ADL

	/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
  Copyright (C) 2009-2012 Michael Möller <mmoeller@openhardwaremonitor.org>
	
*/

	namespace OpenHardwareMonitor.Hardware.ATI
	{

		[StructLayout(LayoutKind.Sequential)]
		internal struct ADLAdapterInfo
		{
			public int Size;
			public int AdapterIndex;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = ADL.ADL_MAX_PATH)]
			public string UDID;
			public int BusNumber;
			public int DeviceNumber;
			public int FunctionNumber;
			public int VendorID;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = ADL.ADL_MAX_PATH)]
			public string AdapterName;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = ADL.ADL_MAX_PATH)]
			public string DisplayName;
			public int Present;
			public int Exist;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = ADL.ADL_MAX_PATH)]
			public string DriverPath;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = ADL.ADL_MAX_PATH)]
			public string DriverPathExt;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = ADL.ADL_MAX_PATH)]
			public string PNPString;
			public int OSDisplayIndex;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct ADLPMActivity
		{
			public int Size;
			public int EngineClock;
			public int MemoryClock;
			public int Vddc;
			public int ActivityPercent;
			public int CurrentPerformanceLevel;
			public int CurrentBusSpeed;
			public int CurrentBusLanes;
			public int MaximumBusLanes;
			public int Reserved;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct ADLTemperature
		{
			public int Size;
			public int Temperature;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct ADLFanSpeedValue
		{
			public int Size;
			public int SpeedType;
			public int FanSpeed;
			public int Flags;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct ADLFanSpeedInfo
		{
			public int Size;
			public int Flags;
			public int MinPercent;
			public int MaxPercent;
			public int MinRPM;
			public int MaxRPM;
		}

		internal class ADL
		{
			public const int ADL_MAX_PATH = 256;
			public const int ADL_MAX_ADAPTERS = 40;
			public const int ADL_MAX_DISPLAYS = 40;
			public const int ADL_MAX_DEVICENAME = 32;
			public const int ADL_OK = 0;
			public const int ADL_ERR = -1;
			public const int ADL_DRIVER_OK = 0;
			public const int ADL_MAX_GLSYNC_PORTS = 8;
			public const int ADL_MAX_GLSYNC_PORT_LEDS = 8;
			public const int ADL_MAX_NUM_DISPLAYMODES = 1024;

			public const int ADL_DL_FANCTRL_SPEED_TYPE_PERCENT = 1;
			public const int ADL_DL_FANCTRL_SPEED_TYPE_RPM = 2;

			public const int ADL_DL_FANCTRL_SUPPORTS_PERCENT_READ = 1;
			public const int ADL_DL_FANCTRL_SUPPORTS_PERCENT_WRITE = 2;
			public const int ADL_DL_FANCTRL_SUPPORTS_RPM_READ = 4;
			public const int ADL_DL_FANCTRL_SUPPORTS_RPM_WRITE = 8;
			public const int ADL_DL_FANCTRL_FLAG_USER_DEFINED_SPEED = 1;

			public const int ATI_VENDOR_ID = 0x1002;

			private delegate int ADL_Main_Control_CreateDelegate(
				ADL_Main_Memory_AllocDelegate callback, int enumConnectedAdapters);
			private delegate int ADL_Adapter_AdapterInfo_GetDelegate(IntPtr info,
				int size);

			public delegate int ADL_Main_Control_DestroyDelegate();
			public delegate int ADL_Adapter_NumberOfAdapters_GetDelegate(
				ref int numAdapters);
			public delegate int ADL_Adapter_ID_GetDelegate(int adapterIndex,
				out int adapterID);
			public delegate int ADL_Display_AdapterID_GetDelegate(int adapterIndex,
				out int adapterID);
			public delegate int ADL_Adapter_Active_GetDelegate(int adapterIndex,
				out int status);
			public delegate int ADL_Overdrive5_CurrentActivity_GetDelegate(
				int iAdapterIndex, ref ADLPMActivity activity);
			public delegate int ADL_Overdrive5_Temperature_GetDelegate(int adapterIndex,
				int thermalControllerIndex, ref ADLTemperature temperature);
			public delegate int ADL_Overdrive5_FanSpeed_GetDelegate(int adapterIndex,
				int thermalControllerIndex, ref ADLFanSpeedValue fanSpeedValue);
			public delegate int ADL_Overdrive5_FanSpeedInfo_GetDelegate(
				int adapterIndex, int thermalControllerIndex,
				ref ADLFanSpeedInfo fanSpeedInfo);
			public delegate int ADL_Overdrive5_FanSpeedToDefault_SetDelegate(
				int adapterIndex, int thermalControllerIndex);
			public delegate int ADL_Overdrive5_FanSpeed_SetDelegate(int adapterIndex,
				int thermalControllerIndex, ref ADLFanSpeedValue fanSpeedValue);

			private static ADL_Main_Control_CreateDelegate
				_ADL_Main_Control_Create;
			private static ADL_Adapter_AdapterInfo_GetDelegate
				_ADL_Adapter_AdapterInfo_Get;

			public static ADL_Main_Control_DestroyDelegate
				ADL_Main_Control_Destroy;
			public static ADL_Adapter_NumberOfAdapters_GetDelegate
				ADL_Adapter_NumberOfAdapters_Get;
			public static ADL_Adapter_ID_GetDelegate
				_ADL_Adapter_ID_Get;
			public static ADL_Display_AdapterID_GetDelegate
				_ADL_Display_AdapterID_Get;
			public static ADL_Adapter_Active_GetDelegate
				ADL_Adapter_Active_Get;
			public static ADL_Overdrive5_CurrentActivity_GetDelegate
				ADL_Overdrive5_CurrentActivity_Get;
			public static ADL_Overdrive5_Temperature_GetDelegate
				ADL_Overdrive5_Temperature_Get;
			public static ADL_Overdrive5_FanSpeed_GetDelegate
				ADL_Overdrive5_FanSpeed_Get;
			public static ADL_Overdrive5_FanSpeedInfo_GetDelegate
				ADL_Overdrive5_FanSpeedInfo_Get;
			public static ADL_Overdrive5_FanSpeedToDefault_SetDelegate
				ADL_Overdrive5_FanSpeedToDefault_Set;
			public static ADL_Overdrive5_FanSpeed_SetDelegate
				ADL_Overdrive5_FanSpeed_Set;

			private static string dllName;

			private static void GetDelegate<T>(string entryPoint, out T newDelegate)
				where T : class
			{
				DllImportAttribute attribute = new DllImportAttribute(dllName);
				attribute.CallingConvention = CallingConvention.Cdecl;
				attribute.PreserveSig = true;
				attribute.EntryPoint = entryPoint;
				PInvokeDelegateFactory.CreateDelegate(attribute, out newDelegate);
			}

			private static void CreateDelegates(string name)
			{
				int p = (int)Environment.OSVersion.Platform;
				if ((p == 4) || (p == 128))
					dllName = name + ".so";
				else
					dllName = name + ".dll";

				GetDelegate("ADL_Main_Control_Create",
					out _ADL_Main_Control_Create);
				GetDelegate("ADL_Adapter_AdapterInfo_Get",
					out _ADL_Adapter_AdapterInfo_Get);
				GetDelegate("ADL_Main_Control_Destroy",
					out ADL_Main_Control_Destroy);
				GetDelegate("ADL_Adapter_NumberOfAdapters_Get",
					out ADL_Adapter_NumberOfAdapters_Get);
				GetDelegate("ADL_Adapter_ID_Get",
					out _ADL_Adapter_ID_Get);
				GetDelegate("ADL_Display_AdapterID_Get",
					out _ADL_Display_AdapterID_Get);
				GetDelegate("ADL_Adapter_Active_Get",
					out ADL_Adapter_Active_Get);
				GetDelegate("ADL_Overdrive5_CurrentActivity_Get",
					out ADL_Overdrive5_CurrentActivity_Get);
				GetDelegate("ADL_Overdrive5_Temperature_Get",
					out ADL_Overdrive5_Temperature_Get);
				GetDelegate("ADL_Overdrive5_FanSpeed_Get",
					out ADL_Overdrive5_FanSpeed_Get);
				GetDelegate("ADL_Overdrive5_FanSpeedInfo_Get",
					out ADL_Overdrive5_FanSpeedInfo_Get);
				GetDelegate("ADL_Overdrive5_FanSpeedToDefault_Set",
					out ADL_Overdrive5_FanSpeedToDefault_Set);
				GetDelegate("ADL_Overdrive5_FanSpeed_Set",
					out ADL_Overdrive5_FanSpeed_Set);
			}

			static ADL()
			{
				CreateDelegates("atiadlxx");
			}

			private ADL() { }

			public static int ADL_Main_Control_Create(int enumConnectedAdapters)
			{
				try
				{
					try
					{
						return _ADL_Main_Control_Create(Main_Memory_Alloc,
							enumConnectedAdapters);
					}
					catch
					{
						CreateDelegates("atiadlxy");
						return _ADL_Main_Control_Create(Main_Memory_Alloc,
							enumConnectedAdapters);
					}
				}
				catch
				{
					return ADL_ERR;
				}
			}

			public static int ADL_Adapter_AdapterInfo_Get(ADLAdapterInfo[] info)
			{
				int elementSize = Marshal.SizeOf(typeof(ADLAdapterInfo));
				int size = info.Length * elementSize;
				IntPtr ptr = Marshal.AllocHGlobal(size);
				int result = _ADL_Adapter_AdapterInfo_Get(ptr, size);
				for (int i = 0; i < info.Length; i++)
					info[i] = (ADLAdapterInfo)
						Marshal.PtrToStructure((IntPtr)((long)ptr + i * elementSize),
							typeof(ADLAdapterInfo));
				Marshal.FreeHGlobal(ptr);

				// the ADLAdapterInfo.VendorID field reported by ADL is wrong on 
				// Windows systems (parse error), so we fix this here
				for (int i = 0; i < info.Length; i++)
				{
					// try Windows UDID format
					Match m = Regex.Match(info[i].UDID, "PCI_VEN_([A-Fa-f0-9]{1,4})&.*");
					if (m.Success && m.Groups.Count == 2)
					{
						info[i].VendorID = Convert.ToInt32(m.Groups[1].Value, 16);
						continue;
					}
					// if above failed, try Unix UDID format
					m = Regex.Match(info[i].UDID, "[0-9]+:[0-9]+:([0-9]+):[0-9]+:[0-9]+");
					if (m.Success && m.Groups.Count == 2)
					{
						info[i].VendorID = Convert.ToInt32(m.Groups[1].Value, 10);
					}
				}

				return result;
			}

			public static int ADL_Adapter_ID_Get(int adapterIndex,
				out int adapterID)
			{
				try
				{
					return _ADL_Adapter_ID_Get(adapterIndex, out adapterID);
				}
				catch (EntryPointNotFoundException)
				{
					try
					{
						return _ADL_Display_AdapterID_Get(adapterIndex, out adapterID);
					}
					catch (EntryPointNotFoundException)
					{
						adapterID = 1;
						return ADL_OK;
					}
				}
			}

			private delegate IntPtr ADL_Main_Memory_AllocDelegate(int size);

			// create a Main_Memory_Alloc delegate and keep it alive
			private static ADL_Main_Memory_AllocDelegate Main_Memory_Alloc =
				delegate (int size)
				{
					return Marshal.AllocHGlobal(size);
				};

			private static void Main_Memory_Free(IntPtr buffer)
			{
				if (IntPtr.Zero != buffer)
					Marshal.FreeHGlobal(buffer);
			}
		}
	}

	#endregion

	#region NVAPI

	/*

	  This Source Code Form is subject to the terms of the Mozilla Public
	  License, v. 2.0. If a copy of the MPL was not distributed with this
	  file, You can obtain one at http://mozilla.org/MPL/2.0/.

	  Copyright (C) 2009-2012 Michael Möller <mmoeller@openhardwaremonitor.org>
		Copyright (C) 2011 Christian Vallières

	*/

	namespace OpenHardwareMonitor.Hardware.Nvidia
	{

		internal enum NvStatus
		{
			OK = 0,
			ERROR = -1,
			LIBRARY_NOT_FOUND = -2,
			NO_IMPLEMENTATION = -3,
			API_NOT_INTIALIZED = -4,
			INVALID_ARGUMENT = -5,
			NVIDIA_DEVICE_NOT_FOUND = -6,
			END_ENUMERATION = -7,
			INVALID_HANDLE = -8,
			INCOMPATIBLE_STRUCT_VERSION = -9,
			HANDLE_INVALIDATED = -10,
			OPENGL_CONTEXT_NOT_CURRENT = -11,
			NO_GL_EXPERT = -12,
			INSTRUMENTATION_DISABLED = -13,
			EXPECTED_LOGICAL_GPU_HANDLE = -100,
			EXPECTED_PHYSICAL_GPU_HANDLE = -101,
			EXPECTED_DISPLAY_HANDLE = -102,
			INVALID_COMBINATION = -103,
			NOT_SUPPORTED = -104,
			PORTID_NOT_FOUND = -105,
			EXPECTED_UNATTACHED_DISPLAY_HANDLE = -106,
			INVALID_PERF_LEVEL = -107,
			DEVICE_BUSY = -108,
			NV_PERSIST_FILE_NOT_FOUND = -109,
			PERSIST_DATA_NOT_FOUND = -110,
			EXPECTED_TV_DISPLAY = -111,
			EXPECTED_TV_DISPLAY_ON_DCONNECTOR = -112,
			NO_ACTIVE_SLI_TOPOLOGY = -113,
			SLI_RENDERING_MODE_NOTALLOWED = -114,
			EXPECTED_DIGITAL_FLAT_PANEL = -115,
			ARGUMENT_EXCEED_MAX_SIZE = -116,
			DEVICE_SWITCHING_NOT_ALLOWED = -117,
			TESTING_CLOCKS_NOT_SUPPORTED = -118,
			UNKNOWN_UNDERSCAN_CONFIG = -119,
			TIMEOUT_RECONFIGURING_GPU_TOPO = -120,
			DATA_NOT_FOUND = -121,
			EXPECTED_ANALOG_DISPLAY = -122,
			NO_VIDLINK = -123,
			REQUIRES_REBOOT = -124,
			INVALID_HYBRID_MODE = -125,
			MIXED_TARGET_TYPES = -126,
			SYSWOW64_NOT_SUPPORTED = -127,
			IMPLICIT_SET_GPU_TOPOLOGY_CHANGE_NOT_ALLOWED = -128,
			REQUEST_USER_TO_CLOSE_NON_MIGRATABLE_APPS = -129,
			OUT_OF_MEMORY = -130,
			WAS_STILL_DRAWING = -131,
			FILE_NOT_FOUND = -132,
			TOO_MANY_UNIQUE_STATE_OBJECTS = -133,
			INVALID_CALL = -134,
			D3D10_1_LIBRARY_NOT_FOUND = -135,
			FUNCTION_NOT_FOUND = -136
		}

		internal enum NvThermalController
		{
			NONE = 0,
			GPU_INTERNAL,
			ADM1032,
			MAX6649,
			MAX1617,
			LM99,
			LM89,
			LM64,
			ADT7473,
			SBMAX6649,
			VBIOSEVT,
			OS,
			UNKNOWN = -1,
		}

		internal enum NvThermalTarget
		{
			NONE = 0,
			GPU = 1,
			MEMORY = 2,
			POWER_SUPPLY = 4,
			BOARD = 8,
			ALL = 15,
			UNKNOWN = -1
		};

		[StructLayout(LayoutKind.Sequential, Pack = 8)]
		internal struct NvSensor
		{
			public NvThermalController Controller;
			public uint DefaultMinTemp;
			public uint DefaultMaxTemp;
			public uint CurrentTemp;
			public NvThermalTarget Target;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 8)]
		internal struct NvGPUThermalSettings
		{
			public uint Version;
			public uint Count;
			[MarshalAs(UnmanagedType.ByValArray,
				SizeConst = NVAPI.MAX_THERMAL_SENSORS_PER_GPU)]
			public NvSensor[] Sensor;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct NvDisplayHandle
		{
			private readonly IntPtr ptr;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct NvPhysicalGpuHandle
		{
			private readonly IntPtr ptr;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 8)]
		internal struct NvClocks
		{
			public uint Version;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = NVAPI.MAX_CLOCKS_PER_GPU)]
			public uint[] Clock;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 8)]
		internal struct NvPState
		{
			public bool Present;
			public int Percentage;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 8)]
		internal struct NvPStates
		{
			public uint Version;
			public uint Flags;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = NVAPI.MAX_PSTATES_PER_GPU)]
			public NvPState[] PStates;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 8)]
		internal struct NvUsages
		{
			public uint Version;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = NVAPI.MAX_USAGES_PER_GPU)]
			public uint[] Usage;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 8)]
		internal struct NvCooler
		{
			public int Type;
			public int Controller;
			public int DefaultMin;
			public int DefaultMax;
			public int CurrentMin;
			public int CurrentMax;
			public int CurrentLevel;
			public int DefaultPolicy;
			public int CurrentPolicy;
			public int Target;
			public int ControlType;
			public int Active;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 8)]
		internal struct NvGPUCoolerSettings
		{
			public uint Version;
			public uint Count;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = NVAPI.MAX_COOLER_PER_GPU)]
			public NvCooler[] Cooler;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 8)]
		internal struct NvLevel
		{
			public int Level;
			public int Policy;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 8)]
		internal struct NvGPUCoolerLevels
		{
			public uint Version;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = NVAPI.MAX_COOLER_PER_GPU)]
			public NvLevel[] Levels;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 8)]
		internal struct NvMemoryInfo
		{
			public uint Version;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst =
				NVAPI.MAX_MEMORY_VALUES_PER_GPU)]
			public uint[] Values;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 8)]
		internal struct NvDisplayDriverVersion
		{
			public uint Version;
			public uint DriverVersion;
			public uint BldChangeListNum;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = NVAPI.SHORT_STRING_MAX)]
			public string BuildBranch;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = NVAPI.SHORT_STRING_MAX)]
			public string Adapter;
		}

		internal class NVAPI
		{

			public const int MAX_PHYSICAL_GPUS = 64;
			public const int SHORT_STRING_MAX = 64;

			public const int MAX_THERMAL_SENSORS_PER_GPU = 3;
			public const int MAX_CLOCKS_PER_GPU = 0x120;
			public const int MAX_PSTATES_PER_GPU = 8;
			public const int MAX_USAGES_PER_GPU = 33;
			public const int MAX_COOLER_PER_GPU = 20;
			public const int MAX_MEMORY_VALUES_PER_GPU = 5;

			public static readonly uint GPU_THERMAL_SETTINGS_VER = (uint)
																   Marshal.SizeOf(typeof(NvGPUThermalSettings)) | 0x10000;
			public static readonly uint GPU_CLOCKS_VER = (uint)
														 Marshal.SizeOf(typeof(NvClocks)) | 0x20000;
			public static readonly uint GPU_PSTATES_VER = (uint)
														  Marshal.SizeOf(typeof(NvPStates)) | 0x10000;
			public static readonly uint GPU_USAGES_VER = (uint)
														 Marshal.SizeOf(typeof(NvUsages)) | 0x10000;
			public static readonly uint GPU_COOLER_SETTINGS_VER = (uint)
																  Marshal.SizeOf(typeof(NvGPUCoolerSettings)) | 0x20000;
			public static readonly uint GPU_MEMORY_INFO_VER = (uint)
															  Marshal.SizeOf(typeof(NvMemoryInfo)) | 0x20000;
			public static readonly uint DISPLAY_DRIVER_VERSION_VER = (uint)
																	 Marshal.SizeOf(typeof(NvDisplayDriverVersion)) | 0x10000;
			public static readonly uint GPU_COOLER_LEVELS_VER = (uint)
																Marshal.SizeOf(typeof(NvGPUCoolerLevels)) | 0x10000;

			private delegate IntPtr nvapi_QueryInterfaceDelegate(uint id);
			private delegate NvStatus NvAPI_InitializeDelegate();
			private delegate NvStatus NvAPI_GPU_GetFullNameDelegate(
				NvPhysicalGpuHandle gpuHandle, StringBuilder name);

			public delegate NvStatus NvAPI_GPU_GetThermalSettingsDelegate(
				NvPhysicalGpuHandle gpuHandle, int sensorIndex,
				ref NvGPUThermalSettings nvGPUThermalSettings);
			public delegate NvStatus NvAPI_EnumNvidiaDisplayHandleDelegate(int thisEnum,
				ref NvDisplayHandle displayHandle);
			public delegate NvStatus NvAPI_GetPhysicalGPUsFromDisplayDelegate(
				NvDisplayHandle displayHandle, [Out] NvPhysicalGpuHandle[] gpuHandles,
				out uint gpuCount);
			public delegate NvStatus NvAPI_EnumPhysicalGPUsDelegate(
				[Out] NvPhysicalGpuHandle[] gpuHandles, out int gpuCount);
			public delegate NvStatus NvAPI_GPU_GetTachReadingDelegate(
				NvPhysicalGpuHandle gpuHandle, out int value);
			public delegate NvStatus NvAPI_GPU_GetAllClocksDelegate(
				NvPhysicalGpuHandle gpuHandle, ref NvClocks nvClocks);
			public delegate NvStatus NvAPI_GPU_GetPStatesDelegate(
				NvPhysicalGpuHandle gpuHandle, ref NvPStates nvPStates);
			public delegate NvStatus NvAPI_GPU_GetUsagesDelegate(
				NvPhysicalGpuHandle gpuHandle, ref NvUsages nvUsages);
			public delegate NvStatus NvAPI_GPU_GetCoolerSettingsDelegate(
				NvPhysicalGpuHandle gpuHandle, int coolerIndex,
				ref NvGPUCoolerSettings nvGPUCoolerSettings);
			public delegate NvStatus NvAPI_GPU_SetCoolerLevelsDelegate(
				NvPhysicalGpuHandle gpuHandle, int coolerIndex,
				ref NvGPUCoolerLevels NvGPUCoolerLevels);
			public delegate NvStatus NvAPI_GPU_GetMemoryInfoDelegate(
				NvDisplayHandle displayHandle, ref NvMemoryInfo nvMemoryInfo);
			public delegate NvStatus NvAPI_GetDisplayDriverVersionDelegate(
				NvDisplayHandle displayHandle, [In, Out] ref NvDisplayDriverVersion
					nvDisplayDriverVersion);
			public delegate NvStatus NvAPI_GetInterfaceVersionStringDelegate(
				StringBuilder version);
			public delegate NvStatus NvAPI_GPU_GetPCIIdentifiersDelegate(
				NvPhysicalGpuHandle gpuHandle, out uint deviceId, out uint subSystemId,
				out uint revisionId, out uint extDeviceId);

			private static readonly bool available;
			private static readonly nvapi_QueryInterfaceDelegate nvapi_QueryInterface;
			private static readonly NvAPI_InitializeDelegate NvAPI_Initialize;
			private static readonly NvAPI_GPU_GetFullNameDelegate
				_NvAPI_GPU_GetFullName;
			private static readonly NvAPI_GetInterfaceVersionStringDelegate
				_NvAPI_GetInterfaceVersionString;

			public static readonly NvAPI_GPU_GetThermalSettingsDelegate
				NvAPI_GPU_GetThermalSettings;
			public static readonly NvAPI_EnumNvidiaDisplayHandleDelegate
				NvAPI_EnumNvidiaDisplayHandle;
			public static readonly NvAPI_GetPhysicalGPUsFromDisplayDelegate
				NvAPI_GetPhysicalGPUsFromDisplay;
			public static readonly NvAPI_EnumPhysicalGPUsDelegate
				NvAPI_EnumPhysicalGPUs;
			public static readonly NvAPI_GPU_GetTachReadingDelegate
				NvAPI_GPU_GetTachReading;
			public static readonly NvAPI_GPU_GetAllClocksDelegate
				NvAPI_GPU_GetAllClocks;
			public static readonly NvAPI_GPU_GetPStatesDelegate
				NvAPI_GPU_GetPStates;
			public static readonly NvAPI_GPU_GetUsagesDelegate
				NvAPI_GPU_GetUsages;
			public static readonly NvAPI_GPU_GetCoolerSettingsDelegate
				NvAPI_GPU_GetCoolerSettings;
			public static readonly NvAPI_GPU_SetCoolerLevelsDelegate
				NvAPI_GPU_SetCoolerLevels;
			public static readonly NvAPI_GPU_GetMemoryInfoDelegate
				NvAPI_GPU_GetMemoryInfo;
			public static readonly NvAPI_GetDisplayDriverVersionDelegate
				NvAPI_GetDisplayDriverVersion;
			public static readonly NvAPI_GPU_GetPCIIdentifiersDelegate
				NvAPI_GPU_GetPCIIdentifiers;

			private NVAPI() { }

			public static NvStatus NvAPI_GPU_GetFullName(NvPhysicalGpuHandle gpuHandle,
				out string name)
			{
				StringBuilder builder = new StringBuilder(SHORT_STRING_MAX);
				NvStatus status;
				if (_NvAPI_GPU_GetFullName != null)
					status = _NvAPI_GPU_GetFullName(gpuHandle, builder);
				else
					status = NvStatus.FUNCTION_NOT_FOUND;
				name = builder.ToString();
				return status;
			}

			public static NvStatus NvAPI_GetInterfaceVersionString(out string version)
			{
				StringBuilder builder = new StringBuilder(SHORT_STRING_MAX);
				NvStatus status;
				if (_NvAPI_GetInterfaceVersionString != null)
					status = _NvAPI_GetInterfaceVersionString(builder);
				else
					status = NvStatus.FUNCTION_NOT_FOUND;
				version = builder.ToString();
				return status;
			}

			private static string GetDllName()
			{
				if (IntPtr.Size == 4)
				{
					return "nvapi.dll";
				}
				else
				{
					return "nvapi64.dll";
				}
			}

			private static void GetDelegate<T>(uint id, out T newDelegate)
				where T : class
			{
				IntPtr ptr = nvapi_QueryInterface(id);
				if (ptr != IntPtr.Zero)
				{
					newDelegate =
						Marshal.GetDelegateForFunctionPointer(ptr, typeof(T)) as T;
				}
				else
				{
					newDelegate = null;
				}
			}

			static NVAPI()
			{
				DllImportAttribute attribute = new DllImportAttribute(GetDllName());
				attribute.CallingConvention = CallingConvention.Cdecl;
				attribute.PreserveSig = true;
				attribute.EntryPoint = "nvapi_QueryInterface";
				PInvokeDelegateFactory.CreateDelegate(attribute,
					out nvapi_QueryInterface);

				try
				{
					GetDelegate(0x0150E828, out NvAPI_Initialize);
				}
				catch (DllNotFoundException) { return; }
				catch (EntryPointNotFoundException) { return; }
				catch (ArgumentNullException) { return; }

				if (NvAPI_Initialize() == NvStatus.OK)
				{
					GetDelegate(0xE3640A56, out NvAPI_GPU_GetThermalSettings);
					GetDelegate(0xCEEE8E9F, out _NvAPI_GPU_GetFullName);
					GetDelegate(0x9ABDD40D, out NvAPI_EnumNvidiaDisplayHandle);
					GetDelegate(0x34EF9506, out NvAPI_GetPhysicalGPUsFromDisplay);
					GetDelegate(0xE5AC921F, out NvAPI_EnumPhysicalGPUs);
					GetDelegate(0x5F608315, out NvAPI_GPU_GetTachReading);
					GetDelegate(0x1BD69F49, out NvAPI_GPU_GetAllClocks);
					GetDelegate(0x60DED2ED, out NvAPI_GPU_GetPStates);
					GetDelegate(0x189A1FDF, out NvAPI_GPU_GetUsages);
					GetDelegate(0xDA141340, out NvAPI_GPU_GetCoolerSettings);
					GetDelegate(0x891FA0AE, out NvAPI_GPU_SetCoolerLevels);
					GetDelegate(0x774AA982, out NvAPI_GPU_GetMemoryInfo);
					GetDelegate(0xF951A4D1, out NvAPI_GetDisplayDriverVersion);
					GetDelegate(0x01053FA5, out _NvAPI_GetInterfaceVersionString);
					GetDelegate(0x2DDFB66E, out NvAPI_GPU_GetPCIIdentifiers);

					available = true;
				}
			}

			public static bool IsAvailable
			{
				get { return available; }
			}

		}
	}

	#endregion

	#region Pair

	/*

	  This Source Code Form is subject to the terms of the Mozilla Public
	  License, v. 2.0. If a copy of the MPL was not distributed with this
	  file, You can obtain one at http://mozilla.org/MPL/2.0/.

	  Copyright (C) 2011 Michael Möller <mmoeller@openhardwaremonitor.org>

	*/

	namespace OpenHardwareMonitor.Collections
	{

		internal struct Pair<F, S>
		{
			private F first;
			private S second;

			public Pair(F first, S second)
			{
				this.first = first;
				this.second = second;
			}

			public F First
			{
				get { return first; }
				set { first = value; }
			}

			public S Second
			{
				get { return second; }
				set { second = value; }
			}

			public override int GetHashCode()
			{
				return (first != null ? first.GetHashCode() : 0) ^
					   (second != null ? second.GetHashCode() : 0);
			}
		}
	}

	#endregion

	#region PInvokeDelegateFactory

	/*

	  This Source Code Form is subject to the terms of the Mozilla Public
	  License, v. 2.0. If a copy of the MPL was not distributed with this
	  file, You can obtain one at http://mozilla.org/MPL/2.0/.

	  Copyright (C) 2009-2012 Michael Mцller <mmoeller@openhardwaremonitor.org>

	*/

	namespace OpenHardwareMonitor.Hardware
	{

		internal static class PInvokeDelegateFactory
		{

			private static readonly ModuleBuilder moduleBuilder =
				AssemblyBuilder.DefineDynamicAssembly(
					new AssemblyName("PInvokeDelegateFactoryInternalAssembly"),
					AssemblyBuilderAccess.Run).DefineDynamicModule(
					"PInvokeDelegateFactoryInternalModule");

			private static readonly IDictionary<Pair<DllImportAttribute, Type>, Type> wrapperTypes =
				new Dictionary<Pair<DllImportAttribute, Type>, Type>();

			public static void CreateDelegate<T>(DllImportAttribute dllImportAttribute,
				out T newDelegate) where T : class
			{
				Type wrapperType;
				Pair<DllImportAttribute, Type> key =
					new Pair<DllImportAttribute, Type>(dllImportAttribute, typeof(T));
				wrapperTypes.TryGetValue(key, out wrapperType);

				if (wrapperType == null)
				{
					wrapperType = CreateWrapperType(typeof(T), dllImportAttribute);
					wrapperTypes.Add(key, wrapperType);
				}

				newDelegate = Delegate.CreateDelegate(typeof(T), wrapperType,
					dllImportAttribute.EntryPoint) as T;
			}


			private static Type CreateWrapperType(Type delegateType,
				DllImportAttribute dllImportAttribute)
			{

				TypeBuilder typeBuilder = moduleBuilder.DefineType(
					"PInvokeDelegateFactoryInternalWrapperType" + wrapperTypes.Count);

				MethodInfo methodInfo = delegateType.GetMethod("Invoke");

				ParameterInfo[] parameterInfos = methodInfo.GetParameters();
				int parameterCount = parameterInfos.GetLength(0);

				Type[] parameterTypes = new Type[parameterCount];
				for (int i = 0; i < parameterCount; i++)
					parameterTypes[i] = parameterInfos[i].ParameterType;

				MethodBuilder methodBuilder = typeBuilder.DefinePInvokeMethod(
					dllImportAttribute.EntryPoint, dllImportAttribute.Value,
					MethodAttributes.Public | MethodAttributes.Static |
					MethodAttributes.PinvokeImpl, CallingConventions.Standard,
					methodInfo.ReturnType, parameterTypes,
					dllImportAttribute.CallingConvention,
					dllImportAttribute.CharSet);

				foreach (ParameterInfo parameterInfo in parameterInfos)
					methodBuilder.DefineParameter(parameterInfo.Position + 1,
						parameterInfo.Attributes, parameterInfo.Name);

				if (dllImportAttribute.PreserveSig)
					methodBuilder.SetImplementationFlags(MethodImplAttributes.PreserveSig);

				return typeBuilder.CreateType();
			}
		}
	}

	#endregion

	#endregion
}