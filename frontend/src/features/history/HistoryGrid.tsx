import type { HabitHistory, DayCompletion } from '../../api/generatedApi';
import StreakBadge from '../today/StreakBadge';

/** A logged-but-incomplete day (e.g. 5 of 8 glasses) — value present, not complete. */
const isPartial = (d: DayCompletion): boolean => d.value !== null && !d.isComplete;

const cellStyle = (d: DayCompletion): React.CSSProperties => {
  const base: React.CSSProperties = {
    width: '16px',
    height: '16px',
    borderRadius: '4px',
    flex: '0 0 auto',
  };
  if (d.isComplete) return { ...base, background: 'var(--accent-primary)' };
  if (isPartial(d)) return { ...base, background: 'rgba(245, 158, 11, 0.55)' };
  return { ...base, background: 'var(--bg-card)', border: '1px solid var(--glass-border)' };
};

const cellTitle = (d: DayCompletion): string => {
  const status = d.isComplete ? 'done' : d.value !== null ? `logged (${d.value})` : 'no entry';
  return `${d.date} — ${status}`;
};

interface HistoryGridProps {
  history: HabitHistory;
}

/** One habit's card: name, current + longest streak, and a 30-day completion grid. */
const HistoryGrid = ({ history }: HistoryGridProps) => {
  const current = Number(history.currentStreak);
  const longest = Number(history.longestStreak);

  return (
    <div
      className="glass"
      style={{
        background: 'var(--bg-card)',
        border: '1px solid var(--glass-border)',
        borderRadius: '16px',
        padding: '20px',
      }}
    >
      <header
        style={{
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'space-between',
          gap: '12px',
          marginBottom: '16px',
        }}
      >
        <h2 style={{ fontSize: '1.05rem', fontWeight: 700, margin: 0 }}>{history.name}</h2>
        <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
          <StreakBadge streak={current} />
          {longest > 0 && (
            <span
              title={`Longest streak: ${longest} days`}
              style={{
                fontSize: '0.8rem',
                fontWeight: 600,
                color: 'var(--text-secondary)',
              }}
            >
              best {longest}
            </span>
          )}
        </div>
      </header>

      <div
        role="img"
        aria-label={`${history.name}: ${history.days.filter(d => d.isComplete).length} of ${history.days.length} days complete in the last ${history.days.length} days`}
        style={{ display: 'flex', flexWrap: 'wrap', gap: '5px' }}
      >
        {history.days.map(d => (
          <span key={d.date} title={cellTitle(d)} style={cellStyle(d)} />
        ))}
      </div>
    </div>
  );
};

export default HistoryGrid;
