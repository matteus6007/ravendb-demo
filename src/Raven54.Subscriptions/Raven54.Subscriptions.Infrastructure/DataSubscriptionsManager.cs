using Raven.Client.Documents;
using Raven.Client.Documents.Subscriptions;
using Raven54.Subscriptions.Domain.Models;
using System.Text.Json;

namespace Raven54.Subscriptions.Infrastructure
{
    public class DataSubscriptionsManager : ISubscriptionManager
    {
        private readonly IDocumentStore _store;

        public DataSubscriptionsManager(
            IDocumentStore store)
        {
            _store = store;
        }

        public SubscriptionType SubscriptionType => SubscriptionType.Data;

        private static string SubscriptionName(string collectionName) => $"{collectionName.ToLowerInvariant()}_subscription";

        public async Task<bool> TrySubscribeToDocumentChangesAsync<T>(string collectionName, CancellationToken ct = default) where T : class
        {
            var subscriptionState = await GetDataSubscription(collectionName, ct);

            var subscriptionName = subscriptionState != null
                ? subscriptionState.SubscriptionName
                : await CreateDataSubscription(collectionName, ct);

            await RunWorker<T>(subscriptionName, ct).ConfigureAwait(false);

            return true;
        }

        private async Task<SubscriptionState?> GetDataSubscription(string collectionName, CancellationToken ct)
        {
            var subscription = await _store.Subscriptions.GetSubscriptionStateAsync(SubscriptionName(collectionName), token: ct).ConfigureAwait(false);

            if (subscription != null)
            {
                Console.WriteLine("Found existing subscription {0} for '{1}'", subscription.SubscriptionId, subscription.SubscriptionName);
            }

            return subscription;
        }

        private async Task<string> CreateDataSubscription(string collectionName, CancellationToken ct)
        {
            var options = new SubscriptionCreationOptions
            {
                Name = SubscriptionName(collectionName),
                Query = $"from {collectionName}"
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

            _ = subscriptionWorker.Run(async batch => await ProcessDocuments(batch).ConfigureAwait(false), cancellationToken);

            Console.WriteLine("Listening to changes for subscription '{0}' for type '{1}'", subscriptionName, typeof(T));

            return Task.CompletedTask;
        }

        private static Task ProcessDocuments<T>(SubscriptionBatch<T> batch) where T : class
        {
            batch.Items.ForEach(item =>
            {
                // can either use Result (type of T) or RawResult (JSON)
                Console.WriteLine("{0} change: {1}", typeof(T), JsonSerializer.Serialize(item.Result));
            });

            return Task.CompletedTask;
        }
    }
}
