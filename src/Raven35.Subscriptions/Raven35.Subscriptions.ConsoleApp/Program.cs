// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Raven.Abstractions.Data;
using Raven35.Changes.Subscription.Domain.Models;
using Raven35.Changes.Subscription.Domain.Options;
using Raven35.Changes.Subscription.Infrastructure;
using Raven35.Changes.Subscription.Infrastructure.Observers;
using System;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton(services => new DocumentStoreManager(new RavenOptions()).Store);
builder.Services.AddSingleton<IObserver<DocumentChangeNotification>, DocumentChangeNotificationObserver>();
builder.Services.AddSingleton<IObserver<MobileDevice>, MobileDeviceObserver>();
builder.Services.AddSingleton<IObserverFactory, ObserverFactory>();
builder.Services.AddSingleton<ISubscriptionManager, DataSubscriptionsManager>();
builder.Services.AddSingleton<ISubscriptionManager, ChangesSubscriptionManager>();
builder.Services.AddSingleton<ISubscriptionManagerFactory, SubscriptionManagerFactory>();

using IHost host = builder.Build();

StartApplication(host.Services, args);

await host.RunAsync();

static void StartApplication(IServiceProvider hostProvider, string[] args)
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

            if (subscriptionType == SubscriptionType.Changes)
            {
                subscriptionManager?.TrySubscribeToDocumentChanges<DocumentChangeNotification>(collectionName);
            }
            else
            {
                subscriptionManager?.TrySubscribeToDocumentChanges<MobileDevice>(collectionName);
            }
        }
        else
        {
            Console.WriteLine("Cannot parse {0} to {1}", args[0], typeof(SubscriptionType));
        }
    }
}
