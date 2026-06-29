namespace SunAdmin.Application.Abstractions;

public interface IRequestContext
{
    string? IpAddress { get; }
    string? UserAgent { get; }
}
