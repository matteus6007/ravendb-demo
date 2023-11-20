using Raven35.Changes.Subscription.Domain.Models;

namespace Raven35.Subscriptions.ConsoleApp.Options
{
    public class ApplicationOptions
    {
        public static readonly Dictionary<string, string> Mappings = new()
        {
            { "-st", "subscriptiontype" },
            { "--subscription-type", "subscriptiontype" },
        };

        public SubscriptionType SubscriptionType { get; set; }
    }
}
