using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using MPB_PORTAL_WEB_APP.Models;
using MPB_PORTAL_WEB_APP.ViewModels;
using System.Text.Encodings.Web; // ✅ Required for HtmlEncoder
using System.Linq;
using System.Threading.Tasks;

namespace MPB_PORTAL_WEB_APP.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<Users> signInManager;
        private readonly UserManager<Users> userManager;
        private readonly RoleManager<IdentityRole> roleManager;

        private readonly IEmailSender _emailSender;
        public AccountController(SignInManager<Users> signInManager,
                         UserManager<Users> userManager,
                         RoleManager<IdentityRole> roleManager,
                         IEmailSender emailSender)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.roleManager = roleManager;
            this._emailSender = emailSender;
        }

        // GET: Login Page
        public IActionResult Login()
        {
            return View();
        }

        // POST: Handle Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    ModelState.AddModelError("", "Invalid login attempt. Please check your credentials.");
                    return View(model);
                }

                var result = await signInManager.PasswordSignInAsync(user.UserName, model.Password, model.RememberMe, false);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Dashboard"); // Redirect to a single dashboard
                }

                ModelState.AddModelError("", "Invalid login attempt. Please check your credentials.");
            }
            return View(model);
        }

        // GET: Registration Page
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                Users user = new Users
                {
                    FullName = model.Name,
                    Email = model.Email,
                    UserName = model.Email,
                    EmailConfirmed = false // Ensure user needs to verify email before logging in
                };

                var result = await userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Assign default role "User"
                    if (!await roleManager.RoleExistsAsync("User"))
                    {
                        await roleManager.CreateAsync(new IdentityRole("User"));
                    }
                    await userManager.AddToRoleAsync(user, "User");

                    // Generate Email Verification Token
                    var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
                    var confirmationLink = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, token }, Request.Scheme);

                    // Send Email Verification
                    await _emailSender.SendEmailAsync(user.Email, "Verify Your Email",
                        $"Please confirm your email by clicking this link: <a href='{HtmlEncoder.Default.Encode(confirmationLink)}'>Verify Email</a>");

                    TempData["SuccessMessage"] = "Registration successful! A verification email has been sent. Please verify your email before logging in.";

                    return RedirectToAction("VerifyEmail", "Account"); // Redirect to verify email page
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }
            return View(model);
        }


        // Logout Method
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }
        // Verify Email View Method
        public IActionResult VerifyEmail()
        {
            return View();
        }

        // Verify Email Method
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyEmail(VerifyEmailViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "No account found with this email.");
                return View(model);
            }

            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, token = token }, Request.Scheme);

            // Send the confirmation link via email (email service implementation needed)
            TempData["SuccessMessage"] = $"Email confirmation link sent to {user.Email}.";
            return RedirectToAction("Login");
        }

        [HttpPost]
        public async Task<IActionResult> ResendVerificationEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                TempData["ErrorMessage"] = "Please enter your email.";
                return RedirectToAction(nameof(VerifyEmail));
            }

            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                TempData["ErrorMessage"] = "No account found with this email.";
                return RedirectToAction(nameof(VerifyEmail));
            }

            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, token }, Request.Scheme);

            await _emailSender.SendEmailAsync(user.Email, "Verify Your Email",
                $"Please confirm your email by clicking this link: <a href='{HtmlEncoder.Default.Encode(confirmationLink)}'>Verify Email</a>");

            TempData["SuccessMessage"] = "Verification email sent successfully. Check your inbox.";
            return RedirectToAction(nameof(VerifyEmail));
        }


        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return BadRequest("Invalid email verification request.");

            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound("User not found.");

            var result = await userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Your email has been verified! You can now log in.";
                return RedirectToAction("Login");
            }

            TempData["ErrorMessage"] = "Email verification failed. Try again.";
            return RedirectToAction("VerifyEmail");
        }
        //Change Password Method
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "No account found with this email.");
                return View(model);
            }

            var result = await userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Password changed successfully.";
                return RedirectToAction("Login");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }
    }
}
