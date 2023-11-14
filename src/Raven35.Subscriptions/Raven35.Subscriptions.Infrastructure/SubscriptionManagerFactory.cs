using Raven35.Changes.Subscription.Domain.Models;
using System.Collections.Generic;
using System.Linq;

namespace Raven35.Changes.Subscription.Infrastructure
{
    public class SubscriptionManagerFactory : ISubscriptionManagerFactory
    {
        private readonly IEnumerable<ISubscriptionManager> _subscriptionManagers;

        public SubscriptionManagerFactory(IEnumerable<ISubscriptionManager> subscriptionManagers)
        {
            _subscriptionManagers = subscriptionManagers;
        }

        public ISubscriptionManager LoadSubscriptionManager(SubscriptionType subscriptionType)
        {
            return _subscriptionManagers.SingleOrDefault(x => x.SubscriptionType == subscriptionType);
        }
    }
}
