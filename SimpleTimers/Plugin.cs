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

        // you might normally want to embed resources and load them from the manifest stream
        var goatImagePath = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "goat.png");

        ConfigWindow = new ConfigWindow(this);
        MainWindow = new MainWindow(this, goatImagePath);
        AddTimerWindow = new AddTimerWindow(this);

        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(AddTimerWindow);
        WindowSystem.AddWindow(MainWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Manage your own customizable timers"
        });

        PluginInterface.UiBuilder.Draw += DrawUI;

        // This adds a button to the plugin installer entry of this plugin which allows
        // to toggle the display status of the configuration ui
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;

        // Adds another button that is doing the same but for the main ui of the plugin
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;

        LastUpdate = DateTime.Now;
        Framework.Update += OnFrameworkUpdate;

        // Add a simple message to the log with level set to information
        // Use /xllog to open the log window in-game
        // Example Output: 00:57:54.959 | INF | [SamplePlugin] ===A cool log message from Sample Plugin===
        Log.Information($"===A cool log message from {PluginInterface.Manifest.Name}===");
    }

    private void OnFrameworkUpdate(IFramework framework)
    {
        if (!ClientState.IsLoggedIn) return;

        var now = DateTime.Now;
        var elapsed = now - LastUpdate;

        if (elapsed.TotalMilliseconds < 500)
            return;


        LastUpdate = now;

        foreach (var timer in Configuration.timers)
        {
            if (!timer.Value.enabled)
                continue;

            var timerValue = timer.Value;
            var next = timerValue.Next();

            if (!timerValue.reminder_fired && (next - timerValue.remind_time) <= now)
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
                Chat.Print($"Alarma: {timer.Key}", "SimpleTimers", 0x0f);
                timerValue.reminder_fired = false;
                timerValue.last = now;

                if (timerValue.play_sound)
                {
                    UIGlobals.PlaySoundEffect(0x26);
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
