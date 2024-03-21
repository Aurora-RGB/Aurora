using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Win32.TaskScheduler;

namespace AuroraRgb.Utils;

public static class AutoStartUtils
{
    private const string StartupTaskId = "AuroraStartup";

    public static bool GetAutostartTask(out int startDelayAmount)
    {
        try
        {
            using var service = new TaskService();
            var task = service.FindTask(StartupTaskId);
            var exePath = Assembly.GetExecutingAssembly().Location.Replace(".dll", ".exe");

            var taskDefinition = task != null ? task.Definition : service.NewTask();

            //Update path of startup task
            taskDefinition.RegistrationInfo.Description = "Start Aurora on Startup";
            taskDefinition.Actions.Clear();
            taskDefinition.Actions.Add(new ExecAction(exePath, "-silent", Path.GetDirectoryName(exePath)));
            if (task != null)
            {
                startDelayAmount = task.Definition.Triggers.FirstOrDefault(t =>
                    t.TriggerType == TaskTriggerType.Logon
                ) is LogonTrigger trigger
                    ? (int)trigger.Delay.TotalSeconds
                    : 0;
            }
            else
            {
                startDelayAmount = 0;
                taskDefinition.Triggers.Add(new LogonTrigger { Enabled = true });

                taskDefinition.Principal.RunLevel = TaskRunLevel.Highest;
                taskDefinition.Settings.DisallowStartIfOnBatteries = false;
                taskDefinition.Settings.DisallowStartOnRemoteAppSession = false;
                taskDefinition.Settings.ExecutionTimeLimit = TimeSpan.Zero;
            }

            task = service.RootFolder.RegisterTaskDefinition(StartupTaskId, taskDefinition);
            return task.Enabled;
        }
        catch (Exception exc)
        {
            Global.logger.Error(exc, "Error caught when updating startup task");
            startDelayAmount = 0;
            return false;
        }
    }

    public static void SetStartupDelay(int delay)
    {
        using var service = new TaskService();
        var task = service.FindTask(StartupTaskId);
        if (task?.Definition.Triggers.FirstOrDefault(t => t.TriggerType == TaskTriggerType.Logon) is not
            LogonTrigger trigger) return;
        trigger.Delay = new TimeSpan(0, 0, delay);
        task.RegisterChanges();
    }

    public static void SetEnabled(bool isEnabled)
    {
        try
        {
            using var ts = new TaskService();
            //Find existing task
            var task = ts.FindTask(StartupTaskId);
            task.Enabled = isEnabled;
        }
        catch (Exception exc)
        {
            Global.logger.Error(exc, "RunAtWinStartup_Checked Exception: ");
        }
    }
}