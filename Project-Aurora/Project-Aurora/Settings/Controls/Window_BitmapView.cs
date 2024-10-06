using System;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using AuroraRgb.Bitmaps;
using AuroraRgb.Bitmaps.GdiPlus;
using AuroraRgb.EffectsEngine;
using Color = System.Windows.Media.Color;
using Image = System.Windows.Controls.Image;

namespace AuroraRgb.Settings.Controls;

/// <summary>
/// A window that shows the Bitmap 
/// </summary>
public class Window_BitmapView : Window
{

    private static Window_BitmapView? winBitmapView;
    private Image imgBitmap = new();

    /// <summary>
    /// Opens the bitmap debug window if not already opened. If opened bring it to the foreground. 
    /// </summary>
    public static void Open()
    {
        if (winBitmapView == null)
        {
            winBitmapView = new Window_BitmapView();
            winBitmapView.UpdateLayout();
            winBitmapView.Show();
        }
        else
        {
            winBitmapView.Activate();
        }
    }

    private Window_BitmapView()
    {
        Closed += WinBitmapView_Closed;
        ResizeMode = ResizeMode.CanResize;

        SetBinding(TopmostProperty, new Binding("BitmapDebugTopMost") { Source = Global.Configuration });

        Title = "Bitmap Debug Window";
        Background = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
        Global.effengine.NewLayerRender += Effengine_NewLayerRender;

        imgBitmap.SnapsToDevicePixels = true;
        imgBitmap.HorizontalAlignment = HorizontalAlignment.Stretch;
        imgBitmap.VerticalAlignment = VerticalAlignment.Stretch;
        imgBitmap.MinWidth = Effects.Canvas.Width;
        imgBitmap.MinHeight = Effects.Canvas.Height;

        Content = imgBitmap;
    }

    private void Effengine_NewLayerRender(IAuroraBitmap bitmap)
    {
        try
        {
            var gdiBitmap = GdiBitmap.GetGdiBitmap(bitmap);
            Dispatcher.Invoke(() =>
            {
                lock (gdiBitmap)
                {
                    using var memory = new MemoryStream();
                    gdiBitmap.Bitmap.Save(memory, ImageFormat.Png);
                    memory.Position = 0;
                    var bitmapimage = new BitmapImage();
                    bitmapimage.BeginInit();
                    bitmapimage.StreamSource = memory;
                    bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapimage.EndInit();

                    imgBitmap.Source = bitmapimage;
                }
            }, DispatcherPriority.Render);
        }
        catch (Exception ex)
        {
            Global.logger.Warning(ex, "New layer render error");
        }
    }

    private void WinBitmapView_Closed(object? sender, EventArgs e)
    {
        Global.effengine.NewLayerRender -= Effengine_NewLayerRender;

        //Set the winBitmapView instance to null if it got closed
        if (winBitmapView!= null && winBitmapView.Equals(this))
            winBitmapView = null;
    }
}