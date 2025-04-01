using System.Data;
using System.Data.Common;

using Dapper;

using XmlFlow.Interfaces;
using XmlFlow.Models;

namespace XmlFlow.Mappings;

public class PostgresTypeMapper : ITypeMapper
{
    private readonly DbConnection _connection;

    public PostgresTypeMapper(DbConnection connection)
    {
        ArgumentNullException.ThrowIfNull(connection);
        
        _connection = connection;
    }

    public DbType GetDbType(string providerTypeName)
    {
        if (string.IsNullOrEmpty(providerTypeName))
            return DbType.String;

        var lowerCaseType = providerTypeName.ToLower().Trim();

        return lowerCaseType switch
               {
                       "bigint" or "bigserial" => DbType.Int64,
                       "bit" or "bit varying" => DbType.Binary,
                       "boolean" => DbType.Boolean,
                       "bytea" => DbType.Binary,
                       "character" => DbType.StringFixedLength,
                       "character varying" or "text" => DbType.String,
                       "date" => DbType.Date,
                       "double precision" => DbType.Double,
                       "integer" or "serial" => DbType.Int32,
                       "json" or "jsonb" => DbType.String,
                       "money" or "numeric" => DbType.Decimal,
                       "real" => DbType.Single,
                       "smallint" or "smallserial" => DbType.Int16,
                       "time" => DbType.Time,
                       "time with time zone" => DbType.DateTimeOffset,
                       "timestamp" => DbType.DateTime,
                       "timestamp with time zone" => DbType.DateTimeOffset,
                       "uuid" => DbType.Guid,
                       "xml" => DbType.Xml,
                       "cidr" or "inet" or "macaddr" or "macaddr8" or "box" or "circle" or "line" or "lseg" or "path" or "point" or "polygon" or "pg_lsn" or "tsquery" or "tsvector" or "txid_snapshot" or "interval" => DbType.Object,
                       _ => DbType.String,
               };
    }

    public async Task<DbType> GetColumnDbTypeAsync(TableMetadata table, string columnName)
    {
        const string sql = "SELECT DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = @Schema AND TABLE_NAME = @TableName AND COLUMN_NAME = @ColumnName";
        var typeName = await _connection.ExecuteScalarAsync<string>(sql, new { table.Schema, table.TableName, ColumnName = columnName });
        return GetDbType(typeName ?? throw new InvalidOperationException($"Column '{columnName}' not found."));
    }
}