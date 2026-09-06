import { useCallback, useEffect, useState } from 'react';
import type { DayView } from '../../api/generatedApi';
import { getDay, upsertEntry } from './todaySource';

/**
 * Data layer for the Today screen. Today it reads/writes the typed fixtures in
 * `todaySource`; the component tree only sees this hook's shape.
 *
 * TO WIRE THE REAL BACKEND: replace the fixture calls below with the generated
 * RTK-Query hooks — the component API stays identical:
 *
 *   const { data: day, isLoading } = useGetDayQuery({ date });
 *   const [upsert] = useUpsertEntryMutation();
 *   const saveEntry = (habitId, value) =>
 *     upsert({ entryUpsertRequest: { habitId, date, value } });
 *
 * The optimistic update below mirrors what RTK-Query's `onQueryStarted` patch
 * would do, so the swap doesn't change the felt behaviour.
 */
export interface TodayData {
  day: DayView;
  isLoading: boolean;
  savingHabitId: number | null;
  saveEntry: (habitId: number, value: number) => void;
}

export function useTodayData(date: string): TodayData {
  const [day, setDay] = useState<DayView>(() => getDay(date));
  const [savingHabitId, setSavingHabitId] = useState<number | null>(null);

  useEffect(() => {
    setDay(getDay(date));
  }, [date]);

  const saveEntry = useCallback(
    (habitId: number, value: number) => {
      // Optimistic: reflect the new value immediately for a snappy tap.
      setDay(prev => ({
        ...prev,
        habits: prev.habits.map(h =>
          Number(h.habitId) === habitId ? { ...h, value } : h,
        ),
      }));
      setSavingHabitId(habitId);

      // Persist, then reconcile (recomputes completion + streak).
      upsertEntry({ habitId, date, value });
      setDay(getDay(date));
      setSavingHabitId(null);
    },
    [date],
  );

  return { day, isLoading: false, savingHabitId, saveEntry };
}
