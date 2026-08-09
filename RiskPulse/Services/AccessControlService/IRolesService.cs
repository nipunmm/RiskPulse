using RiskPulse.Models.DbModel.AccessControl;

namespace RiskPulse.Services.AccessControlService;

public interface IRolesService
{
    Task<List<Role>> GetAllRolesAsync();

    Task<List<Permission>> GetAllPermissionsAsync();

    Task<Role> CreateRoleAsync(string roleDesc, List<int> permissionIds);

    Task<Role> UpdateRoleAsync(int roleId, string roleDesc, List<int> permissionIds);
}
