using CRM.Backend.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CRM.Backend.Infra.ExternalServices;

public class ViaCepService : IViaCepService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ViaCepService> _logger;

    public ViaCepService(HttpClient httpClient, ILogger<ViaCepService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ViaCepResult?> GetAddressAsync(string zipCode, CancellationToken ct = default)
    {
        try
        {
            var cleaned = new string(zipCode.Where(char.IsDigit).ToArray());
            var response = await _httpClient.GetStringAsync($"https://viacep.com.br/ws/{cleaned}/json/", ct);
            var result = JsonConvert.DeserializeObject<ViaCepApiResponse>(response);
            if (result is null) return null;
            return new ViaCepResult(
                result.Cep ?? "",
                result.Logradouro ?? "",
                result.Complemento ?? "",
                result.Bairro ?? "",
                result.Localidade ?? "",
                result.Uf ?? "",
                result.Erro);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch address from ViaCEP for zip code {ZipCode}", zipCode);
            return null;
        }
    }

    private record ViaCepApiResponse(string? Cep, string? Logradouro, string? Complemento, string? Bairro, string? Localidade, string? Uf, bool Erro);
}