using System.Xml;

using Microsoft.Extensions.Logging;

using XmlFlow.Interfaces;
using XmlFlow.Models;

namespace XmlFlow.Handlers;

/// <summary>
/// Обработчик таблиц по умолчанию
/// </summary>
public abstract class BaseTableHandler : ITableHandler
{
    private readonly ILogger<DefaultTableHandler> _logger;

    public BaseTableHandler(ILogger<DefaultTableHandler> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        
        _logger = logger;
    }

    /// <summary>
    /// Схема БД
    /// </summary>
    public virtual string Schema => "public";

    /// <summary>
    /// Название таблицы
    /// </summary>
    /// <param name="node"> XML-узел </param>
    /// <returns> Название таблицы </returns>
    public virtual string TableName(XmlNode? node)
    {
        ArgumentNullException.ThrowIfNull(node);
        
        return node.Name;
    }

    /// <summary>
    /// Проверка возможности обработки узла
    /// </summary>
    /// <param name="node"> XML-узел </param>
    /// <returns> Результат проверки </returns>
    public virtual bool CanHandle(XmlNode? node)
    {
        ArgumentNullException.ThrowIfNull(node);
        
        return true;
    }

    /// <summary>
    /// Предварительная обработка узла
    /// </summary>
    /// <param name="node"> XML-узел </param>
    /// <param name="context"> Контекст импорта </param>
    public virtual void BeforeNodeProcessing(XmlNode? node, ImportContext? context)
    {
        ArgumentNullException.ThrowIfNull(node);
        ArgumentNullException.ThrowIfNull(context);
    }

    /// <summary>
    /// Обработка узла
    /// </summary>
    /// <param name="node"> XML-узел </param>
    /// <param name="context"> Контекст импорта </param>
    /// <returns> Структура таблицы </returns>
    public virtual TableStructure ProcessNode(XmlNode? node, ImportContext? context)
    {
        ArgumentNullException.ThrowIfNull(node);
        ArgumentNullException.ThrowIfNull(context);
        
        var table = new TableStructure
            {
                    Metadata = new TableMetadata
                               {
                                       Schema = Schema,
                                       TableName = TableName(node),
                               },
                    Depth = context.Get<int>("CurrentDepth"),
            };

        if (node.ChildNodes is null)
        {
            _logger.LogWarning("Node '{NodeName}' has no child nodes.", node.Name);
            return table;
        }

        foreach (XmlNode child in node.ChildNodes)
        {
            if (!IsColumn(child))
                continue;
            
            if (string.IsNullOrEmpty(child.Name))
            {
                _logger.LogWarning("Child node in '{NodeName}' has no name, skipping.", node.Name);
                continue;
            }
            
            table.Columns.Add(new ColumnMetadata { Name = child.Name, Value = child.InnerText });
        }

        return table;
    }

    /// <summary>
    /// Постобработка узла
    /// </summary>
    /// <param name="table"> Структура таблицы </param>
    /// <param name="context"> Контекст импорта </param>
    public virtual void AfterNodeProcessing(TableStructure? table, ImportContext? context)
    {
        ArgumentNullException.ThrowIfNull(table);
        ArgumentNullException.ThrowIfNull(context);
        
        if (table.Columns.Count == 0)
        {
            _logger.LogWarning("Empty table detected: {TableName}", table.Metadata.TableName);
        }
    }
    
    /// <summary>
    /// Проверка является ли узел колонкой
    /// </summary>
    /// <param name="node"> XML-узел </param>
    /// <returns> Результат проверки </returns>
    private bool IsColumn(XmlNode? node)
    {
        if (node is null)
            return false;
        
        return node.ChildNodes.OfType<XmlNode>().All(n => n.NodeType is not XmlNodeType.Element);
    }
}