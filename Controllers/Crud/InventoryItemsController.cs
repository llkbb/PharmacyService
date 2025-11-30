using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmacyChain.Data;
using PharmacyChain.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PharmacyChain.Controllers.Crud
{
    public class InventoryItemsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public InventoryItemsController(ApplicationDbContext db)
        {
            _db = db;
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

            _db.Update(item);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
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
            _db.InventoryItems.Remove(item);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
