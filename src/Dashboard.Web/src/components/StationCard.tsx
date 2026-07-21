import { getLatestReading } from "../api/client";
import type { StationConfig } from "../config/stations";
import { formatNumber, formatRelativeTime } from "../format";
import { usePolling } from "../hooks/usePolling";
import { MetricTile } from "./MetricTile";

const STALE_AFTER_MS = 5 * 60 * 1000;

export function StationCard({ station }: { station: StationConfig }) {
  const fetchReading = () => getLatestReading(station.id);
  const { data: reading, error, isLoading, lastUpdated } = usePolling(fetchReading);

  const isStale =
    reading != null && Date.now() - new Date(reading.timestamp).getTime() > STALE_AFTER_MS;

  return (
    <section className={`station-card${isStale ? " station-card--stale" : ""}`}>
      <header className="station-card__header">
        <h2>{station.label}</h2>
        <span className={`status-dot${isStale || !reading ? " status-dot--offline" : ""}`} />
      </header>

      {isLoading && !reading && <p className="station-card__hint">Loading…</p>}

      {error && !reading && <p className="station-card__hint station-card__hint--error">Unable to reach the dashboard API.</p>}

      {!isLoading && !error && !reading && (
        <p className="station-card__hint">No readings yet for this station.</p>
      )}

      {reading && (
        <>
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
        </>
      )}

      {lastUpdated && (
        <p className="station-card__polled">Last checked {formatRelativeTime(lastUpdated)}</p>
      )}
    </section>
  );
}
