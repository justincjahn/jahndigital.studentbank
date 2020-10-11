using jahndigital.studentbank.utils;

namespace jahndigital.studentbank.server.Models
{
    #nullable disable

    public class NewProductRequest
    {
        public string Name {get; set;}

        public string Description {get; set;}

        public Money Cost {get; set;}

        public bool IsLimitedQuantity {get; set;} = false;

        public int Quantity {get; set;} = -1;
    }
}
