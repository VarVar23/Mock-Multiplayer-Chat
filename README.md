# Тестовое задание для Middle-разработчика (Чат и уведомления в мультиплеерной сессии)

Репозиторий для выполнения тестового задания от LLC SmartPayments "Расширение предоставленного шаблона проекта для создания системы чата и уведомлений в мультиплеерной сессии"
[Ссылка на репозиторий](https://github.com/YamakasiTeam/CoreApplicantsRepo)

### Описание
Реализация системы чата и уведомлений для мультиплеерной MOBA-сессии с использованием **mocked сетевой логики**. Поддерживается:

- Командный и публичный чат
- Уведомления (start/kill)
- Эмуляция сетевых задержек, отключений, повторных подключений
- Поддержка нескольких клиентов в одной сессии
- Интеграция с UI (Unity + TextMeshPro)

### Архитектура

Проект реализован с соблюдением **layered архитектуры**, принципов **SOLID**, **паттернов проектирования** и **реактивного программирования**.

### Слои

- **UI Layer** (Unity):
  - ChatUIController.cs — UI-обёртка для ChatManager, поддержка TextMeshPro
- **Application/Core Layer**:
  - ChatManager.cs — главный фасад для чата и уведомлений
  - ChatMediator.cs — реализация паттерна Mediator
  - NotificationBuilder.cs — реализация Builder для событий
- **Network/Mock Layer**:
  - MockChatNetwork.cs, MockChatRoom.cs — имитация мультиплеера (Photon-like поведение)
- **Tests Layer**:
  - ChatManagerTests.cs — Unit-тесты (Moq + NUnit)

### Используемые технологии и подходы

### Паттерны
- **Mediator** — для маршрутизации событий между компонентами
- **Builder** — для создания уведомлений
- **Dependency Injection** — внедрение IChatNetwork, IChatMediator в ChatManager

### Реактивное программирование
- System.Reactive.Subjects (Subject<T>) — подписка на события сообщений и уведомлений
- Используются IObservable<T> потоки

### Асинхронность
- async/await, Task.Delay(...) для имитации latency
- Retry-логика при disconnect → reconnect → resend

### Мультиплеерная логика (mocked)
- MockChatRoom содержит список клиентов
- Сообщения рассылаются по правилам:
  - ChatType.Public — всем
  - ChatType.Team — только команде
- SimulateDisconnect() / SimulateReconnect() — поведение сетевого клиента
- Уведомления рассылаются всем

### Тестирование

Используется **NUnit + Moq**. Покрытие:
- Отправка сообщений
- Уведомления через Builder
- Retry после disconnect
- Проверка рассылаемости на несколько клиентов
- Mediator работает корректно
