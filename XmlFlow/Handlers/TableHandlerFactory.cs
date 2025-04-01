using System.Xml;

using Microsoft.Extensions.Logging;

using XmlFlow.Interfaces;

namespace XmlFlow.Handlers;

/// <summary>
/// Фабрика обработчиков таблиц
/// </summary>
public class TableHandlerFactory : ITableHandlerFactory
{
    private readonly ILogger<TableHandlerFactory> _logger;
    private readonly IEnumerable<ITableHandler> _handlers;
    private readonly DefaultTableHandler _defaultHandler;

    public TableHandlerFactory(ILogger<TableHandlerFactory> logger, IEnumerable<ITableHandler> handlers)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(handlers);
        
        _logger = logger;
        _handlers = handlers.Where(h => h is not DefaultTableHandler).ToList();
        _defaultHandler = handlers.OfType<DefaultTableHandler>().First()
                       ?? throw new ArgumentException("No DefaultTableHandler found in handlers.", nameof(handlers));;
    }

    /// <summary>
    /// Получить обработчик
    /// </summary>
    /// <param name="node"> XML-узел </param>
    /// <returns> Обработчик </returns>
    public ITableHandler GetHandler(XmlNode? node)
    {
        ArgumentNullException.ThrowIfNull(node);
        
        var handler = _handlers.FirstOrDefault(h => h.CanHandle(node)) ?? _defaultHandler;
        
        _logger.LogInformation("Selected handler \"{Handler}\" for node \"{Node}\"", handler.GetType().Name, node.Name);
        
        return handler;
    }
}