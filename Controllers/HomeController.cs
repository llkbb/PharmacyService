using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmacyChain.Data;
using PharmacyChain.Models;
using PharmacyChain.ViewModels;

namespace PharmacyChain.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _db;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            // Перенаправити звичайних користувачів на клієнтську головну
            if (User.Identity?.IsAuthenticated == true && !User.IsInRole("Admin") && !User.IsInRole("Pharmacist"))
            {
                return RedirectToAction("Index", "CustomerHome");
            }

            var dashboard = new DashboardViewModel();

            // Статистика
            dashboard.TotalPharmacies = await _db.Pharmacies.CountAsync();
            dashboard.TotalDrugs = await _db.Drugs.CountAsync();
            dashboard.TotalCustomers = await _db.Customers.CountAsync();
            dashboard.TotalSuppliers = await _db.Suppliers.CountAsync();

            // Продажі за сьогодні - ВИПРАВЛЕНО
            var today = DateTime.UtcNow.Date;
            var salesToday = await _db.Sales
                .Where(s => s.CreatedAt.Date == today)
                .Include(s => s.Lines)
                .ToListAsync();
            
            dashboard.SalesToday = salesToday
                .SelectMany(s => s.Lines)
                .Sum(l => l.Quantity * l.UnitPrice);

            // Низькі залишки
            dashboard.LowStockCount = await _db.InventoryItems
                .Include(i => i.Drug)
                .Where(i => i.Quantity <= i.Drug.ReorderLevel)
                .CountAsync();

            // Топ 5 препаратів за продажами - ВИПРАВЛЕНО
            var topDrugsData = await _db.SaleLines
                .GroupBy(l => l.DrugId)
                .Select(g => new { DrugId = g.Key, Quantity = g.Sum(l => l.Quantity) })
                .OrderByDescending(x => x.Quantity)
                .Take(5)
                .ToListAsync();

            var drugIds = topDrugsData.Select(x => x.DrugId).ToList();
            var drugs = await _db.Drugs
                .Where(d => drugIds.Contains(d.Id))
                .ToListAsync();

            dashboard.TopDrugs = topDrugsData
                .Join(drugs,
                    x => x.DrugId,
                    d => d.Id,
                    (x, d) => new TopDrugViewModel { Name = d.Name, Quantity = x.Quantity })
                .ToList();

            // Останні продажі - ВИПРАВЛЕНО
            var recentSalesData = await _db.Sales
                .Include(s => s.Pharmacy)
                .Include(s => s.Customer)
                .Include(s => s.Lines)
                .OrderByDescending(s => s.CreatedAt)
                .Take(5)
                .ToListAsync();

            dashboard.RecentSales = recentSalesData
                .Select(s => new RecentSaleViewModel
                {
                    Id = s.Id,
                    PharmacyName = s.Pharmacy.Name,
                    CustomerName = s.Customer != null ? s.Customer.FullName : "Гість",
                    CreatedAt = s.CreatedAt,
                    Total = s.Lines.Sum(l => l.Quantity * l.UnitPrice)
                })
                .ToList();

            return View(dashboard);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
