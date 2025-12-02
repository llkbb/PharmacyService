using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PharmacyChain.Data;
using PharmacyChain.Exceptions;
using PharmacyChain.Models;
using PharmacyChain.Services;

namespace PharmacyChain.Controllers.Crud
{
    [Authorize(Roles = "Admin,Pharmacist")]

    public class InventoryItemsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly BusinessLogicService _businessLogic;

        public InventoryItemsController(ApplicationDbContext db, BusinessLogicService businessLogic)
        {
            _db = db;
            _businessLogic = businessLogic;
        }

        public async Task<IActionResult> Index()
        {
            var items = _db.InventoryItems
                .Include(i => i.Pharmacy)
                .Include(i => i.Drug);

            return View(await items.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var item = await _db.InventoryItems
                .Include(i => i.Pharmacy)
                .Include(i => i.Drug)
                .FirstOrDefaultAsync(m => m.Id == id);

            return item == null ? NotFound() : View(item);
        }

        public IActionResult Create()
        {
            ViewBag.Pharmacies = new SelectList(_db.Pharmacies, "Id", "Name");
            ViewBag.Drugs = new SelectList(_db.Drugs, "Id", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(InventoryItem item)
        {
            if (ModelState.IsValid)
            {
                _db.InventoryItems.Add(item);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Pharmacies = new SelectList(_db.Pharmacies, "Id", "Name");
            ViewBag.Drugs = new SelectList(_db.Drugs, "Id", "Name");
            return View(item);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            var item = await _db.InventoryItems.FindAsync(id);
            if (item == null) return NotFound();

            ViewBag.Pharmacies = new SelectList(_db.Pharmacies, "Id", "Name");
            ViewBag.Drugs = new SelectList(_db.Drugs, "Id", "Name");

            return View(item);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, InventoryItem item)
        {
            if (id != item.Id) return NotFound();

            try
            {
                _db.Update(item);
                await _db.SaveChangesAsync();
                TempData["Success"] = "Запас успішно оновлено.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError("", "Помилка при збереженні: " + ex.GetBaseException().Message);
                ViewBag.Pharmacies = new SelectList(_db.Pharmacies, "Id", "Name", item.PharmacyId);
                ViewBag.Drugs = new SelectList(_db.Drugs, "Id", "Name", item.DrugId);
                return View(item);
            }
        }

        public async Task<IActionResult> Delete(int? id)
        {
            var item = await _db.InventoryItems
                .Include(i => i.Pharmacy)
                .Include(i => i.Drug)
                .FirstOrDefaultAsync(m => m.Id == id);

            return item == null ? NotFound() : View(item);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _db.InventoryItems.FindAsync(id);
            if (item == null)
            {
                TempData["Error"] = "Запас не знайдено.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                // Перевірити бізнес-правила перед видаленням
                await _businessLogic.ValidateCanDeleteInventoryItemAsync(id);

                _db.InventoryItems.Remove(item);
                await _db.SaveChangesAsync();

                TempData["Success"] = "Запас успішно видалено.";
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
