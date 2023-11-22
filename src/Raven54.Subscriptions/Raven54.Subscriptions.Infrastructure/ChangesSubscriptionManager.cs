using Microsoft.Extensions.Logging;
using Raven.Client.Documents;
using Raven.Client.Documents.Changes;
using Raven54.Subscriptions.Domain.Models;

namespace Raven54.Subscriptions.Infrastructure
{
    public class ChangesSubscriptionManager : ISubscriptionManager
    {
        private readonly IDocumentStore _store;
        private readonly IObserver<DocumentChange> _observer;
        private readonly ILogger<ChangesSubscriptionManager> _logger;

        public ChangesSubscriptionManager(
            IDocumentStore store,
            IObserver<DocumentChange> observer,
            ILogger<ChangesSubscriptionManager> logger)
        {
            _store = store;
            _observer = observer;
            _logger = logger;
        }

        public SubscriptionType SubscriptionType => SubscriptionType.Changes;

        public async Task<bool> TrySubscribeToDocumentChangesAsync<T>(string collectionName, CancellationToken ct = default) where T : class
        {
            await Task.CompletedTask;

            _store
                .Changes()
                .ForDocumentsInCollection(collectionName)
                .Subscribe(_observer);

            _logger.LogInformation("Listening to changes for collection '{collectionName}'", collectionName);

            return true;
        }
    }
}
