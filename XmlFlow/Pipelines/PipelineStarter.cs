using System.Xml;

using Microsoft.Extensions.Logging;

using XmlFlow.Interfaces;
using XmlFlow.Models;

namespace XmlFlow.Pipelines;

/// <summary>
/// Старт импорта
/// </summary>
public class PipelineStarter : IPipelineStarter
{
    private readonly ILogger<PipelineStarter> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly Func<XmlDocument, ImportContext, Task> _documentPipeline;

    public PipelineStarter(ILogger<PipelineStarter> logger,
                           ILoggerFactory loggerFactory,
                           Func<XmlDocument, ImportContext, Task> documentPipeline)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(loggerFactory);
        ArgumentNullException.ThrowIfNull(documentPipeline);
        
        _logger = logger;
        _loggerFactory = loggerFactory;
        _documentPipeline = documentPipeline;
    }

    /// <summary>
    /// Начало импорта
    /// </summary>
    /// <param name="xml"></param>
    public async Task Start(XmlDocument xml)
    {
        ArgumentNullException.ThrowIfNull(xml);
        if (xml.DocumentElement == null)
            throw new ArgumentException("XML document has no root element.", nameof(xml));
        
        await _documentPipeline(xml, new (_loggerFactory.CreateLogger<ImportContext>()));
        
        _logger.LogInformation("Import completed successfully");
    }
}