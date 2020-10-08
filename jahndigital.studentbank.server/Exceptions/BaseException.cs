using System;

namespace jahndigital.studentbank.server.Exceptions
{
    public class BaseException : Exception {
        public BaseException(string message) : base(message) {}
    }
}
