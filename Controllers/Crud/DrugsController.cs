using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmacyChain.Data;
using PharmacyChain.Exceptions;
using PharmacyChain.Models;
using PharmacyChain.Services;

namespace PharmacyChain.Controllers.Crud
{
    [Authorize(Roles = "Admin")]

    public class DrugsController : Controller
    {

        private readonly ApplicationDbContext _db;
        private readonly BusinessLogicService _businessLogic;


        public DrugsController(ApplicationDbContext db, BusinessLogicService businessLogic)
        {
            _db = db;
            _businessLogic = businessLogic;
        }


        public async Task<IActionResult> Index(string? search, string? category, bool? prescriptionRequired)
        {
            var query = _db.Drugs.AsQueryable();

            // Пошук за назвою або описом
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(d => d.Name.Contains(search) || d.Description.Contains(search));
                ViewBag.Search = search;
            }

            // Фільтрація за категорією
            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(d => d.Category == category);
                ViewBag.Category = category;
            }

            // Фільтрація за рецептурністю
            if (prescriptionRequired.HasValue)
            {
                query = query.Where(d => d.PrescriptionRequired == prescriptionRequired.Value);
                ViewBag.PrescriptionRequired = prescriptionRequired.Value;
            }

            // Список категорій для фільтру
            ViewBag.Categories = await _db.Drugs
                .Select(d => d.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            return View(await query.OrderBy(d => d.Name).ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var item = await _db.Drugs.FirstOrDefaultAsync(m => m.Id == id);
            return View(item);
        }

        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(Drug item)
        {
            if (ModelState.IsValid)
            {
                _db.Drugs.Add(item);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(item);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            var item = await _db.Drugs.FindAsync(id);
            return View(item);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Drug item)
        {
            if (id != item.Id) return NotFound();
            _db.Update(item);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            var item = await _db.Drugs.FirstOrDefaultAsync(m => m.Id == id);
            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _db.Drugs.FindAsync(id);
            if (item == null)
            {
                TempData["Error"] = "Препарат не знайдено.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                // Перевірити бізнес-правила перед видаленням
                await _businessLogic.ValidateCanDeleteDrugAsync(id);

                _db.Drugs.Remove(item);
                await _db.SaveChangesAsync();

                TempData["Success"] = "Препарат успішно видалено.";
                return RedirectToAction(nameof(Index));
            }
            catch (BusinessLogicException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Delete), new { id });
            }
        }
    }
}
