/**
 * Fixture-backed data source for the Today screen.
 *
 * WHY THIS EXISTS: the .NET backend needs Postgres and can't boot in every
 * environment (CI, the autonomous sandbox), so the UI is driven by typed
 * in-memory fixtures instead of a live API — the same pattern macro-metrics
 * uses. Crucially the fixtures are declared as the *generated* OpenAPI types
 * (`DayView`, `HabitDayView`, `EntryUpsertRequest`), so if the real contract
 * changes, `npm run codegen` regenerates the types and any drift here fails to
 * compile. That makes these fixtures self-checking rather than free-floating
 * mock data.
 *
 * TO GO LIVE: nothing in the components changes — only `useTodayData` swaps its
 * internals from `getDay`/`upsertEntry` here to the generated RTK-Query hooks
 * `useGetDayQuery` / `useUpsertEntryMutation`. See the note in useTodayData.ts.
 */
import type { DayView, HabitDayView, EntryUpsertRequest } from '../../api/generatedApi';

/** Mirrors the backend `HabitType` enum (declaration order: Boolean=0, Numeric=1). */
export const HabitType = { Boolean: 0, Numeric: 1 } as const;

const toNum = (v: number | string | null | undefined): number | null =>
  v === null || v === undefined || v === '' ? null : Number(v);

// --- date helpers (calendar-day, no time — matches design A7) ------------------

/** `YYYY-MM-DD` for a Date, in the client's local calendar day. */
export const toIso = (d: Date): string => {
  const y = d.getFullYear();
  const m = String(d.getMonth() + 1).padStart(2, '0');
  const day = String(d.getDate()).padStart(2, '0');
  return `${y}-${m}-${day}`;
};

export const todayIso = (): string => toIso(new Date());

export const addDays = (iso: string, delta: number): string => {
  const d = new Date(`${iso}T00:00:00`);
  d.setDate(d.getDate() + delta);
  return toIso(d);
};

// --- seed model ----------------------------------------------------------------

interface SeedHabit {
  id: number;
  name: string;
  type: number;
  unit: string | null;
  target: number | null;
  sortOrder: number;
  /** raw value logged the days it was done (1 for boolean, the number for numeric) */
  doneValue: number;
  /** how many consecutive days *before today* are already complete */
  priorStreak: number;
  /** value already logged for today, or null if today is still open */
  todayValue: number | null;
}

const SEED: SeedHabit[] = [
  { id: 1, name: 'Sleep', type: HabitType.Numeric, unit: 'h', target: 7.5, sortOrder: 0, doneValue: 7.5, priorStreak: 5, todayValue: null },
  { id: 2, name: 'Exercise', type: HabitType.Boolean, unit: null, target: null, sortOrder: 1, doneValue: 1, priorStreak: 3, todayValue: null },
  { id: 3, name: 'Read', type: HabitType.Boolean, unit: null, target: null, sortOrder: 2, doneValue: 1, priorStreak: 8, todayValue: 1 },
  { id: 4, name: 'Water', type: HabitType.Numeric, unit: 'glasses', target: 8, sortOrder: 3, doneValue: 8, priorStreak: 2, todayValue: 5 },
  { id: 5, name: 'Meditate', type: HabitType.Boolean, unit: null, target: null, sortOrder: 4, doneValue: 1, priorStreak: 0, todayValue: null },
];

/**
 * In-memory entry store: habitId -> (isoDate -> value). Seeded relative to the
 * real "today" so the demo always shows a populated, live-looking day with real
 * streaks. Mutations from upsertEntry persist here for the session.
 */
const store = new Map<number, Map<string, number>>();

(function seed() {
  const today = todayIso();
  for (const h of SEED) {
    const entries = new Map<string, number>();
    for (let back = 1; back <= h.priorStreak; back++) {
      entries.set(addDays(today, -back), h.doneValue);
    }
    if (h.todayValue !== null) entries.set(today, h.todayValue);
    store.set(h.id, entries);
  }
})();

// --- domain logic (mirrors the backend DayService, per design A3/A4) -----------

const habitMeta = (id: number): SeedHabit => SEED.find(h => h.id === id)!;

/** A3: boolean complete when ticked; numeric complete at target, or on any value if no target. */
const isComplete = (habit: SeedHabit, value: number | null): boolean => {
  if (value === null) return false;
  if (habit.type === HabitType.Boolean) return value >= 1;
  if (habit.target !== null) return value >= habit.target;
  return true; // numeric, no target → any logged value counts
};

/**
 * A4: current streak = consecutive complete days counting back from `date`.
 * If `date` is today and today isn't complete yet, the streak isn't broken —
 * we count back from yesterday (today is still "in progress").
 */
const streakAsOf = (habit: SeedHabit, entries: Map<string, number>, date: string): number => {
  const isToday = date === todayIso();
  let cursor = date;
  let streak = 0;

  const completeOn = (iso: string) => isComplete(habit, entries.get(iso) ?? null);

  if (isToday && !completeOn(date)) {
    cursor = addDays(date, -1); // today still open — don't count or break on it
  }
  while (completeOn(cursor)) {
    streak++;
    cursor = addDays(cursor, -1);
  }
  return streak;
};

const buildHabitDay = (habit: SeedHabit, entries: Map<string, number>, date: string): HabitDayView => {
  const value = entries.get(date) ?? null;
  return {
    habitId: habit.id,
    name: habit.name,
    type: habit.type,
    unit: habit.unit,
    target: habit.target,
    sortOrder: habit.sortOrder,
    value,
    isComplete: isComplete(habit, value),
    currentStreak: streakAsOf(habit, entries, date),
  };
};

// --- public "API" (the seam a real backend slots into) -------------------------

/** GET /api/days/{date} */
export const getDay = (date: string): DayView => ({
  date,
  habits: SEED
    .slice()
    .sort((a, b) => a.sortOrder - b.sortOrder)
    .map(h => buildHabitDay(h, store.get(h.id)!, date)),
});

/** PUT /api/entries — idempotent upsert, returns the updated habit day view. */
export const upsertEntry = (req: EntryUpsertRequest): HabitDayView => {
  const habitId = Number(req.habitId);
  const value = toNum(req.value) ?? 0;
  const entries = store.get(habitId)!;
  entries.set(String(req.date), value);
  return buildHabitDay(habitMeta(habitId), entries, String(req.date));
};
