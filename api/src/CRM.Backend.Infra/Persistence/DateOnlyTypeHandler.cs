using Dapper;
using System.Data;

namespace CRM.Backend.Infra.Persistence;

public class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
{
    public override DateOnly Parse(object value)
    {
        return value switch
        {
            DateTime dateTime => DateOnly.FromDateTime(dateTime),
            DateOnly dateOnly => dateOnly,
            _ => throw new InvalidCastException($"Cannot convert {value.GetType()} to DateOnly")
        };
    }

    public override void SetValue(IDbDataParameter parameter, DateOnly value)
    {
        parameter.Value = value.ToDateTime(TimeOnly.MinValue);
        parameter.DbType = DbType.Date;
    }
}

public class NullableDateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly?>
{
    public override DateOnly? Parse(object value)
    {
        if (value == null || value is DBNull)
            return null;

        return value switch
        {
            DateTime dateTime => DateOnly.FromDateTime(dateTime),
            DateOnly dateOnly => dateOnly,
            _ => throw new InvalidCastException($"Cannot convert {value.GetType()} to DateOnly?")
        };
    }

    public override void SetValue(IDbDataParameter parameter, DateOnly? value)
    {
        if (value.HasValue)
        {
            parameter.Value = value.Value.ToDateTime(TimeOnly.MinValue);
            parameter.DbType = DbType.Date;
        }
        else
        {
            parameter.Value = DBNull.Value;
        }
    }
}
