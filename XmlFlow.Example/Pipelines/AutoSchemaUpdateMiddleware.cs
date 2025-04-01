using XmlFlow.Interfaces;
using XmlFlow.Pipelines.Tables;

namespace XmlFlow.Example.Pipelines;

public class AutoSchemaUpdateMiddleware : ITableMiddleware
{
    private readonly ILogger<AutoSchemaUpdateMiddleware> _logger;
    private readonly IDbRepository _dbRepository;

    public AutoSchemaUpdateMiddleware(ILogger<AutoSchemaUpdateMiddleware> logger, IDbRepository dbRepository)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(dbRepository);
        
        _logger = logger;
        _dbRepository = dbRepository;
    }

    public void Configure(PipelineTableBuilder builder)
    {
        builder.Use(async (table, _, next) =>
                    {
                        if (!await _dbRepository.TableExistsAsync(table.Metadata))
                        {
                            var createTableSql = $"CREATE TABLE \"{table.Metadata.Schema}\".\"{table.Metadata.TableName}\" (Id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY)";
                            await _dbRepository.ExecuteSqlAsync(createTableSql);
                            _logger.LogInformation("Created table \"{Table}\"", table.Metadata);
                        }

                        foreach (var column in table.Columns)
                        {
                            if (!await _dbRepository.ColumnExistsAsync(table.Metadata, column.Name))
                            {
                                var addColumnSql = $"ALTER TABLE \"{table.Metadata.Schema}\".\"{table.Metadata.TableName}\" ADD \"{column.Name}\" {column.Type}";
                                await _dbRepository.ExecuteSqlAsync(addColumnSql);
                                _logger.LogInformation("Added column \"{Column} to \"{Table}\"", column.Name, table.Metadata);
                            }
                        }
                        await next();
                    });
    }
}