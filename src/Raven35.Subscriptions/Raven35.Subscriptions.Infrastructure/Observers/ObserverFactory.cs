using Raven.Abstractions.Data;
using Raven35.Changes.Subscription.Domain.Models;
using System;
using System.Collections.Generic;

namespace Raven35.Changes.Subscription.Infrastructure.Observers
{

    public class ObserverFactory : IObserverFactory
    {
        private readonly Dictionary<Type, object> _observers = new Dictionary<Type, object>();

        public ObserverFactory(
            IObserver<DocumentChangeNotification> documentChangeNotificationObserver,
            IObserver<MobileDevice> mobileDeviceObserver)
        {
            AddObserver(documentChangeNotificationObserver);
            AddObserver(mobileDeviceObserver);
        }

        public IObserver<T>? TryLoadObserver<T>()
        {
            if (_observers.TryGetValue(typeof(T), out var observer))
            {
                return (IObserver<T>)observer;
            }

            return null;
        }

        private void AddObserver<T>(IObserver<T> observer)
        {
            _observers.Add(typeof(T), observer);
        }
    }
}
