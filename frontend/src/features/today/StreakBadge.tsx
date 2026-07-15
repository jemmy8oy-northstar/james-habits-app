interface StreakBadgeProps {
  streak: number;
}

/** Small "🔥 N" badge; renders nothing at a zero streak to avoid noise. */
const StreakBadge = ({ streak }: StreakBadgeProps) => {
  if (streak <= 0) return null;
  return (
    <span
      title={`${streak}-day streak`}
      style={{
        display: 'inline-flex',
        alignItems: 'center',
        gap: '4px',
        fontSize: '0.8rem',
        fontWeight: 700,
        color: '#f59e0b',
        background: 'rgba(245, 158, 11, 0.12)',
        padding: '2px 8px',
        borderRadius: '999px',
        lineHeight: 1.4,
      }}
    >
      <span aria-hidden>🔥</span>
      {streak}
    </span>
  );
};

export default StreakBadge;
