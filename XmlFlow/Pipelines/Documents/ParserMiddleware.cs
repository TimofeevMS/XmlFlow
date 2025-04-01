using System.Xml;

using Microsoft.Extensions.Logging;

using XmlFlow.Interfaces;
using XmlFlow.Models;

namespace XmlFlow.Pipelines.Documents;

public class ParserMiddleware : IDocumentMiddleware
{
    private readonly ILogger<ParserMiddleware> _logger;
    private readonly ITableHandlerFactory _handlerFactory;

    public ParserMiddleware(ILogger<ParserMiddleware> logger, ITableHandlerFactory handlerFactory)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(handlerFactory);
        
        _logger = logger;
        _handlerFactory = handlerFactory;
    }
    
    public void Configure(PipelineDocumentBuilder? builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        
        builder.Use(async (document, context, next) =>
                    {
                        var tables = new List<TableStructure>();
                        ParseRecursive(document.DocumentElement, tables, context, depth: 0);
                        context.Set("Tables", tables.OrderByDescending(t => t.Depth).ToList());

                         await next();   
                    });
    }
    
    /// <summary>
    /// Рекурсивная обработка XML документа
    /// </summary>
    private void ParseRecursive(XmlNode? node, List<TableStructure> tables, ImportContext context, int depth)
    {
        if (node is null)
            return;
        
        context.Set("CurrentDepth", depth);
        
        var hasChildElements = node.ChildNodes
                                   .OfType<XmlNode>()
                                   .Any(n => n.NodeType is XmlNodeType.Element);

        if (!hasChildElements)
            return;
        
        var handler = _handlerFactory.GetHandler(node);
        
        handler.BeforeNodeProcessing(node, context);
        var table = handler.ProcessNode(node, context);
        
        _logger.LogInformation("Parsed table \"{Table}\", with columns \"{Columns}\"", table.Metadata, table.Columns);
        
        handler.AfterNodeProcessing(table, context);

        if (IsHasColumn(table))
            tables.Add(table);

        foreach (XmlNode child in node.ChildNodes)
        {
            ParseRecursive(child, tables, context, depth + 1);
        }
    }

    /// <summary>
    /// Проверка наличия колонок в таблице
    /// </summary>
    /// <param name="table"> Структура таблицы </param>
    /// <returns> Результат проверки </returns>
    private bool IsHasColumn(TableStructure? table)
    {
        if (table is null)
            return false;
        
        return table.Columns.Count > 0;
    }
}