using Microsoft.Extensions.Logging;

using XmlFlow.Interfaces;

namespace XmlFlow.Pipelines.Tables;

/// <summary>
/// Middleware трансформации данных
/// </summary>
public class DataTransformationMiddleware : ITableMiddleware
{
    private readonly ILogger<DataTransformationMiddleware> _logger;
    private readonly IEnumerable<IDataTransformationRule> _rules;

    public DataTransformationMiddleware(ILogger<DataTransformationMiddleware> logger, IEnumerable<IDataTransformationRule> rules)
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
                            var rule = _rules.FirstOrDefault(r => r.CanApply(column.Type, column.Name));

                            if (rule is null)
                                continue;

                            try
                            {
                                column.Value = rule.Transform(column.Value);
                
                                _logger.LogInformation("Transformed column \"{Name}\" with value \"{Value}\" with rule \"{Rule}\"", column.Name, column.Value, rule.GetType().Name);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Failed to transform column \"{Name}\" with value \"{Value}\" with rule \"{Rule}\"", column.Name, column.Value, rule.GetType().Name);
                            }
                        }
                        
                        await next();
                    });
    }
}