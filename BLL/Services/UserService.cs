using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using DAL.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Entities.ViewModel;
using X.PagedList.Extensions;
using X.PagedList;
using BLL.Interfaces;

namespace BLL.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;

    private readonly IWebHostEnvironment _webHostEnvironment;


    private readonly String _imageFolderPath = "wwwroot/images/";

    public UserService(IUserRepository userRepository, IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
    {
        _userRepository = userRepository;
        _configuration = configuration;
        _webHostEnvironment = webHostEnvironment;
    }

    public string AuthenticateUser(string email, string password)
    {
        // && BCrypt.Net.BCrypt.Verify(password, user.Password)
        var user = _userRepository.GetUserByEmail(email);
        var pss = BCrypt.Net.BCrypt.Verify(password, user.Password);

        if (user != null && user.IsDeleted == false && user.IsActive == true && BCrypt.Net.BCrypt.Verify(password, user.Password))
        {
            var userRole = _userRepository.GetUserRole(email);
            if (user.LastLogin == null)
            {
                return "Hii";
            }
            else
            {
                return GenerateJwtToken(email, userRole);
            }

        }
        if (user.IsActive == false && BCrypt.Net.BCrypt.Verify(password, user.Password))
        {
            return "User is inactive.";
        }

        return null;

    }
    private string GenerateJwtToken(string email, string role)
    {
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);
        Console.WriteLine("Role Name : " + role);
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, role)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:ExpiryMinutes"])),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"]
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenValue = tokenHandler.WriteToken(token);

        Console.WriteLine("Generated JWT Token: " + tokenValue);

        var jwtToken = tokenHandler.ReadToken(tokenValue) as JwtSecurityToken;
        var roleName = jwtToken?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

        if (jwtToken != null)
        {
            var claims1 = jwtToken.Claims;

            var userId = claims1.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var userEmail = claims1.FirstOrDefault(c => c.Type == "email")?.Value;

            Console.WriteLine($"User ID: {userId}");
            Console.WriteLine($"User Email: {userEmail}");
        }
        else
        {
            Console.WriteLine("Invalid JWT token.");
        }
        Console.WriteLine("Extracted Role from Token: " + roleName);
        return tokenValue;
    }
    public User GetUserByEmail(string email)
    {
        return _userRepository.GetUserByEmail(email);
    }
    public ProfileVM GetUserProfileByEmail(string email)
    {
        var user = _userRepository.GetUserByEmail(email);
        if (user == null) return null;

        return new ProfileVM
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            UserName = user.Username,
            Email = user.Email,
            PhoneNumber = user.Phone,
            CountryId = user.CountryId,
            StateId = user.StateId,
            CityId = user.CityId,
            Country = user.CountryId.HasValue ? GetCountryById(user.CountryId.Value) : "No Country",
            State = user.StateId.HasValue ? GetStateById(user.StateId.Value) : "No State",
            City = user.CityId.HasValue ? GetCityById(user.CityId.Value) : "No City",
            Address = user.Address ?? string.Empty,
            ZipCode = user.Zipcode,
            ProfilePicture = user.ProfilePhoto
        };
    }
    public void UpdateUserProfile(ProfileVM model, IFormFile? profilePhoto)
    {
        var user = _userRepository.GetUserByEmail(model.Email);
        if (user == null) return;

        user.FirstName = model.FirstName;
        user.LastName = model.LastName;
        user.Username = model.UserName;
        user.Phone = model.PhoneNumber;
        user.CountryId = model.CountryId;
        user.StateId = model.StateId;
        user.CityId = model.CityId;
        user.Address = model.Address;
        user.Zipcode = model.ZipCode;
        if (profilePhoto != null)
        {
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
            var uniqueFileName = Guid.NewGuid().ToString() + "_" + profilePhoto.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                profilePhoto.CopyTo(fileStream);
            }
            user.ProfilePhoto = "/images/" + uniqueFileName;
        }

        _userRepository.UpdateUser(user);
    }
    public string GetRoleNameById(int roleId)
    {
        var role = _userRepository.GetRoleById(roleId);
        return role.RoleName;
    }
    public string GetCountryById(int countryId)
    {
        var country = _userRepository.GetCountryById(countryId);
        return country.CountryName;
    }
    public string GetStateById(int stateId)
    {
        var state = _userRepository.GetStateById(stateId);
        return state.StateName;
    }
    public string GetCityById(int cityId)
    {
        var city = _userRepository.GetCityById(cityId);
        return city.CityName;
    }
    public IEnumerable<State> GetStatesByCountry(int countryId)
    {
        return _userRepository.GetStatesByCountry(countryId);
    }
    public IEnumerable<City> GetCitiesByState(int statesId)
    {
        return _userRepository.GetCitiesByState(statesId);
    }
    public User GetUserByToken(string token)
    {
        return _userRepository.GetUserByToken(token);
    }
    public async Task<User> GenerateResetTokenAsync(string email)
    {
        var user = _userRepository.GetUserByEmail(email);
        Console.WriteLine(user.ResetToken);
        Console.WriteLine("In Service " + user.Email);
        if (user != null)
        {
            user.ResetToken = GenerateResetToken();
            user.ResetTokenExpirytime = DateTime.UtcNow.AddHours(1);
            Console.WriteLine("New token generated: " + user.ResetToken);
            Console.WriteLine("Token expiry time (UTC): " + user.ResetTokenExpirytime);
            _userRepository.UpdateUser(user);
            Console.WriteLine(user.Email);
            Console.WriteLine(user.ResetToken);
            Console.WriteLine("User updated with new token.");
        }
        return user;
    }
    public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword)
    {
        if (string.IsNullOrEmpty(email))
        {
            Console.WriteLine("In service");
            Console.WriteLine("Email is null or empty.");
            return false;
        }

        if (_userRepository == null)
        {
            Console.WriteLine("User repository is null.");
            throw new NullReferenceException("User repository is not initialized.");
        }

        var user = _userRepository.GetUserByEmail(email);
        if (user == null)
        {
            Console.WriteLine("User not found.");
            return false;
        }

        Console.WriteLine("Token from user: " + user.ResetToken);
        Console.WriteLine(token);
        Console.WriteLine("Token expiry time (UTC): " + user.ResetTokenExpirytime);
        Console.WriteLine(DateTime.UtcNow);

        if (user.ResetToken == token && user.ResetTokenExpirytime > DateTime.UtcNow)
        {
            Console.WriteLine(newPassword);
            user.Password = HashPassword(newPassword);
            Console.WriteLine(user.Password);
            user.ResetToken = null;
            user.ResetTokenExpirytime = null;
            _userRepository.UpdateUser(user);
            Console.WriteLine("Password reset successful. User updated.");
            return true;
        }
        else
        {
            Console.WriteLine("Invalid token or token has expired.");
        }
        return false;
    }
    public IEnumerable<User> GetAllUsers()
    {
        return _userRepository.GetAllUsers();
    }
    public IPagedList<UserViewModel> GetFilteredUsers(UserFilterOptions filterOptions)
    {
        var users = _userRepository.GetAllUsers().AsQueryable();

        // if (!string.IsNullOrEmpty(filterOptions.FilterBy))
        // {
        //     users = filterOptions.FilterBy switch
        //     {
        //         "Active" => users.Where(u => u.IsActive),
        //         "Inactive" => users.Where(u => !u.IsActive),
        //         _ => users
        //     };
        // }

        if (!string.IsNullOrEmpty(filterOptions.Search))
        {
            string searchLower = filterOptions.Search?.Trim().ToLower();
            users = users.Where(u => u.FirstName.Trim().ToLower().Contains(searchLower));

        }

        users = filterOptions.SortBy?.ToLower() switch
        {
            "name" => filterOptions.IsAsc == true ? users.OrderBy(u => u.FirstName) : users.OrderByDescending(u => u.FirstName),
            "role" => filterOptions.IsAsc == true ? users.OrderBy(u => u.Role.RoleName) : users.OrderByDescending(u => u.Role.RoleName),
            _ => users.OrderBy(u => u.UserId)
        };

        // Preserve PageSize when paginating or sorting
        int pageSize = filterOptions.PageSize > 0 ? filterOptions.PageSize : 10; // Default to 10

        var userViewModels = users.Select(u => new UserViewModel
        {
            UserId = u.UserId,
            FirstName = u.FirstName,
            LastName = u.LastName,
            Email = u.Email,
            Phone = u.Phone,
            Address = u.Address,
            Password = u.Password,
            IsActive = u.IsActive,
            ProfilePhoto = u.ProfilePhoto,
            RoleName = u.RoleId.HasValue ? GetRoleNameById(u.RoleId.Value) : "No Role"
        }).ToPagedList(filterOptions.Page ?? 1, pageSize);

        return userViewModels;
    }

    public IEnumerable<Permission> GetAllRolesAndPermissions()
    {
        return _userRepository.GetAllRolesAndPermissions();
    }
    public IEnumerable<Role> GetAllRoles()
    {
        return _userRepository.GetAllRoles();
    }
    public IEnumerable<Country> GetAllCountries()
    {
        return _userRepository.GetAllCountries();
    }
    public AddUserVM GetUserById(int id)
    {
        var user = _userRepository.GetUserById(id);
        if (user == null)
        {
            return null;
        }

        return new AddUserVM
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            UserName = user.Username,
            RoleId = user.RoleId,
            Email = user.Email,
            Password = user.Password,
            PhoneNumber = user.Phone,
            CountryId = user.CountryId,
            StateId = user.StateId,
            CityId = user.CityId,
            Address = user.Address,
            ZipCode = user.Zipcode,
            IsActive = user.IsActive,
            ProfilePicture = user.ProfilePhoto
        };
    }
    public void AddUser(AddUserVM model, IFormFile profileimage)
    {
        var user = new User
        {
            FirstName = model.FirstName,
            LastName = model.LastName,
            Username = model.UserName,
            Email = model.Email,
            Password = HashPassword(model.Password),
            RoleId = model.RoleId,
            Phone = model.PhoneNumber,
            CountryId = model.CountryId,
            StateId = model.StateId,
            CityId = model.CityId,
            Address = model.Address,
            Zipcode = model.ZipCode,
            ProfilePhoto = model.ProfilePicture
        };

        if (profileimage != null)
        {
            var uniqueFileName = Guid.NewGuid().ToString() + "_" + profileimage.FileName;
            var filePath = Path.Combine(_imageFolderPath, uniqueFileName);

            Directory.CreateDirectory(_imageFolderPath); // Ensure directory exists

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                profileimage.CopyToAsync(stream);
            }

            user.ProfilePhoto = "/images/" + uniqueFileName;
        }
        _userRepository.AddUser(user);
    }
    public bool UpdateUser(AddUserVM model, IFormFile profileimage)
    {
        var user = GetUserByEmail(model.Email);
        if (user == null)
        {
            return false;
        }

        user.FirstName = model.FirstName ?? user.FirstName;
        user.LastName = model.LastName ?? user.FirstName;
        user.Username = model.UserName ?? user.Username;
        user.RoleId = model.RoleId ?? user.RoleId;
        user.Email = model.Email ?? user.Email;
        user.Phone = model.PhoneNumber ?? user.Phone;
        user.CountryId = model.CountryId ?? user.CountryId;
        user.StateId = model.StateId ?? user.StateId;
        user.CityId = model.CityId ?? user.CityId;
        user.Address = model.Address ?? user.Address;
        user.Zipcode = model.ZipCode ?? user.Zipcode;
        user.IsActive = model.IsActive ?? user.IsActive;
        user.LastLogin = model.LastLogin;
        if (profileimage != null)
        {
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
            var uniqueFileName = Guid.NewGuid().ToString() + "_" + profileimage.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                profileimage.CopyTo(fileStream);
            }
            user.ProfilePhoto = "/images/" + uniqueFileName;
        }

        _userRepository.UpdateUser(user);
        return true;
    }
    public bool CheckPassword(User user, string password)
    {
        return BCrypt.Net.BCrypt.Verify(password, user.Password);
    }

    public void ChangePassword(User user, string newPassword)
    {
        user.Password = HashPassword(newPassword);
        _userRepository.UpdateUser(user);
    }
    public void DeleteUser(AddUserVM model)
    {
        var user = GetUserByEmail(model.Email);
        if (user == null)
        {
            return;
        }
        _userRepository.DeleteUser(user);
    }
    private string GenerateResetToken()
    {
        var tokenData = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(tokenData);
        }
        return Convert.ToBase64String(tokenData);
    }
    private string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

}