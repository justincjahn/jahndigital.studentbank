using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using JahnDigital.StudentBank.Application.Common.Behaviors;
using JahnDigital.StudentBank.Application.Common.Interfaces;
using JahnDigital.StudentBank.Application.Common.Utils;

namespace JahnDigital.StudentBank.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddMediatR(Assembly.GetExecutingAssembly());
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
        services.AddSingleton<IInviteCodeGenerator, InviteCodeGenerator>();

        return services;
    }
}
