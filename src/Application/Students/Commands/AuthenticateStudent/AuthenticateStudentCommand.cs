using JahnDigital.StudentBank.Application.Common.DTOs;
using JahnDigital.StudentBank.Application.Common.Exceptions;
using JahnDigital.StudentBank.Application.Common.Interfaces;
using JahnDigital.StudentBank.Domain.Entities;
using JahnDigital.StudentBank.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Role = JahnDigital.StudentBank.Domain.Enums.Role;

namespace JahnDigital.StudentBank.Application.Students.Commands.AuthenticateStudent;

public record AuthenticateStudentCommand
    (string Username, string Password, string IpAddress) : IRequest<AuthenticateResponse>;

public class AuthenticateStudentCommandHandler : IRequestHandler<AuthenticateStudentCommand, AuthenticateResponse>
{
    private readonly IAppDbContext _context;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly string _secret;
    private readonly int _tokenLifetime;

    public AuthenticateStudentCommandHandler(
        IAppDbContext context,
        IPasswordHasher hasher,
        IJwtTokenGenerator tokenGenerator,
        string secret,
        int tokenLifetime)
    {
        _context = context;
        _hasher = hasher;
        _tokenGenerator = tokenGenerator;
        _secret = secret;
        _tokenLifetime = tokenLifetime;
    }
    
    public async Task<AuthenticateResponse> Handle(AuthenticateStudentCommand request, CancellationToken cancellationToken)
    {
        List<Instance> activeInstances = await _context.Instances.Where(x => x.IsActive).ToListAsync(cancellationToken: cancellationToken);

        var student = await _context.Students
                .Include(x => x.Group)
                .Where(x => activeInstances.Select(y => y.Id).Contains(x.Group.InstanceId))
                .Where(x =>
                    (x.AccountNumber == request.Username.PadLeft(10, '0') || x.Email == request.Username.ToLower())
                    && x.DateDeleted == null
                    && x.DateRegistered != null
                ).SingleOrDefaultAsync(cancellationToken: cancellationToken)
            ?? throw new NotFoundException("Invalid username or password.");

        var valid = await _hasher.ValidateAsync(student.Password, request.Password);
        if (!valid) throw new NotFoundException("Invalid username or password.");
        
        student.DateLastLogin = DateTime.UtcNow;

        var tokenRequest = new JwtTokenRequest()
        {
            JwtSecret = _secret,
            Type = UserType.Student,
            Id = student.Id,
            Username = student.AccountNumber,
            Role = Role.Student.Name,
            Email = student.Email ?? "",
            FirstName = student.FirstName,
            LastName = student.LastName,
            Expires = _tokenLifetime
        };

        var jwtToken = _tokenGenerator.Generate(tokenRequest);
        RefreshToken refreshToken = _tokenGenerator.GenerateRefreshToken(request.IpAddress);
        student.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync(cancellationToken);
        return new AuthenticateResponse(student, jwtToken, refreshToken.Token);
    }
}
