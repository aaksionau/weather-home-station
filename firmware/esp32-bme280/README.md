# ESP32 + BME280 firmware

Reads temperature/humidity/pressure from a BME280 and POSTs it to
`WeatherGateway.API` every 60 seconds.

## Setup

1. Install board support: **Boards Manager → esp32 by Espressif Systems**.
2. Install libraries via **Library Manager**:
   - Adafruit BME280 Library
   - Adafruit Unified Sensor
   - ArduinoJson
3. Wire the BME280 over I2C: `SCL -> GPIO22`, `SDA -> GPIO21`, `VCC -> 3V3`, `GND -> GND`.
4. Open `esp32-bme280.ino`, select your ESP32 board + port, and upload.

## Configuration

Edit the constants at the top of `esp32-bme280.ino`:

| Constant | Purpose |
|---|---|
| `WIFI_SSID` / `WIFI_PASSWORD` | WiFi credentials |
| `API_URL` | WeatherGateway.API ingestion endpoint |
| `STATION_ID` | Must start with `inside_` or `outside_` (backend requirement) |
| `BME280_I2C_ADDRESS` | `0x76` (default) or `0x77` depending on the breakout board's SDO pin |
| `READ_INTERVAL_MS` | How often to send a reading |

Open the Serial Monitor at 115200 baud to see WiFi connection status and
each HTTP request/response.
