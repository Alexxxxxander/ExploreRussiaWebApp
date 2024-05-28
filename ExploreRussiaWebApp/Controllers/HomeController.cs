using ExploreRussiaWebApp.Models;
using ExploreRussia.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;
using ExploreRussiaWebApp.Models.Home;
using System;


namespace ExploreRussiaWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ExploreRussiaContext exploreRussiaContext;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, ExploreRussiaContext exploreRussiaContext)
        {
            this.exploreRussiaContext = exploreRussiaContext;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var guides = await exploreRussiaContext.Guides.ToListAsync();
            var trips = await exploreRussiaContext.Trips.ToListAsync();
            var model = new HomeViewModel(guides, trips);
              
            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Guides()
        {
            return RedirectToAction("Index", "Guide");
        }

        public IActionResult Trips()
        {
            return RedirectToAction("Index", "Trip");
        }
        [Authorize]
        public async  Task<IActionResult> User(User model) 
        {
            var userId = GetCurrentUserId();
            var user = await exploreRussiaContext.Users.Include(u => u.Orders)
                                                       .ThenInclude(o => o.Trip)
                                                       .FirstOrDefaultAsync(u => u.Id == userId);
            return View(user);
        }
        public async Task<IActionResult> Profile()
        {
            var userId = GetCurrentUserId();
            var user = await exploreRussiaContext.Users.Include(u => u.Orders)
                                                       .ThenInclude(o => o.Trip)
                                                       .FirstOrDefaultAsync(u => u.Id == userId);
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(User model)
        {
            var userId = GetCurrentUserId;
            var user = await exploreRussiaContext.Users.FindAsync(userId);
            if (user != null)
            {
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Phone = model.Phone;
                await exploreRussiaContext.SaveChangesAsync();
            }
            return RedirectToAction("Profile");
        }

        [HttpPost]
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            var order = await exploreRussiaContext.Orders.FindAsync(orderId);
            if (order != null && order.UserId == GetCurrentUserId())
            {
                order.Status = "Canceled";
                await exploreRussiaContext.SaveChangesAsync();
            }
            return RedirectToAction("Profile");
        }


        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BookTrip(int tripId)
        {
            var user = await exploreRussiaContext.Users.FirstOrDefaultAsync(u => u.Id == GetCurrentUserId());

            if (user == null || string.IsNullOrEmpty(user.Phone) || string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.LastName))
            {
                // Перенаправить пользователя на страницу заполнения информации о пользователе
                return RedirectToAction("FillUserInfo", "Account");
            }

            // Проверяем, есть ли уже заявка на этот поход, сделанная сегодня
            var today = DateTime.UtcNow.Date;
            var existingOrder = await exploreRussiaContext.Orders
                .Where(o => o.UserId == user.Id && o.TripId == tripId && o.OrderDate.Date == today)
                .FirstOrDefaultAsync();

            if (existingOrder != null)
            {
                TempData["OrderError"] = "Вы уже подали заявку на этот поход сегодня.";
                return RedirectToAction("Index", "Home");
            }

            // Пользователь уже имеет необходимую информацию, переходим к созданию заказа
            Order order = new()
            {
                UserId = user.Id,
                TripId = tripId,
                OrderDate = DateTime.UtcNow,
                TotalAmount = 0m
            };

            await exploreRussiaContext.Orders.AddAsync(order);
            await exploreRussiaContext.SaveChangesAsync();

            TempData["OrderSuccess"] = "Заявка успешно отправлена.";
            return RedirectToAction("Index", "Home");
        }

        // Метод для получения ID текущего пользователя
        private int GetCurrentUserId()
        {
            var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
