using Dalamud.Configuration;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;

namespace SimpleTimers;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 1;

    public bool IsConfigWindowMovable { get; set; } = true;

    public OrderedDictionary<string, Timer> timers { get; set; } = [];

    public DateTime last_save { get; set; } = DateTime.Now;

    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
