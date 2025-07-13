using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using Dalamud.Utility;
using ImGuiNET;

namespace SimpleTimers.Windows;

public class AddTimerWindow : Window, IDisposable
{
    private Configuration configuration;

    private Timer timer;
    private string timerName;

    private readonly string[] reminder_time = ["Seconds", "Minutes"];
    private int reminder_idx = 1;
    private bool snap_to_hour = false;
    private int snap_hours = 1;

    // We give this window a hidden ID using ##
    // So that the user will see "My Amazing Window" as window title,
    // but for ImGui the ID is "My Amazing Window##With a hidden ID"
    public AddTimerWindow(Plugin plugin)
        : base("Add a timer##add_timer", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(900, 450),
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

        ImGui.Text("Intervalo del Timer");

        var days = timer.days;
        if (ImGui.InputInt("Dias", ref days))
            timer.days = days;

        var hours = timer.hours;
        if (ImGui.InputInt("Horas", ref hours))
            timer.hours = Math.Min(hours, 23);

        var minutes = timer.minutes;
        if (ImGui.InputInt("Minutos", ref minutes))
            timer.minutes = Math.Min(minutes, 59);

        var seconds = timer.seconds;
        if (ImGui.InputInt("Segundos", ref seconds))
            timer.seconds = Math.Min(seconds, 59);


        ImGui.Spacing();

        var start_time = DateTime.Now;

        ImGui.Checkbox("Empezar en la siguiente hora.", ref snap_to_hour);

        if (snap_to_hour)
        {
            start_time = new DateTime(start_time.Year, start_time.Month, start_time.Day, start_time.Hour + snap_hours, 0, 0);
            ImGui.InputInt($"Empezar la alarma cuando sean las {start_time.ToString()}", ref snap_hours);
        }

        ImGui.Spacing();

        var play_sound = timer.play_sound;
        if (ImGui.Checkbox("Usar un sonido cuando la alarma salta.", ref play_sound))
        {
            timer.play_sound = play_sound;
        }

        var play_sound_reminder = timer.play_sound_reminder;
        if (ImGui.Checkbox("Usar un sonido cuando el recordatorio de la alarma salta.", ref play_sound_reminder))
        {
            timer.play_sound_reminder = play_sound_reminder;
        }

        if (reminder_idx == 0)
        {
            var seconds_reminder = (int)timer.remind_time.TotalSeconds;
            if (ImGui.InputInt("Recordar antes de X segundos", ref seconds_reminder))
                timer.remind_time = new TimeSpan(0, 0, seconds_reminder);
        }
        else
        {
            var minutes_reminder = (int)timer.remind_time.TotalMinutes;
            if (ImGui.InputInt("Recordar antes de X minutos", ref minutes_reminder))
                timer.remind_time = new TimeSpan(0, minutes_reminder, 0);
        }
        ImGui.Spacing();

        ImGui.ListBox("Unidad del recordatorio", ref reminder_idx, reminder_time, reminder_time.Length);

        ImGui.Spacing();

        ImGui.InputText("Nombre del Timer", ref timerName, 128);

        ImGui.Spacing();

        if (ImGui.Button("Crear"))
        {
            if (timer.seconds == 0 && timer.minutes == 0 && timer.hours == 0 && timer.days == 0)
                return;

            if (timerName.IsNullOrWhitespace())
                return;

            timer.start = start_time;
            timer.last = timer.start;

            Plugin.Log.Debug($"Adding timer {timerName}");

            timer.reminder_fired = false;
            configuration.timers.Add(timerName, timer);
            configuration.Save();
            timerName = "";
            timer = new();
            timer.remind_time = new TimeSpan(0, 1, 0);
            timer.reminder_fired = false;
            snap_hours = 1;
            snap_to_hour = false;
            reminder_idx = 1;

            Toggle();
        }
    }
}
