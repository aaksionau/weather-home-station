import { AlertsPanel } from "./components/AlertsPanel";
import { StationCard } from "./components/StationCard";
import { STATIONS } from "./config/stations";

function App() {
  return (
    <div className="app">
      <header className="app__header">
        <h1>Weather Home Station</h1>
      </header>

      <main className="app__main">
        <div className="station-grid">
          {STATIONS.map((station) => (
            <StationCard key={station.id} station={station} />
          ))}
        </div>

        <AlertsPanel />
      </main>
    </div>
  );
}

export default App;
