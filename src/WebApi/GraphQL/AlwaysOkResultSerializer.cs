using System.Linq;
using System.Net;
using HotChocolate;
using HotChocolate.AspNetCore.Serialization;
using HotChocolate.Execution;

namespace JahnDigital.StudentBank.WebApi.GraphQL;

/// <summary>
/// HotChocolate sends HTTP 500 requests when an exception is thrown, however; the GQL spec wants HTTP 200 even if
/// there are errors because they are output in the returned JSON.
/// </summary>
public class AlwaysOkResultSerializer : DefaultHttpResultSerializer
{
    public override HttpStatusCode GetStatusCode(IExecutionResult result)
    {
        var initialStatusCode = base.GetStatusCode(result);

        if (result is IQueryResult && initialStatusCode == HttpStatusCode.InternalServerError &&
            result.Errors?.Count > 0)
        {
            if (result.Errors.Any(x =>
                    x.Code == ErrorCodes.Authentication.NotAuthorized ||
                    x.Code == ErrorCodes.Authentication.NotAuthenticated))
            {
                return HttpStatusCode.Forbidden;
            }

            return HttpStatusCode.OK;
        }

        return initialStatusCode;
    }
}
