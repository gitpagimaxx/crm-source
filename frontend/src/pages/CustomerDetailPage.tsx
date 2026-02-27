import { useEffect, useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { getCustomer } from '../services/customers';
import type { CustomerDto } from '../types';

export function CustomerDetailPage() {
  const { id } = useParams<{ id: string }>();
  const [customer, setCustomer] = useState<CustomerDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    if (!id) return;
    getCustomer(id)
      .then(setCustomer)
      .catch((err) => setError(err instanceof Error ? err.message : 'Erro ao carregar cliente.'))
      .finally(() => setLoading(false));
  }, [id]);

  if (loading) return <div className="page"><p>Carregando…</p></div>;
  if (error) return <div className="page"><p className="error">{error}</p></div>;
  if (!customer) return null;

  return (
    <div className="page">
      <div className="page-header">
        <h2>{customer.name ?? customer.companyName}</h2>
        <Link to="/customers">← Voltar</Link>
      </div>

      <div className="card">
        <dl className="detail-list">
          <dt>ID</dt><dd>{customer.id}</dd>
          <dt>Tipo</dt><dd>{customer.customerType}</dd>
          <dt>Documento</dt><dd>{customer.document}</dd>
          <dt>E-mail</dt><dd>{customer.email}</dd>
          {customer.birthDate && <><dt>Nascimento / Fundação</dt><dd>{customer.birthDate}</dd></>}
          {customer.companyName && <><dt>Razão Social</dt><dd>{customer.companyName}</dd></>}
          {customer.stateRegistration && <><dt>Inscrição Estadual</dt><dd>{customer.stateRegistration}</dd></>}
          <dt>Status</dt><dd>{customer.status}</dd>
          <dt>Criado em</dt><dd>{new Date(customer.createdAt).toLocaleString('pt-BR')}</dd>
          {customer.updatedAt && <><dt>Atualizado em</dt><dd>{new Date(customer.updatedAt).toLocaleString('pt-BR')}</dd></>}
        </dl>

        {customer.address && (
          <>
            <h3>Endereço</h3>
            <dl className="detail-list">
              {customer.address.zipCode && <><dt>CEP</dt><dd>{customer.address.zipCode}</dd></>}
              {customer.address.street && <><dt>Rua</dt><dd>{customer.address.street}</dd></>}
              {customer.address.number && <><dt>Número</dt><dd>{customer.address.number}</dd></>}
              {customer.address.complement && <><dt>Complemento</dt><dd>{customer.address.complement}</dd></>}
              {customer.address.neighborhood && <><dt>Bairro</dt><dd>{customer.address.neighborhood}</dd></>}
              {customer.address.city && <><dt>Cidade</dt><dd>{customer.address.city}</dd></>}
              {customer.address.state && <><dt>Estado</dt><dd>{customer.address.state}</dd></>}
            </dl>
          </>
        )}
      </div>

      <div style={{ marginTop: '1rem' }}>
        <Link to={`/customers/${customer.id}/events`}>📋 Ver Auditoria</Link>
      </div>
    </div>
  );
}
