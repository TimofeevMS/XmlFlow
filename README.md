# XmlFlow

## Описание

`XmlFlow` — это гибкий фреймворк для импорта данных из XML-файлов в базу данных. Он построен на основе паттерна Middleware Pipeline, что позволяет легко настраивать и расширять процесс обработки данных. Проект поддерживает валидацию XML по XSD-схемам, преобразование данных, очистку, а также интеграцию с PostgreSQL (с возможностью адаптации под другие СУБД).

Основные возможности:
- Парсинг XML с автоматическим созданием структуры таблиц.
- Поддержка различных стилей именования колонок (snake_case, kebab-case, camelCase, PascalCase).
- Очистка и трансформация данных через настраиваемые правила.
- Управление транзакциями и кэширование запросов к метаданным БД.
- Логирование для мониторинга и отладки.

Проект написан на C# с использованием .NET 9.0 и предназначен для разработчиков, которым нужен универсальный инструмент для импорта данных.

---

## Требования

- **.NET 9.0 SDK** — для сборки и запуска проекта.
- **PostgreSQL** — для хранения данных (можно адаптировать под другие СУБД).
- Установленные пакеты NuGet (см. ниже).

### Зависимости
- `Microsoft.Extensions.Logging` — для логирования.
- `Microsoft.Extensions.DependencyInjection` — для внедрения зависимостей.
- `Dapper` — для работы с базой данных.
- `Npgsql` — для подключения к PostgreSQL.
- `Serilog` — для расширенного логирования.
- `Microsoft.Extensions.Caching.Memory` — для кэширования.

---

## Установка

1. **Склонируйте репозиторий**:
   ```bash
   git clone https://github.com/username/XmlFlow.git
   cd XmlFlow
   ```

2. **Восстановите зависимости**:
   ```bash
   dotnet restore
   ```

