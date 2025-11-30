using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PharmacyChain.Data;
using PharmacyChain.Models;
using PharmacyChain.ViewModels;

namespace PharmacyChain.Controllers
{
    public class QueriesController : Controller
    {
        private readonly ApplicationDbContext _db;
        public QueriesController(ApplicationDbContext db) => _db = db;

        // Головна сторінка зі списком кнопок
        public IActionResult Index() => View();

        // 1) Фільтрація: рецептурні препарати (Rx)
        public async Task<IActionResult> RxDrugs()
        {
            var data = await _db.Drugs
                .Where(d => d.PrescriptionRequired)
                .OrderBy(d => d.Name)
                .ToListAsync();
            return View(data);
        }

        // 2) Агрегація: сума продажів за період (дата від/до)
        [HttpGet]
        public IActionResult SalesByDate()  // форма з датами
        {
            ViewBag.From = DateTime.UtcNow.AddDays(-7).Date;
            ViewBag.To = DateTime.UtcNow.Date.AddDays(1);
            return View(new List<(DateTime SaleDate, decimal Total)>());
        }

        [HttpPost]
        public async Task<IActionResult> SalesByDate(DateTime from, DateTime to)
        {
            var data = await _db.Sales
                .Where(s => s.CreatedAt >= from && s.CreatedAt < to)
                .SelectMany(s => s.Lines, (s, l) => new { s.CreatedAt, Sum = l.Quantity * l.UnitPrice })
                .GroupBy(x => x.CreatedAt.Date)
                .Select(g => new { SaleDate = g.Key, Total = g.Sum(x => x.Sum) })
                .OrderBy(x => x.SaleDate)
                .ToListAsync();

            ViewBag.From = from; ViewBag.To = to;
            // перетворимо у простий список для View
            var list = data.Select(x => (x.SaleDate, x.Total)).ToList();
            return View(list);
        }

        // 3) JOIN + фільтр: низькі залишки
        public async Task<IActionResult> LowStock()
        {
            var data = await _db.InventoryItems
                .Include(i => i.Pharmacy).Include(i => i.Drug)
                .Where(i => i.Quantity <= i.Drug.ReorderLevel)
                .OrderBy(i => i.Pharmacy.Name).ThenBy(i => i.Drug.Name)
                .ToListAsync();
            return View(data);
        }

        // 4) UPDATE: змінити ціну в конкретній аптеці для конкретного препарату
        [HttpGet]
        public IActionResult UpdatePrice() => View(); // форма

        [HttpPost]
        public async Task<IActionResult> UpdatePrice(int pharmacyId, int drugId, decimal newPrice)
        {
            var rows = await _db.InventoryItems
                .Where(i => i.PharmacyId == pharmacyId && i.DrugId == drugId)
                .ToListAsync();

            rows.ForEach(i => i.UnitPrice = newPrice);
            await _db.SaveChangesAsync();

            TempData["msg"] = $"Оновлено {rows.Count} запис(ів).";
            return RedirectToAction(nameof(UpdatePrice));
        }

        // 5) DELETE: видалити постачальника, якщо немає замовлень
        [HttpGet]
        public IActionResult DeleteSupplier() => View(); // форма

        [HttpPost]
        public async Task<IActionResult> DeleteSupplier(int supplierId)
        {
            var hasOrders = await _db.PurchaseOrders.AnyAsync(p => p.SupplierId == supplierId);
            if (hasOrders)
            {
                TempData["msg"] = "Неможливо видалити: у постачальника є пов'язані замовлення.";
                return RedirectToAction(nameof(DeleteSupplier));
            }

            var s = await _db.Suppliers.FindAsync(supplierId);
            if (s == null)
            {
                TempData["msg"] = "Постачальника не знайдено.";
                return RedirectToAction(nameof(DeleteSupplier));
            }

            _db.Suppliers.Remove(s);
            await _db.SaveChangesAsync();
            TempData["msg"] = "Постачальника видалено.";
            return RedirectToAction(nameof(DeleteSupplier));
        }

        // 6) INSERT: додати продаж (простий чек з 1 позицією)

