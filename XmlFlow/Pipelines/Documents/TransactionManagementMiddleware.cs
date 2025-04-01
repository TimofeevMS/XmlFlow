using Microsoft.Extensions.Logging;

using XmlFlow.Interfaces;

namespace XmlFlow.Pipelines.Documents;

/// <summary>
/// Middleware управления транзакциями
/// </summary>
public class TransactionManagementMiddleware : IDocumentMiddleware
{
    private readonly ILogger<TransactionManagementMiddleware> _logger;
    private readonly IDbRepository _dbRepository;

    public TransactionManagementMiddleware(ILogger<TransactionManagementMiddleware> logger, IDbRepository dbRepository)
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
    public void Configure(PipelineDocumentBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        
        builder.Use(async (_, _, next) =>
                    {
                        _dbRepository.BeginTransaction();
                        _logger.LogInformation("Transaction started");

                        try
                        {
                            await next();
                            _dbRepository.CommitTransaction();
                            _logger.LogInformation("Transaction committed");
                        }
                        catch
                        {
                            _dbRepository.RollbackTransaction();
                            _logger.LogError("Transaction rolled back");
                            throw;
                        }
                    });
    }
}