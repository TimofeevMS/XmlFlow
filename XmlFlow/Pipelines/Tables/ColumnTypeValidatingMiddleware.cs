using Microsoft.Extensions.Logging;

using XmlFlow.Interfaces;
using XmlFlow.Mappings;

namespace XmlFlow.Pipelines.Tables;

/// <summary>
/// Middleware валидации типов столбцов
/// </summary>
public class ColumnTypeValidatingMiddleware : ITableMiddleware
{
    private readonly ILogger<ColumnTypeValidatingMiddleware> _logger;
    private readonly IDbRepository _dbRepository;
    private readonly ITypeMapper _typeMapper;

    public ColumnTypeValidatingMiddleware(ILogger<ColumnTypeValidatingMiddleware> logger, IDbRepository dbRepository, ITypeMapper typeMapper)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(dbRepository);
        ArgumentNullException.ThrowIfNull(typeMapper);
        
        _logger = logger;
        _dbRepository = dbRepository;
        _typeMapper = typeMapper;
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
                        _logger.LogInformation("Validating column types in table \"{Table}\"", table.Metadata.TableName);

                        foreach (var column in table.Columns.Where(c => c.Exists))
                        {
                            column.Type = await _typeMapper.GetColumnDbTypeAsync(table.Metadata, column.Name);
                            _logger.LogInformation("Column \"{Column}\" has type \"{Type}\"", column.Name, column.Type);
                            column.Value = DataConverter.ConvertValue(column.Type, column.Value);
                        }

                        await next();
                    });
    }
}