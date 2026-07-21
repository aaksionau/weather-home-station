// Station ids are assigned by the ESP32 devices themselves (e.g. `inside_01`,
// `outside_01`, `outside_02`) and discovered dynamically from
// `GET /api/readings/latest` — there is no fixed list to maintain here.
export function stationLabel(stationId: string): string {
  const [type, ...rest] = stationId.split("_");
  const label = type.charAt(0).toUpperCase() + type.slice(1);
  return rest.length > 0 ? `${label} ${rest.join("_")}` : label;
}
