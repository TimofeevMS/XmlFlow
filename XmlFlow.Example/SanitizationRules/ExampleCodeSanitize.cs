using XmlFlow.Interfaces;

namespace XmlFlow.Example.SanitizationRules;

public class ExampleCodeSanitize : ISanitizationRule
{
    public bool CanApply(string columnName) => columnName is "Code";

    public string Sanitize(string value) => value.TrimStart('0');
}