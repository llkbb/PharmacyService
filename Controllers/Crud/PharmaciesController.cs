using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmacyChain.Data;
using PharmacyChain.Models;

namespace PharmacyChain.Controllers.Crud
{
    public class PharmaciesController : Controller
    {
        private readonly ApplicationDbContext _db;

        public PharmaciesController(ApplicationDbContext db) => _db = db;

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
            _db.Pharmacies.Remove(p);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
