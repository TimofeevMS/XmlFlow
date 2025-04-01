namespace XmlFlow.Models;

/// <summary>
/// Структура таблицы для импорта
/// </summary>
public class TableStructure
{
    /// <summary>
    /// Метаданные таблицы
    /// </summary>
    public required TableMetadata Metadata { get; init; }
    
    /// <summary>
    /// Коллекция столбцов
    /// </summary>
    public ICollection<ColumnMetadata> Columns { get; } = [];
    
    /// <summary>
    /// Глубина
    /// </summary>
    public int Depth { get; init; }
}