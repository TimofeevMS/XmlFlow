using System.Xml;

namespace XmlFlow.Interfaces;

/// <summary>
/// Импортер данных
/// </summary>
public interface IPipelineStarter
{
    /// <summary>
    /// Импорт Xml в БД
    /// </summary>
    /// <param name="xml"> XML </param>
    /// <returns> Задача импорта </returns>
    Task Start(XmlDocument  xml);
}