import { useState, type FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { createCustomer } from '../services/customers';

const EMPTY_FORM = {
  customerType: 'PF',
  name: '',
  document: '',
  email: '',
  birthDate: '',
  companyName: '',
  stateRegistration: '',
  stateRegistrationIsExempt: false,
  zipCode: '',
  addressNumber: '',
  addressComplement: '',
};

export function NewCustomerPage() {
  const navigate = useNavigate();
  const [form, setForm] = useState(EMPTY_FORM);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  function handleChange(e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) {
    const { name, value, type } = e.target;
    const checked = type === 'checkbox' ? (e.target as HTMLInputElement).checked : undefined;
    setForm((prev) => ({ ...prev, [name]: type === 'checkbox' ? checked : value }));
  }

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setError('');
    setLoading(true);
    try {
      const payload = {
        customerType: form.customerType,
        name: form.name,
        document: form.document,
        email: form.email,
        birthDate: form.birthDate || undefined,
        companyName: form.companyName || undefined,
        stateRegistration: form.stateRegistrationIsExempt ? 'ISENTO' : (form.stateRegistration || undefined),
        zipCode: form.zipCode || undefined,
        addressNumber: form.addressNumber || undefined,
        addressComplement: form.addressComplement || undefined,
      };
      const res = await createCustomer(payload);
      navigate(`/customers/${res.id}`);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Erro ao cadastrar cliente.');
    } finally {
      setLoading(false);
    }
  }

  const isPJ = form.customerType === 'PJ';

  return (
    <div className="page">
      <h2>Novo Cliente</h2>
      {error && <p className="error">{error}</p>}

      <form className="card" onSubmit={handleSubmit}>
        <label>
          Tipo de Pessoa *
          <select name="customerType" value={form.customerType} onChange={handleChange} required>
            <option value="PF">Pessoa Física (PF)</option>
            <option value="PJ">Pessoa Jurídica (PJ)</option>
          </select>
        </label>

        <label>
          {isPJ ? 'Razão Social *' : 'Nome Completo *'}
          <input
            name="name"
            value={form.name}
            onChange={handleChange}
            required
          />
        </label>

        <label>
          {isPJ ? 'CNPJ *' : 'CPF *'}
          <input
            name="document"
            value={form.document}
            onChange={handleChange}
            placeholder={isPJ ? '00.000.000/0000-00' : '000.000.000-00'}
            required
          />
        </label>

        <label>
          E-mail *
          <input
            name="email"
            type="email"
            value={form.email}
            onChange={handleChange}
            required
          />
        </label>

        <label>
          {isPJ ? 'Data de Fundação' : 'Data de Nascimento'}
          <input
            name="birthDate"
            type="date"
            value={form.birthDate}
            onChange={handleChange}
          />
        </label>

        {isPJ && (
          <>
            <label>
              Nome Fantasia
              <input name="companyName" value={form.companyName} onChange={handleChange} />
            </label>

            <label className="checkbox-label">
              <input
                type="checkbox"
                name="stateRegistrationIsExempt"
                checked={form.stateRegistrationIsExempt}
                onChange={handleChange}
              />
              Inscrição Estadual Isenta
            </label>

            {!form.stateRegistrationIsExempt && (
              <label>
                Inscrição Estadual
                <input name="stateRegistration" value={form.stateRegistration} onChange={handleChange} />
              </label>
            )}
          </>
        )}

        <fieldset>
          <legend>Endereço</legend>
          <label>
            CEP
            <input name="zipCode" value={form.zipCode} onChange={handleChange} placeholder="00000-000" />
          </label>
          <label>
            Número
            <input name="addressNumber" value={form.addressNumber} onChange={handleChange} />
          </label>
          <label>
            Complemento
            <input name="addressComplement" value={form.addressComplement} onChange={handleChange} />
          </label>
        </fieldset>

        <div className="form-actions">
          <button type="button" className="btn-secondary" onClick={() => navigate('/customers')}>
            Cancelar
          </button>
          <button type="submit" disabled={loading}>
            {loading ? 'Salvando…' : 'Salvar'}
          </button>
        </div>
      </form>
    </div>
  );
}
