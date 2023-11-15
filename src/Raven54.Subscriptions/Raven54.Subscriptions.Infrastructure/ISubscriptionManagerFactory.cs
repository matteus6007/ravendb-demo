using Raven54.Subscriptions.Domain.Models;

namespace Raven54.Subscriptions.Infrastructure
{
    public interface ISubscriptionManagerFactory
    {
        ISubscriptionManager? LoadSubscriptionManager(SubscriptionType subscriptionType);
    }
}
