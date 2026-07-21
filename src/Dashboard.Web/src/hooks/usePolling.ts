import { useCallback, useEffect, useRef, useState } from "react";

export const POLL_INTERVAL_MS = 30_000;

interface PollingState<T> {
  data: T | undefined;
  error: Error | undefined;
  isLoading: boolean;
  lastUpdated: Date | undefined;
  refresh: () => void;
}

export function usePolling<T>(
  fetchFn: () => Promise<T>,
  intervalMs: number = POLL_INTERVAL_MS,
): PollingState<T> {
  const [data, setData] = useState<T>();
  const [error, setError] = useState<Error>();
  const [isLoading, setIsLoading] = useState(true);
  const [lastUpdated, setLastUpdated] = useState<Date>();
  const fetchFnRef = useRef(fetchFn);
  fetchFnRef.current = fetchFn;

  const load = useCallback(async () => {
    try {
      const result = await fetchFnRef.current();
      setData(result);
      setError(undefined);
      setLastUpdated(new Date());
    } catch (err) {
      setError(err instanceof Error ? err : new Error(String(err)));
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    load();
    const id = setInterval(load, intervalMs);
    return () => clearInterval(id);
  }, [load, intervalMs]);

  return { data, error, isLoading, lastUpdated, refresh: load };
}
