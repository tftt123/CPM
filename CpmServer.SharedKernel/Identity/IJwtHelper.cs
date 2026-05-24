namespace CpmServer.Common;

public interface IJwtHelper
{
    string GenerateToken(long userId, string username, List<string> roles, List<string> permissions, string? site = null, string? app = null);
}
