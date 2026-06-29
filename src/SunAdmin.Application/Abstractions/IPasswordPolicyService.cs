namespace SunAdmin.Application.Abstractions;

public interface IPasswordPolicyService
{
    void Validate(string password);
}
