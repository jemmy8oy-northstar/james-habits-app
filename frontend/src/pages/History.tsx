import HistoryGrid from '../features/history/HistoryGrid';
import { useHistoryData } from '../features/history/useHistoryData';

const DAYS = 30;

const History = () => {
  const histories = useHistoryData(DAYS);

  return (
    <section className="container" style={{ paddingTop: '96px', maxWidth: '560px' }}>
      <header style={{ textAlign: 'center', marginBottom: '24px' }}>
        <h1 style={{ fontSize: '2rem', marginBottom: '8px' }}>History</h1>
        <p style={{ color: 'var(--text-secondary)', fontSize: '0.95rem' }}>
          Last {DAYS} days — one square per day, oldest on the left.
        </p>
      </header>

      <div style={{ display: 'flex', flexDirection: 'column', gap: '16px' }}>
        {histories.map(history => (
          <HistoryGrid key={String(history.habitId)} history={history} />
        ))}
      </div>

      <div
        style={{
          display: 'flex',
          justifyContent: 'center',
          gap: '20px',
          marginTop: '32px',
          fontSize: '0.78rem',
          color: 'var(--text-secondary)',
        }}
      >
        <LegendSwatch color="var(--accent-primary)" label="done" />
        <LegendSwatch color="rgba(245, 158, 11, 0.55)" label="logged, not complete" />
        <LegendSwatch color="var(--bg-card)" label="no entry" bordered />
      </div>

      <p
        style={{
          textAlign: 'center',
          marginTop: '24px',
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

const LegendSwatch = ({
  color,
  label,
  bordered,
}: {
  color: string;
  label: string;
  bordered?: boolean;
}) => (
  <span style={{ display: 'inline-flex', alignItems: 'center', gap: '6px' }}>
    <span
      style={{
        width: '12px',
        height: '12px',
        borderRadius: '3px',
        background: color,
        border: bordered ? '1px solid var(--glass-border)' : undefined,
      }}
    />
    {label}
  </span>
);

export default History;
