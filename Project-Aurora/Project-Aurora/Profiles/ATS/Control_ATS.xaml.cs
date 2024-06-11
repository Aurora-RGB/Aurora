using System.Diagnostics;
using System.IO;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using AuroraRgb.Profiles.ETS2.GSI;
using AuroraRgb.Utils.Steam;

namespace AuroraRgb.Profiles.ATS;

/// <summary>
/// Interaction logic for Control_ATS.xaml
/// </summary>
public partial class Control_ATS
{
    private readonly Application _profileManager;

    private BlinkerComboBoxStates _selectedBlinkerMode = BlinkerComboBoxStates.None;
    private readonly Timer _blinkerTimer = new(500);

    public Control_ATS(Application profile) {
        _profileManager = profile;

        InitializeComponent();

        PopulateComboBoxes();

        _blinkerTimer.Elapsed += BlinkerTimer_Elapsed;
    }

    private GameState_ETS2 GameState => (GameState_ETS2)_profileManager.Config.Event.GameState;

    private void visit_ets2ts_button_Click(object? sender, RoutedEventArgs e) {
        Process.Start("explorer", "https://github.com/Funbit/ets2-telemetry-server");
    }

    /// <summary>
    /// Installs either the 32-bit or 64-bit version of the Telemetry Server DLL.
    /// </summary>
    /// <param name="x64">Install 64-bit (true) or 32-bit (false)?</param>
    private bool InstallDll(bool x64) {
        var gamePath = SteamUtils.GetGamePath(270880);
        if (string.IsNullOrWhiteSpace(gamePath))
            return false;
            
        var installPath = Path.Combine(gamePath, "bin", x64 ? "win_x64" : "win_x86", "plugins", "ets2-telemetry-server.dll");

        if (!File.Exists(installPath))
            Directory.CreateDirectory(Path.GetDirectoryName(installPath));

        using var cfgStream = File.Create(installPath);
        var sourceDll = x64 ? Properties.Resources.ets2_telemetry_server_x64 : Properties.Resources.ets2_telemetry_server_x86;
        cfgStream.Write(sourceDll, 0, sourceDll.Length);

        return true;
    }

    private void install_button_Click(object? sender, RoutedEventArgs e) {
        if (!InstallDll(true)) {
            MessageBox.Show("64-bit ETS2 Telemetry Server DLL installed failed.");
        } else if (!InstallDll(false)) {
            MessageBox.Show("32-bit ETS2 Telemetry Server DLL installed failed.");
        } else {
            MessageBox.Show("ETS2 Telemetry Server DLLs installed successfully.");
        }
    }

    private void uninstall_button_Click(object? sender, RoutedEventArgs e) {
        var gamePath = SteamUtils.GetGamePath(270880);
        if (string.IsNullOrWhiteSpace(gamePath)) return;
        var x86Path = Path.Combine(gamePath, "bin", "win_x86", "plugins", "ets2-telemetry-server.dll");
        var x64Path = Path.Combine(gamePath, "bin", "win_x64", "plugins", "ets2-telemetry-server.dll");
        if (File.Exists(x64Path))
            File.Delete(x64Path);
        if (File.Exists(x86Path))
            File.Delete(x86Path);
        MessageBox.Show("ETS2 Telemetry Server DLLs uninstalled successfully.");
    }

    // -------------------- //
    // --- Preview Tab --- //
    // ------------------ //
    private void PopulateComboBoxes() {
        truckPowerState.Items.Add(TruckPowerComboBoxStates.Off);
        truckPowerState.Items.Add(TruckPowerComboBoxStates.Electric);
        truckPowerState.Items.Add(TruckPowerComboBoxStates.Engine);

        lights.Items.Add(LightComboBoxStates.Off);
        lights.Items.Add(LightComboBoxStates.ParkingLights);
        lights.Items.Add(LightComboBoxStates.LowBeam);
        lights.Items.Add(LightComboBoxStates.HighBeam);

        blinkers.Items.Add(BlinkerComboBoxStates.None);
        blinkers.Items.Add(BlinkerComboBoxStates.Left);
        blinkers.Items.Add(BlinkerComboBoxStates.Right);
        blinkers.Items.Add(BlinkerComboBoxStates.Hazard);
    }

    private void truckPowerState_SelectionChanged(object? sender, SelectionChangedEventArgs e) {
        var selected = (TruckPowerComboBoxStates)(sender as ComboBox).SelectedItem;
        GameState._memdat.value.electricEnabled = boolToByte(selected != TruckPowerComboBoxStates.Off); // Electric is true for both electric and engine states
        GameState._memdat.value.engineEnabled = boolToByte(selected == TruckPowerComboBoxStates.Engine);
    }

