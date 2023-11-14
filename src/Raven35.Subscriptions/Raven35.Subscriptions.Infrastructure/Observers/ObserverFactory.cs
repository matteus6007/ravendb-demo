using Raven.Client;
using System;
using System.Collections.Generic;

namespace Raven35.Changes.Subscription.Infrastructure.Observers
{

    public class ObserverFactory : IObserverFactory
    {
        private readonly Dictionary<Type, object> _observers = new Dictionary<Type, object>();
        private readonly IDocumentStore _store;

        public ObserverFactory(IDocumentStore store)
        {
            _store = store;
        }

        public void AddObserver<T>(IObserver<T> observer)
        {
            _observers.Add(typeof(T), observer);
        }

        public void Register()
        {
            AddObserver(new DocumentChangeNotificationObserver(_store));
            AddObserver(new MobileDeviceObserver(_store));
        }

        public IObserver<T>? TryLoadObserver<T>()
        {
            if (_observers.TryGetValue(typeof(T), out var observer))
            {
                return (IObserver<T>)observer;
            }

            return null;
        }
    }
}
