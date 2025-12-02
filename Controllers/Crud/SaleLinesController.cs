using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmacyChain.Data;
using PharmacyChain.Models;

namespace PharmacyChain.Controllers.Crud
{
    [Authorize(Roles = "Admin")]
    public class SaleLinesController : Controller
    {
        private readonly ApplicationDbContext _db;

        public SaleLinesController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var lines = await _db.SaleLines
                .Include(l => l.Sale)
                .Include(l => l.Drug)
                .ToListAsync();

            return View(lines);
        }

        public async Task<IActionResult> Details(int id)
        {
            var line = await _db.SaleLines
                .Include(l => l.Sale)
                .Include(l => l.Drug)
                .FirstOrDefaultAsync(l => l.Id == id);

            return line == null ? NotFound() : View(line);
        }

        public IActionResult Create()
        {
            ViewBag.Drugs = _db.Drugs.ToList();
            ViewBag.Sales = _db.Sales.ToList();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(int saleId, int drugId, int quantity)
        {
            if (quantity <= 0)
                ModelState.AddModelError("", "Кількість має бути більшою за 0.");

            var sale = await _db.Sales.FindAsync(saleId);
            if (sale == null)
                ModelState.AddModelError("", "Продаж не знайдено.");

            var drug = await _db.Drugs.FindAsync(drugId);
            if (drug == null)
                ModelState.AddModelError("", "Препарат не знайдено.");

            var item = await _db.InventoryItems
                .FirstOrDefaultAsync(i => i.DrugId == drugId && i.PharmacyId == sale!.PharmacyId);

            if (item == null)
                ModelState.AddModelError("", "У цій аптеці немає такого препарату.");

            else if (item.Quantity < quantity)
                ModelState.AddModelError("", "Недостатньо товару на складі.");

            if (!ModelState.IsValid)
            {
                ViewBag.Drugs = _db.Drugs.ToList();
                ViewBag.Sales = _db.Sales.ToList();
                return View();
            }

            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                var line = new SaleLine
                {
                    SaleId = saleId,
                    DrugId = drugId,
                    Quantity = quantity,
                    UnitPrice = item!.UnitPrice
                };

                _db.SaleLines.Add(line);

                item.Quantity -= quantity;
                _db.InventoryItems.Update(item);

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var line = await _db.SaleLines
                .Include(l => l.Drug)
                .Include(l => l.Sale)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (line == null) return NotFound();

            ViewBag.Drugs = _db.Drugs.ToList();
            ViewBag.Sales = _db.Sales.ToList();

            return View(line);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, SaleLine line)
        {
            if (id != line.Id) return NotFound();

            if (ModelState.IsValid)
            {
                _db.Update(line);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Drugs = _db.Drugs.ToList();
            ViewBag.Sales = _db.Sales.ToList();

            return View(line);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var line = await _db.SaleLines
                .Include(l => l.Drug)
                .Include(l => l.Sale)
                .FirstOrDefaultAsync(l => l.Id == id);

            return line == null ? NotFound() : View(line);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var line = await _db.SaleLines
                .Include(l => l.Sale)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (line == null)
                return NotFound();

            var item = await _db.InventoryItems
                .FirstOrDefaultAsync(i => i.DrugId == line.DrugId && i.PharmacyId == line.Sale.PharmacyId);

            if (item != null)
            {
                item.Quantity += line.Quantity;
                _db.InventoryItems.Update(item);
            }

            _db.SaleLines.Remove(line);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
