export interface StationConfig {
  id: string;
  label: string;
}

// Update these to match the `stationId` values your ESP32 devices send to
// WeatherGateway.API.
export const STATIONS: StationConfig[] = [
  { id: "inside", label: "Inside" },
  { id: "outside", label: "Outside" },
];
