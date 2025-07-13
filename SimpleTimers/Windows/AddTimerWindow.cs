using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace SimpleTimers.Windows;

public class AddTimerWindow : Window, IDisposable
{
    private Configuration configuration;

    private Timer timer;
    private string timerName;

    private string[] reminder_time = ["Seconds", "Minutes"];
    private int reminder_idx = 1;

    // We give this window a hidden ID using ##
    // So that the user will see "My Amazing Window" as window title,
    // but for ImGui the ID is "My Amazing Window##With a hidden ID"
    public AddTimerWindow(Plugin plugin)
        : base("Add a timer##add_timer", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(600, 350),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        configuration = plugin.Configuration;
        timer = new();
        timer.remind_time = new TimeSpan(0, 1, 0);
        timer.reminder_fired = false;
        timerName = "";
    }

    public void Dispose() { }

    public override void Draw()
    {

        ImGui.Text("Time Interval");

        var days = timer.days;
        if (ImGui.InputInt("Days", ref days))
            timer.days = days;

        var hours = timer.hours;
        if (ImGui.InputInt("Hours", ref hours))
            timer.hours = Math.Min(hours, 23);

        var minutes = timer.minutes;
        if (ImGui.InputInt("Minutes", ref minutes))
            timer.minutes = Math.Min(minutes, 59);

        var seconds = timer.seconds;
        if (ImGui.InputInt("Seconds", ref seconds))
            timer.seconds = Math.Min(seconds, 59);


        ImGui.Spacing();

        ImGui.InputText("Reminder Name", ref timerName, 128);

        ImGui.Spacing();

        if (reminder_idx == 0)
        {
            var seconds_reminder = (int)timer.remind_time.TotalSeconds;
            if (ImGui.InputInt("Reminder before in seconds", ref seconds_reminder))
                timer.remind_time = new TimeSpan(0, 0, seconds_reminder);
        }
        else
        {
            var minutes_reminder = (int)timer.remind_time.TotalMinutes;
            if (ImGui.InputInt("Reminder before in minutes", ref minutes_reminder))
                timer.remind_time = new TimeSpan(0, minutes_reminder, 0);
        }
        ImGui.Spacing();

        ImGui.ListBox("Reminder unit", ref reminder_idx, reminder_time, reminder_time.Length);

        ImGui.Spacing();




        if (ImGui.Button("Create"))
        {
            if (timer.seconds == 0 && timer.minutes == 0 && timer.hours == 0 && timer.days == 0)
                return;

            timer.start = DateTime.Now;
            timer.last = timer.start;

            Plugin.Log.Debug($"Adding timer {timerName}");

            timer.reminder_fired = false;
            configuration.timers.Add(timerName, timer);
            configuration.Save();
            timerName = "";
            timer = new();
            timer.remind_time = new TimeSpan(0, 1, 0);
            timer.reminder_fired = false;

            Toggle();
        }
    }
}
