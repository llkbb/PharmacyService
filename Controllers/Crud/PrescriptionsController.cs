using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PharmacyChain.Data;
using PharmacyChain.Models;

namespace PharmacyChain.Controllers.Crud
{
    [Authorize(Roles = "Admin,Pharmacist")]

    public class PrescriptionsController : Controller
    {
        private readonly ApplicationDbContext _db;
        public PrescriptionsController(ApplicationDbContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            var items = _db.Prescriptions
                .Include(p => p.Customer)
                .Include(p => p.Drug);
            return View(await items.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            var item = await _db.Prescriptions
                .Include(p => p.Customer)
                .Include(p => p.Drug)
                .FirstOrDefaultAsync(m => m.Id == id);
            return item == null ? NotFound() : View(item);
        }

        public IActionResult Create()
        {
            ViewBag.Customers = new SelectList(_db.Customers, "Id", "FullName");
            ViewBag.Drugs = new SelectList(_db.Drugs, "Id", "Name");
            ViewBag.Pharmacies = new SelectList(_db.Pharmacies, "Id", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Prescription item)
        {
            if (ModelState.IsValid)
            {
                _db.Prescriptions.Add(item);
                await _db.SaveChangesAsync();
                TempData["Success"] = "Рецепт успішно створено!";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Customers = new SelectList(_db.Customers, "Id", "FullName");
            ViewBag.Drugs = new SelectList(_db.Drugs, "Id", "Name");
            ViewBag.Pharmacies = new SelectList(_db.Pharmacies, "Id", "Name");
            return View(item);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            var item = await _db.Prescriptions.FindAsync(id);
            if (item == null) return NotFound();
            ViewBag.Customers = new SelectList(_db.Customers, "Id", "FullName");
            ViewBag.Drugs = new SelectList(_db.Drugs, "Id", "Name");
            ViewBag.Pharmacies = new SelectList(_db.Pharmacies, "Id", "Name");
            return View(item);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Prescription item)
        {
            if (id != item.Id) return NotFound();
            if (ModelState.IsValid)
            {
                _db.Update(item);
                await _db.SaveChangesAsync();
                TempData["Success"] = "Рецепт успішно оновлено!";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Customers = new SelectList(_db.Customers, "Id", "FullName");
            ViewBag.Drugs = new SelectList(_db.Drugs, "Id", "Name");
            ViewBag.Pharmacies = new SelectList(_db.Pharmacies, "Id", "Name");
            return View(item);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            var item = await _db.Prescriptions
                .Include(p => p.Customer)
                .Include(p => p.Drug)
                .FirstOrDefaultAsync(p => p.Id == id);
            return item == null ? NotFound() : View(item);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _db.Prescriptions.FindAsync(id);
            if (item != null)
            {
                _db.Prescriptions.Remove(item);
                await _db.SaveChangesAsync();
                TempData["Success"] = "Рецепт успішно видалено!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
