using System;

namespace jahndigital.studentbank.services.Exceptions
{
    public class BaseException : Exception
    {
        public BaseException(string message) : base(message) { }
    }
}
