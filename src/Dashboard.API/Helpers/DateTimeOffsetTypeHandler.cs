using Dapper;

namespace Dashboard.API.Helpers;

public class DateTimeOffsetTypeHandler : SqlMapper.TypeHandler<DateTimeOffset>
{
    public override DateTimeOffset Parse(object value) =>
        value switch
        {
            DateTimeOffset dateTimeOffset => dateTimeOffset,
            DateTime dateTime => new DateTimeOffset(DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)),
            _ => throw new InvalidCastException($"Cannot convert {value.GetType()} to {nameof(DateTimeOffset)}.")
        };

    public override void SetValue(System.Data.IDbDataParameter parameter, DateTimeOffset value) =>
        parameter.Value = value;
}
