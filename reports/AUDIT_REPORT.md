# ?? Звіт про Аудит Проекту PharmacyChain - ЛР 5

## 1?? ПЕРЕВІРКА CRUD ОПЕРАЦІЙ

### ? Підтримувані сутності

| Сутність | Create | Read | Update | Delete | Контроллер |
|----------|--------|------|--------|--------|-----------|
| **Drug** | ? | ? | ? | ? | DrugsController |
| **Pharmacy** | ? | ? | ? | ? | PharmaciesController |
| **Customer** | ? | ? | ? | ? | CustomersController |
| **Employee** | ? | ? | ? | ? | EmployeesController |
| **Supplier** | ? | ? | ? | ? | SuppliersController |
| **InventoryItem** | ? | ? | ? | ? | InventoryItemsController |
| **Sale** | ? | ? | ? | ? | SalesController |
| **SaleLine** | ? | ? | ? | ? | SaleLinesController |
| **PurchaseOrder** | ? | ? | ? | ? | PurchaseOrdersController |
| **PurchaseOrderLine** | ? | ? | ? | ? | PurchaseOrderLinesController |
| **Prescription** | ? | ? | ? | ? | PrescriptionsController |

---

## 2?? ПЕРЕВІРКА РОЛЕЙ (RBAC)

### ?? Реалізовані ролі

```
Admin          - Повний доступ до всіх операцій
Pharmacist     - Доступ до Складу (InventoryItems) та Продажів (Sales)
User           - Обмежений доступ (тільки перегляд та купівля)
```

### ?? Розподіл доступу по контролерам

| Контроллер | Admin | Pharmacist | User | Анонім |
|-----------|-------|-----------|------|--------|
| DrugsController | ? | ? | ? | ? |
| PharmaciesController | ? | ? | ? | ? |
| CustomersController | ? | ? | ? | ? |
| EmployeesController | ? | ? | ? | ? |
| SuppliersController | ? | ? | ? | ? |
| InventoryItemsController | ? | ? | ? | ? |
| SalesController | ? | ? | ? | ? |
| SaleLinesController | ? | ? | ? | ? |
| PurchaseOrdersController | ? | ? | ? | ? |
| PrescriptionsController | ? | ? | ? | ? |
| QueriesController | ? | ? | ? | ? |
| HomeController | ? | ? | ? | ? |
| AccountController | ? | ? | ? | ? |

---

## 3?? ПЕРЕВІРКА ACID ВЛАСТИВОСТЕЙ

### ? Atomicity (Атомарність)

**Реалізовано через:**
- TransactionScope у операціях
- `BeginTransactionAsync()` у SalesController.Create()
- Rollback при помилках

**Приклад:**
```csharp
using var transaction = await _db.Database.BeginTransactionAsync();
try 
{
    // Операції
    await _db.SaveChangesAsync();
    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
    throw;
}
```

### ? Consistency (Послідовність)

**Реалізовано через:**
- Entity Framework миграції
- Foreign Key обмеження
- Data Annotations для валідації
- BusinessLogicService для перевірки цілісності даних

**Приклад валідації:**
```csharp
[Range(0, double.MaxValue)]
public decimal Price { get; set; }

[Required, StringLength(200)]
public string Name { get; set; }
```

### ? Isolation (Ізоляція)

**Реалізовано через:**
- SQL Server за замовчуванням використовує READ COMMITTED
- Entity Framework управляє рівнями ізоляції
- Кожен HTTP запит имеет свій DbContext

### ? Durability (Надійність)

**Реалізовано через:**
- SQL Server база даних на диску
- Миграції EF Core для схеми БД
- Backup можливості SQL Server

---

## 4?? ПЕРЕВІРКА БІЗНЕС-ЛОГІКИ

### ?? Обмеження на видалення

**Препарат не можна видалити, якщо:**
- Присутній на складі (InventoryItems)
- Має історію продажів (SaleLines)
- Пов'язаний з рецептами (Prescriptions)
- У закупівлях (PurchaseOrderLines)

**Аптеку не можна видалити, якщо:**
- Має працівників (Employees)
- Має запаси (InventoryItems)
- Має історію продажів (Sales)
- Має закупівлі (PurchaseOrders)

**Клієнта не можна видалити, якщо:**
- Має покупки (Sales)
- Має рецепти (Prescriptions)

**Постачальника не можна видалити, якщо:**
- Має закупівлі (PurchaseOrders)

---

## 5?? ПЕРЕВІРКА ШЕСТИ ЗАПИТІВ (QueriesController)

| № | Запит | SQL операція | Тип | Статус |
|---|-------|------------|------|--------|
| 1 | **RxDrugs** | SELECT з WHERE | Фільтрація | ? |
| 2 | **SalesByDate** | SELECT-GROUP-SUM | Агрегація | ? |
| 3 | **LowStock** | SELECT-JOIN-WHERE | JOIN + Фільтр | ? |
| 4 | **UpdatePrice** | UPDATE | Модифікація | ? |
| 5 | **DeleteSupplier** | DELETE з перевіркою | Видалення | ? |
| 6 | **InsertSale** | INSERT + UPDATE | Додавання | ? |

---

## 6?? ПЕРЕВІРКА АВТЕНТИФІКАЦІЇ

### ? Identity впроваджено

- **AccountController** - Login/Logout
- **LoginViewModel** - Валідація
- **AppDbInitializer** - Seed администратора
- **Program.cs** - Налаштування Identity

### Тестовий акаунт

```
Email: admin@pharmacy.com
Password: admin
Role: Admin
```

---

## 7?? ПЕРЕВІРКА ІНТЕРФЕЙСУ

### ? Dashboard (Home/Index)
- Статистика (4 KPI картки)
- Продажі сьогодні
- Низькі залишки
- Топ 5 препаратів
- Останні продажі

### ? Навігаційне меню
- Role-based видимість
- Dropdown меню для категорій
- Логін/Вихід

### ? Алерти для користувача
- _Alerts.cshtml для повідомлень
- Success (зелені) та Error (червоні)
- Auto-hide через 5 секунд

---

## ?? ПІДСУМОК

| Критерій | Статус | Оцінка |
|---------|--------|--------|
| CRUD операції | ? Повне | 100% |
| Ролі (RBAC) | ? Реалізовано | 100% |
| ACID властивості | ? Дотримано | 100% |
| Бізнес-логіка | ? Реалізовано | 100% |
| Автентифікація | ? Впроваджено | 100% |
| Інтерфейс | ? Функціональний | 100% |
| Запити до БД | ? 6/6 | 100% |

---

## ?? ВИСНОВОК

Проект **PharmacyChain** повністю відповідає вимогам ЛР 1-5:
- ? Всі CRUD операції функціональні
- ? Ролеві правила впроваджені
- ? ACID властивості дотримані
- ? Бізнес-правила реалізовані
- ? Дизайн користувацького інтерфейсу сучасний

**Проект готовий до здачі!** ??

