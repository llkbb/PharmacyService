using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmacyChain.Data;
using PharmacyChain.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PharmacyChain.Controllers.Crud
{
    public class SalesController : Controller
    {
        private readonly ApplicationDbContext _db;
        public SalesController(ApplicationDbContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            var sales = await _db.Sales
                .Include(s => s.Pharmacy)
                .Include(s => s.Customer)
                .ToListAsync();
            return View(sales);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var sale = await _db.Sales
                .Include(s => s.Pharmacy)
                .Include(s => s.Customer)
                .Include(s => s.Lines)
                .ThenInclude(l => l.Drug)
                .FirstOrDefaultAsync(m => m.Id == id);

            return sale == null ? NotFound() : View(sale);
        }

        public IActionResult Create()
        {
            ViewBag.Pharmacies = new SelectList(_db.Pharmacies, "Id", "Name");
            ViewBag.Customers = new SelectList(_db.Customers, "Id", "FullName");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Sale sale)
        {
            sale.CreatedAt = DateTime.UtcNow;

            if (ModelState.IsValid)
            {
                _db.Sales.Add(sale);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(sale);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            var sale = await _db.Sales.FindAsync(id);
            if (sale == null) return NotFound();

            ViewBag.Pharmacies = new SelectList(_db.Pharmacies, "Id", "Name");
            ViewBag.Customers = new SelectList(_db.Customers, "Id", "FullName");

            return View(sale);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Sale sale)
        {
            if (id != sale.Id) return NotFound();

            if (ModelState.IsValid)
            {
                _db.Update(sale);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(sale);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            var item = await _db.Sales
                .Include(s => s.Pharmacy)
                .FirstOrDefaultAsync(m => m.Id == id);
            return item == null ? NotFound() : View(item);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sale = await _db.Sales.FindAsync(id);
            _db.Sales.Remove(sale);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
