using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmacyChain.Data;
using PharmacyChain.Models;

namespace PharmacyChain.Controllers.Crud
{
    public class SuppliersController : Controller
    {
        private readonly ApplicationDbContext _db;
        public SuppliersController(ApplicationDbContext db) => _db = db;

        public async Task<IActionResult> Index() =>
            View(await _db.Suppliers.ToListAsync());

        public async Task<IActionResult> Details(int? id)
        {
            var item = await _db.Suppliers.FirstOrDefaultAsync(x => x.Id == id);
            return View(item);
        }

        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(Supplier item)
        {
            if (ModelState.IsValid)
            {
                _db.Suppliers.Add(item);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(item);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            var item = await _db.Suppliers.FindAsync(id);
            return View(item);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Supplier item)
        {
            if (id != item.Id) return NotFound();
            _db.Update(item);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            var item = await _db.Suppliers.FirstOrDefaultAsync(m => m.Id == id);
            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _db.Suppliers.FindAsync(id);
            _db.Suppliers.Remove(item);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
