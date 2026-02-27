import { useEffect, useState, useCallback } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { getCustomers } from '../services/customers';
import { useAuth } from '../hooks/useAuth';
import type { CustomerDto } from '../types';

export function CustomersPage() {
  const { logout } = useAuth();
  const navigate = useNavigate();
  const [allCustomers, setAllCustomers] = useState<CustomerDto[]>([]);
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(1);
  const pageSize = 20;
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const load = useCallback(() => {
    setLoading(true);
    setError('');
    getCustomers({ page, pageSize })
      .then(setAllCustomers)
      .catch((err) => setError(err instanceof Error ? err.message : 'Erro ao carregar clientes.'))
      .finally(() => setLoading(false));
  }, [page, pageSize]);

  // eslint-disable-next-line react-hooks/set-state-in-effect
  useEffect(() => { load(); }, [load]);

  const filtered = search
    ? allCustomers.filter(
        (c) =>
          c.name?.toLowerCase().includes(search.toLowerCase()) ||
          c.companyName?.toLowerCase().includes(search.toLowerCase()) ||
          c.email?.toLowerCase().includes(search.toLowerCase()) ||
          c.document?.includes(search),
      )
    : allCustomers;

  return (
    <div className="page">
      <header className="page-header">
        <h2>Clientes</h2>
        <div className="header-actions">
          <button onClick={() => navigate('/customers/new')}>+ Novo Cliente</button>
          <button className="btn-secondary" onClick={logout}>Sair</button>
        </div>
      </header>

      <input
        className="search-input"
        placeholder="Filtrar por nome, e-mail ou documento…"
        value={search}
        onChange={(e) => setSearch(e.target.value)}
      />

      {error && <p className="error">{error}</p>}
      {loading && <p>Carregando…</p>}

      {!loading && !error && (
        <>
          <table className="data-table">
            <thead>
              <tr>
                <th>Nome / Razão Social</th>
                <th>Tipo</th>
                <th>Documento</th>
                <th>E-mail</th>
                <th>Status</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {filtered.length === 0 && (
                <tr><td colSpan={6}>Nenhum cliente encontrado.</td></tr>
              )}
              {filtered.map((c) => (
                <tr key={c.id}>
                  <td>{c.name ?? c.companyName}</td>
                  <td>{c.customerType}</td>
                  <td>{c.document}</td>
                  <td>{c.email}</td>
                  <td>{c.status}</td>
                  <td>
                    <Link to={`/customers/${c.id}`}>Ver</Link>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>

          <div className="pagination">
            <button disabled={page <= 1} onClick={() => setPage((p) => p - 1)}>‹ Anterior</button>
            <span>Página {page}</span>
            <button
              disabled={allCustomers.length < pageSize}
              onClick={() => setPage((p) => p + 1)}
            >
              Próxima ›
            </button>
          </div>
        </>
      )}
    </div>
  );
}
