using System.Xml;
using System.Xml.Schema;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

using XmlFlow.Handlers;
using XmlFlow.Interfaces;
using XmlFlow.Mappings;
using XmlFlow.Models;
using XmlFlow.Pipelines;
using XmlFlow.Pipelines.Documents;
using XmlFlow.Pipelines.Tables;

namespace XmlFlow;

public static class ImportConfigure
{
    public static IServiceCollection AddParsinPipeline(this IServiceCollection services, LetterCase letterCase, CaseType caseType)
    {
        services.AddScoped<IPipelineStarter, PipelineStarter>();
        services.AddScoped<ICaseConverter>(_ => new CaseConverter(letterCase, caseType));

        return services;
    }

    public static IServiceCollection AddDatabaseServices(this IServiceCollection services, Type dbRepository, Type typeMapper)
    {
        services.AddScoped(typeof(IDbRepository), dbRepository);
        services.AddScoped(typeof(ITypeMapper), typeMapper);
        
        return services;
    }
    
    public static IServiceCollection AddSanitizationRules(this IServiceCollection services, params Type[] rulesTypes)
    {
        foreach (var type in rulesTypes)
        {
            if (!typeof(ISanitizationRule).IsAssignableFrom(type))
            {
                throw new ArgumentException($"The {type.Name} type does not implement the ISanitizationRule interface.");
            }
            
            services.AddScoped(type);
        }        
        
        return services;
    }
    
    public static IServiceCollection AddTransformationRules(this IServiceCollection services, params Type[] rulesTypes)
    {
        foreach (var type in rulesTypes)
        {
            if (!typeof(IDataTransformationRule).IsAssignableFrom(type))
            {
                throw new ArgumentException($"The {type.Name} type does not implement the IDataTransformationRule interface.");
            }
            
            services.AddScoped(typeof(IDataTransformationRule), type);
        }        
        
        return services;
    }
    
    public static IServiceCollection AddTableHandlers(this IServiceCollection services, params Type[] handlersTypes)
    {
        foreach (var type in handlersTypes)
        {
            if (!typeof(ITableHandler).IsAssignableFrom(type))
            {
                throw new ArgumentException($"The {type.Name} type does not implement the ITableHandler interface.");
            }
            
            services.AddScoped(typeof(ITableHandler), type);
        }
        
        services.AddScoped<ITableHandler, DefaultTableHandler>();
        services.AddScoped<ITableHandlerFactory, TableHandlerFactory>();
        
        return services;
    }
    
    public static IServiceCollection AddXmlSchemaDictionary(this IServiceCollection services, string schemaDirectory, Dictionary<string, string> schemaConfig)
    {
        services.AddSingleton<IFileProvider>(new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), schemaDirectory)));
        
        services.AddSingleton<Dictionary<string, XmlSchema?>>(provider =>
                                                              {
                                                                  var fileProvider = provider.GetRequiredService<IFileProvider>();
                                                                  var schemaDict = new Dictionary<string, XmlSchema?>();

                                                                  foreach (var pair in schemaConfig)
                                                                  {
                                                                      var schemaPath = fileProvider.GetFileInfo(pair.Value);
                                                                      if (!schemaPath.Exists)
                                                                          continue;

                                                                      using var schemaReader = XmlReader.Create(schemaPath.PhysicalPath!);
                                                                      var schema = XmlSchema.Read(schemaReader, (_, _) => { });
                                                                      schemaDict[pair.Key] = schema;
                                                                  }

                                                                  return schemaDict;
                                                              });

        return services;
    }
    
    public static IServiceCollection AddDocumentMiddleware(this IServiceCollection services, params Type[] middlewareTypes)
    {
        foreach (var type in middlewareTypes)
        {
            if (!typeof(IDocumentMiddleware).IsAssignableFrom(type))
            {
                throw new ArgumentException($"The {type.Name} type does not implement the IDocumentMiddleware interface.");
            }
            
            services.AddScoped(typeof(IDocumentMiddleware), type);
        }
        
        services.AddScoped<Func<XmlDocument, ImportContext, Task>>(provider =>
                                                                   {
                                                                       var pipelineBuilder = new PipelineDocumentBuilder();

                                                                       foreach (var middleware in provider.GetServices<IDocumentMiddleware>())
                                                                       {
                                                                           middleware.Configure(pipelineBuilder);
                                                                       }

                                                                       return pipelineBuilder.Build();
                                                                   });

        return services;
    }
    
    public static IServiceCollection AddTableMiddleware(this IServiceCollection services, params Type[] middlewareTypes)
    {
        foreach (var type in middlewareTypes)
        {
            if (!typeof(ITableMiddleware).IsAssignableFrom(type))
            {
                throw new ArgumentException($"The {type.Name} type does not implement the ITableMiddleware interface.");
            }
            
            services.AddScoped(typeof(ITableMiddleware), type);
        }
        
        services.AddScoped<Func<TableStructure, ImportContext, Task>>(provider =>
                                                                      {
                                                                          var pipelineBuilder = new PipelineTableBuilder();

                                                                          foreach (var middleware in provider.GetServices<ITableMiddleware>())
                                                                          {
                                                                              middleware.Configure(pipelineBuilder);
                                                                          }

                                                                          return pipelineBuilder.Build();
                                                                      });

        return services;
    }
}