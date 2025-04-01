using System.Data;
using System.Text;

namespace XmlFlow.Mappings;

/// <summary>
/// Конвертер значений
/// </summary>
public static class DataConverter
{
    /// <summary>
    /// Конвертация значения
    /// </summary>
    /// <param name="dbType"> Тип данных </param>
    /// <param name="value"> Значение </param>
    /// <returns> Конвертированное значение </returns>
    public static object ConvertValue(DbType dbType, object? value)
    {
        if (value == null )
            return DBNull.Value;

        return dbType switch
               {
                       DbType.Int64 => Convert.ToInt64(value),
                       DbType.Binary => value is byte[] bytes ? bytes : Encoding.UTF8.GetBytes(value.ToString()),
                       DbType.Boolean => Convert.ToBoolean(value),
                       DbType.StringFixedLength or DbType.String => value.ToString(),
                       DbType.Date => Convert.ToDateTime(value).Date,
                       DbType.DateTime or DbType.DateTime2 => Convert.ToDateTime(value),
                       DbType.DateTimeOffset => DateTimeOffset.Parse(value.ToString()),
                       DbType.Decimal => Convert.ToDecimal(value),
                       DbType.Double => Convert.ToDouble(value),
                       DbType.Currency => Convert.ToDecimal(value),
                       DbType.Single => Convert.ToSingle(value),
                       DbType.Int32 => Convert.ToInt32(value),
                       DbType.Int16 => Convert.ToInt16(value),
                       DbType.Byte => Convert.ToByte(value),
                       DbType.Guid => Guid.Parse(value.ToString()),
                       DbType.Xml => value.ToString(),
                       DbType.Object => value,
                       _ => throw new InvalidOperationException($"Unsupported DbType: \"{dbType}\"")
               };
    }
}