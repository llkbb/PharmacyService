using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmacyChain.Data;
using PharmacyChain.Models;

namespace PharmacyChain.Controllers.Crud
{
    [Authorize(Roles = "Admin,Pharmacist")]
    public class SalesController : Controller
    {
        private readonly ApplicationDbContext _db;

        public SalesController(ApplicationDbContext db)
        {
            _db = db;
        }

        // Список продажів
        public async Task<IActionResult> Index()
        {
            var sales = await _db.Sales
                .Include(s => s.Pharmacy)
                .Include(s => s.Customer)
                .Include(s => s.Lines)
                .ToListAsync();

            return View(sales);
        }

        // Деталі продажу
        public async Task<IActionResult> Details(int id)
        {
            var sale = await _db.Sales
                .Include(s => s.Pharmacy)
                .Include(s => s.Customer)
                .Include(s => s.Lines)
                .ThenInclude(l => l.Drug)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sale == null) return NotFound();

            return View(sale);
        }

        // Створення продажу
        public IActionResult Create()
        {
            ViewBag.Drugs = _db.Drugs.ToList();
            ViewBag.Pharmacies = _db.Pharmacies.ToList();
            ViewBag.Customers = _db.Customers.ToList();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(int pharmacyId, int drugId, int quantity, int? customerId)
        {
            if (quantity <= 0)
            {
                ModelState.AddModelError("", "Кількість має бути більшою за 0.");
            }

            var inventoryItem = await _db.InventoryItems
                .FirstOrDefaultAsync(i => i.DrugId == drugId && i.PharmacyId == pharmacyId);

            if (inventoryItem == null)
            {
                ModelState.AddModelError("", "Цього товару нема в цій аптеці.");
            }
            else if (inventoryItem.Quantity < quantity)
            {
                ModelState.AddModelError("", "Недостатньо товару на складі.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Drugs = _db.Drugs.ToList();
                ViewBag.Pharmacies = _db.Pharmacies.ToList();
                ViewBag.Customers = _db.Customers.ToList();
                return View();
            }

            // ACID транзакція
            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                var sale = new Sale
                {
                    PharmacyId = pharmacyId,
                    CustomerId = customerId,
                    CreatedAt = DateTime.Now
                };

                _db.Sales.Add(sale);
                await _db.SaveChangesAsync();

                var saleLine = new SaleLine
                {
                    SaleId = sale.Id,
                    DrugId = drugId,
                    Quantity = quantity,
                    UnitPrice = inventoryItem.UnitPrice
                };

                _db.SaleLines.Add(saleLine);

                // Оновлюємо склад
                inventoryItem.Quantity -= quantity;
                _db.InventoryItems.Update(inventoryItem);

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

        // Видалення продажу
        public async Task<IActionResult> Delete(int id)
        {
            var sale = await _db.Sales
                .Include(s => s.Lines)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sale == null) return NotFound();

            return View(sale);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sale = await _db.Sales
                .Include(s => s.Lines)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sale == null) return NotFound();

            // Повертаємо товар на склад
            foreach (var line in sale.Lines)
            {
                var stock = await _db.InventoryItems
                    .FirstOrDefaultAsync(i => i.DrugId == line.DrugId && i.PharmacyId == sale.PharmacyId);

                if (stock != null)
                {
                    stock.Quantity += line.Quantity;
                    _db.InventoryItems.Update(stock);
                }
            }

            _db.Sales.Remove(sale);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var sale = await _db.Sales
                .Include(s => s.Pharmacy)
                .Include(s => s.Customer)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sale == null) return NotFound();

            ViewBag.Pharmacies = _db.Pharmacies.ToList();
            ViewBag.Customers = _db.Customers.ToList();

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

            ViewBag.Pharmacies = _db.Pharmacies.ToList();
            ViewBag.Customers = _db.Customers.ToList();

            return View(sale);
        }
    }
}
