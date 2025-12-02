using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmacyChain.Data;
using PharmacyChain.ViewModels;

namespace PharmacyChain.Controllers
{
    [Authorize]
    public class CustomerHomeController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CustomerHomeController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            // Перенаправити Admin та Pharmacist
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "Home");
            }
            else if (User.IsInRole("Pharmacist"))
            {
                return RedirectToAction("Index", "Pharmacist");
            }

            var model = new CustomerDashboardViewModel();

            // Топ 5 препаратів за продажами
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

            model.TopDrugs = topDrugsData
                .Join(drugs,
                    x => x.DrugId,
                    d => d.Id,
                    (x, d) => new TopDrugViewModel { Name = d.Name, Quantity = x.Quantity })
                .ToList();

            // Низькі залишки
            model.LowStockCount = await _db.InventoryItems
                .Include(i => i.Drug)
                .Where(i => i.Quantity <= i.Drug.ReorderLevel)
                .CountAsync();

            return View(model);
        }
    }
}
