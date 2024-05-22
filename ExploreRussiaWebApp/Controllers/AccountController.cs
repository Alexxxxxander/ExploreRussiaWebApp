using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ExploreRussia.Domain.Models;
using ExploreRussia.Domain;
using ExploreRussiaWebApp.Models.Account;
using Microsoft.EntityFrameworkCore;

namespace ExploreRussiaWebApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly ExploreRussiaContext _context;

        public AccountController(ExploreRussiaContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View(new AccountViewModel
            {
                LoginViewModel = new LoginViewModel(),
                RegisterViewModel = new RegisterViewModel()
            });
        }

        public IActionResult FillUserInfo()
        {
            return View();
        }
        //[HttpPost]
        //public async Task<IActionResult> FillUserInfo(User model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == GetCurrentUserId());
        //        if (user != null)
        //        {
        //            user.FirstName = model.FirstName;
        //            user.LastName = model.LastName;
        //            user.Patronymic = model.Patronymic;
        //            user.Phone = model.Phone;

        //            _context.Users.Update(user);
        //            await _context.SaveChangesAsync();

        //            // Перенаправить пользователя на другую страницу после успешного сохранения
        //            return RedirectToAction("Index", "Home");
        //        }
        //    }

        //    // Если модель недействительна, вернуть представление с ошибками валидации
        //    return View(model);
        //}
        private int GetCurrentUserId()
        {
            var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
        }

        [HttpPost]
        public async Task<IActionResult> Login([Bind(Prefix = "l")] LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", new AccountViewModel
                {
                    LoginViewModel = model
                });
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email && u.Password == model.Password);

            if (user == null)
            {
                ViewBag.Error = "Некорректные логин и(или) пароль";
                return View("Index", new AccountViewModel
                {
                    LoginViewModel = model
                });
            }

            await AuthenticateAsync(user);
            return user.RoleId == 2 ? RedirectToAction("Index", "Admin") : RedirectToAction("Index", "Home");
        }

        private async Task AuthenticateAsync(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimsIdentity.DefaultNameClaimType, user.Email),
                new Claim(ClaimTypes.Role, user.RoleId == 2 ? "Admin" : "User")
            };

            var id = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme, ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }

        [HttpPost]
        public async Task<IActionResult> Register([Bind(Prefix = "r")] RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", new AccountViewModel
                {
                    RegisterViewModel = model
                });
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user != null)
            {
                ViewBag.RegisterError = "Пользователь с такой почтой уже существует!";
                return View("Index", new AccountViewModel
                {
                    RegisterViewModel = model
                });
            }

            user = new User(model.Email, model.Password);
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            await AuthenticateAsync(user);
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Account");
        }
    }
}
