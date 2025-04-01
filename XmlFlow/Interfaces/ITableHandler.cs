using System.Xml;

using XmlFlow.Models;

namespace XmlFlow.Interfaces;

/// <summary>
/// Обработчик таблиц
/// </summary>
public interface ITableHandler
{
    /// <summary>
    /// Схема
    /// </summary>
    string Schema { get; }
    
    /// <summary>
    /// Название таблицы
    /// </summary>
    string TableName(XmlNode node);
    
    /// <summary>
    /// Проверка возможности обработки
    /// </summary>
    /// <param name="node"> XML-узел </param>
    /// <returns> true, если можно обработать, иначе false </returns>
    bool CanHandle(XmlNode node);
    
    /// <summary>
    /// Предобработка
    /// </summary>
    /// <param name="node"> XML-узел </param>
    /// <param name="context"> Контекст импорта </param>
    void BeforeNodeProcessing(XmlNode node, ImportContext context);

    /// <summary>
    /// Обработка узла
    /// </summary>
    /// <param name="node"> XML-узел </param>
    /// <param name="context"> Контекст импорта </param>
    /// <returns> Структура таблицы </returns>
    TableStructure ProcessNode(XmlNode node, ImportContext context);

    /// <summary>
    /// Постобработка
    /// </summary>
    /// <param name="table"> Структура таблицы </param>
    /// <param name="context"> Контекст импорта </param>
    void AfterNodeProcessing(TableStructure table, ImportContext context);
}