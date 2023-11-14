using System;

namespace Raven35.Changes.Subscription.Infrastructure.Observers
{
    public interface IObserverFactory
    {
        IObserver<T>? TryLoadObserver<T>();
    }
}
