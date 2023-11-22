using Microsoft.Extensions.Logging;
using Raven.Client.Documents;
using Raven.Client.Documents.Subscriptions;
using Raven54.Subscriptions.Domain.Models;
using Raven54.Subscriptions.Infrastructure.DocumentProcessors;

namespace Raven54.Subscriptions.Infrastructure
{
    public class DataSubscriptionsManager : ISubscriptionManager
    {
        private readonly IDocumentStore _store;
        private readonly IDocumentProcessorFactory _documentProcessorFactory;
        private readonly ILogger<DataSubscriptionsManager> _logger;

        public DataSubscriptionsManager(
            IDocumentStore store,
            IDocumentProcessorFactory documentProcessorFactory,
            ILogger<DataSubscriptionsManager> logger)
        {
            _store = store;
            _documentProcessorFactory = documentProcessorFactory;
            _logger = logger;
        }

        public SubscriptionType SubscriptionType => SubscriptionType.Data;

        private static string SubscriptionName(string collectionName) => $"{collectionName.ToLowerInvariant()}_subscription";

        public async Task<bool> TrySubscribeToDocumentChangesAsync<T>(string collectionName, CancellationToken ct = default) where T : class
        {
            var subscriptionState = await GetDataSubscriptionAsync(collectionName, ct);

            var subscriptionName = subscriptionState != null
                ? subscriptionState.SubscriptionName
                : await CreateDataSubscriptionAsync<T>(collectionName, ct);

            await RunWorkerAsync<T>(subscriptionName, ct).ConfigureAwait(false);

            return true;
        }

        private async Task<SubscriptionState?> GetDataSubscriptionAsync(string collectionName, CancellationToken ct)
        {
            var subscriptions = await _store.Subscriptions.GetSubscriptionsAsync(0, 10, token: ct).ConfigureAwait(false);

            var subscriptionState = subscriptions.FirstOrDefault(x => x.SubscriptionName.Equals(SubscriptionName(collectionName)));

            if (subscriptionState == null)
            {
                return null;
            }
                
            _logger.LogInformation("Found existing subscription {subscriptionId} for '{subscriptionName}'", subscriptionState.SubscriptionId, subscriptionState.SubscriptionName);

            return subscriptionState;
        }

        private async Task<string> CreateDataSubscriptionAsync<T>(string collectionName, CancellationToken ct)
        {
            var options = new SubscriptionCreationOptions<T>
            {
                Name = SubscriptionName(collectionName)
            };

            var subscriptionName = await _store.Subscriptions.CreateAsync(options, token: ct).ConfigureAwait(false);

            _logger.LogInformation("Created new subscription '{subscriptionName}'", subscriptionName);

            return subscriptionName;
        }

        private Task RunWorkerAsync<T>(string subscriptionName, CancellationToken cancellationToken) where T : class
        {
            var options = new SubscriptionWorkerOptions(subscriptionName)
            {
                MaxDocsPerBatch = 100,
                IgnoreSubscriberErrors = false,
                //The server will allow an incoming connection to overthrow an existing one
                Strategy = SubscriptionOpeningStrategy.TakeOver
            };

            var subscriptionWorker = _store.Subscriptions.GetSubscriptionWorker<T>(options);
            subscriptionWorker.OnUnexpectedSubscriptionError += (exception) => _logger.LogError(exception, "Subscription worker failed for type '{documentType}'", typeof(T));

            _ = subscriptionWorker.Run(async batch => await ProcessDocumentsAsync(batch).ConfigureAwait(false), cancellationToken);

            _logger.LogInformation("Listening to changes for subscription '{subscriptionName}' for type '{documentType}'", subscriptionName, typeof(T));

            return Task.CompletedTask;
        }

        private async Task ProcessDocumentsAsync<T>(SubscriptionBatch<T> batch) where T : class
        {
            try
            {
                await _documentProcessorFactory.ProcessDocumentsAsync(batch).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing subscription for type '{documentType}'", typeof(T));
            }
        }
    }
}
