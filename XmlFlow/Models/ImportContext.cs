using System.Collections.Concurrent;

using Microsoft.Extensions.Logging;

namespace XmlFlow.Models;

/// <summary>
/// Контекст выполнения импорта
/// </summary>
public class ImportContext : IDisposable
{
    private readonly Stack<ConcurrentDictionary<string, object>> _scopes = new();
    private readonly ILogger<ImportContext> _logger;
    private bool _disposed;

    public ImportContext(ILogger<ImportContext> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        
        _logger = logger;
        CreateScope();
    }

    /// <summary>
    /// Получить значение по ключу
    /// </summary>
    /// <param name="key"> Ключ значения </param>
    /// <typeparam name="T"> Тип значения </typeparam>
    /// <returns> Значение </returns>
    public T? Get<T>(string key)
    {
        foreach (var scope in _scopes)
        {
            if (!scope.TryGetValue(key, out var value) || value is not T result)
                continue;
            
            _logger.LogDebug("Resolved value for \"{Key}\": \"{Value}\"", key, result);

            return result;
        }

        _logger.LogWarning("Value for \"{Key}\" not found", key);

        return default(T);
    }

    /// <summary>
    /// Установить значение по ключу
    /// </summary>
    /// <param name="key"> Ключ значения </param>
    /// <param name="value"> Значение </param>
    public void Set(string key, object value)
    {
        if (_scopes.TryPeek(out var scope))
        {
            scope[key] = value;
            _logger.LogDebug("Set value for \"{Key}\": \"{Value}\"", key, value);
        }
        else
        {
            _logger.LogWarning("Attempt to set \"{Key}\" without active scope", key);
        }
    }
    
    /// <summary>
    /// Проверка существования значения по ключу
    /// </summary>
    /// <param name="key"> Ключ значения </param>
    /// <returns> true, если значение существует, иначе false </returns>
    public bool ContainsKey(string key)
    {
        foreach (var scope in _scopes)
        {
            if (!scope.TryGetValue(key, out var val))
                continue;
            
            _logger.LogDebug("Value for \"{Key}\" found \"{Value}\"", key, val);
                
            return true;
        }

        _logger.LogWarning("Value for \"{Key}\" not found", key);
        
        return false;
    }

    /// <summary>
    /// Класс для освобождения контекста
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;
        
        while (_scopes.Count > 0)
        {
            _scopes.Pop();
        }
        
        _disposed = true;
        
        _logger.LogDebug("Context disposed");
    }
    
    /// <summary>
    /// Создание нового контекста выполнения 
    /// </summary>
    private void CreateScope()
    {
        _scopes.Push(new());
        
        _logger.LogDebug("New scope started. Total scopes: \"{Count}\"", _scopes.Count);
    }
}