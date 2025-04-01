using System.Data;

namespace XmlFlow.Models;

/// <summary>
/// Метаданные колонки
/// </summary>
public record ColumnMetadata
{
    /// <summary>
    /// Название колонки
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Тип колонки
    /// </summary>
    public DbType Type { get; set; } = DbType.String;

    /// <summary>
    /// Значение колонки
    /// </summary>
    public object? Value { get; set; }
    
    /// <summary>
    /// Признак существования колонки
    /// </summary>
    public bool Exists { get; set; }
}