namespace XmlFlow.Interfaces;

/// <summary>
/// Правило очистки данных
/// </summary>
public interface ISanitizationRule
{
    /// <summary>
    /// Проверка применимости правила
    /// </summary>
    /// <param name="columnName"> Название колонки </param>
    /// <returns> true, если правило применимо </returns>
    bool CanApply(string columnName);
    
    /// <summary>
    /// Очистка значения
    /// </summary>
    /// <param name="value"> Значение </param>
    /// <returns> Очищенное значение </returns>
    string Sanitize(string value);
}