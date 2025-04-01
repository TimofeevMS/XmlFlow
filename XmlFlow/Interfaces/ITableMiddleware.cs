using XmlFlow.Pipelines.Tables;

namespace XmlFlow.Interfaces;

/// <summary>
/// Плагин для импорта
/// </summary>
public interface ITableMiddleware
{
    /// <summary>
    /// Конфигурация плагина
    /// </summary>
    /// <param name="builder"> Построитель конвейера обработки </param>
    void Configure(PipelineTableBuilder builder);
}