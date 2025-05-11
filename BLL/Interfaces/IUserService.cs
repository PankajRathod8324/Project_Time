using System.Threading.Tasks;
using Entities.Models;
using Entities.ViewModel;
using Microsoft.AspNetCore.Http;
using X.PagedList;

namespace  BLL.Interfaces;
public interface IUserService
{
    string AuthenticateUser(string email, string password);
    User GetUserByEmail(string email);
    ProfileVM GetUserProfileByEmail(string email);
    void UpdateUserProfile(ProfileVM model, IFormFile profilePhoto);
    User GetUserByToken(string token);
    Task<User> GenerateResetTokenAsync(string email);
    Task<bool> ResetPasswordAsync(string email, string token, string newPassword);
    IEnumerable<User> GetAllUsers();
    IEnumerable<Role> GetAllRoles();
    IEnumerable<Permission> GetAllRolesAndPermissions();
    IEnumerable<Country> GetAllCountries();
    IEnumerable<State> GetStatesByCountry(int countryId);
    IEnumerable<City> GetCitiesByState(int statesId);
    string GetRoleNameById(int roleId);
    string GetCountryById(int countryId);
    string GetStateById(int stateId);
    string GetCityById(int cityId);
    AddUserVM GetUserById(int id);
    public IPagedList<UserViewModel> GetFilteredUsers(UserFilterOptions filterOptions);
    void AddUser(AddUserVM model, IFormFile profileimage);
    bool UpdateUser(AddUserVM model, IFormFile profileimage);
    bool CheckPassword(User user, string password);
    void ChangePassword(User user, string newPassword);
    void DeleteUser(AddUserVM model);
}