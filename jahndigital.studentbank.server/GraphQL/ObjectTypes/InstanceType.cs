using HotChocolate.Types;
using jahndigital.studentbank.dal.Entities;

namespace jahndigital.studentbank.server.GraphQL.ObjectTypes
{
    public class InstanceType : ObjectType<dal.Entities.Instance>
    {
        protected override void Configure(IObjectTypeDescriptor<Instance> descriptor)
        {
            descriptor.Authorize(Constants.Privilege.ManageInstances.Name);
        }
    }
}
