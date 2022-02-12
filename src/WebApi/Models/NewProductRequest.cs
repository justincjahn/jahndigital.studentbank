using JahnDigital.StudentBank.Domain.ValueObjects;

namespace JahnDigital.StudentBank.WebApi.Models
{
#nullable disable

    public class NewProductRequest
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public Money Cost { get; set; }

        public bool IsLimitedQuantity { get; set; } = false;

        public int Quantity { get; set; } = -1;
    }
}
