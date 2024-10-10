using System;
using System.Timers;


public class WorkerTimer
{
    public WorkerTimer(int interval ) 
    {
        this.interval = interval;
    }
    public void start(System.Timers.ElapsedEventHandler onTimedEvent )
    {
        sysTimer = new System.Timers.Timer(this.interval);
        sysTimer.Elapsed += onTimedEvent; 
        sysTimer.AutoReset = true;
        sysTimer.Enabled = true;
    }

    private static System.Timers.Timer sysTimer = null!;
    private int interval;
    
    
}
