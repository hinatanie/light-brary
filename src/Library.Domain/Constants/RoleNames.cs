namespace Library.Domain.Constants;

/// <summary>
/// Tên các vai trò hệ thống. Các role này được seed cố định trong migration (HasData).
/// </summary>
public static class RoleNames
{
    public const string Admin = "Admin";
    public const string Staff = "Staff";
    public const string Member = "Member";

    /// <summary>Guid cố định của các role để seed bằng HasData và tham chiếu ổn định giữa các môi trường.</summary>
    public static readonly Guid AdminId = new("7a0b1c10-1d2e-4f30-8a41-5b6c7d8e9f01");
    public static readonly Guid StaffId = new("7a0b1c10-1d2e-4f30-8a41-5b6c7d8e9f02");
    public static readonly Guid MemberId = new("7a0b1c10-1d2e-4f30-8a41-5b6c7d8e9f03");

    public static readonly IReadOnlyList<string> All = new[] { Admin, Staff, Member };
}
