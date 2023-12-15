﻿namespace R3;

public static partial class EventExtensions
{
    public static Observable<T> OnErrorResumeAsFailure<T>(this Observable<T> source)
    {
        return new OnErrorResumeAsFailure<T>(source);
    }
}

internal class OnErrorResumeAsFailure<T>(Observable<T> source) : Observable<T>
{
    protected override IDisposable SubscribeCore(Observer<T> subscriber)
    {
        return source.Subscribe(new _OnErrorAsComplete(subscriber));
    }

    sealed class _OnErrorAsComplete(Observer<T> subscriber) : Observer<T>
    {
        protected override void OnNextCore(T value)
        {
            subscriber.OnNext(value);
        }

        protected override void OnErrorResumeCore(Exception error)
        {
            subscriber.OnCompleted(error);
        }

        protected override void OnCompletedCore(Result complete)
        {
            subscriber.OnCompleted(complete);
        }
    }
}