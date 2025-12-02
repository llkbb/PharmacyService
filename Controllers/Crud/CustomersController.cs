using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmacyChain.Data;
using PharmacyChain.Exceptions;
using PharmacyChain.Models;
using PharmacyChain.Services;



namespace PharmacyChain.Controllers.Crud
{
    [Authorize(Roles = "Admin")]
    public class CustomersController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly BusinessLogicService _businessLogic;

        public CustomersController(ApplicationDbContext db, BusinessLogicService businessLogic)
        {
            _db = db;
            _businessLogic = businessLogic;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _db.Customers.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            var item = await _db.Customers.FirstOrDefaultAsync(m => m.Id == id);
            return item == null ? NotFound() : View(item);
        }

        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(Customer item)
        {
            if (ModelState.IsValid)
            {
                _db.Customers.Add(item);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(item);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            var item = await _db.Customers.FindAsync(id);
            return item == null ? NotFound() : View(item);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Customer item)
        {
            if (id != item.Id) return NotFound();

            _db.Update(item);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            var item = await _db.Customers.FirstOrDefaultAsync(m => m.Id == id);
            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _db.Customers.FindAsync(id);
            if (item == null)
            {
                TempData["Error"] = "Клієнта не знайдено.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                // Перевірити бізнес-правила перед видаленням
                await _businessLogic.ValidateCanDeleteCustomerAsync(id);

                _db.Customers.Remove(item);
                await _db.SaveChangesAsync();

                TempData["Success"] = "Клієнта успішно видалено.";
                return RedirectToAction(nameof(Index));
            }
            catch (BusinessLogicException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Delete), new { id });
            }
        }
    }
}
