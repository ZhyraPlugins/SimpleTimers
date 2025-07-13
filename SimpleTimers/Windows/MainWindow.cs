using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using Dalamud.Utility;
using Humanizer;
using ImGuiNET;
using Lumina.Excel.Sheets;

namespace SimpleTimers.Windows;

public class MainWindow : Window, IDisposable
{
    private Plugin plugin;

    // We give this window a hidden ID using ##
    // So that the user will see "My Amazing Window" as window title,
    // but for ImGui the ID is "My Amazing Window##With a hidden ID"
    public MainWindow(Plugin plugin, string goatImagePath)
        : base("SimpleTimers##simplertimers_main", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(500, 300),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        this.plugin = plugin;
    }

    public void Dispose() { }

    public override void Draw()
    {

        ImGui.Text("Timers");

        using (var table = ImRaii.Table("Timers", 7))
        {
            ImGui.TableSetupColumn("Name");
            ImGui.TableNextColumn();
            ImGui.TableSetupColumn("Time Remaining");
            ImGui.TableNextColumn();
            ImGui.TableSetupColumn("Interval");
            ImGui.TableNextColumn();
            ImGui.TableSetupColumn("Last");
            ImGui.TableNextColumn();
            ImGui.TableSetupColumn("Reminder");
            ImGui.TableNextColumn();
            ImGui.TableSetupColumn("Restart");
            ImGui.TableNextColumn();
            ImGui.TableSetupColumn("Remove");
            ImGui.TableHeadersRow();


            var now = DateTime.Now;

            List<string> toRemove = [];

            foreach (var timer in plugin.Configuration.timers)
            {
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.Text(timer.Key);

                ImGui.TableNextColumn();
                var remaining = timer.Value.RemaningTime().Humanize(precision: 3, minUnit: Humanizer.Localisation.TimeUnit.Second);
                ImGui.Text(remaining);

                ImGui.TableNextColumn();
                ImGui.Text(timer.Value.GetInterval());

                ImGui.TableNextColumn();
                ImGui.Text(timer.Value.last.ToString());

                ImGui.TableNextColumn();
                ImGui.Text(timer.Value.remind_time.Humanize());

                ImGui.TableNextColumn();
                if (ImGui.Button("Restart"))
                {
                    timer.Value.start = DateTime.Now;
                    timer.Value.last = timer.Value.start;
                    timer.Value.reminder_fired = false;
                    plugin.Configuration.Save();
                }

                ImGui.TableNextColumn();
                if (ImGui.Button("Remove"))
                {
                    toRemove.Add(timer.Key);
                }
            }

            foreach (var rem in toRemove)
            {
                plugin.Configuration.timers.Remove(rem);
                plugin.Configuration.Save();
            }
        }

        if (ImGui.Button("Add timer"))
        {
            plugin.Configuration.Save();
            plugin.ToggleAddTimerWindow();
        }
    }
}
