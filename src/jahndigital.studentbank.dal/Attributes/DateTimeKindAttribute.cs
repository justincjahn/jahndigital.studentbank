using System;

namespace jahndigital.studentbank.dal.Attributes
{
    /// <summary>
    ///     Specify the <see cref="DateTimeKind" /> via an <see cref="Attribute" />.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class DateTimeKindAttribute : Attribute
    {
        public DateTimeKindAttribute(DateTimeKind kind = DateTimeKind.Utc)
        {
            Kind = kind;
        }

        public DateTimeKind Kind { get; }
    }
}
