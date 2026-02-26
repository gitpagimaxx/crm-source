namespace CRM.Backend.Application.Interfaces;

public interface IViaCepService
{
    Task<ViaCepResult?> GetAddressAsync(string zipCode, CancellationToken ct = default);
}

public record ViaCepResult(
    string Cep,
    string Logradouro,
    string Complemento,
    string Bairro,
    string Localidade,
    string Uf,
    bool Erro
);