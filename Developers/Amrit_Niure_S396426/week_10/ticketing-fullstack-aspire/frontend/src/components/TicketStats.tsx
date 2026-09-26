import { PRIORITIES, STATUSES, type TicketStats as Stats } from '../api/tickets'

interface TicketStatsProps {
  stats: Stats
  onSelectStatus: (status: number) => void
}

function Tile({ label, value, onClick }: { label: string; value: number; onClick?: () => void }) {
  const content = (
    <>
      <span className="text-2xl font-semibold tabular-nums">{value}</span>
      <span className="text-xs text-muted-foreground">{label}</span>
    </>
  )
  const className = 'flex flex-col items-start gap-0.5 rounded-lg border bg-card p-3 text-card-foreground'

  return onClick ? (
    <button type="button" onClick={onClick} className={`${className} text-left transition-colors hover:bg-muted/50`}>
      {content}
    </button>
  ) : (
    <div className={className}>{content}</div>
  )
}

export function TicketStats({ stats, onSelectStatus }: TicketStatsProps) {
  const highestPriority = PRIORITIES.length - 1
  const topPriorityCount = stats.byPriority[highestPriority]

  return (
    <section className="flex flex-col gap-2" aria-label="Ticket statistics">
      <div className="grid grid-cols-2 gap-2 sm:grid-cols-4">
        <Tile label="Total tickets" value={stats.total} />
        <Tile label="Raised by me" value={stats.mine} />
        <Tile label="Unassigned" value={stats.unassigned} />
        <Tile label={`${PRIORITIES[highestPriority]} priority`} value={topPriorityCount} />
      </div>
      <div className="grid grid-cols-2 gap-2 sm:grid-cols-4">
        {STATUSES.map((label, value) => (
          <Tile key={label} label={label} value={stats.byStatus[value] ?? 0} onClick={() => onSelectStatus(value)} />
        ))}
      </div>
    </section>
  )
}
