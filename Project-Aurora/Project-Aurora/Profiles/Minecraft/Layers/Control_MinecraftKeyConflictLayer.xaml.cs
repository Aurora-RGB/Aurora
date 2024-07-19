using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AuroraRgb.Utils;

namespace AuroraRgb.Profiles.Minecraft.Layers {
    /// <summary>
    /// Interaction logic for Control_MinecraftKeyConflictLayer.xaml
    /// </summary>
    public partial class Control_MinecraftKeyConflictLayer : UserControl {

        private bool settingSet = false;

        public Control_MinecraftKeyConflictLayer() {
            InitializeComponent();
        }

        public Control_MinecraftKeyConflictLayer(MinecraftKeyConflictLayerHandler context) {
            InitializeComponent();
            DataContext = context;
        }

        private MinecraftKeyConflictLayerHandler Context => (MinecraftKeyConflictLayerHandler)DataContext;
        private bool CanSet => IsLoaded && settingSet && DataContext is MinecraftKeyConflictLayerHandler;

        private void SetSettings() {
            if (DataContext is MinecraftKeyConflictLayerHandler && !settingSet) {
                NoConflict_ColorPicker.SelectedColor = ColorUtils.DrawingColorToMediaColor(Context.Properties._PrimaryColor ?? System.Drawing.Color.Empty);
                HardConflict_ColorPicker.SelectedColor = ColorUtils.DrawingColorToMediaColor(Context.Properties.SecondaryColor);
                SoftConflict_ColorPicker.SelectedColor = ColorUtils.DrawingColorToMediaColor(Context.Properties._TertiaryColor ?? System.Drawing.Color.Empty);
                settingSet = true;
            }
        }

        private void UserControl_Loaded(object? sender, RoutedEventArgs e) {
            SetSettings();
            Loaded -= UserControl_Loaded;
        }

        private void NoConflict_ColorPicker_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e) {
            if (CanSet && e.NewValue.HasValue)
                Context.Properties._PrimaryColor = ColorUtils.MediaColorToDrawingColor(e.NewValue.Value);
        }

        private void HardConflict_ColorPicker_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e) {
            if (CanSet && e.NewValue.HasValue)
                Context.Properties.SecondaryColor = ColorUtils.MediaColorToDrawingColor(e.NewValue.Value);
        }

        private void SoftConflict_ColorPicker_SelectedColorChanged(object? sender, RoutedPropertyChangedEventArgs<Color?> e) {
            if (CanSet && e.NewValue.HasValue)
                Context.Properties._TertiaryColor = ColorUtils.MediaColorToDrawingColor(e.NewValue.Value);
        }
    }
}
