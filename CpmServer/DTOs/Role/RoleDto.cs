namespace CpmServer.DTOs.Role;

public class RoleDto
{
    public long Id { get; set; }
    public string RoleCode { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public string? Site { get; set; }
    public List<string> Permissions { get; set; } = new();
}

public class RoleCreateDto
{
    public string RoleCode { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public string? Site { get; set; }
}

public class RoleUpdateDto
{
    public string RoleName { get; set; } = string.Empty;
    public string? Site { get; set; }
}
