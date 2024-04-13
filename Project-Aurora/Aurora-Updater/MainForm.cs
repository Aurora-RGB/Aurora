using System;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Aurora_Updater.Data;
using Aurora_Updater.Properties;

namespace Aurora_Updater;

public partial class MainForm : Form
{
    private readonly System.Windows.Forms.Timer _progressTimer = new();
    private readonly Thread _updaterThread;
    private CancellationTokenSource _cancellation = new();

    public MainForm()
    {
        _updaterThread = new Thread(() => StaticStorage.Manager.RetrieveUpdate().Wait());
        InitializeComponent();
        StaticStorage.Manager.ClearLog();
        Icon = Resources.Aurora_updater;
    }

    private void Form1_Shown(object? sender, EventArgs e)
    {
        _progressTimer.Interval = 1500;
        _progressTimer.Tick += UpdateProgressTick;

        var logs = StaticStorage.Manager.GetObservable();
        logs.CollectionChanged += LogAdded;
        
        _progressTimer.Enabled = true;
        _progressTimer.Start();
        _updaterThread.IsBackground = true;
        _updaterThread.Start();
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    private CancellationTokenSource CancelPreviousTick()
    {
        _cancellation.Cancel();
        var cancelled = new CancellationTokenSource();
        _cancellation = cancelled;
        return cancelled;
    }

    private void LogAdded(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems == null)
        {
            return;
        }
        var tickCancellation = CancelPreviousTick();

        Invoke(() =>
        {
            if (tickCancellation.IsCancellationRequested)
            {
                return;
            }
            foreach (var o in e.NewItems)
            {
                if (tickCancellation.IsCancellationRequested)
                {
                    return;
                }
                
                var log = (LogEntry)o;
                richtextUpdateLog.SelectionStart = richtextUpdateLog.TextLength;
                richtextUpdateLog.SelectionLength = 0;

                richtextUpdateLog.SelectionColor = log.GetColor();
                richtextUpdateLog.AppendText(log.Message);
                richtextUpdateLog.AppendText("\r\n");
            }

            richtextUpdateLog.SelectionColor = richtextUpdateLog.ForeColor;
        });
    }

    private void UpdateProgressTick(object? sender, EventArgs args)
    {
        Task.Run(() =>
        {
            var tickCancellation = CancelPreviousTick();

            var logs = StaticStorage.Manager.GetLog();

            var stringBuilder = new StringBuilder();
            for (var i = 0; i < logs.Length && !tickCancellation.IsCancellationRequested; i++)
            {
                stringBuilder.Append(logs[i].Message);
                stringBuilder.Append("\r\n");
            }

            var text = stringBuilder.ToString();

            if (tickCancellation.IsCancellationRequested)
            {
                return;
            }

            Invoke(() =>
            {
                update_progress.Value = StaticStorage.Manager.GetTotalProgress();

                richtextUpdateLog.SelectionStart = 0;
                richtextUpdateLog.SelectionLength = richtextUpdateLog.TextLength;
                richtextUpdateLog.SelectedText = text;

                var colorStart = 0;
                for (var i = 0; i < logs.Length && !tickCancellation.IsCancellationRequested; i++)
                {
                    richtextUpdateLog.SelectionStart = colorStart;
                    richtextUpdateLog.SelectionLength = richtextUpdateLog.TextLength;
                    colorStart += richtextUpdateLog.TextLength;

                    richtextUpdateLog.SelectionColor = logs[i].GetColor();
                }
            });
        });
    }
}