import type { Alert, WeatherReading } from "./types";

async function getJson<T>(path: string): Promise<T> {
  const response = await fetch(path, { headers: { Accept: "application/json" } });
  if (!response.ok) {
    throw new Error(`${path} responded with ${response.status}`);
  }
  return (await response.json()) as T;
}

export function getLatestReadings(): Promise<WeatherReading[]> {
  return getJson<WeatherReading[]>("/api/readings/latest");
}

export function getRecentAlerts(limit = 25): Promise<Alert[]> {
  return getJson<Alert[]>(`/api/alerts?limit=${limit}`);
}
