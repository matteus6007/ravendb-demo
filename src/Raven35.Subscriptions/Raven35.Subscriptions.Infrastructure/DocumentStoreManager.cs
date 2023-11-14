using Raven.Client;
using Raven.Client.Document;
using Raven35.Changes.Subscription.Domain.Options;
using System;

namespace Raven35.Changes.Subscription.Infrastructure
{
    public class DocumentStoreManager
    {
        private readonly Lazy<IDocumentStore> _store;
        private readonly RavenOptions _options;

        public DocumentStoreManager(RavenOptions options)
        {
            _store = new Lazy<IDocumentStore>(CreateStore);
            _options = options;
        }

        public IDocumentStore Store
        {
            get { return _store.Value; }
        }

        private IDocumentStore CreateStore()
        {
            var store = new DocumentStore()
            {
                Url = _options.Url.ToString(),
                DefaultDatabase = _options.DefaultDatabase
            };

            store.Initialize();

            return store;
        }
    }
}
