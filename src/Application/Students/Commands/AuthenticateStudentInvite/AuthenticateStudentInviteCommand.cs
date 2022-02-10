using JahnDigital.StudentBank.Application.Common.DTOs;
using JahnDigital.StudentBank.Application.Common.Exceptions;
using JahnDigital.StudentBank.Application.Common.Interfaces;
using JahnDigital.StudentBank.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Role = JahnDigital.StudentBank.Domain.Enums.Role;

namespace JahnDigital.StudentBank.Application.Students.Commands.AuthenticateStudentInvite;

public record AuthenticateStudentInviteCommand(string InviteCode, string AccountNumber) : IRequest<string>;

public class AuthenticateStudentInviteCommandHandler : IRequestHandler<AuthenticateStudentInviteCommand, string>
{
    private readonly IAppDbContext _context;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly string _secret;

    public AuthenticateStudentInviteCommandHandler(IAppDbContext context, IJwtTokenGenerator tokenGenerator, string secret)
    {
        _context = context;
        _tokenGenerator = tokenGenerator;
        _secret = secret;
    }

    public async Task<string> Handle(AuthenticateStudentInviteCommand request, CancellationToken cancellationToken)
    {
        /*
            SELECT * FROM INSTANCES
            WHERE
                INSTANCES.INVITECODE = INVITECODE
                AND INSTANCES.DATEDELETED = NULL
        */
        var instance = await _context.Instances
                .Where(x => x.IsActive && x.InviteCode.ToUpper() == request.InviteCode.ToUpper())
                .SingleOrDefaultAsync(cancellationToken: cancellationToken)
            ?? throw new NotFoundException(nameof(request.InviteCode),
                "No instances available with the provided invite code.");
        
        /*
            SELECT * FROM STUDENTS
                LEFT JOIN GROUP ON STUDENT.GROUPID = GROUP.ID
            WHERE
                GROUP.INSTANCEID == 0
                AND STUDENTS.ACCOUNTNUMBER = 0000000000;
                AND STUDENTS.DATEREGISTERED = NULL
                AND STUDENTS.DATEDELETED = NULL
        */
        var student = await _context.Students
                .Include(x => x.Group)
                .Where(x => x.Group.InstanceId == instance.Id)
                .Where(x =>
                    x.DateDeleted == null
                    && x.DateRegistered == null
                    && x.AccountNumber == request.AccountNumber.PadLeft(10, '0'))
                .FirstOrDefaultAsync(cancellationToken: cancellationToken)
            ?? throw new NotFoundException(nameof(request.AccountNumber),
                "No instances available with the provided invite code.");

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
            Expires = 5,
            Preauthorization = true
        };

        return _tokenGenerator.Generate(tokenRequest);
    }
}
