﻿using Microsoft.Extensions.DependencyInjection;
using Raven.Client.Documents.Subscriptions;

namespace Raven54.Subscriptions.Infrastructure.DocumentProcessors
{
    public class DocumentProcessorFactory : IDocumentProcessorFactory
    {
        private readonly IServiceProvider _services;

        public DocumentProcessorFactory(IServiceProvider services)
        {
            _services = services;
        }

        public async Task ProcessDocumentsAsync<T>(SubscriptionBatch<T> batch) where T : class
        {
            if (_services.GetService<IDocumentProcessor<T>>() is not IDocumentProcessor<T> documentProcessor)
            {
                return;
            }

            foreach (var document in batch.Items)
            {
                await documentProcessor.ProcessDocumentAsync(document.Result).ConfigureAwait(false);
            }
        }
    }
}
