using Raven.Client;
using Raven35.Changes.Subscription.Domain.Models;
using System;
using System.Text.Json;

namespace Raven35.Changes.Subscription.Infrastructure.Observers
{
    public class MobileDeviceObserver : IObserver<MobileDevice>
    {
        private readonly IDocumentStore _store;

        public MobileDeviceObserver(IDocumentStore store)
        {
            _store = store;
        }

        public void OnCompleted()
        {
            // release all subscriptions
            // note: dangerous as other applications may be using subscriptions
            var configs = _store.Subscriptions.GetSubscriptions(0, 10);

            foreach (var config in configs)
            {
                _store.Subscriptions.Release(config.SubscriptionId);
            }

            Console.WriteLine("{0} completed", typeof(MobileDeviceObserver));
        }

        public void OnError(Exception error)
        {
            Console.WriteLine($"Error: {error}");
        }

        public void OnNext(MobileDevice value)
        {
            Console.WriteLine("{0} change: {1}", typeof(MobileDeviceObserver), JsonSerializer.Serialize(value));
        }
    }
}
