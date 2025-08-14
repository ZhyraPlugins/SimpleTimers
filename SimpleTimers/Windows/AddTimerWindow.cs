using System;
using System.Numerics;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using Dalamud.Utility;
using Dalamud.Bindings.ImGui;

namespace SimpleTimers.Windows;

public class AddTimerWindow : Window, IDisposable
{
    private Configuration configuration;

    private Timer timer;
    private string timerName;

    private readonly string[] reminder_time = ["Seconds", "Minutes"];
    private int reminder_idx = 1;

    private int start_delay_hour = 0;

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

        ImGui.PushItemWidth(200);
        ImGui.InputText("Nombre del Timer", ref timerName, 128);

        ImGui.Spacing();

        ImGui.Text("Intervalo del Timer");
        ImGui.PushItemWidth(150);
        var hours = timer.hours;
        ImGui.InputInt("Horas", ref hours);
        timer.hours = hours;

        var minutes = timer.minutes;
        if (ImGui.InputInt("Minutos", ref minutes))
            timer.minutes = Math.Min(minutes, 59);

        var seconds = timer.seconds;
        if (ImGui.InputInt("Segundos", ref seconds))
            timer.seconds = Math.Min(seconds, 59);


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

        ImGui.Spacing();

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

        using (var x = ImRaii.ListBox("Unidad del recordatorio", new Vector2(150, 50)))
        {
            if (ImGui.Selectable("Segundos"))
            {
                reminder_idx = 0;
            }
            if (ImGui.Selectable("Minutos"))
            {
                reminder_idx = 1;
            }
        }

        ImGui.InputInt("Atrasar la primera alarma en (horas):", ref start_delay_hour);

        ImGui.Spacing();

        ImGui.PopItemWidth();

        var now = DateTime.Now;
        var start_time = now;

        if (hours != 0)
        {
            start_time = new DateTime(start_time.Year, start_time.Month, start_time.Day, 0, 0, 0);
            start_time = start_time.AddHours(multipleOf(now.Hour + 1, hours));
        }
        else if (minutes != 0)
        {
            start_time = new DateTime(start_time.Year, start_time.Month, start_time.Day, start_time.Hour, 0, 0);
            start_time = start_time.AddMinutes(multipleOf(now.Minute + 1, minutes));
        }
        else if (seconds != 0)
        {
            start_time = new DateTime(start_time.Year, start_time.Month, start_time.Day, start_time.Hour, start_time.Minute, 0);
            start_time = start_time.AddSeconds(multipleOf(now.Second + 1, seconds));
        }

        if (start_delay_hour != 0)
        {
            start_time = start_time.AddHours(start_delay_hour);
        }


        ImGui.Text($"La alarma empezará a las {start_time}");

        if (ImGui.Button("Crear"))
        {
            if (timer.seconds == 0 && timer.minutes == 0 && timer.hours == 0)
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
            reminder_idx = 1;

            Toggle();
        }
    }

    private int multipleOf(int value, int multiple)
    {
        if (value % multiple == 0)
            return value;
        return value + (multiple - (value % multiple));
    }
}
