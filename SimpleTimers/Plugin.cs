using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using SimpleTimers.Windows;
using Dalamud.Game.ClientState.Objects;
using System;
using Humanizer;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace SimpleTimers;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static IDataManager DataManager { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;
    [PluginService] public static IFramework Framework { get; private set; } = null!;
    [PluginService] public static IChatGui Chat { get; private set; } = null!;

    private const string CommandName = "/stimers";

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("SimpleTimers");
    private ConfigWindow ConfigWindow { get; init; }
    private MainWindow MainWindow { get; init; }
    private AddTimerWindow AddTimerWindow { get; init; }

    private DateTime LastUpdate { get; set; }

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        ConfigWindow = new ConfigWindow(this);
        MainWindow = new MainWindow(this);
        AddTimerWindow = new AddTimerWindow(this);

        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(AddTimerWindow);
        WindowSystem.AddWindow(MainWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Manage your own customizable timers"
        });

        PluginInterface.UiBuilder.Draw += DrawUI;
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;

        LastUpdate = DateTime.Now;

        // update offline timers
        var now = DateTime.Now;


        foreach (var timer in Configuration.timers)
        {
            var timerValue = timer.Value;

            while (timerValue.last < now)
                timerValue.last = timerValue.Next();
        }

        Configuration.Save();

        Framework.Update += OnFrameworkUpdate;
    }

    private void OnFrameworkUpdate(IFramework framework)
    {
        if (!ClientState.IsLoggedIn) return;

        var now = DateTime.Now;
        var elapsed = now - LastUpdate;

        if (elapsed.TotalMilliseconds < 200)
            return;


        LastUpdate = now;

        foreach (var timer in Configuration.timers)
        {

            var timerValue = timer.Value;
            var next = timerValue.Next();

            if (timer.Value.enabled && !timerValue.reminder_fired && (next - timerValue.remind_time) <= now)
            {
                Chat.Print($"Recordatorio de alarma: {timer.Key}, faltan {timerValue.remind_time.Humanize()}", "SimpleTimers", 0x0f);
                timerValue.reminder_fired = true;

                if (timerValue.play_sound_reminder)
                {
                    UIGlobals.PlaySoundEffect(0x26);
                }
            }

            if (next <= now)
            {
                timerValue.reminder_fired = false;
                timerValue.last = next;

                if (timerValue.enabled)
                {
                    Chat.Print($"Alarma: {timer.Key}", "SimpleTimers", 0x0f);
                    if (timerValue.play_sound)
                    {
                        UIGlobals.PlaySoundEffect(0x26);
                    }
                }
            }
        }

        if ((now - Configuration.last_save).TotalSeconds > 60)
        {
            Configuration.last_save = now;
            Configuration.Save();
        }
    }

    public void Dispose()
    {
        Framework.Update -= OnFrameworkUpdate;

        WindowSystem.RemoveAllWindows();

        ConfigWindow.Dispose();
        MainWindow.Dispose();
        AddTimerWindow.Dispose();

        CommandManager.RemoveHandler(CommandName);

        Configuration.Save();
    }

    private void OnCommand(string command, string args)
    {
        // in response to the slash command, just toggle the display status of our main ui
        ToggleMainUI();
    }

    private void DrawUI() => WindowSystem.Draw();

    public void ToggleConfigUI() => ConfigWindow.Toggle();
    public void ToggleMainUI() => MainWindow.Toggle();
    public void ToggleAddTimerWindow() => AddTimerWindow.Toggle();
}
