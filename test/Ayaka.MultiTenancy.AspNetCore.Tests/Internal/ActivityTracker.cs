// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.AspNetCore.Tests.Internal;

using System.Diagnostics;

internal sealed class ActivityTracker : IDisposable
{
    private readonly List<Activity> _startedActivities = [];
    private readonly List<Activity> _stoppedActivities = [];

    public ActivityTracker()
    {
        Source = new ActivitySource(Path.GetRandomFileName());

        Listener = new ActivityListener
        {
            ShouldListenTo = s => ReferenceEquals(s, Source),
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
            ActivityStarted = a => _startedActivities.Add(a),
            ActivityStopped = a => _stoppedActivities.Add(a)
        };

        ActivitySource.AddActivityListener(Listener);
    }

    public ActivitySource Source { get; }

    public ActivityListener Listener { get; }

    public IReadOnlyList<Activity> StartedActivities => _startedActivities;

    public IReadOnlyList<Activity> StoppedActivities => _stoppedActivities;

    public void Dispose()
    {
        Source.Dispose();
        Listener.Dispose();
    }
}
