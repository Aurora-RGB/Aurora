using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;

namespace Aurora_Updater;

public class AuroraInterface
{
    private readonly byte[] _end = "\n"u8.ToArray();

    public async Task RestartDeviceManager()
    {
        await SendAuroraCommand("restartDevices");
    }

    public async Task ShutdownDeviceManager()
    {
        await SendAuroraCommand("quitDevices");
    }

    public async Task RestartAurora()
    {
        await SendAuroraCommand("restartAurora");
    }

    private Task SendAuroraCommand(string command)
    {
        return SendCommand(Encoding.UTF8.GetBytes(command), "aurora\\interface");
    }

    private async Task SendCommand(byte[] command, string pipeName)
    {
        var client = new NamedPipeClientStream(".", pipeName, PipeDirection.Out, PipeOptions.Asynchronous);
        await client.ConnectAsync(2000);
        if (!client.IsConnected)
            return;

        client.Write(command, 0, command.Length);
        client.Write(_end, 0, _end.Length);

        client.Flush();
        client.Close();
    }
}