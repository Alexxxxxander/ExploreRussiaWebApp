using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ExploreRussia.Domain.Models;
using System.IO;
using Microsoft.AspNetCore.Authorization;

namespace ExploreRussiaWebApp.Controllers
{
    public class TripController : Controller
    {
        private readonly ExploreRussiaContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public TripController(ExploreRussiaContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Trip
        public async Task<IActionResult> Index()
        {
            var exploreRussiaContext = _context.Trips.Include(t => t.Guide);
            return View(await exploreRussiaContext.ToListAsync());
        }

        // GET: Trip/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trip = await _context.Trips
                .Include(t => t.Guide)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (trip == null)
            {
                return NotFound();
            }

            return View(trip);
        }

        // GET: Trip/Create
        public IActionResult Create()
        {
            ViewData["GuideId"] = new SelectList(_context.Guides, "Id", "Email");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("TripName,Description,StartDate,EndDate,Price,MaxParticipants,GuideId,IsActual")] Trip trip, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    if (!IsValidImageExtension(imageFile.FileName))
                    {
                        ModelState.AddModelError("", "Допустимы только файлы с расширениями .jpg или .jpeg.");
                        ViewData["GuideId"] = new SelectList(_context.Guides, "Id", "Email");
                        return View(trip);
                    }

                    var id = await _context.Trips.MaxAsync(g => (int?)g.Id) ?? 0 + 2;
                    var fileName = "trip" + id + Path.GetExtension(imageFile.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }
                    trip.ImageUrl = "/images/" + fileName;
                }

                _context.Add(trip);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["GuideId"] = new SelectList(_context.Guides, "Id", "Email", trip.GuideId);
            return View(trip);
        }

        // GET: Trip/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trip = await _context.Trips.FindAsync(id);
            if (trip == null)
            {
                return NotFound();
            }
            ViewData["GuideId"] = new SelectList(_context.Guides, "Id", "Email", trip.GuideId);
            return View(trip);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id, [Bind("Id,TripName,Description,StartDate,EndDate,Price,MaxParticipants,GuideId,ImageUrl,IsActual")] Trip trip, IFormFile? imageFile)
        {
            if (id != trip.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var existingTrip = await _context.Trips.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
                if (existingTrip == null)
                {
                    return NotFound();
                }

                if (imageFile != null && imageFile.Length > 0)
                {
                    if (!IsValidImageExtension(imageFile.FileName))
                    {
                        ModelState.AddModelError("", "Допустимы только файлы с расширениями .jpg или .jpeg.");
                        ViewData["GuideId"] = new SelectList(_context.Guides, "Id", "Email", trip.GuideId);
                        return View(trip);
                    }

                    var fileName = "trip" + trip.Id + Path.GetExtension(imageFile.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }
                    trip.ImageUrl = "/images/" + fileName;
                }
                else
                {                    
                    trip.ImageUrl = existingTrip.ImageUrl;
                }

                try
                {
                    _context.Update(trip);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Trips.Any(t => t.Id == id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["GuideId"] = new SelectList(_context.Guides, "Id", "Email", trip.GuideId);
            return View(trip);
        }


        private bool IsValidImageExtension(string fileName)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg" };
            var extension = Path.GetExtension(fileName)?.ToLower();
            return allowedExtensions.Contains(extension);
        }



        private bool TripExists(int id)
        {
            return _context.Trips.Any(e => e.Id == id);
        }
    }
}
