using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AuroraRgb.Utils;
using KeySequence = AuroraRgb.Settings.KeySequence;

namespace AuroraRgb.Profiles.Minecraft.Layers {
    /// <summary>
    /// Interaction logic for Control_MinecraftBackgroundLayer.xaml
    /// </summary>
    public partial class Control_MinecraftBackgroundLayer : UserControl {

        private bool settingsSet = false;

        public Control_MinecraftBackgroundLayer() {
            InitializeComponent();
        }

        public Control_MinecraftBackgroundLayer(MinecraftBackgroundLayerHandler context) {
            InitializeComponent();
            DataContext = context;
        }

        private MinecraftBackgroundLayerHandler Context => (MinecraftBackgroundLayerHandler)DataContext;

        public void SetSettings() {
            if (DataContext is MinecraftBackgroundLayerHandler && !settingsSet) {
                ColorPicker_DayTime.SelectedColor = ColorUtils.DrawingColorToMediaColor(Context.Properties._PrimaryColor ?? System.Drawing.Color.Empty);
                ColorPicker_NightTime.SelectedColor = ColorUtils.DrawingColorToMediaColor(Context.Properties.SecondaryColor);
                KeySequence_Keys.Sequence = Context.Properties._Sequence;
                settingsSet = true;
            }
        }

        private void UserControl_Loaded(object? sender, RoutedEventArgs e) {
            SetSettings();
            Loaded -= UserControl_Loaded;
        }

        private void ColorPicker_DayTime_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e) {
            if (IsLoaded && settingsSet && DataContext is MinecraftBackgroundLayerHandler && e.NewValue.HasValue)
                Context.Properties._PrimaryColor = ColorUtils.MediaColorToDrawingColor(e.NewValue.Value);
        }

        private void ColorPicker_NightTime_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e) {
            if (IsLoaded && settingsSet && DataContext is MinecraftBackgroundLayerHandler && e.NewValue.HasValue)
                Context.Properties.SecondaryColor = ColorUtils.MediaColorToDrawingColor(e.NewValue.Value);
        }

        private void KeySequence_Keys_SequenceUpdated(object sender, RoutedPropertyChangedEventArgs<KeySequence> e) {
            if (IsLoaded && settingsSet && DataContext is MinecraftBackgroundLayerHandler)
                Context.Properties._Sequence = e.NewValue;
        }
    }
}
