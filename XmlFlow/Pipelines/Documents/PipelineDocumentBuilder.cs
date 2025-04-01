using System.Xml;

using XmlFlow.Models;

namespace XmlFlow.Pipelines.Documents;

/// <summary>
/// Построитель конвейера промежуточных ПО для документа
/// </summary>
public class PipelineDocumentBuilder
{
    private readonly List<DocumentImportMiddleware> _middlewares = new();

    public PipelineDocumentBuilder Use(DocumentImportMiddleware middleware)
    {
        ArgumentNullException.ThrowIfNull(middleware);
        
        _middlewares.Add(middleware);
        return this;
    }

    public Func<XmlDocument, ImportContext, Task> Build()
    {
        return async (xml, context) =>
               {
                   var pipeline = () => Task.CompletedTask;

                   foreach (var middleware in _middlewares.AsEnumerable().Reverse())
                   {
                       var current = middleware;
                       var next = pipeline;
                       pipeline = () => current(xml, context, next);
                   }

                   await pipeline().ConfigureAwait(false);
               };
    }
}