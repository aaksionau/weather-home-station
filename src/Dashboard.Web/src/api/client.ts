import type { Alert, WeatherReading } from "./types";

async function getJson<T>(path: string): Promise<T> {
  const response = await fetch(path, { headers: { Accept: "application/json" } });
  if (!response.ok) {
    throw new Error(`${path} responded with ${response.status}`);
  }
  return (await response.json()) as T;
}

export function getLatestReading(stationId: string): Promise<WeatherReading | null> {
  return getJson<WeatherReading | null>(
    `/api/readings/${encodeURIComponent(stationId)}/latest`,
  ).catch((error: Error) => {
    if (error.message.includes("404")) {
      return null;
    }
    throw error;
  });
}

export function getRecentAlerts(limit = 25): Promise<Alert[]> {
  return getJson<Alert[]>(`/api/alerts?limit=${limit}`);
}
