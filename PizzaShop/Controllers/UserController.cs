using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using BLL.Interfaces;
using DAL.Interfaces;
using Entities.Models;
using Entities.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PizzaShop.Controllers;


public class UserController : Controller
{
    private readonly IUserService _userService;

    private readonly IHttpContextAccessor _httpContextAccessor;
    public UserController(IUserService userService, IHttpContextAccessor httpContextAccessor)
    {
        _userService = userService;
        _httpContextAccessor = httpContextAccessor;
    }

    // [Authorize]
    public IActionResult Profile()
    {
        var email = Request.Cookies["email"];


        if (string.IsNullOrEmpty(email))
        {
            // If email is not found in cookies, get it from session
            email = HttpContext.Session.GetString("UserEmail");
        }

        if (string.IsNullOrEmpty(email))
        {
            return BadRequest("Email is required.");
        }

        var profileVM = _userService.GetUserProfileByEmail(email);
        if (profileVM == null)
        {
            return NotFound();
        }

        PopulateDropdowns(profileVM.CountryId, profileVM.StateId);
        return View(profileVM);
    }

    // [Authorize]
    [HttpPost]
    public IActionResult Profile(ProfileVM model, IFormFile ProfilePhoto)
    {

        PopulateDropdowns(model.CountryId, model.StateId);
        var email = Request.Cookies["email"];
        model.Email = email;

        _userService.UpdateUserProfile(model, ProfilePhoto);

        TempData["Message"] = "User updated successfully!";
        TempData["MessageType"] = "info";

        return RedirectToAction("Profile", "User");
    }

