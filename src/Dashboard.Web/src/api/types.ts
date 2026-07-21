export interface WeatherReading {
  id: number;
  stationId: string;
  temperature: number;
  humidity: number;
  pressure: number;
  dewPoint: number;
  heatIndex: number;
  absoluteHumidity: number;
  vaporPressureDeficit: number;
  timestamp: string;
  processedAt: string;
}

export type AlertSeverity = "Info" | "Warning" | "Critical";

export interface Alert {
  id: number;
  stationId: string;
  ruleName: string;
  severity: AlertSeverity;
  message: string;
  readingTimestamp: string;
  triggeredAt: string;
}
