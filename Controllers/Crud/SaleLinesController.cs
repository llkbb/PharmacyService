using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmacyChain.Data;
using PharmacyChain.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PharmacyChain.Controllers.Crud
{
    public class SaleLinesController : Controller
    {
        private readonly ApplicationDbContext _db;
        public SaleLinesController(ApplicationDbContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            var lines = _db.SaleLines
                .Include(l => l.Sale)
                .Include(l => l.Drug);
            return View(await lines.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            var item = await _db.SaleLines
                .Include(l => l.Sale)
                .Include(l => l.Drug)
                .FirstOrDefaultAsync(m => m.Id == id);
            return item == null ? NotFound() : View(item);
        }

        public IActionResult Create()
        {
            ViewBag.Sales = new SelectList(_db.Sales, "Id", "Id");
            ViewBag.Drugs = new SelectList(_db.Drugs, "Id", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(SaleLine item)
        {
            if (ModelState.IsValid)
            {
                _db.SaleLines.Add(item);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(item);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            var item = await _db.SaleLines.FindAsync(id);

            ViewBag.Sales = new SelectList(_db.Sales, "Id", "Id");
            ViewBag.Drugs = new SelectList(_db.Drugs, "Id", "Name");

            return item == null ? NotFound() : View(item);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, SaleLine item)
        {
            if (id != item.Id) return NotFound();

            _db.Update(item);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            var item = await _db.SaleLines
                .Include(l => l.Drug)
                .Include(l => l.Sale)
                .FirstOrDefaultAsync(m => m.Id == id);

            return item == null ? NotFound() : View(item);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _db.SaleLines.FindAsync(id);
            _db.SaleLines.Remove(item);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
