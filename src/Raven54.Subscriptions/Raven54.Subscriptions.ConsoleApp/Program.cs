// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Raven.Client.Documents.Changes;
using Raven54.Subscriptions.Domain.Models;
using Raven54.Subscriptions.Domain.Options;
using Raven54.Subscriptions.Infrastructure;
using Raven54.Subscriptions.Infrastructure.Observers;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton(services => new DocumentStoreManager(new RavenOptions()).Store);
builder.Services.AddSingleton<IObserver<DocumentChange>, DocumentChangeObserver>();
builder.Services.AddSingleton<ISubscriptionManager, ChangesSubscriptionManager>();
builder.Services.AddSingleton<ISubscriptionManager, DataSubscriptionsManager>();
builder.Services.AddSingleton<ISubscriptionManagerFactory, SubscriptionManagerFactory>();

using IHost host = builder.Build();

await StartApplication(host.Services, args);

await host.RunAsync();

static async Task StartApplication(IServiceProvider hostProvider, string[] args)
{
    using IServiceScope serviceScope = hostProvider.CreateScope();
    IServiceProvider provider = serviceScope.ServiceProvider;

    var subscriptionManagerFactory = provider.GetService<ISubscriptionManagerFactory>();

    // TODO: add to appsettings
    const string collectionName = "MobileDevices";

    if (subscriptionManagerFactory != null && args.Length > 0)
    {
        if (Enum.TryParse(args[0], true, out SubscriptionType subscriptionType))
        {
            var subscriptionManager = subscriptionManagerFactory.LoadSubscriptionManager(subscriptionType);

            if (subscriptionManager == null)
            {
                Console.WriteLine("No '{0}' found for type '{1}'", typeof(ISubscriptionManager), subscriptionType);

                return;
            }

            if (subscriptionType == SubscriptionType.Changes)
            {
                await subscriptionManager.TrySubscribeToDocumentChangesAsync<DocumentChange>(collectionName);
            }
            else
            {
                var cancellationToken = new CancellationTokenSource();

                await subscriptionManager.TrySubscribeToDocumentChangesAsync<MobileDevice>(collectionName, cancellationToken.Token).ConfigureAwait(false);
            }
        }
        else
        {
            Console.WriteLine("Cannot parse '{0}' to '{1}'", args[0], typeof(SubscriptionType));
        }
    }
}