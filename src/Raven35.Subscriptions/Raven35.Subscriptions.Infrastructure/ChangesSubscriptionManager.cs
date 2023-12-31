﻿using Raven.Abstractions.Data;
using Raven.Client;
using Raven35.Changes.Subscription.Domain.Models;
using System;

namespace Raven35.Changes.Subscription.Infrastructure
{
    public class ChangesSubscriptionManager : ISubscriptionManager
    {
        private readonly IDocumentStore _store;
        private readonly IObserver<DocumentChangeNotification> _observer;

        public ChangesSubscriptionManager(
            IDocumentStore store,
            IObserver<DocumentChangeNotification> observer)
        {
            _store = store;
            _observer = observer;
        }

        public SubscriptionType SubscriptionType => SubscriptionType.Changes;

        public bool TrySubscribeToDocumentChanges<T>(string collectionName) where T : class
        {
            _store
                .Changes()
                .ForDocumentsInCollection(collectionName)
                .Subscribe(_observer);

            Console.WriteLine("Listening to changes for collection {0}", collectionName);

            return true;
        }
    }
}
