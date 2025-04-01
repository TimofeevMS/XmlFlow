using Microsoft.Extensions.Logging;

using XmlFlow.Interfaces;

namespace XmlFlow.Pipelines.Tables;

/// <summary>
/// Плагин вставки данных в БД
/// </summary>
public class DataInsertionMiddleware : ITableMiddleware
{
    private readonly ILogger<DataInsertionMiddleware> _logger;
    private readonly IDbRepository _dbRepository;

    public DataInsertionMiddleware(ILogger<DataInsertionMiddleware> logger, IDbRepository dbRepository)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(dbRepository);
        
        _logger = logger;
        _dbRepository = dbRepository;
    }

    /// <summary>
    /// Конфигурация плагина
    /// </summary>
    /// <param name="builder"> Построитель конвейера обработки </param>
    public void Configure(PipelineTableBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        
        builder.Use(async (table, _, next) =>
                    {
                        if (table.Metadata.Exists == false ||
                            table.Columns.Count == 0 ||
                            table.Columns.Any(c => c.Exists == false))
                        {
                            _logger.LogInformation("Data not inserted into \"{Table}\"", table.Metadata.TableName);
                            await next();
                            return;
                        }
                        
                        await _dbRepository.InsertTableAsync(table);
                        
                        _logger.LogInformation("Data inserted into \"{Table}\"", table.Metadata.TableName);
                        
                        await next();
                    });
    }
}