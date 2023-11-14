using Raven35.Changes.Subscription.Domain.Models;

namespace Raven35.Changes.Subscription.Infrastructure
{
    public interface ISubscriptionManagerFactory
    {
        ISubscriptionManager LoadSubscriptionManager(SubscriptionType subscriptionType);
    }
}