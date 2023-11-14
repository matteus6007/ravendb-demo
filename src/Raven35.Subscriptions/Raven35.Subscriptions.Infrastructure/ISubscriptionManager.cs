using Raven35.Changes.Subscription.Domain.Models;

namespace Raven35.Changes.Subscription.Infrastructure
{
    public interface ISubscriptionManager
    {
        SubscriptionType SubscriptionType { get; }
        bool TrySubscribeToDocumentChanges<T>(string collectionName) where T : class;
    }
}
