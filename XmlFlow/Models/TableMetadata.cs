namespace XmlFlow.Models;

/// <summary>
/// Метаданные таблицы БД
/// </summary>
public record TableMetadata
{
    /// <summary>
    /// Схема
    /// </summary>
    public string Schema { get; set; }

    /// <summary>
    /// Название таблицы
    /// </summary>
    public string TableName { get; set; }

    /// <summary>
    /// Существует ли таблица
    /// </summary>
    public bool Exists { get; set; }
}