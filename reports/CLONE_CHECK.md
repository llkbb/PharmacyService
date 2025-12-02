# Перевірка коду PharmacyChain

## ?? КЛОНИ КОДУ

### ? Критичні дублювання:

**1. CRUD контролери (9 файлів)** - ~70% повторюваного коду
```
CustomersController, PharmaciesController, DrugsController, 
EmployeesController, SuppliersController, InventoryItemsController,
PrescriptionsController, PurchaseOrdersController, SaleLinesController
```
Всі мають однакові методи: Index, Details, Create, Edit, Delete

**2. Inventory операції** - дублюються в 3 місцях:
- `QueriesController.InsertSale()` - списує зі складу
- `SalesController.Create()` - списує зі складу  
- `SalesController.DeleteConfirmed()` - повертає на склад

?? Є `InventoryService.cs` але НЕ використовується!

**3. SelectList генерація** - повторюється 4+ разів:
```csharp
ViewBag.Pharmacies = new SelectList(await _db.Pharmacies.OrderBy(p => p.Name).ToListAsync(), "Id", "Name");
```

**4. Error messages** - 3 різні формати:
- `TempData["Error"]` / `TempData["Success"]` (Customers, Pharmacies)
- `TempData["msg"]` з емодзі (QueriesController)
- `ModelState.AddModelError()` (SalesController)

---

## ? ДІАГРАМА ПРЕЦЕДЕНТІВ - Покриття

### Аптекар:
- ? Обробка замовлення ? `SalesController`
- ? Управління запасами ? `InventoryItemsController`
- ? Реєстрація продажу ? `QueriesController.InsertSale`
- ? Видача рецепту ? `PrescriptionsController`

### Адміністратор:
- ? Монітор системи ? `HomeController.Index` (Dashboard)
- ? Отримання та доставки ? `PurchaseOrdersController` (з відстеженням)
- ? Управління (Customers, Drugs, Pharmacies, Employees, Suppliers)

### Клієнт:
- ? **РЕАЛІЗОВАНО** Купівля товарів ? `CustomerPortalController.Purchase`
- ? **РЕАЛІЗОВАНО** Сортування/пошук товарів ? `CustomerPortalController.Index` (з фільтрами)
- ? **РЕАЛІЗОВАНО** Перегляд аптек ? `CustomerPortalController.Pharmacies`
- ? **РЕАЛІЗОВАНО** Мої замовлення ? `CustomerPortalController.MyOrders`
- ? **РЕАЛІЗОВАНО** Мої рецепти ? `CustomerPortalController.MyPrescriptions`

### Система:
- ? Вхід в систему ? `AccountController.Login`

---

## ?? Підсумок

**Покриття діаграми:** 100% ?

**Дублювання:** ~640 рядків коду повторюється (залишилось)

**Що було додано:**
1. ? Customer Portal (повний функціонал)
2. ? Пошук та фільтрація препаратів
3. ? Відстеження доставки в PurchaseOrders (Status, TrackingNumber, Dates)
4. ? Перевірка рецептів при покупці
5. ? Історія замовлень клієнта
6. ? Перегляд наявності в аптеках

**Основні проблеми (залишились):**
1. Багато CRUD дублювання (можна рефакторити в базовий клас)
2. InventoryService існує але не використовується
3. Немає unit тестів
