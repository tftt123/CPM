namespace CpmServer.DTOs.User;

public class UserDto
{
    public long? Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? RealName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? AvatarUrl { get; set; }
    public bool IsActive { get; set; }
    public string? Site { get; set; }
    public List<long> RoleIds { get; set; } = new();
    public List<string> Roles { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class UserCreateDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? RealName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Site { get; set; }
    public bool IsActive { get; set; } = true;
    public List<long> RoleIds { get; set; } = new();
}

public class UserUpdateDto
{
    public string? RealName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Site { get; set; }
    public bool IsActive { get; set; } = true;
    public List<long> RoleIds { get; set; } = new();
}

public class ProfileUpdateDto
{
    public string? RealName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? AvatarUrl { get; set; }
}

public class ChangePasswordDto
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
