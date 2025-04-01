using System.Data;

namespace XmlFlow.Interfaces;

/// <summary>
/// Правило преобразования данных
/// </summary>
public interface IDataTransformationRule
{
    /// <summary>
    /// Проверка применимости правила
    /// </summary>
    /// <param name="dataType">Тип данных</param>
    /// <param name="columnName">Имя столбца</param>
    /// <returns>Результат проверки</returns>
    bool CanApply(DbType dataType, string columnName);
    
    /// <summary>
    /// Преобразование значения
    /// </summary>
    /// <param name="value">Значение</param>
    /// <returns>Преобразованное значение</returns>
    object Transform(object? value);
}