using Raven54.Subscriptions.Domain.Models;

namespace Raven54.Subscriptions.Infrastructure
{
    public interface ISubscriptionManager
    {
        SubscriptionType SubscriptionType { get; }
        Task<bool> TrySubscribeToDocumentChangesAsync<T>(string collectionName, CancellationToken ct = default) where T : class;
    }
}
