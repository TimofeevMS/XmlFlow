using XmlFlow.Models;

namespace XmlFlow.Pipelines.Tables;

/// <summary>
/// Построитель конвейера промежуточных ПО для таблиц
/// </summary>
public class PipelineTableBuilder
{
    private readonly List<TableImportMiddleware> _middlewares = new();

    /// <summary>
    /// Добавить промежуточное ПО
    /// </summary>
    /// <param name="middleware"> Промежуточное ПО </param>
    /// <returns> Построитель </returns>
    public PipelineTableBuilder Use(TableImportMiddleware middleware)
    {
        ArgumentNullException.ThrowIfNull(middleware);
        
        _middlewares.Add(middleware);
        return this;
    }

    /// <summary>
    /// Построить конвейер
    /// </summary>
    /// <returns> Конвейер </returns>
    public Func<TableStructure, ImportContext, Task> Build()
    {
        return async (table, context) =>
               {
                   var pipeline = () => Task.CompletedTask;
            
                   foreach (var middleware in _middlewares.AsEnumerable().Reverse())
                   {
                       var current = middleware;
                       var next = pipeline;
                       pipeline = () => current(table, context, next);
                   }
                   
                   await pipeline().ConfigureAwait(false);
               };
    }
}