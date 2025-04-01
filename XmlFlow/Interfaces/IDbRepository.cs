using XmlFlow.Models;

namespace XmlFlow.Interfaces;

/// <summary>
/// Интерфейс репозитория БД
/// </summary>
public interface IDbRepository
{
    /// <summary>
    /// Создание транзакции
    /// </summary>
    void BeginTransaction();
    
    /// <summary>
    /// Завершение транзакции
    /// </summary>
    void CommitTransaction();
    
    /// <summary>
    /// Отмена транзакции
    /// </summary>
    void RollbackTransaction();
    
    /// <summary>
    /// Выполнение SQL-запроса
    /// </summary>
    /// <param name="sql"> SQL-запрос </param>
    /// <param name="parameters"> Параметры запроса </param>
    Task<int> ExecuteSqlAsync(string sql, object? parameters = null);

    /// <summary>
    /// Проверка существования таблицы
    /// </summary>
    /// <param name="table"> Метаданные таблицы </param>
    /// <returns> True, если таблица существует, иначе false </returns>
    Task<bool> TableExistsAsync(TableMetadata table);

    /// <summary>
    /// Проверка существования столбца
    /// </summary>
    /// <param name="table"> Метаданные таблицы </param>
    /// <param name="columnName"> Название столбца </param>
    /// <returns> True, если столбец существует, иначе false </returns>
    Task<bool> ColumnExistsAsync(TableMetadata table, string columnName);

    /// <summary>
    /// Получение типа столбца
    /// </summary>
    /// <param name="table"> Метаданные таблицы </param>
    /// <param name="columnName"> Название столбца </param>
    /// <returns> Тип столбца </returns>
    Task<string> GetColumnTypeAsync(TableMetadata table, string columnName);
    
    /// <summary>
    /// Вставка таблицы в БД
    /// </summary>
    /// <param name="table"> Структура таблицы </param>
    Task InsertTableAsync(TableStructure table);
}