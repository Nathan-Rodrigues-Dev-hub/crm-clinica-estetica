export function Spinner({ full = false, label }: { full?: boolean; label?: string }) {
  return (
    <div className={full ? 'spinner-wrap spinner-full' : 'spinner-wrap'}>
      <div className="spinner" aria-hidden />
      {label && <span className="spinner-label">{label}</span>}
    </div>
  )
}
