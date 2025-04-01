using System.Xml;

namespace XmlFlow.Interfaces;

/// <summary>
/// Фабрика обработчиков таблиц
/// </summary>
public interface ITableHandlerFactory
{
    /// <summary>
    /// Получить обработчик
    /// </summary>
    /// <param name="node"> XML-узел </param>
    /// <returns> Обработчик </returns>
    ITableHandler GetHandler(XmlNode node);
}