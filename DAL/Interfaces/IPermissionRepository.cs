using System.Threading.Tasks;
using Entities.Models;
using Entities.ViewModel;

namespace  DAL.Interfaces;
public interface IPermissionRepository
{
    Permission GetPermissionByName(string name);
    List<PermissionVM> GetAllPermissionsByroleId(int roleId);
    public int GetRoleIdByRoleName(string rolename);
    bool UpdateRolePermissions(List<PermissionVM> permissions);

}