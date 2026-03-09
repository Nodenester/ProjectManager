namespace ProjectManager.Models;

public class WhitelistedUser
{
    public int Id { get; set; }
    public required string GitHubUsername { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    public string? AddedBy { get; set; }
    public bool IsActive { get; set; } = true;
}