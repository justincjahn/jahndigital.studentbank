using JahnDigital.StudentBank.Application.Common.Interfaces;
using JahnDigital.StudentBank.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JahnDigital.StudentBank.Application.Instances.Commands.NewInstance;

public record NewInstanceCommand(string Description, int InviteCodeLength) : IRequest<long>;

public class NewInstanceCommandHandler: IRequestHandler<NewInstanceCommand, long>
{
    private readonly IAppDbContext _context;
    private readonly IInviteCodeGenerator _inviteCodeGenerator;

    public NewInstanceCommandHandler(IAppDbContext context, IInviteCodeGenerator inviteCodeGenerator)
    {
        _context = context;
        _inviteCodeGenerator = inviteCodeGenerator;
    }

    public async Task<long> Handle(NewInstanceCommand request, CancellationToken cancellationToken)
    {
        bool hasInstance =
            await _context.Instances.AnyAsync(x => x.Description == request.Description, cancellationToken);

        if (hasInstance)
        {
            throw new InvalidOperationException($"An instance already exists with the name '{request.Description}'.");
        }

        // Generate a unique invite code for this new instance
        string code;

        do
        {
            code = _inviteCodeGenerator.NewCode(request.InviteCodeLength);
        } while (await _context.Instances.AnyAsync(x => x.InviteCode == code, cancellationToken));

        var instance = new Instance() { Description = request.Description, InviteCode = code };
        _context.Instances.Add(instance);
        await _context.SaveChangesAsync(cancellationToken);
        return instance.Id;
    }
}
