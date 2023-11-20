// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Raven.Abstractions.Data;
using Raven35.Changes.Subscription.Domain.Models;
using Raven35.Changes.Subscription.Domain.Options;
using Raven35.Changes.Subscription.Infrastructure;
using Raven35.Changes.Subscription.Infrastructure.Observers;
using Raven35.Subscriptions.ConsoleApp.Options;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Configuration
    .AddCommandLine(args, ApplicationOptions.Mappings)
    .Build();

builder.Services.AddSingleton(services => new DocumentStoreManager(new RavenOptions()).Store);
builder.Services.AddSingleton<IObserver<DocumentChangeNotification>, DocumentChangeNotificationObserver>();
builder.Services.AddSingleton<IObserver<MobileDevice>, MobileDeviceObserver>();
builder.Services.AddSingleton<IObserverFactory, ObserverFactory>();
builder.Services.AddSingleton<ISubscriptionManager, DataSubscriptionsManager>();
builder.Services.AddSingleton<ISubscriptionManager, ChangesSubscriptionManager>();
builder.Services.AddSingleton<ISubscriptionManagerFactory, SubscriptionManagerFactory>();

using IHost host = builder.Build();

StartApplication(host.Services, builder.Configuration);

await host.RunAsync();

static void StartApplication(IServiceProvider hostProvider, IConfiguration configuration)
{
    using IServiceScope serviceScope = hostProvider.CreateScope();
    IServiceProvider provider = serviceScope.ServiceProvider;

    if (provider.GetService<ISubscriptionManagerFactory>() is not ISubscriptionManagerFactory subscriptionManagerFactory)
    {
        Console.WriteLine("No '{0}' found", typeof(ISubscriptionManagerFactory));

        return;
    }

    var options = configuration.Get<ApplicationOptions>() ?? new ApplicationOptions();

    if (subscriptionManagerFactory.LoadSubscriptionManager(options.SubscriptionType) is not ISubscriptionManager subscriptionManager)
    {
        Console.WriteLine("No '{0}' found for type '{1}'", typeof(ISubscriptionManager), options.SubscriptionType);

        return;
    }

    const string collectionName = "MobileDevices";

    if (options.SubscriptionType == SubscriptionType.Changes)
    {
        subscriptionManager.TrySubscribeToDocumentChanges<DocumentChangeNotification>(collectionName);
    }
    else
    {
        subscriptionManager.TrySubscribeToDocumentChanges<MobileDevice>(collectionName);
    }
}
