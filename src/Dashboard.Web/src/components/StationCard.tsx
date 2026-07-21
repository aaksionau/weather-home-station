import type { WeatherReading } from "../api/types";
import { stationLabel } from "../config/stations";
import { formatNumber, formatRelativeTime } from "../format";
import { MetricTile } from "./MetricTile";

const STALE_AFTER_MS = 5 * 60 * 1000;

export function StationCard({ reading }: { reading: WeatherReading }) {
  const isStale = Date.now() - new Date(reading.timestamp).getTime() > STALE_AFTER_MS;

  return (
    <section className={`station-card${isStale ? " station-card--stale" : ""}`}>
      <header className="station-card__header">
        <h2>{stationLabel(reading.stationId)}</h2>
        <span className={`status-dot${isStale ? " status-dot--offline" : ""}`} />
      </header>

      <div className="station-card__primary">
        <span className="station-card__temp">{formatNumber(reading.temperature)}°F</span>
        {isStale && <span className="badge badge--stale">Stale</span>}
      </div>

      <div className="metric-grid">
        <MetricTile label="Humidity" value={formatNumber(reading.humidity)} unit="%" />
        <MetricTile label="Pressure" value={formatNumber(reading.pressure)} unit="hPa" />
        <MetricTile label="Dew point" value={formatNumber(reading.dewPoint)} unit="°F" />
        <MetricTile label="Heat index" value={formatNumber(reading.heatIndex)} unit="°F" />
      </div>

      <p className="station-card__updated">
        Reading from {formatRelativeTime(reading.timestamp)}
      </p>
    </section>
  );
}
