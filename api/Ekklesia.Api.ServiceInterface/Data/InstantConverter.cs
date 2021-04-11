using System;
using NodaTime;
using ServiceStack.OrmLite;

namespace Ekklesia.Api.Data
{
    public class InstantConverter : OrmLiteConverter
    {
        private readonly IOrmLiteConverter _dateTimeConverter;

        public InstantConverter(IOrmLiteConverter dateTimeConverter)
        {
            _dateTimeConverter = dateTimeConverter;
        }

        public override object ToDbValue(Type fieldType, object value)
        {
            Instant instantValue = (Instant) value;
            return instantValue.ToDateTimeUtc();
        }

        public override object FromDbValue(Type fieldType, object value)
        {
            DateTime datetimeValue = DateTime.SpecifyKind((DateTime)value, DateTimeKind.Utc);
            return Instant.FromDateTimeUtc(datetimeValue);
        }

        public override string ColumnDefinition => _dateTimeConverter.ColumnDefinition;
    }
}