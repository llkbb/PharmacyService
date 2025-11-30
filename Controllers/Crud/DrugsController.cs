using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmacyChain.Data;
using PharmacyChain.Models;

namespace PharmacyChain.Controllers.Crud
{
    public class DrugsController : Controller
    {
        private readonly ApplicationDbContext _db;
        public DrugsController(ApplicationDbContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            return View(await _db.Drugs.ToListAsync());
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
            _db.Drugs.Remove(item);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
