import { useMemo, useState } from 'react';
import DateStepper from '../features/today/DateStepper';
import HabitRow from '../features/today/HabitRow';
import { todayIso } from '../features/today/todaySource';
import { useTodayData } from '../features/today/useTodayData';

const Today = () => {
  const [date, setDate] = useState<string>(todayIso());
  const { day, savingHabitId, saveEntry } = useTodayData(date);

  const { done, total } = useMemo(() => {
    const habits = day.habits;
    return { done: habits.filter(h => h.isComplete).length, total: habits.length };
  }, [day]);

  const allDone = total > 0 && done === total;

  return (
    <section className="container" style={{ paddingTop: '96px', maxWidth: '560px' }}>
      <header style={{ textAlign: 'center', marginBottom: '24px' }}>
        <h1 style={{ fontSize: '2rem', marginBottom: '16px' }}>Habits</h1>
        <DateStepper date={date} onChange={setDate} />
        <p style={{ marginTop: '16px', color: 'var(--text-secondary)', fontSize: '0.95rem' }}>
          {allDone ? '🎉 All done for the day' : `${done} of ${total} done`}
        </p>
      </header>

      <div style={{ display: 'flex', flexDirection: 'column', gap: '12px' }}>
        {day.habits.map(habit => (
          <HabitRow
            key={String(habit.habitId)}
            habit={habit}
            saving={savingHabitId === Number(habit.habitId)}
            onSave={value => saveEntry(Number(habit.habitId), value)}
          />
        ))}
      </div>

      <p
        style={{
          textAlign: 'center',
          marginTop: '40px',
          fontSize: '0.8rem',
          color: 'var(--text-secondary)',
          opacity: 0.7,
        }}
      >
        Showing sample data — the real backend wires in without changing this screen.
      </p>
    </section>
  );
};

export default Today;
