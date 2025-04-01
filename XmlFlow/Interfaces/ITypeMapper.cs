using System.Data;
using System.Data.Common;

using XmlFlow.Models;

namespace XmlFlow.Interfaces;

public interface ITypeMapper
{
    DbType GetDbType(string providerTypeName);
    
    Task<DbType> GetColumnDbTypeAsync(TableMetadata table, string columnName);
}