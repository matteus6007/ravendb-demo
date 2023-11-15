using Raven.Client.Documents;
using Raven.Client.Documents.Changes;
using Raven54.Subscriptions.Domain.Models;

namespace Raven54.Subscriptions.Infrastructure
{
    public class ChangesSubscriptionManager : ISubscriptionManager
    {
        private readonly IDocumentStore _store;
        private readonly IObserver<DocumentChange> _observer;

        public ChangesSubscriptionManager(
            IDocumentStore store,
            IObserver<DocumentChange> observer)
        {
            _store = store;
            _observer = observer;
        }

        public SubscriptionType SubscriptionType => SubscriptionType.Changes;

        public async Task<bool> TrySubscribeToDocumentChangesAsync<T>(string collectionName, CancellationToken ct = default) where T : class
        {
            await Task.CompletedTask;

            _store
                .Changes()
                .ForDocumentsInCollection(collectionName)
                .Subscribe(_observer);

            Console.WriteLine("Listening to changes for collection {0}", collectionName);

            return true;
        }
    }
}
