using System.Windows;
using System.Windows.Media;
using AuroraRgb.Utils;

namespace AuroraRgb.Controls;

public partial class Control_UnsecureDrivers
{
    public Control_UnsecureDrivers()
    {
        InitializeComponent();
    }

    private void Control_UnsecureDrivers_OnLoaded(object sender, RoutedEventArgs e)
    {
        if (!IsVisible)
        {
            return;
        }

        UpdateInpOutStatus();
        UpdateWinRing0Status();
    }

    private void UpdateInpOutStatus()
    {
        var inpoutExists = UnsecureDrivers.InpOutDriverExists();
        if (inpoutExists)
        {
            InpOut64Status.Foreground = Brushes.Red;
            InpOut64Status.Text = "Exists";
            InpOutDeleteButton.Visibility = Visibility.Visible;
        }
        else
        {
            InpOut64Status.Foreground = Brushes.Green;
            InpOut64Status.Text = "Not installed";
            InpOutDeleteButton.Visibility = Visibility.Hidden;
        }
    }

    private void UpdateWinRing0Status()
    {
        var winRing0Exists = UnsecureDrivers.WinRing0DriverExists();
        if (winRing0Exists)
        {
            WinRing0Status.Foreground = Brushes.Coral;
            WinRing0Status.Text = "Exists";
            WinRing0DeleteButton.Visibility = Visibility.Visible;
        }
        else
        {
            WinRing0Status.Foreground = Brushes.Green;
            WinRing0Status.Text = "Not installed";
            WinRing0DeleteButton.Visibility = Visibility.Hidden;
        }
    }

    private void InpOutDeleteButton_OnClick(object sender, RoutedEventArgs e)
    {
        DeleteInpOut();
    }

    private void WinRing0DeleteButton_OnClick(object sender, RoutedEventArgs e)
    {
        DeleteWinRing0();
    }

    private void DeleteInpOut()
    {
        UnsecureDrivers.DeleteDriver(UnsecureDrivers.InpOutDriverName);
        UpdateInpOutStatus();
    }

    private void DeleteWinRing0()
    {
        UnsecureDrivers.DeleteDriver(UnsecureDrivers.WinRing0DriverName);
        UpdateWinRing0Status();
    }
}