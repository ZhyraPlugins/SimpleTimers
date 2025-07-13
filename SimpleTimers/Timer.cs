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


    public DateTime Next()
    {
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
        var i = "Every";

        if (days > 0)
        {
            i += $" {days} days";
        }

        if (hours > 0)
        {
            i += $" {hours} hours";
        }

        if (minutes > 0)
        {
            i += $" {minutes} minutes";
        }

        if (seconds > 0)
        {
            i += $" {seconds} seconds";
        }

        return i;
    }
}
