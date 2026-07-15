namespace FSH.Modules.Identity.Contracts.v1.Dtos;

public class GroupMemberDto
{
    public string UserId { get; set; } = default!;
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime AddedAt { get; set; }
}