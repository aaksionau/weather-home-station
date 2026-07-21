interface MetricTileProps {
  label: string;
  value: string;
  unit: string;
}

export function MetricTile({ label, value, unit }: MetricTileProps) {
  return (
    <div className="metric-tile">
      <span className="metric-tile__label">{label}</span>
      <span className="metric-tile__value">
        {value}
        <span className="metric-tile__unit">{unit}</span>
      </span>
    </div>
  );
}
