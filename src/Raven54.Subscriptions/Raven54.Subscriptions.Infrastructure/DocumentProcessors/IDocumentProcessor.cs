namespace Raven54.Subscriptions.Infrastructure.DocumentProcessors
{
    public interface IDocumentProcessor { }

    public interface IDocumentProcessor<in T> : IDocumentProcessor where T : class
    {
        Task ProcessDocumentAsync(T document);
    }
}
