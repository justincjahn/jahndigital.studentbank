using System;
using System.Reflection;
using jahndigital.studentbank.dal.Attributes;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace jahndigital.studentbank.dal.Annotations
{
    /// <summary>
    ///     EF Core extension methods that specifies the DateTimeKind for the property.
    /// </summary>
    public static class DateTimeKindAnnotation
    {
        public static readonly string IsDateTimeKindAnnotation = "DateTimeKind";

        /// <summary>
        ///     Specify the <see cref="DateTimeKind" /> for this property.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="kind"></param>
        /// <typeparam name="TProperty"></typeparam>
        /// <returns></returns>
        public static PropertyBuilder<TProperty> IsDateTimeKind<TProperty>(
            this PropertyBuilder<TProperty> builder,
            DateTimeKind kind
        )
        {
            return builder.HasAnnotation(IsDateTimeKindAnnotation, kind);
        }

        /// <summary>
        ///     Get the <see cref="DateTimeKind" /> for this property via Annotation or Attribute.
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public static DateTimeKind DateTimeKind(this IMutableProperty property)
        {
            var attr = property.PropertyInfo.GetCustomAttribute<DateTimeKindAttribute>();

            if (attr != null) {
                return attr.Kind;
            }

            return (DateTimeKind?) property.FindAnnotation(IsDateTimeKindAnnotation)?.Value ??
                System.DateTimeKind.Unspecified;
        }
    }
}