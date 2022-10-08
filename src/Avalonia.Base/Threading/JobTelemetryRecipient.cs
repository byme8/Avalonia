using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Avalonia.Threading;

public class JobTelemetryRecipient : IJobTelemetryRecipient
{
    private readonly Stopwatch _jobTime;
    private readonly Stopwatch _globalTime;
    private List<(DispatcherPriority Priority, int Duration)> _tempHistory;

    public event Action? FrameReady; 
        
    public JobTelemetryRecipient(int framesToRemember)
    {
        _jobTime = new Stopwatch();
        _globalTime = Stopwatch.StartNew();
        CurrentFrameIndex = 0;
        _tempHistory = new List<(DispatcherPriority Priority, int Duration)>();
        History = new Dictionary<DispatcherPriority, int>[framesToRemember];
    }

    public Dictionary<DispatcherPriority, int>?[] History { get; }

    public int CurrentFrameIndex { get; private set; }

    public void OnFrameStart()
    {
        _jobTime.Start();
    }

    public void OnFrameEnd(DispatcherPriority priority)
    {
        _tempHistory.Add((priority, (int)_jobTime.ElapsedMilliseconds));
        _jobTime.Reset();

        if (_globalTime.Elapsed.TotalMilliseconds > 16)
        {
            var history = _tempHistory
                .GroupBy(o => o.Priority, o => o.Duration)
                .ToDictionary(o => o.Key, o => o.Sum());

            History[CurrentFrameIndex] = history;
                
            _tempHistory.Clear();
            CurrentFrameIndex = (CurrentFrameIndex + 1) % History.Length;
            _globalTime.Restart();
            FrameReady?.Invoke();
        }
    }
}
