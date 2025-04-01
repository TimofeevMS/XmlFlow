using Microsoft.Extensions.Logging;

using XmlFlow.Interfaces;
using XmlFlow.Models;

namespace XmlFlow.Pipelines.Documents;

/// <summary>
/// Middleware конвейера промежуточных ПО для импорта данных
/// </summary>
public class TablePipelineMiddleware : IDocumentMiddleware
{
    private readonly ILogger<TablePipelineMiddleware> _logger;
    private readonly Func<TableStructure, ImportContext, Task> _tablePipeline;

    public TablePipelineMiddleware(ILogger<TablePipelineMiddleware> logger, Func<TableStructure, ImportContext, Task> tablePipeline)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(tablePipeline);
        
        _logger = logger;
        _tablePipeline = tablePipeline;
    }
    
    /// <summary>
    /// Конфигурация Middleware
    /// </summary>
    /// <param name="builder"> Построитель конвейера обработки </param>
    public void Configure(PipelineDocumentBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        
        builder.Use(async (document, context, next) =>
                    {
                        var tables = context.Get<List<TableStructure>>("Tables");

                        if (tables is null)
                        {
                            _logger.LogInformation("No tables found in document \"{Document}\"", document.Name);
                            await next();
                            return;
                        }

                        foreach (var table in tables)
                        {
                            await _tablePipeline(table, context);
                        }
                        
                        await next();
                    });
    }
}