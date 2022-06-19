namespace JahnDigital.StudentBank.Application.Common.Interfaces;

public interface IInviteCodeGenerator
{
    public string NewCode(int length);
}
