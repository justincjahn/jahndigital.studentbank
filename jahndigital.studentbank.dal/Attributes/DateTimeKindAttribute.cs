using System;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace jahndigital.studentbank.dal.Attributes
{
    /// <summary>
    /// Specify the <see cref="DateTimeKind"/> via an <see cref="Attribute"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class DateTimeKindAttribute : Attribute
    {
        public DateTimeKind Kind { get; }

        public DateTimeKindAttribute(DateTimeKind kind = DateTimeKind.Utc) => this.Kind = kind;
    }
}
