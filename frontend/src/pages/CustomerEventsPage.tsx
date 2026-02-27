import { useEffect, useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { getCustomerEvents } from '../services/customers';
import type { EventDto } from '../types';

export function CustomerEventsPage() {
  const { id } = useParams<{ id: string }>();
  const [events, setEvents] = useState<EventDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    if (!id) return;
    getCustomerEvents(id)
      .then(setEvents)
      .catch((err) => setError(err instanceof Error ? err.message : 'Erro ao carregar eventos.'))
      .finally(() => setLoading(false));
  }, [id]);

  if (loading) return <div className="page"><p>Carregando…</p></div>;
  if (error) return <div className="page"><p className="error">{error}</p></div>;

  return (
    <div className="page">
      <div className="page-header">
        <h2>Auditoria de Eventos</h2>
        <Link to={`/customers/${id}`}>← Voltar ao Cliente</Link>
      </div>

      {events.length === 0 && <p>Nenhum evento registrado.</p>}

      <div className="timeline">
        {events.map((ev) => (
          <div key={ev.id} className="timeline-item">
            <div className="timeline-dot" />
            <div className="timeline-content">
              <p className="timeline-type">{ev.eventType}</p>
              <p className="timeline-meta">
                {new Date(ev.createdAt).toLocaleString('pt-BR')}
                {ev.actorName && ` · ${ev.actorName}`}
                {ev.actorEmail && ` (${ev.actorEmail})`}
              </p>
              <details>
                <summary>Payload</summary>
                <pre className="timeline-payload">
                  {JSON.stringify(JSON.parse(ev.eventData), null, 2)}
                </pre>
              </details>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
