using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmacyChain.Data;
using PharmacyChain.ViewModels;

namespace PharmacyChain.Controllers
{
    [Authorize(Roles = "Pharmacist")]
    public class PharmacistController : Controller
    {
        private readonly ApplicationDbContext _db;

        public PharmacistController(ApplicationDbContext db)
        {
            _db = db;
        }

        // Dashboard аптекаря
        public async Task<IActionResult> Index()
        {
            var model = new PharmacistDashboardViewModel();

            // Сьогоднішні продажі
            var today = DateTime.UtcNow.Date;
            model.TodaySalesCount = await _db.Sales
                .Where(s => s.CreatedAt.Date == today)
                .CountAsync();

            model.TodaySalesTotal = await _db.Sales
                .Where(s => s.CreatedAt.Date == today)
                .SumAsync(s => (decimal?)s.Lines.Sum(l => l.Quantity * l.UnitPrice)) ?? 0;

            // Низькі залишки
            model.LowStockCount = await _db.InventoryItems
                .Include(i => i.Drug)
                .Where(i => i.Quantity <= i.Drug.ReorderLevel)
                .CountAsync();

            // Кількість аптек
            model.PharmaciesCount = await _db.Pharmacies.CountAsync();

            // Очікуючі рецепти (нова логіка)
            model.PendingPrescriptionsCount = await _db.Prescriptions
                .Where(p => p.Status == "Pending")
                .CountAsync();

            return View(model);
        }

        // Швидкий продаж
        [HttpGet]
        public IActionResult QuickSale()
        {
            ViewBag.Pharmacies = _db.Pharmacies.ToList();
            ViewBag.Drugs = _db.Drugs.ToList();
            ViewBag.Customers = _db.Customers.ToList();
            return View();
        }

        // Мій склад (аптекаря)
        [HttpGet]
        public async Task<IActionResult> MyInventory(int? pharmacyId)
        {
            var query = _db.InventoryItems
                .Include(i => i.Drug)
                .Include(i => i.Pharmacy)
                .AsQueryable();

            if (pharmacyId.HasValue)
            {
                query = query.Where(i => i.PharmacyId == pharmacyId.Value);
            }

            var inventory = await query
                .OrderBy(i => i.Pharmacy.Name)
                .ThenBy(i => i.Drug.Name)
                .ToListAsync();

            ViewBag.Pharmacies = await _db.Pharmacies.ToListAsync();
            ViewBag.SelectedPharmacy = pharmacyId;

            return View(inventory);
        }

        // Активні рецепти
        [HttpGet]
        public async Task<IActionResult> ActivePrescriptions()
        {
            var prescriptions = await _db.Prescriptions
                .Include(p => p.Customer)
                .Include(p => p.Drug)
                .Include(p => p.Pharmacy)
                .Where(p => p.DateIssued.AddDays(p.ValidDays) >= DateTime.UtcNow)
                .OrderByDescending(p => p.DateIssued)
                .ToListAsync();

            return View(prescriptions);
        }

        // Очікуючі на апрув рецепти
        [HttpGet]
        public async Task<IActionResult> PendingPrescriptions()
        {
            var prescriptions = await _db.Prescriptions
                .Include(p => p.Customer)
                .Include(p => p.Drug)
                .Where(p => p.Status == "Pending")
                .OrderBy(p => p.CreatedAt)
                .ToListAsync();

            ViewBag.Pharmacies = await _db.Pharmacies.ToListAsync();

            return View(prescriptions);
        }

        // Апрувнути рецепт
        [HttpPost]
        public async Task<IActionResult> ApprovePrescription(int id, int pharmacyId)
        {
            var prescription = await _db.Prescriptions.FindAsync(id);
            if (prescription == null)
            {
                TempData["Error"] = "Рецепт не знайдено";
                return RedirectToAction(nameof(PendingPrescriptions));
            }

            prescription.Status = "Approved";
            prescription.PharmacyId = pharmacyId;
            prescription.DateIssued = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            TempData["Success"] = "Рецепт затверджено!";
            return RedirectToAction(nameof(PendingPrescriptions));
        }

        // Відхилити рецепт
        [HttpPost]
        public async Task<IActionResult> RejectPrescription(int id, string reason)
        {
            var prescription = await _db.Prescriptions.FindAsync(id);
            if (prescription == null)
            {
                TempData["Error"] = "Рецепт не знайдено";
                return RedirectToAction(nameof(PendingPrescriptions));
            }

            prescription.Status = "Rejected";
            prescription.RejectionReason = reason;

            await _db.SaveChangesAsync();

            TempData["Success"] = "Рецепт відхилено";
            return RedirectToAction(nameof(PendingPrescriptions));
        }

        // Низькі залишки
        [HttpGet]
        public async Task<IActionResult> LowStock()
        {
            var lowStock = await _db.InventoryItems
                .Include(i => i.Drug)
                .Include(i => i.Pharmacy)
                .Where(i => i.Quantity <= i.Drug.ReorderLevel)
                .OrderBy(i => i.Quantity)
                .ToListAsync();

            return View(lowStock);
        }
    }
}
