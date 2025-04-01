using XmlFlow.Pipelines.Documents;

namespace XmlFlow.Interfaces;

/// <summary>
/// Плагин для импорта
/// </summary>
public interface IDocumentMiddleware
{
    /// <summary>
    /// Конфигурация плагина
    /// </summary>
    /// <param name="builder"> Построитель конвейера обработки </param>
    void Configure(PipelineDocumentBuilder builder);
}