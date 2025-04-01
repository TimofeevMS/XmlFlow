using Microsoft.Extensions.Logging;

using XmlFlow.Interfaces;
using XmlFlow.Models;

namespace XmlFlow.Pipelines.Tables;

/// <summary>
/// Middleware валидации схемы БД
/// </summary>
public class DatabaseSchemaValidationMiddleware : ITableMiddleware
{
    private readonly ILogger<DatabaseSchemaValidationMiddleware> _logger;
    private readonly IDbRepository _dbRepository;

    public DatabaseSchemaValidationMiddleware(ILogger<DatabaseSchemaValidationMiddleware> logger, IDbRepository dbRepository)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(dbRepository);
        
        _logger = logger;
        _dbRepository = dbRepository;
    }

    /// <summary>
    /// Конфигурация Middleware
    /// </summary>
    /// <param name="builder"> Построитель конвейера обработки </param>
    public void Configure(PipelineTableBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        
        builder.Use(async (table, _, next) =>
                    {
                        _logger.LogInformation("Validated table \"{Table}\"", table.Metadata.TableName);
            
                        var tableExists = await _dbRepository.TableExistsAsync(table.Metadata);
                        table.Metadata.Exists = tableExists;

                        if (tableExists)
                        {
                            _logger.LogInformation("Table '{Table}' found", table.Metadata.TableName);
                            await ValidateColumnsAsync(table);
                        }
                        else
                        {
                            _logger.LogWarning("Table '{Table}' not found", table.Metadata.TableName);
                        }

                        await next();
                    });
    }
    
    private async Task ValidateColumnsAsync(TableStructure table)
    {
        foreach (var column in table.Columns)
        {
            _logger.LogDebug("Validating column '{Column}'", column.Name);

            var columnExists = await _dbRepository.ColumnExistsAsync(table.Metadata, column.Name);
            column.Exists = columnExists;

            if (columnExists)
            {
                _logger.LogInformation("Column \"{Column}\" found", column.Name);
            }
            else
            {
                _logger.LogInformation("Column \"{Column}\" not found", column.Name);
            }
        }
    }
}