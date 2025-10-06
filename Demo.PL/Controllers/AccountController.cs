using Demo.BLL.EmailSettings;
using Demo.DAL.Models.IdentityModule;
using Demo.PL.Models.IdentityModule;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Demo.PL.Controllers
{
    public class AccountController(UserManager<ApplicationUser> _userManager,
        SignInManager<ApplicationUser> _signinManager, IEmailSetting _emailSetting, IWebHostEnvironment _env) : Controller
    {
        #region Register 
        [HttpGet]
        public IActionResult Register() => View();
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel registerViewModel)
        {
            if (!ModelState.IsValid) return View(registerViewModel);

            var userRegister = new ApplicationUser()
            {
                UserName = registerViewModel.UserName,
                Email = registerViewModel.Email,
                FirstName = registerViewModel.FirstName,
                LastName = registerViewModel.LastName,
                PhoneNumber = registerViewModel.PhoneNumber
            };

            var result = await _userManager.CreateAsync(userRegister, registerViewModel.Password);
            if (result.Succeeded)
                return RedirectToAction("Login");

            else
            {
                foreach (var Error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, Error.Description);
                }
            }
            return View(registerViewModel);
        }

        #endregion

        #region LogIn
        [HttpGet]
        public IActionResult LogIn() => View();
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogIn(LogInViewModel logInViewModel)
        {
            if (!ModelState.IsValid) return BadRequest();

            var user = await _userManager.FindByEmailAsync(logInViewModel.Email);
            if (user is { })
            {
                var checkPassword = await _userManager.CheckPasswordAsync(user, logInViewModel.Password);
                if (checkPassword)
                {
                    var Result = await _signinManager
                        .PasswordSignInAsync(user, logInViewModel.Password, logInViewModel.RememberMe, false);
                    if (Result.IsNotAllowed)
                        ModelState.AddModelError("", "Your Account is not confirmed yet.");
                    if (Result.IsLockedOut)
                        ModelState.AddModelError("", "Your Account is Locked.");
                    if (Result.Succeeded)
                    {

                        return RedirectToAction(nameof(HomeController.Index), "Home");
                    }

                }
                TempData["Error"] = "Failed, try again!";

                ModelState.AddModelError("", "Login Faild!");
            }
            return View(logInViewModel);
        }


        #endregion

        #region Logout
        [HttpPost]

        public async Task<IActionResult> Logout(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            await _signinManager.SignOutAsync();

            return RedirectToAction(nameof(LogIn));
        }
        #endregion


        #region ForgetPassword
        [HttpGet]

        public IActionResult ForgetPassword()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordViewModel _forgetPasswordViewModel)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(_forgetPasswordViewModel.Email);
                if (user != null && await _userManager.IsEmailConfirmedAsync(user))
                {

                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var PasswordLink = Url.Action("ResetPassword", "Account",
                        new { email = _forgetPasswordViewModel.Email, token = token }, Request.Scheme);
                    var email = new Email()
                    {
                        To = _forgetPasswordViewModel.Email,
                        Subject = "Reset Your Password",
                        Body = PasswordLink
                    };
                    _emailSetting.SendEmail(email);
                    return View("ForgetPasswordConfirmMessage");
                }
            }
            ModelState.AddModelError("", "Fiald to resest password");
            return View(_forgetPasswordViewModel);
        }
        #endregion
        #region Reset Password
        [HttpGet]

        public IActionResult ResetPassword(string email, string token)
        {
            if (email == null || token == null)
                return NotFound();
            return View();
        }

        [HttpPost]

        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel reset)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(reset.Email);
                if (user != null)
                {
                    var result = await _userManager.ResetPasswordAsync(user, reset.Token, reset.Password);
                    if (result.Succeeded)
                        return View("ResetPasswordConfirmation");
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View(reset);
                }
            }
            return View(reset);
        }
        #endregion
    }
}
