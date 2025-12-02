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

    public class PharmaciesController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly BusinessLogicService _businessLogic;

        public PharmaciesController(ApplicationDbContext db, BusinessLogicService businessLogic)
        {
            _db = db;
            _businessLogic = businessLogic;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _db.Pharmacies.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var pharmacy = await _db.Pharmacies.FirstOrDefaultAsync(m => m.Id == id);
            if (pharmacy == null) return NotFound();
            return View(pharmacy);
        }

        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(Pharmacy pharmacy)
        {
            if (ModelState.IsValid)
            {
                _db.Add(pharmacy);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(pharmacy);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var pharmacy = await _db.Pharmacies.FindAsync(id);
            return View(pharmacy);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Pharmacy pharmacy)
        {
            if (id != pharmacy.Id) return NotFound();
            _db.Update(pharmacy);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            var item = await _db.Pharmacies.FirstOrDefaultAsync(m => m.Id == id);
            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var p = await _db.Pharmacies.FindAsync(id);
            if (p == null)
            {
                TempData["Error"] = "Аптеку не знайдено.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                // Перевірити бізнес-правила перед видаленням
                await _businessLogic.ValidateCanDeletePharmacyAsync(id);

                _db.Pharmacies.Remove(p);
                await _db.SaveChangesAsync();

                TempData["Success"] = "Аптеку успішно видалено.";
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
