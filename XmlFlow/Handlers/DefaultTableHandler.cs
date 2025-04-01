using Microsoft.Extensions.Logging;

namespace XmlFlow.Handlers;

/// <summary>
/// Обработчик таблиц по умолчанию
/// </summary>
public class DefaultTableHandler : BaseTableHandler
{
    public DefaultTableHandler(ILogger<DefaultTableHandler> logger) : base(logger) { }
}