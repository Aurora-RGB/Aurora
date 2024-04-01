using System.IO;
using System.Windows;
using System.Windows.Forms;

namespace AuroraRgb.Settings.Layers.Controls;

public partial class Control_ImageLayer
{
    public Control_ImageLayer() {
        InitializeComponent();
    }

    public Control_ImageLayer(ImageLayerHandler context) {
        InitializeComponent();
        DataContext = context;
    }

    private void SelectImage_Click(object? sender, RoutedEventArgs e) {
        var dialog = new OpenFileDialog {
            Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.png, *.gif, *.bmp, *.tiff, *.tif) | *.jpg; *.jpeg; *.jpe; *.png; *.gif; *.bmp; *.tiff; *.tif",
            Title = "Please select an image."
        };
        if (dialog.ShowDialog() == DialogResult.OK && File.Exists(dialog.FileName))
            ((ImageLayerHandler)DataContext).Properties.ImagePath = dialog.FileName;
    }
}