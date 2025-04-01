using System.Xml;

namespace XmlFlow.Models;

/// <summary>
/// Промежуточное ПО для импорта таблиц
/// </summary>
/// <param name="document"> XML-документ </param>
/// <param name="context"> Контекст импорта </param>
/// <param name="next"> Следующее ПО </param>
public delegate Task DocumentImportMiddleware(XmlDocument document, ImportContext context, Func<Task> next);