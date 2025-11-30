using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmacyChain.Data;
using PharmacyChain.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PharmacyChain.Controllers.Crud
{
    public class PurchaseOrdersController : Controller
    {
        private readonly ApplicationDbContext _db;
        public PurchaseOrdersController(ApplicationDbContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            var items = _db.PurchaseOrders
                .Include(p => p.Supplier)
                .Include(p => p.Pharmacy);

            return View(await items.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            var item = await _db.PurchaseOrders
                .Include(p => p.Supplier)
                .Include(p => p.Pharmacy)
                .Include(p => p.Lines)
                .ThenInclude(l => l.Drug)
                .FirstOrDefaultAsync(p => p.Id == id);

            return item == null ? NotFound() : View(item);
        }

        public IActionResult Create()
        {
            ViewBag.Suppliers = new SelectList(_db.Suppliers, "Id", "Name");
            ViewBag.Pharmacies = new SelectList(_db.Pharmacies, "Id", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(PurchaseOrder item)
        {
            item.CreatedAt = DateTime.UtcNow;

            if (ModelState.IsValid)
            {
                _db.PurchaseOrders.Add(item);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(item);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            var item = await _db.PurchaseOrders.FindAsync(id);

            ViewBag.Suppliers = new SelectList(_db.Suppliers, "Id", "Name");
            ViewBag.Pharmacies = new SelectList(_db.Pharmacies, "Id", "Name");

            return item == null ? NotFound() : View(item);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, PurchaseOrder item)
        {
            if (id != item.Id) return NotFound();

            _db.Update(item);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            var item = await _db.PurchaseOrders
                .Include(p => p.Supplier)
                .Include(p => p.Pharmacy)
                .FirstOrDefaultAsync(m => m.Id == id);

            return item == null ? NotFound() : View(item);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _db.PurchaseOrders.FindAsync(id);
            _db.PurchaseOrders.Remove(item);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
