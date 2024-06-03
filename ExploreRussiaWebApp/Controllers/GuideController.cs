using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ExploreRussia.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace ExploreRussiaWebApp.Controllers
{
    public class GuideController : Controller
    {
        private readonly ExploreRussiaContext _context;

        public GuideController(ExploreRussiaContext context)
        {
            _context = context;
        }

        // GET: Guide
        public async Task<IActionResult> Index()
        {
            var exploreRussiaContext = _context.Guides.Include(g => g.Gender);
            return View(await exploreRussiaContext.ToListAsync());
        }

        public async Task<IActionResult> EditList()
        {
            var exploreRussiaContext = _context.Guides.Include(g => g.Gender);
            return View(await exploreRussiaContext.ToListAsync());
        }

        // GET: Guide/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var guide = await _context.Guides
                .Include(g => g.Gender)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (guide == null)
            {
                return NotFound();
            }

            return View(guide);
        }

        // GET: Guide/Create
        public IActionResult Create()
        {
            ViewData["GenderId"] = new SelectList(_context.Genders, "Id", "Name");
            return View();
        }

        private bool IsValidImageExtension(string fileName)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg" };
            string extension = Path.GetExtension(fileName).ToLowerInvariant();
            return allowedExtensions.Contains(extension);
        }

        private string ProcessUploadedFile(IFormFile uploadedFile, int guideId)
        {
            if (uploadedFile != null && uploadedFile.Length > 0)
            {
                var fileName = "guide" + guideId + Path.GetExtension(uploadedFile.FileName);
                var filePath = Path.Combine( "/images/", fileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    uploadedFile.CopyTo(fileStream);
                }
                return "/images/" + fileName;
            }
            return string.Empty;
        }

        // POST: Guide/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("LastName,FirstName,Patronymic,Phone,Email,ExperienceYears,GenderId,ImageUrl, Gender")] Guide guide, IFormFile? uploadedFile)
        {
            if (ModelState.IsValid)
            {
                if (uploadedFile != null && uploadedFile.Length > 0)
                {
                    if (!IsValidImageExtension(uploadedFile.FileName))
                    {
                        ModelState.AddModelError("", "Допустимы только файлы с расширениями .jpg или .jpeg.");
                        ViewData["GenderId"] = new SelectList(_context.Genders, "Id", "Name");
                        return View(guide);
                    }

                    var id = await _context.Guides.MaxAsync(g => (int?)g.Id) ?? 0 + 1;
                    guide.ImageUrl = ProcessUploadedFile(uploadedFile, id);
                }
                await _context.AddAsync(guide);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(EditList));
            }
            ViewData["GenderId"] = new SelectList(_context.Genders, "Id", "Name", guide.GenderId);
            return View(guide);
        }


        // GET: Guide/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var guide = await _context.Guides.FindAsync(id);
            if (guide == null)
            {
                return NotFound();
            }
            ViewData["GenderId"] = new SelectList(_context.Genders, "Id", "Name", guide.GenderId);
            return View(guide);
        }

        // POST: Guide/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,LastName,FirstName,Patronymic,Phone,Email,ExperienceYears,GenderId, Gender")] Guide guide, IFormFile? uploadedFile)
        {
            if (id != guide.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var existingGuide = await _context.Guides.FirstOrDefaultAsync(g => g.Id == id);
                if (existingGuide == null)
                {
                    return NotFound();
                }

                if (uploadedFile != null && uploadedFile.Length > 0)
                {
                    if (!IsValidImageExtension(uploadedFile.FileName))
                    {
                        ModelState.AddModelError("", "Допустимы только файлы с расширениями .jpg или .jpeg.");
                        ViewData["GenderId"] = new SelectList(_context.Genders, "Id", "Name", guide.GenderId);
                        return View(guide);
                    }

                    guide.ImageUrl = ProcessUploadedFile(uploadedFile, guide.Id);
                }
                else
                {
                    // Если файл изображения не был загружен, сохранить старое значение ImageUrl
                    guide.ImageUrl = existingGuide.ImageUrl;
                }

                _context.Update(guide);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["GenderId"] = new SelectList(_context.Genders, "Id", "Name", guide.GenderId);
            return View(guide);
        }

        // GET: Guide/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var guide = await _context.Guides
                .Include(g => g.Gender)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (guide == null)
            {
                return NotFound();
            }

            return View(guide);
        }

        // POST: Guide/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var guide = await _context.Guides.FindAsync(id);
            if (guide != null)
            {
                _context.Guides.Remove(guide);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GuideExists(int id)
        {
            return _context.Guides.Any(e => e.Id == id);
        }
    }
}
