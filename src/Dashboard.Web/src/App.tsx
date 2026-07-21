import { getLatestReadings } from "./api/client";
import { AlertsPanel } from "./components/AlertsPanel";
import { StationCard } from "./components/StationCard";
import { usePolling } from "./hooks/usePolling";

function App() {
  const { data: readings, error, isLoading } = usePolling(getLatestReadings);
  const sortedReadings = [...(readings ?? [])].sort((a, b) =>
    a.stationId.localeCompare(b.stationId),
  );

  return (
    <div className="app">
      <header className="app__header">
        <h1>Weather Home Station</h1>
      </header>

      <main className="app__main">
        {isLoading && !readings && <p className="station-card__hint">Loading stations…</p>}

        {error && !readings && (
          <p className="station-card__hint station-card__hint--error">Unable to reach the dashboard API.</p>
        )}

        {readings && readings.length === 0 && (
          <p className="station-card__hint">No stations have reported readings yet.</p>
        )}

        <div className="station-grid">
          {sortedReadings.map((reading) => (
            <StationCard key={reading.stationId} reading={reading} />
          ))}
        </div>

        <AlertsPanel />
      </main>
    </div>
  );
}

export default App;
