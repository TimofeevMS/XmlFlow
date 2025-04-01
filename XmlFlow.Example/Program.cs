using System.Data.Common;

using XmlFlow;
using XmlFlow.Example.SanitizationRules;
using XmlFlow.Example.TransformationRules;
using XmlFlow.Mappings;
using XmlFlow.Models;
using XmlFlow.Pipelines.Documents;
using XmlFlow.Pipelines.Tables;
using XmlFlow.Repositories;

using Npgsql;

using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
             .MinimumLevel.Information()
             .WriteTo.Console()
             .CreateLogger();

builder.Services.AddLogging(loggingBuilder =>
                    {
                        loggingBuilder.ClearProviders();
                        loggingBuilder.AddSerilog();
                    });

builder.Services.AddControllers(options =>
                                {
                                    options.InputFormatters.Insert(0, new TextPlainInputFormatter());
                                });
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(c =>
                               {
                                   c.SwaggerDoc("v1", new() { Title = "API", Version = "v1" });
    
                                   c.OperationFilter<AddConsumesOperationFilter>();
                               });
builder.Services.AddMemoryCache();

builder.Services.AddScoped<DbConnection>(_ => new NpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddParsinPipeline(LetterCase.Lower, CaseType.Snake);

builder.Services.AddTableHandlers();
builder.Services.AddSanitizationRules(typeof(ExampleCodeSanitize));
builder.Services.AddTransformationRules(typeof(DateTransformationRule));

builder.Services.AddDocumentMiddleware(typeof(ErrorHandlingMiddleware),
                                       typeof(PerformanceMonitoringMiddleware),
                                       typeof(XsdSchemaValidationMiddleware),
                                       typeof(TransactionManagementMiddleware),
                                       typeof(ParserMiddleware),
                                       typeof(TablePipelineMiddleware));

builder.Services.AddTableMiddleware(typeof(DataSanitizationMiddleware),
                                    typeof(DataTransformationMiddleware),
                                    typeof(ApplyCaseConvertorMiddleware),
                                    typeof(DatabaseSchemaValidationMiddleware),
                                    //typeof(AutoSchemaUpdateMiddleware),
                                    typeof(DataInsertionMiddleware));

builder.Services.AddXmlSchemaDictionary("xsd", new()
                                        {
                                                ["Example"] = "example.xsd",
                                        });

builder.Services.AddDatabaseServices(typeof(CachedDbRepository), typeof(PostgresTypeMapper));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();