    [Authorize(Policy = "UserEditPolicy")]
    public IActionResult Adduser()
    {
        var email = Request.Cookies["email"];
        if (string.IsNullOrEmpty(email))
        {
            return BadRequest("Email is required.");
        }

        var user = _userService.GetUserByEmail(email);

        if (user == null)
        {
            return NotFound();
        }
        PopulateDropdowns(user.CountryId, user.StateId);
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> AdduserAsync(AddUserVM model, IFormFile profileimage)
    {
        var olduser = _userService.GetUserByEmail(model.Email);
        if (olduser != null)
        {
            TempData["Message"] = "Account already exist with Email";
            TempData["MessageType"] = "error";
        }
        else
        {
            PopulateDropdowns(model.CountryId, model.StateId);
            _userService.AddUser(model, profileimage);
            // await SendInfoEmail(model.Email);
            TempData["Message"] = "User added successfully!";
            TempData["MessageType"] = "success";

            return RedirectToAction("Userpage", "Home");
        }

        return RedirectToAction("Adduser", "User");

    }

    [Authorize(Policy = "UserEditPolicy")]
    [HttpGet]
    public IActionResult Edituser(int id)
    {
        var user = _userService.GetUserById(id);
        var email = Request.Cookies["email"];
        var loginuser = _userService.GetUserByEmail(email);
        if (loginuser.RoleId == 3 || loginuser.RoleId == 1)
        {
            if (user.RoleId == 2)
            {
                TempData["Message"] = "You can't updated Super Admin";
                TempData["MessageType"] = "warning";
                return RedirectToAction("Userpage", "Home");
            }
        }

        if (user == null)
        {
            return NotFound();
        }
        PopulateDropdowns(user.CountryId, user.StateId);
        return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edituser(AddUserVM model, IFormFile ProfileImage)
    {
        var email = Request.Cookies["email"];
        var loginuser = _userService.GetUserByEmail(email);
        if (loginuser.RoleId == 3 || loginuser.RoleId == 1)
        {
            if (model.RoleId == 2)
            {
                TempData["Message"] = "You can't updated Super Admin";
                TempData["MessageType"] = "warning";
                return RedirectToAction("Userpage", "Home");
            }
        }
        if (!ModelState.IsValid)
        {
            PopulateDropdowns(model.CountryId, model.StateId);
            var result = _userService.UpdateUser(model, ProfileImage);

            if (!result)
            {
                return NotFound();
            }

            TempData["Message"] = "User updated successfully!";
            TempData["MessageType"] = "info";
            return RedirectToAction("Edituser", "User");
        }
        return RedirectToAction("Userpage", "Home");

    }

    [Authorize]
    [HttpGet]
    public JsonResult GetStatesByCountry(int countryId)
    {
        var states = _userService.GetStatesByCountry(countryId);
        var stateSelectList = states.Select(s => new SelectListItem
        {
            Value = s.StateId.ToString(),
            Text = s.StateName
        }).ToList();

        return Json(stateSelectList);
    }

    [Authorize]
    [HttpGet]
    public JsonResult GetCitiesByState(int stateId)
    {
        var cities = _userService.GetCitiesByState(stateId);
        var citySelectList = cities.Select(c => new SelectListItem
        {
            Value = c.CityId.ToString(),
            Text = c.CityName
        }).ToList();

        return Json(citySelectList);
    }

    [Authorize(Policy = "UserDeletePolicy")]
    [HttpPost]
    public IActionResult Delete(int userId)
    {
        AddUserVM user = _userService.GetUserById(userId);
        _userService.DeleteUser(user);
        return RedirectToAction("Userpage", "Home");
    }

    // [Authorize]
    [HttpGet]
    public IActionResult Changepassword()
    {
        return View();
    }

    // [Authorize]
    [HttpPost]
    public IActionResult ChangePassword(ChangePasswordVM model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var email = Request.Cookies["email"];
        if (string.IsNullOrEmpty(email))
        {
            return BadRequest("Email is required.");
        }

        var user = _userService.GetUserByEmail(email);
        if (user == null)
        {
            return NotFound();
        }

        if (!_userService.CheckPassword(user, model.CurrentPassword))
        {
            ModelState.AddModelError(string.Empty, "The current password is incorrect.");
            TempData["Message"] = "The current password is incorrect.";
            TempData["MessageType"] = "error";
            return View("Changepassword", model);
        }
        if (user.LastLogin == null)
        {
            user.LastLogin = (DateTime)user.LastLogin;
            _userService.ChangePassword(user, model.NewPassword);
            TempData["Message"] = "Password changed successfully!";
            TempData["MessageType"] = "success";
            return RedirectToAction("Loginpage", "Authentication");
        }
        else
        {
            _userService.ChangePassword(user, model.NewPassword);
            TempData["Message"] = "Password changed successfully!";
            TempData["MessageType"] = "success";
            return RedirectToAction("ChangePassword");
        }
        // return RedirectToAction("ChangePassword");
    }

    private void PopulateDropdowns(int? countryId = null, int? stateId = null)
    {
        var email = Request.Cookies["email"];
        var user = _userService.GetUserByEmail(email);
        var role = _userService.GetRoleNameById((int)user.RoleId);
        ViewBag.Roles = _userService.GetAllRoles().Where(r => !(role == "Account Manager" && r.RoleName == "Super Admin")).OrderBy(r => r.Priority).Select(r => new SelectListItem
        {
            Value = r.RoleId.ToString(),
            Text = r.RoleName
        }).ToList();

        ViewBag.Countries = _userService.GetAllCountries().Select(c => new SelectListItem
        {
            Value = c.CountryId.ToString(),
            Text = c.CountryName
        }).ToList();

        ViewBag.States = countryId.HasValue ?
            _userService.GetStatesByCountry(countryId.Value).Select(s => new SelectListItem
            {
                Value = s.StateId.ToString(),
                Text = s.StateName
            }).ToList()
            : new List<SelectListItem>();

        ViewBag.Cities = stateId.HasValue ?
            _userService.GetCitiesByState(stateId.Value).Select(c => new SelectListItem
            {
                Value = c.CityId.ToString(),
                Text = c.CityName
            }).ToList()
            : new List<SelectListItem>();
    }

    private async Task SendInfoEmail(string email)
    {
        // Fetch user details from the database
        var user = _userService.GetUserByEmail(email);
        if (user == null) return; // Exit if user doesn't exist

        // Get the request context
        var request = _httpContextAccessor.HttpContext.Request;

        // Fetch logo URL dynamically from wwwroot
        var logoUrl = $"{request.Scheme}://{request.Host}/assest/logos/pizzashop_logo.png";

        // Load the email template from wwwroot
        string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/templates/EmailTemplate.html");
        string emailBody = await System.IO.File.ReadAllTextAsync(templatePath);

        // Generate reset link
        // var resetLink = $"{request.Scheme}://{request.Host}/Authentication/Resetpasswordpage?email={email}";

        // Replace placeholders in the email template
        emailBody = emailBody.Replace("{{LOGO_URL}}", logoUrl)
                             .Replace("{{USERNAME}}", user.Username)  // Fetch username from DB
                             .Replace("{{EMAIL}}", user.Email)
                             .Replace("{{TEMP_PASSWORD}}", user.Password)  // Fetch temp password
                                                                           //  .Replace("{{RESET_LINK}}", resetLink)
                             .Replace("{{YEAR}}", DateTime.UtcNow.Year.ToString());

        // Create the email message
        var message = new MailMessage
        {
            From = new MailAddress("test.dotnet@etatvasoft.com", "Your Company"),
            Subject = "Your Account Details",
            Body = emailBody,
            IsBodyHtml = true
        };
        message.To.Add(new MailAddress(email));

        // Configure SMTP client
        using (var smtp = new SmtpClient("mail.etatvasoft.com", 587))
        {
            smtp.EnableSsl = true;
            smtp.Credentials = new NetworkCredential("test.dotnet@etatvasoft.com", "P}N^{z-]7Ilp");
            await smtp.SendMailAsync(message);
        }
    }
    //     var resetLink = Url.Action("Resetpasswordpage", "Authentication", new { token, email }, Request.Scheme);
    //     var message = new MailMessage("test.dotnet@etatvasoft.com", email);
    //     message.To.Add(new MailAddress(email));
    //         message.Subject = "Password Reset Request";
    //         message.Body = $"Please reset your password by clicking here: <a href='{resetLink}'>link</a>";
    //         message.IsBodyHtml = true;
    //         using (var smtp = new SmtpClient())
    //         {
    //             smtp.Host = "mail.etatvasoft.com";
    //             smtp.Port = 587;
    //             smtp.EnableSsl = true;
    //             smtp.Credentials = new NetworkCredential("test.dotnet@etatvasoft.com", "P}N^{z-]7Ilp");
    //     await smtp.SendMailAsync(message);
    // }
    // }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}