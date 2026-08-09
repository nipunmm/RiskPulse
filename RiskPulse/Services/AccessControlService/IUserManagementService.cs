using RiskPulse.Models.DbModel.AccessControl;

namespace RiskPulse.Services.AccessControlService;

public interface IUserManagementService
{
    Task<List<User>> GetAllAsync();

    Task<List<Unit>> GetAllUnitsAsync();

    Task<List<Role>> GetAllRolesAsync();

    Task<User> CreateUserAsync(User user);

    Task<User> UpdateUserAsync(User user);
}
