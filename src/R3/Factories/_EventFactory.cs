﻿
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;

namespace R3;

public static partial class Event
{
    // TODO: this is working space, will remove this file after complete.

    // TODO: Defer, DeferAsync, FromAsync, FromAsyncPattern, FromEvent, FromEventPattern, Start, Using, Create
    // Timer, Interval, TimerFrame, IntervalFrame, ToObservable(ToEvent)



    // ToAsyncEnumerable?
    // ToEvent
    // ToEventPattern



    // AsObservable
    // AsSingleUnitObservable

    // AsUnitObservable
    // AsUniResult
    // AsNeverComplete

    // TODO: use SystemDefault

    public static Event<Unit> EveryUpdate()
    {
        return new EveryUpdate(EventSystem.DefaultFrameProvider, CancellationToken.None, cancelImmediately: false);
    }

    public static Event<Unit> EveryUpdate(CancellationToken cancellationToken)
    {
        return new EveryUpdate(EventSystem.DefaultFrameProvider, cancellationToken, cancelImmediately: false);
    }

    public static Event<Unit> EveryUpdate(FrameProvider frameProvider)
    {
        return new EveryUpdate(frameProvider, CancellationToken.None, cancelImmediately: false);
    }

    public static Event<Unit> EveryUpdate(FrameProvider frameProvider, CancellationToken cancellationToken)
    {
        return new EveryUpdate(frameProvider, cancellationToken, cancelImmediately: false);
    }

    public static Event<Unit> EveryUpdate(FrameProvider frameProvider, CancellationToken cancellationToken, bool cancelImmediately)
    {
        return new EveryUpdate(frameProvider, cancellationToken, cancelImmediately: cancelImmediately);
    }
}



internal sealed class EveryUpdate(FrameProvider frameProvider, CancellationToken cancellationToken, bool cancelImmediately) : Event<Unit>
{
    protected override IDisposable SubscribeCore(Subscriber<Unit> subscriber)
    {
        var runner = new EveryUpdateRunnerWorkItem(subscriber, cancellationToken, cancelImmediately);
        frameProvider.Register(runner);
        return runner;
    }

    class EveryUpdateRunnerWorkItem : IFrameRunnerWorkItem, IDisposable
    {
        Subscriber<Unit> subscriber;
        CancellationToken cancellationToken;
        CancellationTokenRegistration cancellationTokenRegistration;
        bool isDisposed;

        public EveryUpdateRunnerWorkItem(Subscriber<Unit> subscriber, CancellationToken cancellationToken, bool cancelImmediately)
        {
            this.subscriber = subscriber;
            this.cancellationToken = cancellationToken;

            if (cancelImmediately && cancellationToken.CanBeCanceled)
            {
                cancellationTokenRegistration = cancellationToken.UnsafeRegister(static state =>
                {
                    var s = (EveryUpdateRunnerWorkItem)state!;
                    s.subscriber.OnCompleted();
                    s.Dispose();
                }, this);
            }
        }

        public bool MoveNext(long frameCount)
        {
            if (isDisposed)
            {
                return false;
            }

            if (cancellationToken.IsCancellationRequested)
            {
                subscriber.OnCompleted();
                Dispose();
                return false;
            }

            subscriber.OnNext(default);
            return true;
        }

        public void Dispose()
        {
            isDisposed = true;
            cancellationTokenRegistration.Dispose();
        }
    }
}
