// Copyright (c) Raphael Strotz. All rights reserved.

namespace Ayaka.MultiTenancy.AspNetCore.Tests.Internal;

using System.Collections.Concurrent;
using System.Diagnostics;

internal sealed class ActivityTracker : IDisposable
{
    private static readonly TimeSpan _stopTimeout = TimeSpan.FromSeconds(10);

    private readonly ConcurrentQueue<Activity> _startedActivities = new();
    private readonly ConcurrentQueue<Activity> _stoppedActivities = new();
    private readonly TaskCompletionSource _activityStopped = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public ActivityTracker()
    {
        Source = new ActivitySource(Path.GetRandomFileName());

        Listener = new ActivityListener
        {
            ShouldListenTo = s => ReferenceEquals(s, Source),
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
            ActivityStarted = a => _startedActivities.Enqueue(a),
            ActivityStopped = a =>
            {
                _stoppedActivities.Enqueue(a);
                _activityStopped.TrySetResult();
            }
        };

        ActivitySource.AddActivityListener(Listener);
    }

    public ActivitySource Source { get; }

    public ActivityListener Listener { get; }

    public IReadOnlyList<Activity> StartedActivities => [.. _startedActivities];

    public IReadOnlyList<Activity> StoppedActivities => [.. _stoppedActivities];

    /// <summary>
    ///     Waits until the first activity has stopped.
    /// </summary>
    /// <remarks>
    ///     The test server returns the response to the client before it stops the request activity,
    ///     so asserting on <see cref="StoppedActivities"/> right after a request races with the server.
    /// </remarks>
    public Task WaitForStoppedActivityAsync(CancellationToken cancellationToken)
        => _activityStopped.Task.WaitAsync(_stopTimeout, cancellationToken);

    public void Dispose()
    {
        Source.Dispose();
        Listener.Dispose();
    }
}
