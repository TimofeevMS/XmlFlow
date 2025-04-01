using System.Data;
using System.Data.Common;

using Dapper;

using Microsoft.Extensions.Logging;

using XmlFlow.Interfaces;
using XmlFlow.Models;

namespace XmlFlow.Repositories;

/// <summary>
/// Реализация репозитория БД
/// </summary>
public class DbRepository : IDbRepository, IDisposable
{
    private readonly DbConnection _dbConnection;
    private readonly ITypeMapper _typeMapper;
    private readonly ILogger<DbRepository> _logger;
    private DbTransaction? _transaction;
    private bool _disposed;

    public DbRepository(ILogger<DbRepository> logger, DbConnection dbConnection, ITypeMapper typeMapper)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(dbConnection);
        ArgumentNullException.ThrowIfNull(typeMapper);
        
        _logger = logger;
        _dbConnection = dbConnection;
        _typeMapper = typeMapper;
    }

    /// <summary>
    /// Начало транзакции
    /// </summary>
    public void BeginTransaction()
    {
        EnsureConnectionOpen();
        _transaction = _dbConnection.BeginTransaction();
        _logger.LogInformation("Transaction started.");
    }
    
    /// <summary>
    /// Завершение транзакции
    /// </summary>
    public void CommitTransaction()
    {
        _transaction?.Commit();
        _logger.LogInformation("Transaction committed.");
    }

    /// <summary>
    /// Отмена транзакции
    /// </summary>
    public void RollbackTransaction()
    {
        _transaction?.Rollback();
        _logger.LogInformation("Transaction rolled back.");
    }

    /// <summary>
    /// Выполнение SQL-запроса
    /// </summary>
    /// <param name="sql"> SQL-запрос </param>
    /// <param name="parameters"> Параметры запроса </param>
    /// <returns> Количество обработанных строк </returns>
    public async Task<int> ExecuteSqlAsync(string sql, object? parameters)
    {
        EnsureConnectionOpenIfTransaction();
        
        _logger.LogInformation("Executing SQL: {Sql}", sql);
        
        return await _dbConnection.ExecuteAsync(sql, parameters, _transaction);
    }

    /// <summary>
    /// Проверка существования таблицы
    /// </summary>
    /// <param name="table"> Метаданные таблицы </param>
    /// <returns> True, если таблица существует, иначе false </returns>
    public async Task<bool> TableExistsAsync(TableMetadata table)
    {
        EnsureConnectionOpenIfTransaction();
        
        const string sql = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = @Schema AND TABLE_NAME = @TableName";
        
        _logger.LogInformation("Executing SQL: {Sql}", sql);
        
        var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { table.Schema, table.TableName }, _transaction);
        
        return count > 0;
    }

    /// <summary>
    /// Проверка существования колонки в таблице
    /// </summary>
    /// <param name="table"> Метаданные таблицы </param>
    /// <param name="columnName"> Название колонки </param>
    /// <returns> True, если колонка существует, иначе false </returns>
    public async Task<bool> ColumnExistsAsync(TableMetadata table, string columnName)
    {
        EnsureConnectionOpenIfTransaction();
        
        const string sql = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @Schema AND TABLE_NAME = @TableName AND COLUMN_NAME = @ColumnName";
        
        _logger.LogInformation("Executing SQL: {Sql}", sql);
        
        var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { table.Schema, table.TableName, ColumnName = columnName }, _transaction);
        
        return count > 0;
    }

    /// <summary>
    /// Получение типа колонки
    /// </summary>
    /// <param name="table"> Метаданные таблицы </param>
    /// <param name="columnName"> Название колонки </param>
    /// <returns> Тип колонки </returns>
    public async Task<string> GetColumnTypeAsync(TableMetadata table, string columnName)
    {
        EnsureConnectionOpenIfTransaction();
        
        var dbType = await _typeMapper.GetColumnDbTypeAsync(table, columnName);
        
        return dbType.ToString();
    }

    /// <summary>
    /// Добавление таблицы в БД
    /// </summary>
    /// <param name="table"> Структура таблицы </param>
    public async Task InsertTableAsync(TableStructure table)
    {
        EnsureConnectionOpenIfTransaction();
        
        var sql = GenerateInsertSql(table.Metadata, table.Columns);
        
        _logger.LogInformation("Executing SQL: {Sql}", sql);
        
        await _dbConnection.ExecuteAsync(sql, GetParameters(table.Columns), _transaction);
    }

    /// <summary>
    /// Генерация SQL запроса на вставку данных в таблицу
    /// </summary>
    /// <param name="metadata"> Метаданные таблицы </param>
    /// <param name="columns"> Список колонок </param>
    /// <returns> SQL-запрос </returns>
    private static string GenerateInsertSql(TableMetadata metadata, IEnumerable<ColumnMetadata> columns)
    {
        var columnList = string.Join(",", columns.Select(c => $"\"{c.Name}\""));
        var parameters = string.Join(",", columns.Select(c => $"@{c.Name}"));
        
        return $"INSERT INTO \"{metadata.Schema}\".\"{metadata.TableName}\" ({columnList}) VALUES ({parameters})";
    }
    
    /// <summary>
    /// Получить словарь параметров запроса
    /// </summary>
    /// <param name="columns"> Список колонок </param>
    /// <returns> Словарь параметров запроса </returns>
    private static object GetParameters(IEnumerable<ColumnMetadata> columns)
    {
        return columns.ToDictionary(c => c.Name, c => c.Value ?? DBNull.Value);
    }
    
    /// <summary>
    /// Открытие соединения с БД
    /// </summary>
    private void EnsureConnectionOpen()
    {
        if (_dbConnection.State is ConnectionState.Closed)
        {
            _dbConnection.Open();
            _logger.LogInformation("Database connection opened.");
        }
    }

    /// <summary>
    /// Открываем соединение с БД если есть транзакция
    /// </summary>
    private void EnsureConnectionOpenIfTransaction()
    {
        if (_transaction is not null && _dbConnection.State is ConnectionState.Closed)
        {
            _dbConnection.Open();
            _logger.LogInformation("Database connection opened for transaction.");
        }
    }
    
    /// <summary>
    /// Освобождение ресурсов
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    /// <summary>
    /// Освобождение ресурсов
    /// </summary>
    /// <param name="disposing"> Флаг освобождения ресурсов </param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            _transaction?.Dispose();
            _dbConnection?.Close();
            _dbConnection?.Dispose();
        }
        _disposed = true;
    }
}