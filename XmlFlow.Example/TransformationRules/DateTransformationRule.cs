using System.Data;

using XmlFlow.Interfaces;

namespace XmlFlow.Example.TransformationRules;

public class DateTransformationRule : IDataTransformationRule
{
    public bool CanApply(DbType columnType, string columnName) => columnType is DbType.Date or DbType.DateTime or DbType.DateTime2 && columnName.EndsWith("Date", StringComparison.OrdinalIgnoreCase);

    public object Transform(object? value) => DateTime.TryParse(value?.ToString(), out var date) ? date.Date.ToString("d") : null;
}