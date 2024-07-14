namespace MultipleScreenPowersave.App;

using System;

public class MainWindowHandleOfProcess
{
    public MainWindowHandleOfProcess(int processId, IntPtr mainWindowHandle, long lastSeenTicks)
    {
        ProcessId = processId;
        MainWindowHandle = mainWindowHandle;
        LastSeenTicks = lastSeenTicks;
    }

    public int ProcessId { get; }

    public IntPtr MainWindowHandle { get; }

    public long LastSeenTicks { get; }
}
