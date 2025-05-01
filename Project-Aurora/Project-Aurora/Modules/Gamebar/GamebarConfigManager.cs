using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AuroraRgb.Settings;

namespace AuroraRgb.Modules.Gamebar;

public class GamebarConfigManager(GamebarConfig config)
{
    public EventHandler? ExcludedProgramsChanged;
    
    public List<string> GetExcludedPrograms()
    {
        return config.IgnoredPrograms;
    }
    
    public async Task SetExcludedPrograms(List<string> excludedPrograms)
    {
        config.IgnoredPrograms = excludedPrograms;
        ExcludedProgramsChanged?.Invoke(this, EventArgs.Empty);
        await SaveConfig();
    }
    
    private async Task SaveConfig()
    {
        await ConfigManager.SaveAsync(config);
    }
}