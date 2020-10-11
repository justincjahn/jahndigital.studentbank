using HotChocolate.Types;
using jahndigital.studentbank.dal.Entities;

namespace jahndigital.studentbank.server.GraphQL.ObjectTypes
{
    public class ProductType : ObjectType<dal.Entities.Product>
    {
        protected override void Configure(IObjectTypeDescriptor<Product> descriptor)
        {
            // Only administrators can manage product groups
            descriptor.Field(f => f.ProductInstances)
                .Authorize(Constants.Privilege.ManageProducts.Name);
        }
    }
}
