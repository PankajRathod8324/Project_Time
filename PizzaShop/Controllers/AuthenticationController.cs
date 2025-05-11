using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Entities.ViewModel;
using DAL.Interfaces;
using System.Net.Mail;
using System.Net;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.IdentityModel.Tokens.Jwt;
using Entities.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Newtonsoft.Json;
using BLL.Interfaces;

namespace PizzaShop.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly IUserService _userService;


        public AuthenticationController(IUserService userService)
        {
            _userService = userService;
        }
        [HttpGet("Loginpage")]
        public IActionResult Loginpage()
        {
            var user = new LoginVM();
            // string accessToken = Request.Cookies["Email"];
            string accessToken = Request.Cookies["AuthToken"];

            if (Request.Cookies["Email"] != null && Request.Cookies["Password"] != null)
            {
                user.Email = Request.Cookies["Email"];
                user.Password = Request.Cookies["Password"];
            }
            Console.WriteLine(accessToken);
            Console.WriteLine(!string.IsNullOrEmpty(accessToken));
            if (!string.IsNullOrEmpty(accessToken))
            {
                Console.WriteLine("Hellooo");
                return RedirectToAction("Dashboard", "Home");
            }
            return View(user);
        }
        [HttpPost("Loginpage")]
        public async Task<IActionResult> Loginpage(LoginVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Authenticate user and get token
            string token = _userService.AuthenticateUser(model.Email, model.Password);
            if (token == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                TempData["Message"] = "Password or Email is incorrect.";
                TempData["MessageType"] = "error";
                return View(model);
            }
            if (token == "User is inactive.")
            {
                TempData["Message"] = "User is not Active.";
                TempData["MessageType"] = "error";
                return View(model);
            }
            var olduser = _userService.GetUserByEmail(model.Email);
            if (olduser.LastLogin == null)
            {
                HttpContext.Session.SetString("UserEmail", model.Email);
                var cookieOptions = new CookieOptions
                {
                    Expires = DateTime.UtcNow.AddMinutes(30)
                };
                Response.Cookies.Append("Email", model.Email, cookieOptions);
                // Response.Cookies.Append("ProfilePhoto", selecteduser.ProfilePhoto, cookieOptions);

                return RedirectToAction("Changepassword", "User");
            }
            else
            {
                // Store user email in session (even if Remember Me is not checked)
                HttpContext.Session.SetString("UserEmail", model.Email);

                // If Remember Me is checked, store email & token in cookies
                if (model.RememberMe)
                {
                    var cookieOptions = new CookieOptions
                    {
                        Expires = DateTime.UtcNow.AddDays(30)
                    };
                    Response.Cookies.Append("Email", model.Email, cookieOptions);
                    var selecteduser = _userService.GetUserByEmail(model.Email);
                    Response.Cookies.Append("ProfilePhoto", selecteduser.ProfilePhoto, cookieOptions);
                    Response.Cookies.Append("Name", selecteduser.FirstName, cookieOptions);
                    Response.Cookies.Append("AuthToken", token, cookieOptions);
                }
                else
                {
                    var cookieOptions = new CookieOptions
                    {
                        Expires = DateTime.UtcNow.AddMinutes(30)
                    };
                    Response.Cookies.Append("Email", model.Email, cookieOptions);
                    var selecteduser = _userService.GetUserByEmail(model.Email);
                    Response.Cookies.Append("ProfilePhoto", selecteduser.ProfilePhoto, cookieOptions);
                    Response.Cookies.Append("Name", selecteduser.FirstName, cookieOptions);
                    Response.Cookies.Append("AuthToken", token, cookieOptions);
                }

                // Store JWT in session
                HttpContext.Session.SetString("AuthToken", token);

                // Fetch user data and store it in session
                var user = _userService.GetUserByEmail(model.Email);
                HttpContext.Session.SetString("UserData", JsonConvert.SerializeObject(user));

                // Extract claims from JWT token
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;
                var claims = jwtToken?.Claims.ToList();

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);
                if (olduser.RoleId == 1)
                {
                    return RedirectToAction("KOT", "AccountManagerOrderApp");
                }
                else
                {
                    return RedirectToAction("Dashboard", "Home");
                }
            }


        }
        public async Task<IActionResult> Logout()
        {
            // Sign out authentication scheme
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Remove session data
            HttpContext.Session.Remove("UserData");
            HttpContext.Session.Clear();

            // Expire authentication-related cookies
            if (Request.Cookies["AuthToken"] != null)
            {
                Response.Cookies.Delete("AuthToken");
            }
            if (Request.Cookies["Email"] != null)
            {
                Response.Cookies.Delete("Email");
                Response.Cookies.Delete("ProfilePhoto");
                Response.Cookies.Delete("Name");

            }

            // Redirect to login page
            return RedirectToAction("Loginpage");
        }

        public IActionResult Forgotpasswordpage(string? email)
        {

            var user = new ForgotPasswordVM();
            user.Email = email;
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordVM model)
        {
            var user = await _userService.GenerateResetTokenAsync(model.Email);
            if (user != null)
            {
                ViewBag.Message = "A reset link has been sent to your email.";
                TempData["Message"] = "Email has been send successfully!";
                TempData["MessageType"] = "success";
                await SendResetEmail(user.Email, user.ResetToken);

            }

            return RedirectToAction("Forgotpasswordpage", "Authentication");
        }


        // [HttpGet("ResetPassword")]
        public IActionResult Resetpasswordpage(string email, string token)
        {

            var user = _userService.GetUserByToken(token);
            if (user == null)
            {
                TempData["Message"] = "Your reset password link has been used or expired!";
                TempData["MessageType"] = "error"; // Types: success, error, warning, info
                return RedirectToAction("Loginpage", "Authentication");

            }

            var model = new ResetPasswordVM
            {
                Email = email,
                Token = token
            };
            Console.WriteLine(model.Token);
            Console.WriteLine("Auth Controller:  " + model.Email);
            Console.WriteLine(model.Token);

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordVM model)
        {
            Console.WriteLine("In resetPage");
            Console.WriteLine(model.Email);
            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState is not valid.");
                return View(model);
            }

            if (model.NewPassword != model.ConfirmPassword)
            {
                ModelState.AddModelError("", "Passwords do not match.");
                Console.WriteLine("Passwords do not match.");
                return View(model);
            }
            Console.WriteLine(model.Email);
            var result = await _userService.ResetPasswordAsync(model.Email, model.Token, model.NewPassword);
            Console.WriteLine("EMAIL:" + model.Email);

            if (result)
            {
                ViewBag.Message = "A Password Reset Successful.";
                Console.WriteLine("Password reset process successful.");
                TempData["Message"] = "Password has been resert successfully!";
                TempData["MessageType"] = "success";
                return RedirectToAction("Loginpage", "Authentication");
            }

            ModelState.AddModelError("", "Invalid token or the token has expired.");
            Console.WriteLine("Invalid token or the token has expired.");
            return RedirectToAction("Loginpage", "Authentication");
        }
        private async Task SendResetEmail(string email, string token)
        {
            var resetLink = Url.Action("Resetpasswordpage", "Authentication", new { token, email }, Request.Scheme);
            var message = new MailMessage("test.dotnet@etatvasoft.com", email);
            message.To.Add(new MailAddress(email));
            message.Subject = "Password Reset Request";
            message.Body = $"Please reset your password by clicking here: <a href='{resetLink}'>link</a>";
            message.IsBodyHtml = true;
            using (var smtp = new SmtpClient())
            {
                smtp.Host = "mail.etatvasoft.com";
                smtp.Port = 587;
                smtp.EnableSsl = true;
                smtp.Credentials = new NetworkCredential("test.dotnet@etatvasoft.com", "P}N^{z-]7Ilp");
                await smtp.SendMailAsync(message);
            }
        }
    }
}