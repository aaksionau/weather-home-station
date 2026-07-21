# Weather Home Station

A home weather station (ESP32 + BME280 + wind/rain/light sensors) feeding a
distributed, event-driven backend built to practice distributed systems
patterns (Kafka, workers, CQRS-ish read/write splits) and to experiment with
AI-based weather prediction using my own sensor data.

## Target Architecture

Four systems, connected by Kafka (events) and PostgreSQL (shared source of
truth for reads):

```
 OUTSIDE STATION
 ESP32 + BME280 + Wind + Rain + Light
        │ HTTP/HTTPS
        ▼
 WeatherGateway.API  (validation, auth, rate limit)
        │
        ▼
 Kafka: weather.raw
        │
        ▼
 WeatherProcessor.Worker (enrichment) ──► PostgreSQL: readings
        │                                 (owned by Processor)
        ▼
 Kafka: weather.processed                     Predictions Worker (scheduled)
        │                                     pulls history from
        │                                     PostgreSQL: readings, runs
        ▼                                     model, writes to PostgreSQL:
 Rules Engine Worker  ◄── Kafka: weather.forecast ◄── forecasts (owned by
 (thresholds, anomaly checks)                         Predictions)
        │
        ├──────────────► PostgreSQL: alerts
        │                 (owned by Rules Engine)
        ▼
 Kafka: weather.alerts
        │
        ▼
 Notification Worker (optional)


 Dashboard.API (reads readings, alerts, forecasts — no Kafka dep, no writes)
        │
        ▼
 React / Angular Dashboard
```

| Kafka topic | Producer | Consumer(s) |
|---|---|---|
| `weather.raw` | WeatherGateway.API | WeatherProcessor.Worker |
| `weather.processed` | WeatherProcessor.Worker | Rules Engine Worker |
| `weather.forecast` | **Predictions Worker** | **Rules Engine Worker** |
| `weather.alerts` | Rules Engine Worker | Notification Worker (optional) |

1. **Ingestion + enrichment** — `WeatherGateway.API` → `weather.raw` (Kafka)
   → `WeatherProcessor.Worker` enriches and owns the `readings` table,
   then republishes to `weather.processed`.
2. **Dashboard** (`Dashboard.API` + React/Angular) — reads across
   `readings`, `alerts`, and `forecasts`, but owns none of them and never
   writes. No Kafka dependency; the dashboard's schema stays decoupled
   from event schemas.
3. **Rules Engine + Alerts** — consumes `weather.processed` (real-time
   thresholds) and `weather.forecast` (predicted conditions), owns the
   `alerts` table, and publishes `weather.alerts` for an optional
   Notification Worker (email/push).
4. **Predictions** — scheduled job that pulls historical readings from
   PostgreSQL, runs a model, owns the `forecasts` table, and publishes to
   `weather.forecast` (Kafka) so the Rules Engine can evaluate it.

Each table has exactly one writer; `Dashboard.API` is the only
cross-cutting reader.

## Current Status

Four pieces of this diagram exist today (see `src/` and
`docker-compose.yml`):

- **WeatherGateway.API** — accepts readings, publishes to `weather.raw`.
- **WeatherProcessor.Worker** — consumes `weather.raw`, enriches, writes
  raw readings to **PostgreSQL**, and publishes to `weather.processed`.
- **WeatherRules.Worker** (Rules Engine Worker) — consumes
  `weather.processed`, evaluates threshold rules via
  [microsoft/RulesEngine](https://github.com/microsoft/RulesEngine), owns
  the `alerts` table in **PostgreSQL**, and publishes to `weather.alerts`.
  Rule definitions are JSON workflows stored in **Azure Blob Storage**
  (Azurite locally) rather than in code, so thresholds can change without
  a redeploy; a default rule set is seeded into the blob container on
  first run if none exists. It does not yet consume `weather.forecast`
  (Predictions Worker doesn't exist yet).
- **Dashboard.API** — read-only API over `readings` and `alerts` in
  **PostgreSQL** (`GET /api/readings`, `GET /api/readings/{stationId}/latest`,
  `GET /api/alerts`). No Kafka dependency and no writes, per the target
  design. Doesn't read `forecasts` yet (Predictions Worker doesn't exist
  yet, so that table doesn't either).

Predictions Worker and the Notification Worker are not built yet — the
diagram above is the target design, not the current state.

### Note: current enrichment is not enough for real prediction

`WeatherReading` (both `WeatherGateway.API` and `WeatherProcessor.Worker`)
only carries `Temperature`, `Humidity`, and `Pressure` — wind, rain, and
light aren't in the schema yet, despite being listed as station sensors.
`WeatherEnrichmentCalculator` only derives dew point, heat index, absolute
humidity, and vapor pressure deficit — all deterministic functions of
temperature + humidity, good for dashboard display but not new signal for
a model.

Before building the Predictions Worker:

- **Add wind + rain (+ light if easy) to the ingestion schema.** Pressure
  tendency + wind shift are the classic short-term forecasting signals;
  months of temp/humidity/pressure-only history can't be recovered later.
- **Decide where trend features live.** Instantaneous readings are weak
  predictors — 3h pressure/temp tendency matters more. `Enrich()` only
  sees one reading at a time, so this likely belongs in the Predictions
  Worker (windowed SQL query over history) rather than in enrichment.
- **Add time/seasonality features** (hour-of-day, day-of-year) cheaply at
  the Predictions Worker/query layer.
- **Handle data quality** (sensor dropout, duplicate/out-of-order
  timestamps) before relying on the logged history for training.
