using DAL.Interfaces;
using Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repository;

public class UserRepository : IUserRepository
{
    private readonly PizzaShopContext _context;

    public UserRepository(PizzaShopContext context)
    {
        _context = context;
    }
    public User GetUserByEmailAndPassword(string email, string password)
    {
        return _context.Users.FirstOrDefault(u => u.Email == email && u.Password == password);
    }
    public User GetUserByEmail(string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            Console.WriteLine("Email is null or empty.");
            return null;
        }

        Console.WriteLine("Fetching user with email: " + email);

        if (_context == null)
        {
            Console.WriteLine("Database context is null.");
            throw new NullReferenceException("Database context is not initialized.");
        }

        var user = _context.Users.FirstOrDefault(u => u.Email == email);
        Console.WriteLine(user);

        Console.WriteLine(user != null ? "User found." : "User not found.");
        return user;
    }
    public User GetUserByToken(string token)
    {
        var user = _context.Users.SingleOrDefault(u => u.ResetToken == token);
        // Console.WriteLine("GetUserBy Token" + user.Email);
        return user;
    }
    public string GetUserRole(string email)
    {
        var userRole = (from user in _context.Users
                        join role in _context.Roles on user.RoleId equals role.RoleId
                        where user.Email == email
                        select role.RoleName).FirstOrDefault();
        return userRole;
    }
    public IEnumerable<State> GetStatesByCountry(int countryId)
    {
        var states = _context.States.Where(s => s.CountryId == countryId).ToList();
        return states;
    }
    public IEnumerable<City> GetCitiesByState(int statesId)
    {
        var cities = _context.Cities.Where(s => s.StateId == statesId).ToList();
        return cities;
    }
    public Role GetRoleById(int roleId)
    {
        return _context.Roles.FirstOrDefault(r => r.RoleId == roleId);
    }
    public State GetStateById(int stateId)
    {
        return _context.States.FirstOrDefault(r => r.StateId == stateId);
    }
    public City GetCityById(int cityId)
    {
        return _context.Cities.FirstOrDefault(r => r.CityId == cityId);
    }
    public Country GetCountryById(int countryId)
    {
        return _context.Countries.FirstOrDefault(r => r.CountryId == countryId);
    }
    public IEnumerable<User> GetAllUsers()
    {
        return _context.Users.Include(u => u.Role).Where(u => (bool)!u.IsDeleted).ToList();
    }
    public IEnumerable<Permission> GetAllRolesAndPermissions()
    {
        return _context.Permissions.ToList();
    }
    public IEnumerable<Role> GetAllRoles()
    {
        return _context.Roles.ToList();
    }
    public IEnumerable<Country> GetAllCountries()
    {
        return _context.Countries.ToList();
    }
    public User GetUserById(int id)
    {
        return _context.Users.Find(id);
    }
    public void AddUser(User user)
    {
        _context.Users.Add(user);
        Save();
    }
    public void DeleteUser(User user)
    {
        Console.WriteLine(user);
        if (user != null)
        {
            user.IsActive = false;
            user.IsDeleted = true;
            Save();
        }
    }
    public void UpdateUser(User user)
    {
        var existingUser = _context.Users.FirstOrDefault(u => u.Email == user.Email);

        Console.WriteLine(existingUser);
        Console.WriteLine("------");
        if (existingUser != null)
        {
            if (!string.IsNullOrEmpty(user.Username) && existingUser.Username != user.Username)
            {
                existingUser.Username = user.Username;
            }

            if (!string.IsNullOrEmpty(user.FirstName) && existingUser.FirstName != user.FirstName)
            {
                existingUser.FirstName = user.FirstName;
            }

            if (!string.IsNullOrEmpty(user.LastName) && existingUser.LastName != user.LastName)
            {
                existingUser.LastName = user.LastName;
            }

            if (!string.IsNullOrEmpty(user.Email) && existingUser.Email != user.Email)
            {
                existingUser.Email = user.Email;
            }

            if (!string.IsNullOrEmpty(user.Password) && existingUser.Password != user.Password)
            {
                existingUser.Password = user.Password;
            }

            if (!string.IsNullOrEmpty(user.Phone) && existingUser.Phone != user.Phone)
            {
                existingUser.Phone = user.Phone;
            }

            if (!string.IsNullOrEmpty(user.Address) && existingUser.Address != user.Address)
            {
                existingUser.Address = user.Address;
            }

            if (user.IsActive != existingUser.IsActive)
            {
                existingUser.IsActive = user.IsActive;
            }

            if (user.RoleId != existingUser.RoleId)
            {
                existingUser.RoleId = user.RoleId;
            }

            if (user.CountryId != existingUser.CountryId)
            {
                existingUser.CountryId = user.CountryId;
            }

            if (user.StateId != existingUser.StateId)
            {
                existingUser.StateId = user.StateId;
            }

            if (user.CityId != existingUser.CityId)
            {
                existingUser.CityId = user.CityId;
            }

            if (!string.IsNullOrEmpty(user.ProfilePhoto) && existingUser.ProfilePhoto != user.ProfilePhoto)
            {
                existingUser.ProfilePhoto = user.ProfilePhoto;
            }
            if (user.LastLogin == null)
            {
                existingUser.LastLogin = user.LastLogin;
            }




            Console.WriteLine(existingUser);
            Console.WriteLine("------");

            _context.Users.Update(existingUser);
            _context.SaveChanges();
        }
    }
    public void Save()
    {
        _context.SaveChanges();
    }

}