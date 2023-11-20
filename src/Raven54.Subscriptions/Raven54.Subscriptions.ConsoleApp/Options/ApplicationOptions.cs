using Raven54.Subscriptions.Domain.Models;

namespace Raven54.Subscriptions.ConsoleApp.Options
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
