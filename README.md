# MyApiApp

## Описание
MyApiApp — это серверное приложение для реализации телефонного справочника с авторизацией пользователей. Приложение использует SQLite в качестве базы данных и предоставляет API для взаимодействия с клиентами через HTTP-запросы.

## Структура проекта
    └───MyApiApp — основная папка проекта.
        ├───Controllers                     # папка с контроллерами
        │       AuthController.cs           # контроллер для авторизации
        │       ContactsController.cs       # контроллер для контактов
        │       RequestHistoryController.cs # контроллер для истории
        │
        ├───Services
        │       TokenService.cs             # Сервис для генерации токенов JWT
        │       PasswordService.cs          # Сервис для хеширования паролей 
        │
        ├───Data
        │       ApplicationDbContext.cs     # Контекст базы данных (SQLite)
        │
        ├───Middleware
        │       RequestLoggingMiddleware.cs # Запись информации о запросах
        │
        ├───DB
        │       users.db                    # база данных с пользователями (SQLite)
        │       contacts.db                 # база данных с контактами (SQLite)
        │       history.db                  # база данных с историей (SQLite)
        │
        ├───Models                          # Папка для моделей данных 
        │       User.cs                     # Модель пользователя
        │       Contact.cs                  # Модель контакта
        │       RequestHistory.cs           # Модель истории запросов
        │
        ├───Properties
        │       launchSettings.json         # Конфигурация для запуска приложения
        │
        ├───obj                             # Папка с объектными файлами (автоматически создается)
        │
        ├───bin                             # Папка для скомпилированных файлов (автоматически создается)
        │
        ├───appsettings.json                # Конфигурационный файл приложения (включает строку подключения к БД)
        ├───Program.cs                      # Основной файл для запуска приложения
        ├───BDback.cs                       # Основной файл для инициализации баз данных
        ├───Client.cs                       # Основной файл для запуска клиента
        ├───Server.cs                       # Основной файл для запуска сервера
        ├───MyApiApp.csproj                 # Файл проекта ASP.NET Core
        └───README.md                       # Дополнительные инструкции\описание проекта

## Объяснение основных папок и файлов

- **Controllers** – здесь хранятся контроллеры, которые отвечают за обработку HTTP-запросов. Например, файл `AuthController.cs` содержит логику для авторизации пользователей.

- **Data** – папка для контекста базы данных. В файле `ApplicationDbContext.cs` определяются наборы данных, с которыми будет работать приложение (например, `DbSet<User>` для сущности пользователя).

- **Models** – здесь хранятся модели данных, которые отражают структуру таблиц в базе данных. Например, файл `User.cs` содержит описание модели пользователя.

- **DB** – папка для хранения баз данных. Например `users.db`, которая содержит информацию о пользователях.

- **Middleware** – содержит промежуточное ПО, которое обрабатывает запросы и ответы, например, `RequestLoggingMiddleware.cs` для записи информации о запросах.

- **Services** – содержит сервисы, которые инкапсулируют общие функции и упрощают контроллеры, например, `TokenService.cs` – отвечает за генерацию и проверку JWT токенов, `PasswordService.cs` – отвечает за хеширование и верификацию паролей.

- **appsettings.json** – конфигурационный файл, который содержит настройки приложения, такие как строка подключения к базе данных.

- **Program.cs** – файл, который содержит основной код для запуска приложения ASP.NET Core.

- **Client.cs** – файл, реализующий клиентскую часть приложения, отправляющий запросы к API и обрабатывающий ответы.

- **Server.cs** – файл, отвечающий за запуск серверной части приложения, включая его настройку и управление работой.

## API
### Пользовательская аутентификация и авторизация
- POST /api/Auth/register — регистрация пользователя с генерацией токена.
- POST /api/Auth/login — авторизация пользователя с выдачей JWT токена.
- PATCH /api/Auth/password — изменение пароля пользователя и обновление токена.
### Контакты
- POST /api/Contacts — добавление нового контакта. Контакт включает поля: Name, PhoneNumber, Email, Address и UserAdd (идентификатор пользователя, который добавил контакт).
- GET /api/Contacts — получение всех контактов, добавленных авторизованным пользователем.
- GET /api/Contacts/{id} — получение одного контакта по идентификатору id.
- DELETE /api/Contacts/{id} — удаление контакта по его id, если он был добавлен авторизованным пользователем.
- PATCH /api/Contacts/{id} — обновление информации о контакте по его id (можно изменить Name, PhoneNumber, Email, Address).
- POST /api/Contacts/search — поиск контактов по ключевым словам среди контактов, добавленных пользователем. Поиск осуществляется по имени, телефону или email.
### История запросов
- GET /api/History — получение истории запросов пользователя (доступных действий, выполненных данным пользователем).
- DELETE /api/History — удаление истории запросов пользователя.