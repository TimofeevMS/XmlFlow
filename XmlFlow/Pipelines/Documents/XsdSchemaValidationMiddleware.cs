using System.Xml;
using System.Xml.Schema;

using Microsoft.Extensions.Logging;

using XmlFlow.Interfaces;

namespace XmlFlow.Pipelines.Documents;

/// <summary>
/// Middleware для определения типа XML через XSD-валидацию
/// </summary>
public class XsdSchemaValidationMiddleware : IDocumentMiddleware
{
    private readonly ILogger<XsdSchemaValidationMiddleware> _logger;
    private readonly Dictionary<string, XmlSchema?> _schemas;

    public XsdSchemaValidationMiddleware(ILogger<XsdSchemaValidationMiddleware> logger, Dictionary<string, XmlSchema?> schemas)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(schemas);
        
        _logger = logger;
        _schemas = schemas;
    }

    /// <summary>
    /// Конфигурация Middleware
    /// </summary>
    /// <param name="builder"> Построитель конвейера обработки </param>
    public void Configure(PipelineDocumentBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        
        builder.Use(async (document, context, next) =>
                    {
                        var detectedType = ValidateAgainstSchemas(document);
            
                        _logger.LogInformation("XML type detected: \"{XmlType}\"", detectedType);
                        
                        context.Set("XmlType", detectedType);
            
                        await next();
                    });
    }

    /// <summary>
    /// Валидация XML по всем XSD схемам
    /// </summary>
    /// <param name="doc"> XML-документ </param>
    private string ValidateAgainstSchemas(XmlDocument doc)
    {
        ArgumentNullException.ThrowIfNull(doc);
        
        foreach (var schemaPair in _schemas)
        {
            if (schemaPair.Value is null)
                continue;
            
            try
            {
                var schemaSet = new XmlSchemaSet();
                schemaSet.Add(schemaPair.Value);
                doc.Schemas.Add(schemaSet);
                doc.Validate((_, args) => throw args.Exception);
                return schemaPair.Key;
            }
            catch (XmlSchemaValidationException ex)
            {
                _logger.LogWarning("Validation failed for \"{SchemaType}\": \"{Message}\"", schemaPair.Key, ex.Message);
            }
            finally
            {
                doc.Schemas.Remove(schemaPair.Value);
            }
        }
        
        _logger.LogError("XML does not match any registered XSD schema");

        throw new InvalidOperationException("XML does not match any registered XSD schema");
    }
}