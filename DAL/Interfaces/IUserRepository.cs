using System.Threading.Tasks;
using Entities.Models;

namespace  DAL.Interfaces;
public interface IUserRepository
{
    User GetUserByEmailAndPassword(string email, string password);
    User GetUserByEmail(string email);
    User GetUserByToken(string token);
    string GetUserRole(string email);
    void UpdateUser(User user);
    Role GetRoleById(int roleId);
    State GetStateById(int stateId);
    City GetCityById(int cityId);
    Country GetCountryById(int countryId);
    IEnumerable<User> GetAllUsers();
    IEnumerable<Role> GetAllRoles();
    IEnumerable<Permission> GetAllRolesAndPermissions();
    IEnumerable<State> GetStatesByCountry(int countryId);
    IEnumerable<City> GetCitiesByState(int statesId);
    IEnumerable<Country> GetAllCountries();
    User GetUserById(int id);
    void AddUser(User user);
    void DeleteUser(User user);
    void Save();
}