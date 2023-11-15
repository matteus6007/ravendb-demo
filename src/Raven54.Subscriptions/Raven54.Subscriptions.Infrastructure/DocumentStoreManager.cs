using Raven.Client.Documents;
using Raven54.Subscriptions.Domain.Options;

namespace Raven54.Subscriptions.Infrastructure
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
                Urls = new[] { _options.Url.ToString() },
                Database = _options.DefaultDatabase
            };

            store.Initialize();

            return store;
        }
    }
}
