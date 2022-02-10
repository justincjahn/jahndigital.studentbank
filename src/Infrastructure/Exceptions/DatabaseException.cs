using JahnDigital.StudentBank.Domain.Exceptions;

namespace JahnDigital.StudentBank.Infrastructure.Exceptions;

public class DatabaseException : BaseException
{
    public DatabaseException(string message) : base(message) { }
}
