using JahnDigital.StudentBank.Domain.Exceptions;

namespace JahnDigital.StudentBank.Application.Common.Exceptions;

public class NotFoundException : BaseException
{
    public NotFoundException(string message) : base(message) { }
    
    public NotFoundException(string message, Exception innerException) : base(message, innerException) { }

    public NotFoundException(string name, object key) : base($"Entity \"{name}\" ({key}) was not found.") { }
}
