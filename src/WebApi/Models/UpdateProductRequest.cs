using JahnDigital.StudentBank.Domain.ValueObjects;

namespace JahnDigital.StudentBank.WebApi.Models
{
    public class UpdateProductRequest
    {
        public long Id { get; set; }

        public string? Name { get; set; }

        public string? Description { get; set; }

        public Money? Cost { get; set; }

        public bool? IsLimitedQuantity { get; set; }

        public int? Quantity { get; set; }
    }
}
