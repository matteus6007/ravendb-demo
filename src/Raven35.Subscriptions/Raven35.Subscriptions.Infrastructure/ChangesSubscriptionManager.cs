using Raven.Abstractions.Data;
using Raven.Client;
using Raven35.Changes.Subscription.Domain.Models;
using Raven35.Changes.Subscription.Infrastructure.Observers;
using System;

namespace Raven35.Changes.Subscription.Infrastructure
{
    public class ChangesSubscriptionManager : ISubscriptionManager
    {
        private readonly IDocumentStore _store;
        private readonly IObserverFactory _observerFactory;

        public ChangesSubscriptionManager(
            IDocumentStore store,
            IObserverFactory observerFactory)
        {
            _store = store;
            _observerFactory = observerFactory;
        }

        public SubscriptionType SubscriptionType => SubscriptionType.Changes;

        public bool TrySubscribeToDocumentChanges<T>(string collectionName) where T : class
        {
            var observer = _observerFactory.TryLoadObserver<DocumentChangeNotification>();

            if (observer == null)
            {
                Console.WriteLine("Observer of type {0} not found", typeof(DocumentChangeNotification));

                return false;
            }

            _store
                .Changes()
                .ForDocumentsInCollection(collectionName)
                .Subscribe(observer);

            Console.WriteLine("Listening to changes for collection {0}", collectionName);

            return true;
        }
    }
}
