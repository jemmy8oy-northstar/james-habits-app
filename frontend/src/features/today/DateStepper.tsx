import { addDays, todayIso } from './todaySource';

interface DateStepperProps {
  date: string;
  onChange: (date: string) => void;
}

const arrowStyle: React.CSSProperties = {
  width: '40px',
  height: '40px',
  borderRadius: '12px',
  display: 'flex',
  alignItems: 'center',
  justifyContent: 'center',
  fontSize: '1.2rem',
  color: 'var(--text-primary)',
};

/** Human label for a date relative to today: Today / Yesterday / weekday date. */
const label = (iso: string): string => {
  const today = todayIso();
  if (iso === today) return 'Today';
  if (iso === addDays(today, -1)) return 'Yesterday';
  const d = new Date(`${iso}T00:00:00`);
  return d.toLocaleDateString(undefined, { weekday: 'short', month: 'short', day: 'numeric' });
};

/** ‹ Today › — step to previous days to back-fill; can't go past today. */
const DateStepper = ({ date, onChange }: DateStepperProps) => {
  const atToday = date >= todayIso();

  return (
    <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'center', gap: '12px' }}>
      <button
        className="glass"
        aria-label="Previous day"
        onClick={() => onChange(addDays(date, -1))}
        style={arrowStyle}
      >
        ‹
      </button>
      <div
        style={{
          minWidth: '160px',
          textAlign: 'center',
          fontFamily: 'var(--font-display)',
          fontWeight: 700,
          fontSize: '1.15rem',
          color: 'var(--text-primary)',
        }}
      >
        {label(date)}
      </div>
      <button
        className="glass"
        aria-label="Next day"
        disabled={atToday}
        onClick={() => onChange(addDays(date, 1))}
        style={{ ...arrowStyle, opacity: atToday ? 0.35 : 1, cursor: atToday ? 'default' : 'pointer' }}
      >
        ›
      </button>
    </div>
  );
};

export default DateStepper;
