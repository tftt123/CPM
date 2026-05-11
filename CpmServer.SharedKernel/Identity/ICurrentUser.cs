namespace CpmServer.Services;

public interface ICurrentUser
{
    long? UserId { get; }
    string? Username { get; }
    string? Site { get; }
    bool IsAuthenticated { get; }
    bool IsInRole(string role);
}