    private void lights_SelectionChanged(object? sender, SelectionChangedEventArgs e) {
        var selected = (LightComboBoxStates)(sender as ComboBox).SelectedItem;
        GameState._memdat.value.lightsParking = boolToByte(selected == LightComboBoxStates.ParkingLights); // Parking light only comes on when the parking lights are on
        GameState._memdat.value.lightsBeamLow = boolToByte(selected is LightComboBoxStates.LowBeam or LightComboBoxStates.HighBeam); // Low beam light is on when the low beam is on OR when high beam is on
        GameState._memdat.value.lightsBeamHigh = boolToByte(selected == LightComboBoxStates.HighBeam); // Highbeam only on when high beam is on
    }

    private void blinkers_SelectionChanged(object? sender, SelectionChangedEventArgs e) {
        _selectedBlinkerMode = (BlinkerComboBoxStates)(sender as ComboBox).SelectedItem;
        if (_selectedBlinkerMode == BlinkerComboBoxStates.None) {
            _blinkerTimer.Stop();
            setLeftBlinker(false);
            setRightBlinker(false);
        } else {
            _blinkerTimer.Stop(); // Stop then start to reset the timer (so if we change from left to right for example, we don't have a partial phase)
            _blinkerTimer.Start();
            // Immediately start the blinkers (as happens in the actual game and real life)
            setLeftBlinker(_selectedBlinkerMode is BlinkerComboBoxStates.Left or BlinkerComboBoxStates.Hazard);
            setRightBlinker(_selectedBlinkerMode is BlinkerComboBoxStates.Right or BlinkerComboBoxStates.Hazard);
        }
    }

    private void BlinkerTimer_Elapsed(object? sender, ElapsedEventArgs e) {
        // When the timer ticks, toggle the hazard lights based on the selected blinker mode
        if (_selectedBlinkerMode is BlinkerComboBoxStates.Left or BlinkerComboBoxStates.Hazard)
            setLeftBlinker();
        if (_selectedBlinkerMode is BlinkerComboBoxStates.Right or BlinkerComboBoxStates.Hazard)
            setRightBlinker();
    }

    /// <summary>Sets or toggles the left blinker flag. Set v to null to toggle or a boolean to set to that value.</summary>
    private void setLeftBlinker(bool? v=null) {
        var newState = v.HasValue ? v.Value : GameState._memdat.value.blinkerLeftOn == 0;
        GameState._memdat.value.blinkerLeftOn = (byte)(newState ? 1 : 0);
    }

    /// <summary>Sets or toggles the left blinker flag. Set v to null to toggle or a boolean to set to that value.</summary>
    private void setRightBlinker(bool? v=null) {
        var newState = v.HasValue ? v.Value : GameState._memdat.value.blinkerRightOn == 0;
        GameState._memdat.value.blinkerRightOn = (byte)(newState ? 1 : 0);
    }

    private void beacon_Checked(object? sender, RoutedEventArgs e) {
        GameState._memdat.value.lightsBeacon = boolToByte((sender as CheckBox).IsChecked);
    }

    private void trailerAttached_Checked(object? sender, RoutedEventArgs e) {
        GameState._memdat.value.trailer_attached = boolToByte((sender as CheckBox).IsChecked);
    }

    private void cruiseControl_Checked(object? sender, RoutedEventArgs e) {
        GameState._memdat.value.cruiseControlSpeed = boolToByte((sender as CheckBox).IsChecked);
    }

    private void throttleSlider_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<double> e) {
        GameState._memdat.value.gameThrottle = (float)(sender as Slider).Value;
    }

    private void brakeSlider_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<double> e) {
        GameState._memdat.value.gameBrake = (float)(sender as Slider).Value;
    }

    private void engineRpmSlider_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<double> e) {
        GameState._memdat.value.engineRpm = (float)(sender as Slider).Value;
        GameState._memdat.value.engineRpmMax = 1f;
    }

    private void handbrake_Checked(object? sender, RoutedEventArgs e) {
        GameState._memdat.value.parkBrake = boolToByte((sender as CheckBox).IsChecked);
    }

    private void fuelSlider_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<double> e) {
        GameState._memdat.value.fuel = (float)(sender as Slider).Value;
        GameState._memdat.value.fuelCapacity = 1;
    }

    private void airSlider_ValueChanged(object? sender, RoutedPropertyChangedEventArgs<double> e) {
        GameState._memdat.value.airPressure = (float)(sender as Slider).Value * GameState.Truck.airPressureMax;
    }

    private byte boolToByte(bool? v) {
        return (byte)(v.HasValue && v.Value ? 1 : 0);
    }
}

enum TruckPowerComboBoxStates {
    Off, Electric, Engine
}

enum LightComboBoxStates {
    Off, ParkingLights, LowBeam, HighBeam
}

enum BlinkerComboBoxStates {
    None, Left, Right, Hazard
}