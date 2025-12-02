using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PharmacyChain.Data;
using PharmacyChain.Models;

namespace PharmacyChain.Controllers.Crud
{
    [Authorize(Roles = "Admin,Pharmacist")]

    public class PurchaseOrdersController : Controller
    {
        private readonly ApplicationDbContext _db;
        public PurchaseOrdersController(ApplicationDbContext db) => _db = db;

        public async Task<IActionResult> Index(string? status)
        {
            var query = _db.PurchaseOrders
                .Include(p => p.Supplier)
                .Include(p => p.Pharmacy)
                .AsQueryable();

            // Фільтрація за статусом
            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(p => p.Status == status);
                ViewBag.Status = status;
            }

            ViewBag.Statuses = new[] { "Draft", "Sent", "InTransit", "Delivered", "Cancelled" };

            return View(await query.OrderByDescending(p => p.CreatedAt).ToListAsync());
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

            ViewBag.Suppliers = new SelectList(_db.Suppliers, "Id", "Name");
            ViewBag.Pharmacies = new SelectList(_db.Pharmacies, "Id", "Name");
            return View(item);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            var item = await _db.PurchaseOrders.FindAsync(id);

            ViewBag.Suppliers = new SelectList(_db.Suppliers, "Id", "Name");
            ViewBag.Pharmacies = new SelectList(_db.Pharmacies, "Id", "Name");
            ViewBag.Statuses = new SelectList(new[] { "Draft", "Sent", "InTransit", "Delivered", "Cancelled" });

            return item == null ? NotFound() : View(item);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, PurchaseOrder item)
        {
            if (id != item.Id) return NotFound();

            // Автоматичне встановлення дат при зміні статусу
            var originalOrder = await _db.PurchaseOrders.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
            
            if (originalOrder != null)
            {
                if (item.Status == "Sent" && originalOrder.Status != "Sent" && !item.SentDate.HasValue)
                {
                    item.SentDate = DateTime.UtcNow;
                }
                
                if (item.Status == "Delivered" && originalOrder.Status != "Delivered" && !item.DeliveryDate.HasValue)
                {
                    item.DeliveryDate = DateTime.UtcNow;
                }
            }

            _db.Update(item);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Замовлення оновлено";
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
            if (item != null)
            {
                _db.PurchaseOrders.Remove(item);
                await _db.SaveChangesAsync();
                TempData["Success"] = "Замовлення видалено";
            }
            return RedirectToAction(nameof(Index));
        }

        // Швидка зміна статусу
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string newStatus)
        {
            var order = await _db.PurchaseOrders.FindAsync(id);
            if (order == null) return NotFound();

            order.Status = newStatus;

            // Автоматичне встановлення дат
            if (newStatus == "Sent" && !order.SentDate.HasValue)
            {
                order.SentDate = DateTime.UtcNow;
            }
            else if (newStatus == "Delivered" && !order.DeliveryDate.HasValue)
            {
                order.DeliveryDate = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();

            TempData["Success"] = $"Статус змінено на '{newStatus}'";
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
