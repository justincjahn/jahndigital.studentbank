using HotChocolate.Types;
using JahnDigital.StudentBank.Domain.Entities;
using Privilege = JahnDigital.StudentBank.Domain.Enums.Privilege;

namespace JahnDigital.StudentBank.WebApi.GraphQL.ObjectTypes
{
    public class ProductType : ObjectType<Product>
    {
        protected override void Configure(IObjectTypeDescriptor<Product> descriptor)
        {
            // Only administrators can manage product groups
            descriptor.Field(f => f.ProductInstances)
                .Authorize(Privilege.ManageProducts.Name);
        }
    }
}
