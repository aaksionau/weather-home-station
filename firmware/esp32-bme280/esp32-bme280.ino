// ESP32 + BME280 weather station firmware.
//
// Reads temperature/humidity/pressure from a BME280 over I2C and POSTs
// each reading as JSON to WeatherGateway.API's /api/weather-readings
// endpoint, matching the WeatherReading contract:
//   { StationId, Temperature, Humidity, Pressure, Timestamp }
//
// Libraries required (install via Arduino Library Manager):
//   - "Adafruit BME280 Library" (by Adafruit)
//   - "Adafruit Unified Sensor" (dependency of the above)
//   - "ArduinoJson" (by Benoit Blanchon)
// Board support: install "esp32 by Espressif Systems" via Boards Manager.
//
// Wiring (I2C, default ESP32 pins):
//   BME280 VCC  -> 3V3
//   BME280 GND  -> GND
//   BME280 SCL  -> GPIO 22
//   BME280 SDA  -> GPIO 21
//   BME280 addr -> 0x76 (default on most breakout boards; set 0x77 below if
//                  your module has the SDO pin pulled high)

#include <WiFi.h>
#include <HTTPClient.h>
#include <Wire.h>
#include <Adafruit_Sensor.h>
#include <Adafruit_BME280.h>
#include <ArduinoJson.h>

// ---- Configuration -------------------------------------------------------

const char* WIFI_SSID     = "ssid";
const char* WIFI_PASSWORD = "password";

// WeatherGateway.API ingestion endpoint. Update the host/port if the
// gateway moves; the path must stay /api/weather-readings.
const char* API_URL = "http://192.168.1.240:30135/api/weather-readings";

// Must start with "inside_" or "outside_" — WeatherGateway.API rejects
// anything else (see StationLocationParser on the backend).
const char* STATION_ID = "inside_01";

const uint8_t BME280_I2C_ADDRESS = 0x76;

const unsigned long READ_INTERVAL_MS = 60UL * 1000UL; // send a reading every 60s

// ---------------------------------------------------------------------------

Adafruit_BME280 bme;
bool bmeReady = false;

void connectWifi() {
  Serial.printf("Connecting to WiFi '%s'...\n", WIFI_SSID);
  WiFi.mode(WIFI_STA);
  WiFi.begin(WIFI_SSID, WIFI_PASSWORD);

  while (WiFi.status() != WL_CONNECTED) {
    delay(500);
    Serial.print(".");
  }

  Serial.printf("\nWiFi connected, IP address: %s\n", WiFi.localIP().toString().c_str());
}

void ensureWifiConnected() {
  if (WiFi.status() == WL_CONNECTED) {
    return;
  }
  Serial.println("WiFi disconnected, reconnecting...");
  connectWifi();
}

void sendReading(float temperatureC, float humidityPct, float pressureHpa) {
  HTTPClient http;
  http.begin(API_URL);
  http.addHeader("Content-Type", "application/json");

  JsonDocument doc;
  doc["StationId"] = STATION_ID;
  doc["Temperature"] = temperatureC;
  doc["Humidity"] = humidityPct;
  doc["Pressure"] = pressureHpa;
  // WeatherGateway.API expects an ISO-8601 timestamp with offset
  // (DateTimeOffset), sourced from NTP (see configTime in setup()).
  time_t now;
  time(&now);
  struct tm timeinfo;
  gmtime_r(&now, &timeinfo);
  char isoTimestamp[32];
  strftime(isoTimestamp, sizeof(isoTimestamp), "%Y-%m-%dT%H:%M:%S+00:00", &timeinfo);
  doc["Timestamp"] = isoTimestamp;

  String payload;
  serializeJson(doc, payload);

  Serial.printf("POST %s -> %s\n", API_URL, payload.c_str());
  int statusCode = http.POST(payload);

  if (statusCode > 0) {
    Serial.printf("Response: %d %s\n", statusCode, http.getString().c_str());
  } else {
    Serial.printf("HTTP request failed: %s\n", http.errorToString(statusCode).c_str());
  }

  http.end();
}

void setup() {
  Serial.begin(115200);
  delay(1000);

  Wire.begin();
  bmeReady = bme.begin(BME280_I2C_ADDRESS);
  if (!bmeReady) {
    Serial.println("Could not find a BME280 sensor, check wiring/address!");
  }

  connectWifi();

  // NTP sync so Timestamp reflects real UTC time instead of the ESP32's
  // epoch-since-boot clock.
  configTime(0, 0, "pool.ntp.org", "time.nist.gov");
}

void loop() {
  ensureWifiConnected();

  if (bmeReady) {
    float temperatureC = bme.readTemperature();
    float pressureHpa = bme.readPressure() / 100.0F;
    float humidityPct = bme.readHumidity();

    sendReading(temperatureC, humidityPct, pressureHpa);
  } else {
    Serial.println("Skipping read: BME280 not initialized.");
  }

  delay(READ_INTERVAL_MS);
}
