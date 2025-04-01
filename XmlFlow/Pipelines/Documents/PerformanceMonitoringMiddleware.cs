using System.Diagnostics;

using Microsoft.Extensions.Logging;

using XmlFlow.Interfaces;

namespace XmlFlow.Pipelines.Documents;

/// <summary>
/// Middleware мониторинга производительности
/// </summary>
public class PerformanceMonitoringMiddleware : IDocumentMiddleware
{
    private readonly ILogger<PerformanceMonitoringMiddleware> _logger;

    public PerformanceMonitoringMiddleware(ILogger<PerformanceMonitoringMiddleware> logger)
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
                        var stopwatch = Stopwatch.StartNew();

                        try
                        {
                            await next();
                        }
                        finally
                        {
                            stopwatch.Stop();
                        }

                        _logger.LogInformation("Processed \"{Document}\" in {Time}ms", document.Name, stopwatch.ElapsedMilliseconds);
                    });
    }
}