using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using AuroraRgb.Settings;
using Serilog.Core;
using Constants = Common.Constants;
using MessageBox = System.Windows.MessageBox;

namespace AuroraRgb;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App
{
    public static bool Closing { get; private set; }
    private static readonly Mutex Mutex = new(false, "{C88D62B0-DE49-418E-835D-CE213D58444C}");

    private static bool IsSilent { get; set; }

    private static readonly SemaphoreSlim PreventShutdown = new(0);
    public AuroraApp? AuroraApp { get; private set; }

    protected override async void OnStartup(StartupEventArgs e)
    {
        UseArgs(e);
        CheckRunningProcesses();
        base.OnStartup(e);

        Global.Initialize();

        var currentDomain = AppDomain.CurrentDomain;
        currentDomain.AppendPrivatePath("x64");
        if (!Global.isDebug)
            currentDomain.UnhandledException += CurrentDomain_UnhandledException;

        AuroraApp = new AuroraApp(IsSilent);
        await AuroraApp.OnStartup();

        SessionEnding += (_, sessionEndingParams) =>
        {
            Global.logger.Information("Session ending. Reason: {Reason}", sessionEndingParams.ReasonSessionEnding);
            Shutdown();
            PreventShutdown.Wait();
        };
    }
    private void CheckRunningProcesses()
    {
        try
        {
            if (Mutex.WaitOne(TimeSpan.FromMilliseconds(0), true)) return;
            try
            {
                var client = new NamedPipeClientStream(
                    ".", Constants.AuroraInterfacePipe, PipeDirection.Out, PipeOptions.Asynchronous);
                client.Connect(2);
                if (!client.IsConnected)
                    throw new InvalidOperationException();
                var command = "restore"u8.ToArray();
                client.Write(command, 0, command.Length);
                client.Close();
            }
            catch
            {
                MessageBox.Show("Aurora is already running.\r\nExiting.", "Aurora - Error");
                ForceShutdownApp(0);
            }

            Closing = true;
            AuroraApp?.Dispose();
            Environment.Exit(0);
        }
        catch (AbandonedMutexException)
        {
            /* Means previous instance closed anyway */
        }
    }

    [DoesNotReturn]
    internal static void ForceShutdownApp(int exitCode)
    {
        PreventShutdown.Release();
        Environment.ExitCode = exitCode;
        Environment.Exit(exitCode);
    }

    private void UseArgs(StartupEventArgs e)
    {
        for (var i = 0; i < e.Args.Length; i++)
        {
            var arg = e.Args[i];

            switch (arg)
            {
                case "-debug":
                    Global.isDebug = true;
                    break;
                case "-restart":
                    var pid = int.Parse(e.Args[++i]);
                    try
                    {
                        var previousAuroraProcess = Process.GetProcessById(pid);
                        previousAuroraProcess.WaitForExit();
                    }
                    catch (ArgumentException) { /* process doesn't exist */ }
                    break;
                case "-minimized":
                case "-silent":
                    IsSilent = true;
                    break;
                case "-delay":
                    if (i + 1 >= e.Args.Length || !int.TryParse(e.Args[i++], CultureInfo.InvariantCulture, out var delayTime))
                        delayTime = 5000;
                    Thread.Sleep(delayTime);
                    break;
            }
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (Closing)
        {
            return;
        }
        Closing = true;
        base.OnExit(e);

        if (Global.Configuration != null)
            ConfigManager.Save(Global.Configuration);

        var forceExitTimer = StartForceExitTimer();
        if (AuroraApp != null)
        {
            var auroraShutdownTask = AuroraApp.Shutdown();
            AuroraApp.Dispose();
            await auroraShutdownTask;
        }
        (Global.logger as Logger)?.Dispose();
        forceExitTimer.GetApartmentState(); //statement just to keep referenced

        Mutex.ReleaseMutex();
        Mutex.Dispose();

        PreventShutdown.Release();
    }

    private static Thread StartForceExitTimer()
    {
        var thread = new Thread(() =>
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            Thread.Sleep(6000);
            if (stopwatch.ElapsedMilliseconds > 5000)
            {
                ForceShutdownApp(0);
            }
        })
        {
            IsBackground = true,
            Name = "Exit timer"
        };
        thread.Start();
        return thread;
    }

    private static void CurrentDomain_UnhandledException(object? sender, UnhandledExceptionEventArgs e)
    {
        var exc = (Exception)e.ExceptionObject;
        if (exc is COMException { Message: "0x88890004" })
        {
            return;
        }
        Global.logger.Fatal(exc, "Fatal Exception caught");

        if (!e.IsTerminating || Current == null || Closing)
        {
            return;
        }
        if (exc is SEHException sehException && sehException.CanResume())
        {
            return;
        }

        QuitFromError(exc);
    }

    private void App_DispatcherUnhandledException(object? sender, DispatcherUnhandledExceptionEventArgs e)
    {
        var exc = e.Exception;
        if (exc is COMException { Message: "0x88890004" })
        {
            e.Handled = true;
            return;
        }
        Global.logger.Fatal(exc, "Fatal Exception caught");

        if (!Global.isDebug)
            e.Handled = true;
        else
            throw exc;

        QuitFromError(exc);
    }

    private static void QuitFromError(Exception exc)
    {
        if (!Global.Configuration?.CloseProgramOnException ?? false) return;
        if (Closing) return;
        MessageBox.Show("Aurora fatally crashed. Please report the follow to author: \r\n\r\n" + exc,
            "Aurora has stopped working");
        //Perform exit operations
        Current?.Shutdown();
        (Global.logger as Logger)?.Dispose();
    }
}