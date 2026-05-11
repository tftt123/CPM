namespace CpmServer.DTOs.Auth;

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? Site { get; set; }
}

public class SwitchSiteRequest
{
    public string Site { get; set; } = string.Empty;
}