using Raven54.Subscriptions.Domain.Models;

namespace Raven54.Subscriptions.Infrastructure
{
    public class SubscriptionManagerFactory : ISubscriptionManagerFactory
    {
        private readonly IEnumerable<ISubscriptionManager> _subscriptionManagers;

        public SubscriptionManagerFactory(IEnumerable<ISubscriptionManager> subscriptionManagers)
        {
            _subscriptionManagers = subscriptionManagers;
        }

        public ISubscriptionManager? LoadSubscriptionManager(SubscriptionType subscriptionType)
        {
            return _subscriptionManagers?.SingleOrDefault(x => x.SubscriptionType == subscriptionType);
        }
    }
}
