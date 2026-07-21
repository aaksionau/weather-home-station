import { getRecentAlerts } from "../api/client";
import { stationLabel } from "../config/stations";
import { formatRelativeTime } from "../format";
import { usePolling } from "../hooks/usePolling";
import type { AlertSeverity } from "../api/types";

function severityClass(severity: AlertSeverity): string {
  return `badge badge--${severity.toLowerCase()}`;
}

export function AlertsPanel() {
  const { data: alerts, error, isLoading } = usePolling(() => getRecentAlerts());

  return (
    <section className="alerts-panel">
      <h2>Recent alerts</h2>

      {isLoading && !alerts && <p className="station-card__hint">Loading…</p>}

      {error && !alerts && (
        <p className="station-card__hint station-card__hint--error">Unable to reach the dashboard API.</p>
      )}

      {alerts && alerts.length === 0 && <p className="station-card__hint">No alerts. All clear.</p>}

      {alerts && alerts.length > 0 && (
        <ul className="alerts-list">
          {alerts.map((alert) => (
            <li key={alert.id} className="alerts-list__item">
              <span className={severityClass(alert.severity)}>{alert.severity}</span>
              <div className="alerts-list__body">
                <p className="alerts-list__message">{alert.message}</p>
                <p className="alerts-list__meta">
                  {stationLabel(alert.stationId)} · {formatRelativeTime(alert.triggeredAt)}
                </p>
              </div>
            </li>
          ))}
        </ul>
      )}
    </section>
  );
}
