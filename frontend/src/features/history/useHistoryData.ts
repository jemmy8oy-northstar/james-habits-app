import { useMemo } from 'react';
import type { HabitHistory } from '../../api/generatedApi';
import { getHabitHistory, habitIds } from '../today/todaySource';

/**
 * Data layer for the History screen. Today it reads the typed fixtures in
 * `todaySource`; the component tree only sees `HabitHistory[]`.
 *
 * TO WIRE THE REAL BACKEND: swap the fixture call for the generated RTK-Query
 * hook, one call per habit — the component API stays identical:
 *
 *   const habits = useGetHabitsQuery().data ?? [];
 *   const histories = habits.map(h =>
 *     useGetHabitHistoryQuery({ id: Number(h.id), historyDays: days }).data);
 *
 * (In practice you'd add a `GET /api/history?days=30` batch endpoint rather than
 * fan out N calls, but the shape returned here — one `HabitHistory` per habit —
 * is exactly what the screen consumes either way.)
 */
export function useHistoryData(days = 30): HabitHistory[] {
  return useMemo(() => habitIds().map(id => getHabitHistory(id, days)), [days]);
}
