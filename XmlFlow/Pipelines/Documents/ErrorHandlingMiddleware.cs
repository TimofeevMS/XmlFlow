using Microsoft.Extensions.Logging;

using XmlFlow.Interfaces;

namespace XmlFlow.Pipelines.Documents;

/// <summary>
/// Middleware обработки ошибок
/// </summary>
public class ErrorHandlingMiddleware : IDocumentMiddleware
{
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(ILogger<ErrorHandlingMiddleware> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        
        _logger = logger;
    }

    /// <summary>
    /// Конфигурация Middleware
    /// </summary>
    /// <param name="builder"> Построитель конвейера обработки </param>
    public void Configure(PipelineDocumentBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        
        builder.Use(async (document, _, next) =>
                    {
                        try
                        {
                            await next();
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error processing document \"{Document}\"", document.Name);

                            throw;
                        }
                    });
    }
}