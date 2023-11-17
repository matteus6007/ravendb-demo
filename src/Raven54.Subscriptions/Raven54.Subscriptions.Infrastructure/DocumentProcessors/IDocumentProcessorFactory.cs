using Raven.Client.Documents.Subscriptions;

namespace Raven54.Subscriptions.Infrastructure.DocumentProcessors
{
    public interface IDocumentProcessorFactory
    {
        Task ProcessDocumentsAsync<T>(SubscriptionBatch<T> batch);
    }
}