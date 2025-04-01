using Microsoft.Extensions.Logging;

using XmlFlow.Interfaces;

namespace XmlFlow.Pipelines.Tables;

/// <summary>
/// Middleware очистки данных
/// </summary>
public class DataSanitizationMiddleware : ITableMiddleware
{
    private readonly ILogger<DataSanitizationMiddleware> _logger;
    private readonly IEnumerable<ISanitizationRule> _rules;

    public DataSanitizationMiddleware(ILogger<DataSanitizationMiddleware> logger, IEnumerable<ISanitizationRule> rules)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(rules);
        
        _logger = logger;
        _rules = rules;
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
                        foreach (var column in table.Columns)
                        {
                            if (column.Value is not string strValue)
                                continue;
                            
                            var sanitized = strValue.Trim();
                            
                            var rule = _rules.FirstOrDefault(r => r.CanApply(column.Name));
                            if (rule is not null)
                            {
                                sanitized = rule.Sanitize(sanitized);
                            }
                                
                            column.Value = sanitized;
                                
                            _logger.LogInformation("Sanitized \"{Name}\": \"{Old}\" -> \"{New}\"", column.Name, strValue, sanitized);
                        }
                        
                        await next();
                    });
    }
}