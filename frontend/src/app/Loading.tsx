export default function Loading({ text = 'Cargando…' }: { text?: string }) {
  return (
    <div className="loading">
      <div className="loading-dots">
        <span />
        <span />
        <span />
      </div>
      <span>{text}</span>
    </div>
  );
}
