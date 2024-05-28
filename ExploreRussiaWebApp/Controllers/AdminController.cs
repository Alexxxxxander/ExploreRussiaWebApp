using ExploreRussia.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using Microsoft.EntityFrameworkCore;

namespace ExploreRussiaWebApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ExploreRussiaContext _context;

        public AdminController(ExploreRussiaContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Trip)
                .ToListAsync();

            return View(orders);
        }

        public async Task<IActionResult> Statistics()
        {
            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Trip)
                .ToListAsync();

            return View(orders);
        }
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditOrder(int id)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Trip)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditOrder(int id, decimal totalAmount)
        {
            var order = await _context.Orders.FindAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            order.TotalAmount = totalAmount;
            await _context.SaveChangesAsync();

            TempData["OrderUpdated"] = "Стоимость заказа успешно обновлена.";
            return RedirectToAction("Admin", "Index");
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ExportToExcel()
        {
            var trips = await _context.Trips.Include(t => t.Orders).ToListAsync();

            var stream = new MemoryStream();
            using (var package = new ExcelPackage(stream))
            {
                var worksheet = package.Workbook.Worksheets.Add("Trips");
                worksheet.Cells.LoadFromCollection(trips.Select(t => new {
                    TripName = t.TripName,
                    OrdersCount = t.Orders.Count
                }), true);
                package.Save();
            }
            stream.Position = 0;
            string excelName = $"Trip-Statistics-{DateTime.Now.ToString("yyyyMMddHHmmss")}.xlsx";

            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
        }
    }
}

