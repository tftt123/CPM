namespace CpmServer.Modules.SequenceRule.DTOs;

public class SequenceRuleDto
{
    public long? Id { get; set; }
    public string ModuleType { get; set; } = string.Empty;
    public string? ModuleName { get; set; }
    public string Prefix { get; set; } = string.Empty;
    public long CurrentSequence { get; set; }
    public int SequenceLength { get; set; } = 5;
    public int ResetRule { get; set; } = 0;
    public DateTime? LastResetDate { get; set; }
    public string? LastGeneratedNo { get; set; }
    public string? Site { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class SequenceRuleCreateDto
{
    public string ModuleType { get; set; } = string.Empty;
    public string? ModuleName { get; set; }
    public string Prefix { get; set; } = string.Empty;
    public int SequenceLength { get; set; } = 5;
    public int ResetRule { get; set; } = 0;
}

public class SequenceRuleUpdateDto
{
    public string ModuleType { get; set; } = string.Empty;
    public string? ModuleName { get; set; }
    public string Prefix { get; set; } = string.Empty;
    public int SequenceLength { get; set; } = 5;
    public int ResetRule { get; set; } = 0;
}

public class GenerateSequenceRequestDto
{
    public string ModuleType { get; set; } = string.Empty;
    public string? Site { get; set; }
}
