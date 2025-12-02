using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PharmacyChain.Data;
using PharmacyChain.Models;

namespace PharmacyChain.Controllers.Crud
{
    [Authorize(Roles = "Admin")]

    public class EmployeesController : Controller
    {

        private readonly ApplicationDbContext _db;
        public EmployeesController(ApplicationDbContext db) => _db = db;


        public async Task<IActionResult> Index()
        {
            return View(await _db.Employees.Include(e => e.Pharmacy).ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            var item = await _db.Employees
                .Include(e => e.Pharmacy)
                .FirstOrDefaultAsync(e => e.Id == id);
            return item == null ? NotFound() : View(item);
        }

        public IActionResult Create()
        {
            ViewBag.Pharmacies = new SelectList(_db.Pharmacies, "Id", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Employee item)
        {
            if (ModelState.IsValid)
            {
                _db.Employees.Add(item);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Pharmacies = new SelectList(_db.Pharmacies, "Id", "Name");
            return View(item);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            var item = await _db.Employees.FindAsync(id);
            ViewBag.Pharmacies = new SelectList(_db.Pharmacies, "Id", "Name");
            return View(item);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Employee item)
        {
            if (id != item.Id) return NotFound();

            _db.Update(item);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            var item = await _db.Employees
                .Include(e => e.Pharmacy)
                .FirstOrDefaultAsync(e => e.Id == id);
            return item == null ? NotFound() : View(item);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _db.Employees.FindAsync(id);
            _db.Employees.Remove(item);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
