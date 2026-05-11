namespace CpmServer.DTOs.Auth;

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? RealName { get; set; }
    public string? Site { get; set; }
    public List<string> Roles { get; set; } = new();
}