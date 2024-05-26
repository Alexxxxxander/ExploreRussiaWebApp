using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ExploreRussia.Domain.Models;
using System;
using System.Threading.Tasks;

namespace ExploreRussiaWebApp.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly ExploreRussiaContext _context;

        public OrderController(ExploreRussiaContext context)
        {
            _context = context;
        }

        [HttpGet]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int tripId)
        {
            var userId = GetCurrentUserId();

            if (userId == null)
            {
                return RedirectToAction("Index", "Home");
            }

            Order order = new()
            {
                UserId = userId.Value,
                TripId = tripId,
                OrderDate = DateTime.UtcNow,
                TotalAmount = 0m 
            };

            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();

            TempData["OrderSuccess"] = "Заявка успешно отправлена.";
            return RedirectToAction("Index", "Home");
        }


        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : (int?)null;
        }
    }
}
