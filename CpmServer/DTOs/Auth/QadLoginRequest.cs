namespace CpmServer.DTOs.Auth;

public class QadLoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
}
