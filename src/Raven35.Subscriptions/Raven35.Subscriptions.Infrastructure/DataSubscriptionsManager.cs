using Raven.Abstractions.Data;
using Raven.Client;
using Raven35.Changes.Subscription.Domain.Models;
using Raven35.Changes.Subscription.Infrastructure.Observers;
using System;
using System.Linq;

namespace Raven35.Changes.Subscription.Infrastructure
{
    public class DataSubscriptionsManager : ISubscriptionManager
    {
        private readonly IDocumentStore _store;
        private readonly IObserverFactory _observerFactory;

        public DataSubscriptionsManager(
            IDocumentStore store,
            IObserverFactory observerFactory)
        {
            _store = store;
            _observerFactory = observerFactory;
        }

        public SubscriptionType SubscriptionType => SubscriptionType.Data;

        public bool TrySubscribeToDocumentChanges<T>(string collectionName) where T : class
        {
            var collectionConfig = GetDataSubscription(collectionName);

            var subscriptionId = collectionConfig != null
                ? collectionConfig.SubscriptionId
                : CreateDataSubscription(collectionName);

            return OpenDataSubscription<T>(subscriptionId);
        }

        private SubscriptionConfig? GetDataSubscription(string collectionName)
        {
            var configs = _store.Subscriptions.GetSubscriptions(0, 10);

            // there may be more than one, this will return the first based on Etag
            return configs?.FirstOrDefault(x => x.Criteria?.BelongsToAnyCollection.Contains(collectionName) == true);
        }

        private long CreateDataSubscription(string collectionName)
        {
            var criteria = new SubscriptionCriteria
            {
                BelongsToAnyCollection = new[] { collectionName }
            };

            var subscriptionId = _store
                .Subscriptions
                .Create(criteria);

            return subscriptionId;
        }

        private bool OpenDataSubscription<T>(long subscriptionId) where T : class
        {
            var options = new SubscriptionConnectionOptions
            {
                BatchOptions = new SubscriptionBatchOptions()
                {
                    MaxDocCount = 16 * 1024,
                    MaxSize = 4 * 1024 * 1024,
                    AcknowledgmentTimeout = TimeSpan.FromMinutes(3)
                },
                IgnoreSubscribersErrors = false,
                ClientAliveNotificationInterval = TimeSpan.FromSeconds(60),
                Strategy = SubscriptionOpeningStrategy.TakeOver
            };

            var data = _store.Subscriptions.Open<T>(subscriptionId, options);

            if (data == null)
            {
                return false;
            }

            var observer = _observerFactory.TryLoadObserver<T>();

            if (observer == null)
            {
                Console.WriteLine("Observer of type {0} not found", typeof(T));

                return false;
            }

            data.Subscribe(observer);

            Console.WriteLine("Listening to changes for subscription {0} for type {1}", subscriptionId, typeof(T));

            return true;
        }
    }
}
