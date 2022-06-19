﻿using FluentValidation.Results;

namespace JahnDigital.StudentBank.Application.Common.Exceptions;

public class ValidationException : BaseException
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException() : base("One or more fields are invalid.")
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(IEnumerable<ValidationFailure> failures) : this()
    {
        Errors = failures
            .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
            .ToDictionary(g => g.Key, g => g.ToArray());
    }
}