        [HttpGet]
        public async Task<IActionResult> InsertSale()
        {
            // 1) Аптеки
            var pharmacies = await _db.Pharmacies.OrderBy(p => p.Name).ToListAsync();
            ViewBag.Pharmacies = new SelectList(pharmacies, "Id", "Name");

            // 2) За замовчуванням — показати валідні препарати для ПЕРШОЇ аптеки
            var firstPharmacyId = pharmacies.First().Id;
            var validDrugIds = await _db.InventoryItems
                .Where(i => i.PharmacyId == firstPharmacyId && i.Quantity > 0)
                .Select(i => i.DrugId)
                .ToListAsync();

            var drugs = await _db.Drugs
                .Where(d => validDrugIds.Contains(d.Id))
                .OrderBy(d => d.Name).ToListAsync();
            ViewBag.Drugs = new SelectList(drugs, "Id", "Name");

            return View(new SaleCreateSimpleVm { PharmacyId = firstPharmacyId, Qty = 1 });
        }

    // AJAX-допоміжна: отримати валідні препарати для обраної аптеки
    [HttpGet]
    public async Task<IActionResult> GetDrugsForPharmacy(int pharmacyId)
    {
        var drugs = await _db.InventoryItems
            .Where(i => i.PharmacyId == pharmacyId && i.Quantity > 0)
            .Include(i => i.Drug)
            .OrderBy(i => i.Drug.Name)
            .Select(i => new { i.Drug.Id, i.Drug.Name })
            .ToListAsync();
        return Json(drugs);
    }

    [HttpPost]
    public async Task<IActionResult> InsertSale(SaleCreateSimpleVm vm)
    {
        // Повторно заповнюємо списки, щоб на помилках не ламалось
        ViewBag.Pharmacies = new SelectList(await _db.Pharmacies.OrderBy(p => p.Name).ToListAsync(), "Id", "Name");
        var validDrugs = await _db.InventoryItems
            .Where(i => i.PharmacyId == vm.PharmacyId && i.Quantity > 0)
            .Include(i => i.Drug).OrderBy(i => i.Drug.Name).ToListAsync();
        ViewBag.Drugs = new SelectList(validDrugs.Select(i => i.Drug), "Id", "Name");

        if (!ModelState.IsValid) return View(vm);

        // 1) Перевіряємо, що існує сам препарат і аптека
        if (!await _db.Pharmacies.AnyAsync(p => p.Id == vm.PharmacyId))
        { ModelState.AddModelError("", "Обраної аптеки не існує."); return View(vm); }
        if (!await _db.Drugs.AnyAsync(d => d.Id == vm.DrugId))
        { ModelState.AddModelError("", "Обраного препарату не існує."); return View(vm); }

        // 2) Ключова перевірка: є запис у InventoryItems для цієї пари
        var inventory = await _db.InventoryItems
            .FirstOrDefaultAsync(i => i.PharmacyId == vm.PharmacyId && i.DrugId == vm.DrugId);

        if (inventory == null)
        {
            ModelState.AddModelError("", "У цій аптеці такого препарату немає на складі.");
            return View(vm);
        }
        if (inventory.Quantity < vm.Qty)
        {
            ModelState.AddModelError("", $"Недостатній залишок. Доступно: {inventory.Quantity}.");
            return View(vm);
        }

        // 3) Ціну краще брати з інвентарю (щоб не ввести невалідну)
        var price = vm.Price > 0 ? vm.Price : inventory.UnitPrice;

        // 4) Створюємо продаж + списуємо склад в ОДНІЙ транзакції
        using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            var sale = new Sale { PharmacyId = vm.PharmacyId, CreatedAt = DateTime.UtcNow };
            sale.Lines.Add(new SaleLine { DrugId = vm.DrugId, Quantity = vm.Qty, UnitPrice = price });
            _db.Sales.Add(sale);

            inventory.Quantity -= vm.Qty;

            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            TempData["msg"] = $"Продаж #{sale.Id} додано. Списано {vm.Qty} шт.";
            return RedirectToAction(nameof(InsertSale));
        }
        catch (DbUpdateException ex)
        {
            await tx.RollbackAsync();
            ModelState.AddModelError("", "Помилка збереження: " + ex.GetBaseException().Message);
            return View(vm);
        }
    }

}
}
