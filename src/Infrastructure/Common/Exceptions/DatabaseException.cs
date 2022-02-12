using JahnDigital.StudentBank.Domain.Exceptions;

namespace JahnDigital.StudentBank.Infrastructure.Common.Exceptions;

public class DatabaseException : BaseException
{
    public DatabaseException(string message) : base(message) { }
}
