using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmacyChain.Data;
using PharmacyChain.Models;
using PharmacyChain.ViewModels;

namespace PharmacyChain.Controllers
{
    [Authorize]
    public class CustomerPortalController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CustomerPortalController(ApplicationDbContext db)
        {
            _db = db;
        }

        // Головна сторінка - каталог препаратів
        public async Task<IActionResult> Index(string? search, string? category, bool? prescriptionRequired, int? pharmacyId)
        {
            // Перенаправити Admin та Pharmacist на їх панелі
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "Home");
            }
            else if (User.IsInRole("Pharmacist"))
            {
                return RedirectToAction("Index", "Pharmacist");
            }

            var query = _db.Drugs.AsQueryable();

            // Фільтрація за пошуком
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(d => d.Name.Contains(search) || d.Description.Contains(search));
                ViewBag.Search = search;
            }

            // Фільтрація за категорією
            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(d => d.Category == category);
                ViewBag.Category = category;
            }

            // Фільтрація за рецептурністю
            if (prescriptionRequired.HasValue)
            {
                query = query.Where(d => d.PrescriptionRequired == prescriptionRequired.Value);
                ViewBag.PrescriptionRequired = prescriptionRequired.Value;
            }

            var drugs = await query.OrderBy(d => d.Name).ToListAsync();

            // Отримуємо список категорій для фільтру
            ViewBag.Categories = await _db.Drugs
                .Select(d => d.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            // Отримуємо список аптек
            ViewBag.Pharmacies = await _db.Pharmacies
                .OrderBy(p => p.Name)
                .ToListAsync();

            ViewBag.SelectedPharmacyId = pharmacyId;

            return View(drugs);
        }

        // Деталі препарату з наявністю в аптеках
        public async Task<IActionResult> DrugDetails(int id)
        {
            var drug = await _db.Drugs.FindAsync(id);
            if (drug == null) return NotFound();

            // Отримуємо наявність в аптеках
            var availability = await _db.InventoryItems
                .Include(i => i.Pharmacy)
                .Where(i => i.DrugId == id && i.Quantity > 0)
                .OrderBy(i => i.Pharmacy.Name)
                .ToListAsync();

            ViewBag.Availability = availability;

            return View(drug);
        }

        // Мої замовлення (історія покупок)
        [HttpGet]
        public async Task<IActionResult> MyOrders()
        {
            // Отримуємо email поточного користувача
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login", "Account");
            }

            // Знаходимо або створюємо клієнта
            var customer = await GetOrCreateCustomerAsync(userEmail);

            // Отримуємо всі замовлення клієнта
            var orders = await _db.Sales
                .Include(s => s.Pharmacy)
                .Include(s => s.Lines)
                .ThenInclude(l => l.Drug)
                .Where(s => s.CustomerId == customer.Id)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            ViewBag.CustomerName = customer.FullName;

            return View(orders);
        }

        // Мої рецепти
        [HttpGet]
        public async Task<IActionResult> MyPrescriptions()
        {
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login", "Account");
            }

            var customer = await GetOrCreateCustomerAsync(userEmail);

            var prescriptions = await _db.Prescriptions
                .Include(p => p.Drug)
                .Include(p => p.Pharmacy)
                .Where(p => p.CustomerId == customer.Id)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            ViewBag.CustomerName = customer.FullName;

            return View(prescriptions);
        }

        // Завантажити рецепт (новий)
        [HttpGet]
        public async Task<IActionResult> UploadPrescription()
        {
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.Drugs = await _db.Drugs
                .Where(d => d.PrescriptionRequired)
                .OrderBy(d => d.Name)
                .ToListAsync();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadPrescription(IFormFile? photo, string doctorName, int? drugId, string? notes)
        {
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login", "Account");
            }

            if (string.IsNullOrWhiteSpace(doctorName))
            {
                TempData["Error"] = "Вкажіть ім'я лікаря";
                return RedirectToAction(nameof(UploadPrescription));
            }

            var customer = await GetOrCreateCustomerAsync(userEmail);

            string? photoPath = null;
            if (photo != null && photo.Length > 0)
            {
                // Збереження фото
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "prescriptions");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(photo.FileName)}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await photo.CopyToAsync(stream);
                }

                photoPath = $"/uploads/prescriptions/{uniqueFileName}";
            }

            var prescription = new Prescription
            {
                CustomerId = customer.Id,
                DrugId = drugId,
                DoctorName = doctorName,
                Notes = notes,
                PhotoPath = photoPath,
                DateIssued = DateTime.UtcNow,
                ValidDays = 30,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            _db.Prescriptions.Add(prescription);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Рецепт успішно завантажено! Очікуйте перевірки аптекарем.";
            return RedirectToAction(nameof(MyPrescriptions));
        }

        // Купівля препарату
        [HttpGet]
        public async Task<IActionResult> Purchase(int drugId, int? pharmacyId)
        {
            var drug = await _db.Drugs.FindAsync(drugId);
            if (drug == null) return NotFound();

            // Отримуємо наявність в аптеках
            var availability = await _db.InventoryItems
                .Include(i => i.Pharmacy)
                .Where(i => i.DrugId == drugId && i.Quantity > 0)
                .ToListAsync();

            if (!availability.Any())
            {
                TempData["Error"] = "Цей препарат наразі відсутній в усіх аптеках.";
                return RedirectToAction(nameof(DrugDetails), new { id = drugId });
            }

            var model = new CustomerPurchaseViewModel
            {
                DrugId = drugId,
                DrugName = drug.Name,
                PharmacyId = pharmacyId ?? availability.First().PharmacyId,
                Quantity = 1
            };

            ViewBag.Availability = availability;
            ViewBag.Drug = drug;

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Purchase(CustomerPurchaseViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var drugData = await _db.Drugs.FindAsync(model.DrugId);
                ViewBag.Drug = drugData;
                ViewBag.Availability = await _db.InventoryItems
                    .Include(i => i.Pharmacy)
                    .Where(i => i.DrugId == model.DrugId && i.Quantity > 0)
                    .ToListAsync();
                return View(model);
            }

            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login", "Account");
            }

            // Автоматично створюємо або отримуємо профіль клієнта
            var customer = await GetOrCreateCustomerAsync(userEmail);

            // Перевірка наявності на складі
            var inventory = await _db.InventoryItems
                .FirstOrDefaultAsync(i => i.PharmacyId == model.PharmacyId && i.DrugId == model.DrugId);

            if (inventory == null)
            {
                TempData["Error"] = "Препарат відсутній в обраній аптеці.";
                return RedirectToAction(nameof(Purchase), new { drugId = model.DrugId });
            }

            if (inventory.Quantity < model.Quantity)
            {
                TempData["Error"] = $"Недостатня кількість на складі. Доступно: {inventory.Quantity} шт.";
                return RedirectToAction(nameof(Purchase), new { drugId = model.DrugId });
            }

            // Перевірка рецептурності
            var drugInfo = await _db.Drugs.FindAsync(model.DrugId);
            if (drugInfo!.PrescriptionRequired)
            {
                // Перевірка наявності дійсного рецепту
                var validPrescription = await _db.Prescriptions
                    .Where(p => p.CustomerId == customer.Id 
                                && p.DrugId == model.DrugId 
                                && p.DateIssued.AddDays(p.ValidDays) >= DateTime.UtcNow)
                    .FirstOrDefaultAsync();

                if (validPrescription == null)
                {
                    TempData["Error"] = "Для купівлі цього препарату потрібен дійсний рецепт.";
                    return RedirectToAction(nameof(DrugDetails), new { id = model.DrugId });
                }
            }

            // Створюємо продаж
            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                var sale = new Sale
                {
                    PharmacyId = model.PharmacyId,
                    CustomerId = customer.Id,
                    CreatedAt = DateTime.UtcNow,
                    PaymentMethod = model.PaymentMethod,
                    PaymentStatus = model.PaymentMethod == "Cash" ? "Pending" : "Paid",
                    PaidAt = model.PaymentMethod != "Cash" ? DateTime.UtcNow : null,
                    TransactionId = model.PaymentMethod != "Cash" ? Guid.NewGuid().ToString("N").Substring(0, 12).ToUpper() : null
                };

                sale.Lines.Add(new SaleLine
                {
                    DrugId = model.DrugId,
                    Quantity = model.Quantity,
                    UnitPrice = inventory.UnitPrice
                });

                _db.Sales.Add(sale);

                // Списуємо зі складу
                inventory.Quantity -= model.Quantity;
                _db.InventoryItems.Update(inventory);

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                var paymentInfo = sale.PaymentStatus == "Paid" 
                    ? $"Оплачено {sale.PaymentMethod} (ID: {sale.TransactionId})" 
                    : "Оплата при отриманні";

                TempData["Success"] = $"Замовлення #{sale.Id} успішно створено! Сума: {sale.Total:F2} грн. {paymentInfo}";
                return RedirectToAction(nameof(MyOrders));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = "Помилка при оформленні замовлення: " + ex.Message;
                return RedirectToAction(nameof(Purchase), new { drugId = model.DrugId });
            }
        }

        // Пошук аптек
        public async Task<IActionResult> Pharmacies(string? search)
        {
            var query = _db.Pharmacies.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p => p.Name.Contains(search) || (p.Address != null && p.Address.Contains(search)));
                ViewBag.Search = search;
            }

            var pharmacies = await query.OrderBy(p => p.Name).ToListAsync();

            return View(pharmacies);
        }

        // Наявність товарів в аптеці
        public async Task<IActionResult> PharmacyInventory(int id)
        {
            var pharmacy = await _db.Pharmacies.FindAsync(id);
            if (pharmacy == null) return NotFound();

            var inventory = await _db.InventoryItems
                .Include(i => i.Drug)
                .Where(i => i.PharmacyId == id && i.Quantity > 0)
                .OrderBy(i => i.Drug.Name)
                .ToListAsync();

            ViewBag.Pharmacy = pharmacy;

            return View(inventory);
        }

        // Допоміжний метод для отримання або створення клієнта
        private async Task<Customer> GetOrCreateCustomerAsync(string email)
        {
            var customer = await _db.Customers.FirstOrDefaultAsync(c => c.Email == email);

            if (customer == null)
            {
                // Автоматично створюємо профіль клієнта
                customer = new Customer
                {
                    Email = email,
                    FullName = User.Identity?.Name ?? email,
                    Phone = ""
                };

                _db.Customers.Add(customer);
                await _db.SaveChangesAsync();
            }

            return customer;
        }

        // Профіль клієнта
        [HttpGet]
        public async Task<IActionResult> MyProfile()
        {
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login", "Account");
            }

            var customer = await GetOrCreateCustomerAsync(userEmail);
            return View(customer);
        }

        [HttpPost]
        public async Task<IActionResult> MyProfile(Customer model)
        {
            var userEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login", "Account");
            }

            var customer = await _db.Customers.FirstOrDefaultAsync(c => c.Email == userEmail);
            if (customer == null)
            {
                TempData["Error"] = "Профіль не знайдено";
                return RedirectToAction(nameof(MyProfile));
            }

            // Оновлюємо тільки дозволені поля
            customer.FullName = model.FullName;
            customer.Phone = model.Phone;
            // Email не змінюємо - він прив'язаний до облікового запису

            await _db.SaveChangesAsync();

            TempData["Success"] = "Профіль успішно оновлено";
            return RedirectToAction(nameof(MyProfile));
        }
    }
}
