using Dalamud.Configuration;
using Dalamud.Plugin;
using Microsoft.VisualBasic;
using System;

namespace SimpleTimers;

[Serializable]
public class Timer
{
    public int seconds { get; set; } = 0;
    public int minutes { get; set; } = 0;
    public int hours { get; set; } = 0;

    public int days { get; set; } = 0;

    public TimeSpan remind_time { get; set; }

    public bool reminder_fired { get; set; }
    public DateTime start { get; set; }
    public DateTime last { get; set; }

    public bool enabled { get; set; } = true;

    public bool play_sound = true;
    public bool play_sound_reminder = true;


    public DateTime Next()
    {
        if (last >= DateTime.Now)
            return last;

        var next = last;
        next = next.AddSeconds(seconds);
        next = next.AddMinutes(minutes);
        next = next.AddHours(hours);
        next = next.AddDays(days);

        return next;
    }

    public TimeSpan RemaningTime()
    {
        var next = Next();

        return next - DateTime.Now;
    }

    public string GetInterval()
    {
        var i = "Cada";

        if (days > 0)
        {
            i += $" {days} dias";
        }

        if (hours > 0)
        {
            i += $" {hours} horas";
        }

        if (minutes > 0)
        {
            i += $" {minutes} minutos";
        }

        if (seconds > 0)
        {
            i += $" {seconds} segundos";
        }

        return i;
    }
}
