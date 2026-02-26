using CRM.Backend.Domain.Enums;
using CRM.Backend.Domain.Exceptions;

namespace CRM.Backend.Domain.ValueObjects;

public record Document
{
    public string Value { get; }
    public CustomerType Type { get; }

    public Document(string value, CustomerType type)
    {
        var cleaned = new string(value.Where(char.IsDigit).ToArray());
        if (type == CustomerType.PF)
        {
            if (!IsValidCpf(cleaned))
                throw new DomainException($"CPF inválido: {value}");
        }
        else
        {
            if (!IsValidCnpj(cleaned))
                throw new DomainException($"CNPJ inválido: {value}");
        }
        Value = cleaned;
        Type = type;
    }

    private static bool IsValidCpf(string cpf)
    {
        if (cpf.Length != 11) return false;
        if (cpf.Distinct().Count() == 1) return false;
        int[] mult1 = { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
        int[] mult2 = { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };
        var sum = 0;
        for (int i = 0; i < 9; i++) sum += (cpf[i] - '0') * mult1[i];
        var remainder = sum % 11;
        var digit1 = remainder < 2 ? 0 : 11 - remainder;
        sum = 0;
        for (int i = 0; i < 10; i++) sum += (cpf[i] - '0') * mult2[i];
        remainder = sum % 11;
        var digit2 = remainder < 2 ? 0 : 11 - remainder;
        return cpf[9] - '0' == digit1 && cpf[10] - '0' == digit2;
    }

    private static bool IsValidCnpj(string cnpj)
    {
        if (cnpj.Length != 14) return false;
        if (cnpj.Distinct().Count() == 1) return false;
        int[] mult1 = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        int[] mult2 = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        var sum = 0;
        for (int i = 0; i < 12; i++) sum += (cnpj[i] - '0') * mult1[i];
        var remainder = sum % 11;
        var digit1 = remainder < 2 ? 0 : 11 - remainder;
        sum = 0;
        for (int i = 0; i < 13; i++) sum += (cnpj[i] - '0') * mult2[i];
        remainder = sum % 11;
        var digit2 = remainder < 2 ? 0 : 11 - remainder;
        return cnpj[12] - '0' == digit1 && cnpj[13] - '0' == digit2;
    }

    public override string ToString() => Value;
}