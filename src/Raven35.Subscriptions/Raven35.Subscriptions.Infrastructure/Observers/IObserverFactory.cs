using System;

namespace Raven35.Changes.Subscription.Infrastructure.Observers
{
    public interface IObserverFactory
    {
        void Register();
        void AddObserver<T>(IObserver<T> observer);
        IObserver<T>? TryLoadObserver<T>();
    }
}
