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
            var trips = await exploreRussiaContext.Trips.Include(x => x.Reviews).ToListAsync();
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
        [HttpGet]
        public async Task<IActionResult> BookTrip(int tripId)
        {
            var trip = await exploreRussiaContext.Trips.FirstOrDefaultAsync(t => t.Id == tripId);
            if (trip == null)
            {
                return NotFound();
            }

            return View(trip);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmBooking(int tripId, int participantsQty)
        {
            var user = await exploreRussiaContext.Users.FirstOrDefaultAsync(u => u.Id == GetCurrentUserId());
            var trip = await exploreRussiaContext.Trips.FirstOrDefaultAsync(u => u.Id == tripId);

            if (user == null || string.IsNullOrEmpty(user.Phone) || string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.LastName))
            {
                return RedirectToAction("FillUserInfo", "Account");
            }

            var today = DateTime.UtcNow.Date;
            var existingOrder = await exploreRussiaContext.Orders
                .Where(o => o.UserId == user.Id && o.TripId == tripId && o.OrderDate.Date == today)
                .FirstOrDefaultAsync();

            if (existingOrder != null)
            {
                TempData["OrderError"] = "Вы уже подали заявку на этот поход сегодня.";
                return RedirectToAction("Index", "Home");
            }

            Order order = new()
            {
                UserId = user.Id,
                TripId = tripId,
                OrderDate = DateTime.UtcNow,
                TotalAmount = (trip.Price.HasValue ? (decimal)trip.Price : 0) * participantsQty,
                ParicipansQty = participantsQty,
                Status = "Pending"
            };

            await exploreRussiaContext.Orders.AddAsync(order);
            await exploreRussiaContext.SaveChangesAsync();

            TempData["OrderSuccess"] = "Заявка успешно отправлена.";
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public async Task<IActionResult> UserProfile()
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
            var userId = GetCurrentUserId();
            var user = await exploreRussiaContext.Users.FindAsync(userId);
            if (user != null)
            {
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Phone = model.Phone;
                user.Email = model.Email;

                exploreRussiaContext.Update(user);
                await exploreRussiaContext.SaveChangesAsync();
            }
            return RedirectToAction("UserProfile");
        }

        [Authorize]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> SubmitReview(int rating, string? comment, int tripId, int userId)
        {
            var existingReview = await exploreRussiaContext.Reviews.Where(x => x.TripId == tripId && x.UserId == userId).FirstOrDefaultAsync();
            if (existingReview != null)
            {
                TempData["ReviewAlready"] = "Вы уже оставляли отзыв на этот поход";
                return RedirectToAction("UserProfile");
            }

            Review review = new Review()
            {
                Rating = rating,
                Comment = comment,
                TripId = tripId,
                UserId = userId,
                ReviewDate = DateTime.Now
            };
            TempData["SubmitSuccess"] = "Отзыв успешно добавлен";
            await exploreRussiaContext.AddAsync(review);
            await exploreRussiaContext.SaveChangesAsync();

            return RedirectToAction("UserProfile");
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
            return RedirectToAction("UserProfile");
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
        }
    }
}
