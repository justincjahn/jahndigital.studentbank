using HotChocolate.Types;
using JahnDigital.StudentBank.Application.Common.DTOs;

namespace JahnDigital.StudentBank.WebApi.GraphQL.ObjectTypes
{
    /// <summary>
    /// Prevent the JWT refresh token from appearing in GraphQL responses.  The token is presented as an HTTP only cookie
    /// instead.
    /// </summary>
    public class AuthenticateResponseType : ObjectType<AuthenticateResponse>
    {
        protected override void Configure(IObjectTypeDescriptor<AuthenticateResponse> descriptor)
        {
            base.Configure(descriptor);
            descriptor.Field(f => f.RefreshToken).Ignore();
        }
    }
}
