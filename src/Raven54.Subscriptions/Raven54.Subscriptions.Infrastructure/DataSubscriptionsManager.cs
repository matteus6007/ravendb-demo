﻿using Raven.Client.Documents;
using Raven.Client.Documents.Subscriptions;
using Raven54.Subscriptions.Domain.Models;
using Raven54.Subscriptions.Infrastructure.DocumentProcessors;

namespace Raven54.Subscriptions.Infrastructure
{
    public class DataSubscriptionsManager : ISubscriptionManager
    {
        private readonly IDocumentStore _store;
        private readonly IDocumentProcessorFactory _documentProcessorFactory;

        public DataSubscriptionsManager(
            IDocumentStore store,
            IDocumentProcessorFactory documentProcessorFactory)
        {
            _store = store;
            _documentProcessorFactory = documentProcessorFactory;
        }

        public SubscriptionType SubscriptionType => SubscriptionType.Data;

        private static string SubscriptionName(string collectionName) => $"{collectionName.ToLowerInvariant()}_subscription";

        public async Task<bool> TrySubscribeToDocumentChangesAsync<T>(string collectionName, CancellationToken ct = default) where T : class
        {
            var subscriptionState = await GetDataSubscription(collectionName, ct);

            var subscriptionName = subscriptionState != null
                ? subscriptionState.SubscriptionName
                : await CreateDataSubscription<T>(collectionName, ct);

            await RunWorker<T>(subscriptionName, ct).ConfigureAwait(false);

            return true;
        }

        private async Task<SubscriptionState?> GetDataSubscription(string collectionName, CancellationToken ct)
        {
            var subscriptions = await _store.Subscriptions.GetSubscriptionsAsync(0, 10, token: ct).ConfigureAwait(false);

            var subscriptionState = subscriptions.FirstOrDefault(x => x.SubscriptionName.Equals(SubscriptionName(collectionName)));

            if (subscriptionState == null)
            {
                return null;
            }
                
            Console.WriteLine("Found existing subscription {0} for '{1}'", subscriptionState.SubscriptionId, subscriptionState.SubscriptionName);

            return subscriptionState;
        }

        private async Task<string> CreateDataSubscription<T>(string collectionName, CancellationToken ct)
        {
            var options = new SubscriptionCreationOptions<T>
            {
                Name = SubscriptionName(collectionName)
            };

            var subscriptionName = await _store.Subscriptions.CreateAsync(options, token: ct).ConfigureAwait(false);

            Console.WriteLine("Created new subscription '{0}'", subscriptionName);

            return subscriptionName;
        }

        private Task RunWorker<T>(string subscriptionName, CancellationToken cancellationToken) where T : class
        {
            var options = new SubscriptionWorkerOptions(subscriptionName)
            {
                MaxDocsPerBatch = 100,
                IgnoreSubscriberErrors = false,
                //The server will allow an incoming connection to overthrow an existing one
                Strategy = SubscriptionOpeningStrategy.TakeOver
            };

            var subscriptionWorker = _store.Subscriptions.GetSubscriptionWorker<T>(options);

            _ = subscriptionWorker.Run(async batch => await _documentProcessorFactory.ProcessDocumentsAsync(batch).ConfigureAwait(false), cancellationToken);

            Console.WriteLine("Listening to changes for subscription '{0}' for type '{1}'", subscriptionName, typeof(T));

            return Task.CompletedTask;
        }
    }
}
