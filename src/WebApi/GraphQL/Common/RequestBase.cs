using MediatR;

namespace JahnDigital.StudentBank.WebApi.GraphQL.Common;

public abstract class RequestBase
{
    protected readonly ISender _mediatr;

    protected RequestBase(ISender mediatr)
    {
        _mediatr = mediatr;
    }
}
