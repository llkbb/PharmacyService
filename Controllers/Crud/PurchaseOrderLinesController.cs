using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmacyChain.Data;
using PharmacyChain.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PharmacyChain.Controllers.Crud
{
    public class PurchaseOrderLinesController : Controller
    {
        private readonly ApplicationDbContext _db;
        public PurchaseOrderLinesController(ApplicationDbContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            var items = _db.PurchaseOrderLines
                .Include(l => l.PurchaseOrder)
                .Include(l => l.Drug);
            return View(await items.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            var item = await _db.PurchaseOrderLines
                .Include(l => l.PurchaseOrder)
                .Include(l => l.Drug)
                .FirstOrDefaultAsync(m => m.Id == id);

            return item == null ? NotFound() : View(item);
        }

        public IActionResult Create()
        {
            ViewBag.Orders = new SelectList(_db.PurchaseOrders, "Id", "Id");
            ViewBag.Drugs = new SelectList(_db.Drugs, "Id", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(PurchaseOrderLine item)
        {
            if (ModelState.IsValid)
            {
                _db.PurchaseOrderLines.Add(item);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(item);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            var item = await _db.PurchaseOrderLines.FindAsync(id);

            ViewBag.Orders = new SelectList(_db.PurchaseOrders, "Id", "Id");
            ViewBag.Drugs = new SelectList(_db.Drugs, "Id", "Name");

            return item == null ? NotFound() : View(item);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, PurchaseOrderLine item)
        {
            if (id != item.Id) return NotFound();

            _db.Update(item);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            var item = await _db.PurchaseOrderLines
                .Include(l => l.PurchaseOrder)
                .Include(l => l.Drug)
                .FirstOrDefaultAsync(m => m.Id == id);

            return item == null ? NotFound() : View(item);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _db.PurchaseOrderLines.FindAsync(id);
            _db.PurchaseOrderLines.Remove(item);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
