import { useEffect, useState } from 'react';
import type { HabitDayView } from '../../api/generatedApi';
import { HabitType } from './todaySource';
import StreakBadge from './StreakBadge';

interface HabitRowProps {
  habit: HabitDayView;
  saving: boolean;
  onSave: (value: number) => void;
}

const SUCCESS = '#10b981';

const num = (v: number | string | null): number | null =>
  v === null || v === '' ? null : Number(v);

/** A single habit's row on the Today screen: name + streak on the left, its
 *  control (checkbox for boolean, number stepper for numeric) on the right. */
const HabitRow = ({ habit, saving, onSave }: HabitRowProps) => {
  const isBoolean = Number(habit.type) === HabitType.Boolean;
  const target = num(habit.target);
  const streak = Number(habit.currentStreak);

  const meta = isBoolean
    ? null
    : [target !== null ? `Goal ${target}` : null, habit.unit].filter(Boolean).join(' ');

  return (
    <div
      className="glass"
      style={{
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'space-between',
        gap: '16px',
        padding: '16px 20px',
        borderRadius: '18px',
        borderColor: habit.isComplete ? 'rgba(16, 185, 129, 0.4)' : 'var(--glass-border)',
        opacity: saving ? 0.7 : 1,
        transition: 'border-color 0.3s ease, opacity 0.2s ease',
      }}
    >
      <div style={{ display: 'flex', flexDirection: 'column', gap: '4px', minWidth: 0 }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
          <span
            style={{
              fontWeight: 600,
              fontSize: '1.05rem',
              color: 'var(--text-primary)',
              overflow: 'hidden',
              textOverflow: 'ellipsis',
              whiteSpace: 'nowrap',
            }}
          >
            {habit.name}
          </span>
          <StreakBadge streak={streak} />
        </div>
        {meta && (
          <span style={{ fontSize: '0.8rem', color: 'var(--text-secondary)' }}>{meta}</span>
        )}
      </div>

      {isBoolean ? (
        <BooleanControl complete={habit.isComplete} onToggle={() => onSave(habit.isComplete ? 0 : 1)} />
      ) : (
        <NumericControl
          value={num(habit.value)}
          unit={habit.unit}
          complete={habit.isComplete}
          onSave={onSave}
        />
      )}
    </div>
  );
};

const BooleanControl = ({ complete, onToggle }: { complete: boolean; onToggle: () => void }) => (
  <button
    onClick={onToggle}
    role="checkbox"
    aria-checked={complete}
    aria-label={complete ? 'Mark not done' : 'Mark done'}
    style={{
      width: '40px',
      height: '40px',
      borderRadius: '50%',
      flexShrink: 0,
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'center',
      border: complete ? `2px solid ${SUCCESS}` : '2px solid var(--glass-border)',
      background: complete ? SUCCESS : 'transparent',
      color: '#fff',
      transition: 'all 0.2s ease',
    }}
  >
    {complete && (
      <svg viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" strokeWidth="3" strokeLinecap="round" strokeLinejoin="round">
        <polyline points="20 6 9 17 4 12" />
      </svg>
    )}
  </button>
);

const NumericControl = ({
  value,
  unit,
  complete,
  onSave,
}: {
  value: number | null;
  unit: string | null;
  complete: boolean;
  onSave: (value: number) => void;
}) => {
  const step = unit === 'h' ? 0.5 : 1;
  const [text, setText] = useState(value === null ? '' : String(value));

  // Keep the field in sync when the underlying value changes (e.g. date change).
  useEffect(() => {
    setText(value === null ? '' : String(value));
  }, [value]);

  const commit = (raw: string) => {
    const n = raw === '' ? 0 : Number(raw);
    if (!Number.isNaN(n)) onSave(Math.max(0, n));
  };

  const bump = (delta: number) => {
    const next = Math.max(0, (value ?? 0) + delta);
    setText(String(next));
    onSave(next);
  };

  const btnStyle: React.CSSProperties = {
    width: '32px',
    height: '32px',
    borderRadius: '10px',
    fontSize: '1.1rem',
    color: 'var(--text-primary)',
    background: 'var(--glass-hover-bg)',
    border: '1px solid var(--glass-border)',
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'center',
  };

  return (
    <div style={{ display: 'flex', alignItems: 'center', gap: '8px', flexShrink: 0 }}>
      <button aria-label="Decrease" onClick={() => bump(-step)} style={btnStyle}>−</button>
      <input
        type="number"
        inputMode="decimal"
        step={step}
        min={0}
        aria-label={unit ? `value in ${unit}` : 'value'}
        value={text}
        onChange={e => setText(e.target.value)}
        onBlur={e => commit(e.target.value)}
        onKeyDown={e => {
          if (e.key === 'Enter') (e.target as HTMLInputElement).blur();
        }}
        style={{
          width: '64px',
          textAlign: 'center',
          padding: '8px',
          borderRadius: '10px',
          fontSize: '1rem',
          fontFamily: 'inherit',
          color: 'var(--text-primary)',
          background: 'var(--bg-card)',
          border: `1px solid ${complete ? SUCCESS : 'var(--glass-border)'}`,
          outlineColor: 'var(--accent-primary)',
        }}
      />
      <button aria-label="Increase" onClick={() => bump(step)} style={btnStyle}>+</button>
    </div>
  );
};

export default HabitRow;
