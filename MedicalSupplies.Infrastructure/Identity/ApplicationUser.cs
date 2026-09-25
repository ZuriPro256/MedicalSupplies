using Microsoft.AspNetCore.Identity;

namespace MedicalSupplies.Infrastructure.Identity;

/// <summary>
/// Extends ASP.NET Core Identity's default user. Kept in Infrastructure
/// (not Core) so the domain layer stays framework-agnostic.
/// </summary>
public class ApplicationUser : IdentityUser
{
    public string? FullName { get; set; }
}
