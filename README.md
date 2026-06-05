# Sreda.Backend

Локальний backend **мовного інтерфейсу Sreda**: голос або текст → розпізнавання наміру → перевірка доступу → виконання → журнал подій → (опційно) синтез мовлення.

Стек: **.NET 10**, **ASP.NET Core**, **PostgreSQL**, **EF Core**, **MediatR**, **FluentValidation**, **SignalR**, **Serilog**, **Swashbuckle**.

---

## Структура рішення

| Проєкт | Призначення |
|--------|-------------|
| `Sreda.Backend.Domain` | Сутності, enum |
| `Sreda.Backend.Application` | Конвеєр команд, CQRS (MediatR), бізнес-логіка |
| `Sreda.Backend.Contracts` | DTO для API |
| `Sreda.Backend.Infrastructure` | EF Core, Whisper/Piper/ffmpeg, MQTT, сідер |
| `Sreda.Backend.API` | HTTP API, SignalR, Swagger |

---

## Вимоги

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- **PostgreSQL** (локально або Docker)
- Для голосу (`Voice:Provider = Real`):
  - [whisper.cpp](https://github.com/ggml-org/whisper.cpp) (`whisper-cli`) + модель (наприклад `ggml-small.bin`)
  - [Piper](https://github.com/rhasspy/piper) + голосові моделі (`en`, `ru`)
  - **ffmpeg** — конвертація аудіо, якщо формат не PCM WAV mono 16-bit

Без зовнішніх CLI можна працювати в режимі **`Voice:Provider = Mock`** (STT/TTS-заглушки).

---

## Швидкий старт

```bash
# 1. Клон / відкрити репозиторій
cd Sreda.Backend

# 2. Налаштувати PostgreSQL у appsettings.json (див. нижче)

# 3. Запуск API (міграції + сід виконуються автоматично при старті)
dotnet run --project Sreda.Backend.API
```

За замовчуванням (профіль `https`):

- HTTP: `http://localhost:5122`
- HTTPS: `https://localhost:7186`
- Swagger (Development): `/swagger`
- SignalR: `/hubs/events`

---

## Конфігурація

Основний файл: `Sreda.Backend.API/appsettings.json`.

```json
{
  "ConnectionStrings": {
    "Sreda": "Host=localhost;Port=5432;Database=SredaBD;Username=postgres;Password=..."
  },
  "Voice": {
    "Provider": "Real"
  },
  "Whisper": {
    "ExecutablePath": "C:\\tools\\whisper\\Release\\whisper-cli.exe",
    "ModelPath": "C:\\tools\\whisper\\models\\ggml-small.bin",
    "Language": "auto"
  },
  "Piper": {
    "DefaultLanguage": "en",
    "Voices": {
      "en": "C:\\tools\\piper\\models\\en\\...\\en_US-hfc_female-medium.onnx",
      "ru": "C:\\tools\\piper\\models\\ru\\...\\ru_RU-irina-medium.onnx"
    }
  },
  "CommandResolver": {
    "MinConfidence": 0.6
  },
  "Mqtt": {
    "Enabled": false
  }
}
```

| Ключ | Опис |
|------|------|
| `Voice:Provider` | `Mock` — заглушки; `Real` — whisper.cpp + Piper |
| `Whisper:Language` | `auto` — автовизначення мови (використовується для тексту відповіді та TTS) |
| `CommandResolver:MinConfidence` | Поріг впевненості для прийняття команди |
| `Mqtt:Enabled` | Шлюз пристроїв через MQTT (за замовчуванням вимкнено) |

Секрети й локальні шляхи до CLI краще виносити в `appsettings.Development.json` (не комітити паролі).

---

## Конвеєр обробки команди

Один шлях для голосу та тексту (`ICommandPipeline`):

```
аудіо/текст → STT (лише голос) → збереження Command
  → CommandResolver (фрази з БД, fuzzy)
  → CommandAccessPolicy
  → CommandExecutor (відповідь + оновлення DeviceState)
  → EventLogger + SignalR
```

Мова відповіді та озвучки для голосових запитів береться з **Whisper** (`auto-detected language`); тексти та TTS локалізовані (`en` / `ru` / `uk`). Підписи пристроїв — таблиця **`DeviceLocalizedLabels`**.

---

## API (огляд)

### Голос

| Метод | Шлях | Опис |
|-------|------|------|
| POST | `/api/voice/process` | Аудіо → STT → конвеєр → JSON |
| POST | `/api/voice/process-with-speech` | Те саме + TTS → `audioBase64` |
| POST | `/api/voice/speak` | Текст → TTS → WAV |

Query для `process-with-speech`: `userId`, `language` (перевизначення голосу Piper).

### Команди

| Метод | Шлях | Опис |
|-------|------|------|
| GET | `/api/commands` | Історія команд (`userId`, `status`, `skip`, `take`) |
| POST | `/api/commands/process-text` | Текст → конвеєр |
| POST | `/api/commands/resolve` | Діагностика наміру без виконання |

### Каталог і адміністрування

| Шлях | Ресурс |
|------|--------|
| `/api/available-commands` | Довідник команд (TurnLightOn, GetTime, …) |
| `/api/command-phrases` | Фрази для резолвера (ru/en) |
| `/api/users` | Користувачі та рівні доступу |
| `/api/user-command-overrides` | Індивідуальні дозволи |
| `/api/devices` | Пристрої + локалізації (`localizations[]`) |
| `/api/events` | Журнал подій |

### Realtime

- **SignalR hub:** `GET/WS` → `/hubs/events` — події `VoiceRecognized`, `CommandExecuted`, `DeviceUpdated` тощо.

Детальні схеми запитів/відповідей — у **Swagger** (`/swagger` у Development).

---

## Демо-дані (сід)

При старті (`DbSeeder`):

- користувач **Admin** (якщо `userId` не передано — використовується він);
- каталог команд і фраз (uk/ru/en);
- пристрій **`Light-1`** з локалізованими назвами та станом `{ "power": false, "brightness": 100 }`.

Приклад голосової команди: *«Выключи свет»* → `TurnLightOff` → оновлення `DeviceState`.

---

## Міграції БД

Міграції застосовуються автоматично в `Program.cs`. Вручну:

```bash
dotnet ef database update \
  --project Sreda.Backend.Infrastructure \
  --startup-project Sreda.Backend.API
```

Нова міграція:

```bash
dotnet ef migrations add <Name> \
  --project Sreda.Backend.Infrastructure \
  --startup-project Sreda.Backend.API
```

---

## Збірка

```bash
dotnet build Sreda.Backend.slnx
```

---

## Приклади

**Текстова команда:**

```http
POST /api/commands/process-text
Content-Type: application/json

{ "text": "который час", "userId": null }
```

**Голос (multipart):**

```http
POST /api/voice/process-with-speech
Content-Type: multipart/form-data

audio=<file.wav>
```

**Історія:**

```http
GET /api/commands?take=20
```

---

## Обмеження прототипу

- Повноцінна автентифікація/CORS для production не реалізовані (CORS відкритий для розробки).
- `OpenBrowser`, `Shutdown` тощо — текстові заглушки без інтеграції з ОС.
- MQTT вимкнено за замовчуванням; віртуальні пристрої оновлюються в PostgreSQL.
- React UI — окремий репозиторій / етап.

---

## Ліцензія

Курсовий / дипломний проєкт. Уточніть ліцензію в репозиторії за потреби.
