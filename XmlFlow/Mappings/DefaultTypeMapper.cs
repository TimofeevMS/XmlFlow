using System.Data;
using System.Data.Common;

using XmlFlow.Interfaces;
using XmlFlow.Models;

namespace XmlFlow.Mappings;

public class DefaultTypeMapper : ITypeMapper
{
    private readonly DbConnection _connection;

    public DefaultTypeMapper(DbConnection connection)
    {
        ArgumentNullException.ThrowIfNull(connection);
        
        _connection = connection;
    }

    public DbType GetDbType(string providerTypeName)
    {
        return providerTypeName.ToLower() switch
               {
                       "int" or "integer" => DbType.Int32,
                       "bigint" => DbType.Int64,
                       "smallint" => DbType.Int16,
                       "tinyint" => DbType.Byte,
                       "varchar" or "nvarchar" or "text" => DbType.String,
                       "char" or "nchar" => DbType.StringFixedLength,
                       "boolean" => DbType.Boolean,
                       "decimal" or "numeric" => DbType.Decimal,
                       "float" or "double" => DbType.Double,
                       "date" => DbType.Date,
                       "datetime" or "timestamp" => DbType.DateTime,
                       "guid" or "uuid" => DbType.Guid,
                       _ => DbType.String,
               };
    }

    public async Task<DbType> GetColumnDbTypeAsync(TableMetadata table, string columnName)
    {
        var sql = $"SELECT \"{columnName}\" FROM \"{table.Schema}\".\"{table.TableName}\" WHERE 1=0";
        await using var command = _connection.CreateCommand();
        command.CommandText = sql;

        if (_connection.State == ConnectionState.Closed)
        {
            await _connection.OpenAsync();
        }

        await using var reader = await command.ExecuteReaderAsync(CommandBehavior.SchemaOnly);
        var schemaTable = await reader.GetSchemaTableAsync();

        foreach (DataRow row in schemaTable?.Rows)
        {
            if (row["ColumnName"].ToString() == columnName)
            {
                return (DbType)row["ProviderType"];
            }
        }

        throw new InvalidOperationException($"Column '{columnName}' not found in table '{table.TableName}'.");
    }
}