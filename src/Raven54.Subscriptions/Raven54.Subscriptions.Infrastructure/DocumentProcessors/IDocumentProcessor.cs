namespace Raven54.Subscriptions.Infrastructure.DocumentProcessors
{
    public interface IDocumentProcessor<in T>
    {
        Task ProcessDocumentAsync(T document);
    }
}
