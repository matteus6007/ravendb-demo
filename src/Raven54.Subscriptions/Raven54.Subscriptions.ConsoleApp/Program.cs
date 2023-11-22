// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Raven.Client.Documents.Changes;
using Raven54.Subscriptions.ConsoleApp;
using Raven54.Subscriptions.ConsoleApp.Options;
using Raven54.Subscriptions.Domain.Models;
using Raven54.Subscriptions.Domain.Options;
using Raven54.Subscriptions.Infrastructure;
using Raven54.Subscriptions.Infrastructure.DocumentProcessors;
using Raven54.Subscriptions.Infrastructure.Observers;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Configuration
    .AddCommandLine(args, ApplicationOptions.Mappings)
    .Build();

builder.Services.AddSingleton(services => new DocumentStoreManager(new RavenOptions()).Store);
builder.Services.AddSingleton<IObserver<DocumentChange>, DocumentChangeObserver>();
builder.Services.RegisterAllTypes<ISubscriptionManager>(new[] { typeof(ISubscriptionManager).Assembly }, ServiceLifetime.Singleton);
builder.Services.AddSingleton<ISubscriptionManagerFactory, SubscriptionManagerFactory>();
builder.Services.AddSingleton<IDocumentProcessorFactory, DocumentProcessorFactory>();
builder.Services.AddSingleton<IDocumentProcessor<MobileDevice>, MobileDeviceProcessor>();
builder.Services.RegisterAllTypes<IDocumentProcessor>(new[] { typeof(IDocumentProcessor).Assembly }, ServiceLifetime.Singleton);

builder.Logging.AddConsole();

using IHost host = builder.Build();

await StartApplication(host.Services, builder.Configuration);

await host.RunAsync();

static async Task StartApplication(IServiceProvider hostProvider, IConfiguration configuration)
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
        await subscriptionManager.TrySubscribeToDocumentChangesAsync<DocumentChange>(collectionName);
    }
    else
    {
        var cancellationToken = new CancellationTokenSource();

        await subscriptionManager.TrySubscribeToDocumentChangesAsync<MobileDevice>(collectionName, cancellationToken.Token).ConfigureAwait(false);
    }
}