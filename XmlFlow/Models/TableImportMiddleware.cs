namespace XmlFlow.Models;

/// <summary>
/// Промежуточное ПО для импорта таблиц
/// </summary>
/// <param name="table"> Структура таблицы </param>
/// <param name="context"> Контекст импорта </param>
/// <param name="next"> Следующее ПО </param>
public delegate Task TableImportMiddleware(TableStructure table, ImportContext context, Func<Task> next);