3. **Настройте подключение к базе данных**:
   Отредактируйте файл `appsettings.json` или укажите строку подключения в коде:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Port=5432;Database=your_db;Username=your_user;Password=your_password"
     }
   }
   ```

4. **Соберите проект**:
   ```bash
   dotnet build
   ```

5. **Запустите приложение**:
   ```bash
   dotnet run --project XmlFlow.Example
   ```

---

## Архитектура компонентов

Проект построен на модульной архитектуре с четким разделением ответственности между компонентами. Основные элементы взаимодействуют через внедрение зависимостей (`Microsoft.Extensions.DependencyInjection`) и паттерн Middleware Pipeline. Ниже описаны ключевые компоненты и их роли.

### 1. **PipelineStarter (Точка входа)**
- **Расположение**: `XmlFlow.Pipelines.PipelineStarter`
- **Описание**: Отвечает за запуск процесса импорта XML. Принимает `XmlDocument` и передает его в конвейер обработки документов.
- **Роль**: Является основным интерфейсом для внешнего взаимодействия (`IPipelineStarter`).
- **Взаимодействие**: Создает экземпляр `ImportContext` и вызывает построенный конвейер документов (`Func<XmlDocument, ImportContext, Task>`).

### 2. **ImportContext (Контекст выполнения)**
- **Расположение**: `XmlFlow.Models.ImportContext`
- **Описание**: Хранит состояние импорта, включая текущую глубину вложенности XML и промежуточные данные (например, список таблиц).
- **Роль**: Обеспечивает доступ к данным между middleware через ключ-значение с поддержкой областей видимости (scopes).
- **Особенности**: Использует стек словарей для управления вложенностью и реализует `IDisposable` для освобождения ресурсов.

### 3. **Document Pipeline (Конвейер документов)**
- **Расположение**: `XmlFlow.Pipelines.Documents`
- **Описание**: Цепочка middleware для обработки всего XML-документа.
- **Ключевые компоненты**:
  - **`PipelineDocumentBuilder`**: Строит конвейер, комбинируя middleware в нужном порядке.
  - **`IDocumentMiddleware`**: Интерфейс для добавления шагов обработки (например, `ParserMiddleware`, `TransactionManagementMiddleware`).
- **Примеры middleware**:
  - `ErrorHandlingMiddleware`: Перехватывает и логирует ошибки.
  - `ParserMiddleware`: Рекурсивно парсит XML и создает структуру таблиц.
  - `XsdSchemaValidationMiddleware`: Проверяет XML на соответствие XSD-схемам.
  - `TablePipelineMiddleware`: Передает таблицы в конвейер таблиц.
- **Взаимодействие**: Принимает `XmlDocument` и `ImportContext`, передавая управление следующему middleware.

### 4. **Table Pipeline (Конвейер таблиц)**
- **Расположение**: `XmlFlow.Pipelines.Tables`
- **Описание**: Цепочка middleware для обработки отдельных таблиц, созданных из XML.
- **Ключевые компоненты**:
  - **`PipelineTableBuilder`**: Строит конвейер для таблиц.
  - **`ITableMiddleware`**: Интерфейс для шагов обработки таблиц (например, `DataInsertionMiddleware`, `DataSanitizationMiddleware`).
- **Примеры middleware**:
  - `DatabaseSchemaValidationMiddleware`: Проверяет наличие таблиц и колонок в БД.
  - `ApplyCaseConvertorMiddleware`: Преобразует имена таблиц и колонок в заданный стиль.
  - `DataInsertionMiddleware`: Вставляет данные в БД.
- **Взаимодействие**: Работает с `TableStructure` и `ImportContext`, передавая управление следующему middleware.

### 5. **Table Handlers (Обработчики таблиц)**
- **Расположение**: `XmlFlow.Handlers`
- **Описание**: Отвечают за преобразование XML-узлов в структуру таблиц (`TableStructure`).
- **Ключевые компоненты**:
  - **`BaseTableHandler`**: Абстрактный базовый класс с общей логикой обработки.
  - **`DefaultTableHandler`**: Реализация по умолчанию.
  - **`TableHandlerFactory`**: Фабрика для выбора подходящего обработчика на основе узла XML.
- **Роль**: Определяют схему, имя таблицы и извлекают колонки из XML-узлов.
- **Взаимодействие**: Используются `ParserMiddleware` для обработки каждого узла.

### 6. **Database Repository (Репозиторий БД)**
- **Расположение**: `XmlFlow.Repositories`
- **Описание**: Об- **CachedDbRepository**: Реализация с кэшированием метаданных таблиц и колонок.
- **Описание**: Обеспечивает доступ к базе данных через интерфейс `IDbRepository`.
- **Функции**: Проверка существования таблиц/колонок, вставка данных, управление транзакциями.
- **Особенности**: Использует Dapper для выполнения SQL-запросов и `IMemoryCache` для оптимизации.

### 7. **Mappings (Преобразования данных)**
- **Расположение**: `XmlFlow.Mappings`
- **Описание**: Модули для преобразования типов данных и имен.
- **Ключевые компоненты**:
  - **`CaseConverter`**: Преобразует имена в snake_case, kebab-case и т.д.
  - **`DataConverter`**: Конвертирует значения в нужный тип данных (`DbType`).
  - **`ITypeMapper`**: Сопоставляет типы данных БД с .NET-типами (например, `PostgresTypeMapper`).
- **Роль**: Обеспечивает совместимость данных между XML и БД.

### 8. **Rules (Правила обработки)**
- **Расположение**: `XmlFlow.Interfaces` и примеры в `XmlFlow.Example`
- **Описание**: Настраиваемые правила для очистки (`ISanitizationRule`) и трансформации (`IDataTransformationRule`) данных.
- **Примеры**: Удаление ведущих нулей (`ExampleCodeSanitize`), форматирование дат (`DateTransformationRule`).

### Взаимодействие компонентов
1. `PipelineStarter` запускает процесс, передавая XML в `Document Pipeline`.
2. `ParserMiddleware` использует `TableHandlerFactory` для создания `TableStructure` из XML-узлов.
3. `TablePipelineMiddleware` передает таблицы в `Table Pipeline`.
4. Middleware в `Table Pipeline` проверяют схему (`DatabaseSchemaValidationMiddleware`), преобразуют данные (`ApplyCaseConvertorMiddleware`) и вставляют их в БД (`DataInsertionMiddleware`) через `IDbRepository`.
5. `ImportContext` хранит промежуточные данные между шагами.

---

## Настройка

Проект использует `IServiceCollection` для настройки зависимостей. Основная конфигурация находится в `Program.cs`. Вот пример минимальной настройки:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogging(logging => logging.AddSerilog());
builder.Services.AddControllers();
builder.Services.AddScoped<DbConnection>(_ => new NpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddParsinPipeline(LetterCase.Lower, CaseType.Snake);
builder.Services.AddTableHandlers();
builder.Services.AddDatabaseServices(typeof(CachedDbRepository), typeof(PostgresTypeMapper));

var app = builder.Build();
app.UseRouting();
app.MapControllers();
app.Run();
```

