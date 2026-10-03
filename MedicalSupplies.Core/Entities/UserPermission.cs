namespace MedicalSupplies.Core.Entities;

public class UserPermission
{
    public int UserPermissionId { get; set; }

    public string UserId { get; set; } = string.Empty;

    public string Permission { get; set; } = string.Empty;
}
