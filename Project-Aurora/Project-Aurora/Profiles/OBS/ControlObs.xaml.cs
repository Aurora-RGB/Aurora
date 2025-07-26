using System.Windows;
using System.Windows.Controls;

namespace AuroraRgb.Profiles.OBS;

public partial class ControlObs
{
    private readonly ObsApplication _profile;
    
    public ControlObs(Application profile)
    {
        _profile = (ObsApplication)profile;
        InitializeComponent();
    }

    private void WsUrl_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        Global.Configuration.ObsWebsocketUrl = WsUrl.Text;
    }

    private void SavePassword_OnClick(object sender, RoutedEventArgs e)
    {
        Global.SensitiveData.ObsWebSocketPassword = ObsPasswordTextBox.Password;
        ObsPasswordTextBox.Password = string.Empty;
    }

    private async void Reconnect_Clicked(object sender, RoutedEventArgs e)
    {
        if (_profile.Config.Event is not GameEventObs obsEvent)
        {
            return;
        }
        
        ReconnectStatus.Text = "Reconnecting to OBS WebSocket...";
        var result = await obsEvent.ReconnectWebSocket();
        ReconnectStatus.Text = result;
    }
}