### Добавление Middleware
Для настройки обработки XML и таблиц добавьте нужные middleware:

```csharp
builder.Services.AddDocumentMiddleware(
    typeof(ErrorHandlingMiddleware),
    typeof(ParserMiddleware),
    typeof(TransactionManagementMiddleware)
);

builder.Services.AddTableMiddleware(
    typeof(DataSanitizationMiddleware),
    typeof(DataInsertionMiddleware)
);
```

### Добавление XSD-схем
Для валидации XML добавьте схемы в папку `xsd`, настройте словарь и добавьте middleware `XsdSchemaValidationMiddleware` в пайплайн обработки документа:

```csharp
builder.Services.AddXmlSchemaDictionary("xsd", new Dictionary<string, string>
{
    ["Example"] = "example.xsd"
});
```

---

## Использование

1. **Запустите API**:
   После запуска приложение будет доступно по адресу `https://localhost:5001` (или другой порт, указанный в конфигурации).

2. **Отправьте XML**:
   Используйте POST-запрос к endpoint `/api/import` с XML в теле запроса:
   ```bash
   curl -X POST https://localhost:5001/api/import \
   -H "Content-Type: application/xml" \
   -d '<?xml version="1.0" encoding="UTF-8"?><root><item><id>1</id><name>Test</name></item></root>'
   ```

   Или через код:
   ```csharp
   var client = new HttpClient();
   var content = new StringContent("<root><item><id>1</id><name>Test</name></item></root>", Encoding.UTF8, "application/xml");
   var response = await client.PostAsync("https://localhost:5001/api/import", content);
   ```

3. **Проверьте логи**:
   Логи будут выводиться в консоль (или другой sink, настроенный через Serilog).

---

## Пример XML

```xml
<?xml version="1.0" encoding="UTF-8"?>
<root>
    <item>
        <id>1</id>
        <name>Test Item</name>
        <code>00123</code>
        <createdDate>2025-04-01</createdDate>
    </item>
    <item>
        <id>2</id>
        <name>Another Item</name>
        <code>00456</code>
        <createdDate>2025-04-02</createdDate>
    </item>
</root>
```

Этот XML будет преобразован в таблицу `item` со столбцами `id`, `name`, `code`, `created_date` (в snake_case), очищен от ведущих нулей в `code` и вставлен в базу данных.

---

## Расширение функциональности

1. **Добавление обработчиков таблиц**:
   Реализуйте интерфейс `ITableHandler` и добавьте в DI:
   ```csharp
   public class CustomTableHandler : ITableHandler { /* Реализация */ }
   builder.Services.AddTableHandlers(typeof(CustomTableHandler));
   ```

2. **Добавление правил очистки**:
   Реализуйте `ISanitizationRule`:
   ```csharp
   public class CustomSanitize : ISanitizationRule
   {
       public bool CanApply(string columnName) => columnName == "name";
       public string Sanitize(string value) => value.ToUpper();
   }
   builder.Services.AddSanitizationRules(typeof(CustomSanitize));
   ```

3. **Добавление трансформаций**:
   Реализуйте `IDataTransformationRule`:
   ```csharp
   public class CustomTransform : IDataTransformationRule
   {
       public bool CanApply(DbType type, string columnName) => type == DbType.Int32;
       public object Transform(object? value) => Convert.ToInt32(value) * 2;
   }
   builder.Services.AddTransformationRules(typeof(CustomTransform));
   ```

---

## Лицензия

Copyright (c) 2025 Тимофеев Михаил Сергеевич. Все права защищены.

---

Если у вас есть вопросы или предложения, создайте issue в репозитории или свяжитесь с разработчиком!