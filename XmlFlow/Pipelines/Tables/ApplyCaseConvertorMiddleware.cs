using Microsoft.Extensions.Logging;

using XmlFlow.Interfaces;

namespace XmlFlow.Pipelines.Tables;

/// <summary>
/// Middleware обработки ошибок
/// </summary>
public class ApplyCaseConvertorMiddleware : ITableMiddleware
{
    private readonly ICaseConverter _caseConverter;
    private readonly ILogger<ApplyCaseConvertorMiddleware> _logger;

    public ApplyCaseConvertorMiddleware(ILogger<ApplyCaseConvertorMiddleware> logger, ICaseConverter caseConverter)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(caseConverter);
        
        _logger = logger;
        _caseConverter = caseConverter;
    }

    /// <summary>
    /// Конфигурация Middleware
    /// </summary>
    /// <param name="builder"> Построитель конвейера обработки </param>
    public void Configure(PipelineTableBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        
        builder.Use(async (tables, _, next) =>
                    {
                        var oldTableName = tables.Metadata.TableName;
                            
                        tables.Metadata.TableName = _caseConverter.Convert(tables.Metadata.TableName);
                        
                        _logger.LogInformation("Converted table name \"{OldTableName}\" -> \"{TableName}\"", oldTableName, tables.Metadata.TableName);
                        
                        foreach (var column in tables.Columns)
                        {
                            var oldColumnName = column.Name;
                            
                            column.Name = _caseConverter.Convert(column.Name);
                            
                            _logger.LogInformation("Converted column name \"{OldColumnName}\" -> \"{ColumnName}\"", oldColumnName, column.Name);
                        }
                        
                        await next();
                    });
    }
}