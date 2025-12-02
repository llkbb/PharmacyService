# ? QUICK START GUIDE - PharmacyChain

## ?? 3 Кроки до запуску

### Крок 1: Встановлення
```bash
git clone https://github.com/llkbb/PharmacyService.git
cd PharmacyChain
dotnet restore
```

### Крок 2: База даних
```bash
# У Visual Studio Package Manager Console:
Update-Database

# Або через CLI:
dotnet ef database update
```

### Крок 3: Запуск
```bash
dotnet run
# https://localhost:7299
```

---

## ?? Логін (выберите один)

| Роль | Email | Пароль |
|------|-------|--------|
| **Admin** | admin@pharmacy.com | Admin123! |
| **Pharmacist** | pharmacist@pharmacy.com | Pharm123! |
| **User** | user@pharmacy.com | User123! |

---

## ?? Основні маршрути

```
HOME:         /Home/Index            (Dashboard)
DRUGS:        /Drugs/Index           (Препарати)
PHARMACIES:   /Pharmacies/Index      (Аптеки)
CUSTOMERS:    /Customers/Index       (Клієнти)
INVENTORY:    /InventoryItems/Index  (Склад)
SALES:        /Sales/Index           (Продажи)
QUERIES:      /Queries/Index         (6 запитів)
```

---

## ? Що перевірити

### ? CRUD операції
1. Додати препарат: `/Drugs/Create`
2. Редагувати аптеку: `/Pharmacies/Edit/1`
3. Видалити клієнта: `/Customers/Delete/1`
4. Переглянути продажи: `/Sales/Index`

### ? Бізнес-логіка
1. Спробувати видалити препарат на складі ? помилка
2. Спробувати видалити аптеку з продажами ? помилка

### ? Трансакції
1. Перейти на `/Queries/InsertSale`
2. Додати продаж ? автоматично зменшується склад

### ? Запити
1. `/Queries/RxDrugs` - рецептурні препарати
2. `/Queries/SalesByDate` - продажи за період
3. `/Queries/LowStock` - низькі залишки
4. `/Queries/UpdatePrice` - оновити ціну
5. `/Queries/DeleteSupplier` - видалити постачальника
6. `/Queries/InsertSale` - додати продаж

---

## ?? Структура БД

```
DRUGS ??????????
                ???> SALE_LINES
INVENTORY ??????          ???> SALES ??????> CUSTOMERS
PHARMACY ???????          ???> PURCHASE_ORDER_LINES
SUPPLIER ???????          ???> PURCHASE_ORDERS
EMPLOYEES
PRESCRIPTIONS
```

---

## ?? Структура коду

| Папка | Пропозиція |
|-------|-----------|
| `Controllers/Crud/` | 11 CRUD контролерів |
| `Models/` | 11 моделей даних |
| `Views/` | Razor Pages представлення |
| `Services/` | Бізнес-логіка |
| `Data/` | DbContext, Миграції |
| `Exceptions/` | Користувацькі виключення |

---

## ?? Ролі доступу

| Функція | Admin | Pharmacist | User |
|---------|-------|-----------|------|
| CRUD Препарати | ? | ? | ? |
| CRUD Аптеки | ? | ? | ? |
| CRUD Клієнти | ? | ? | ? |
| Склад | ? | ? | ? |
| Продажи | ? | ? | ? |
| Dashboard | ? | ? | ? |

---

## ?? ACID на практиці

### Atomicity
```csharp
// SalesController.Create()
using var transaction = await _db.Database.BeginTransactionAsync();
try {
    // Операції
    await transaction.CommitAsync();
} catch {
    await transaction.RollbackAsync();
}
```

### Consistency
```csharp
// Model validations
[Range(0, double.MaxValue)]
public decimal Price { get; set; }

[Required]
public string Name { get; set; }
```

### Isolation
- SQL Server READ COMMITTED

### Durability
- SQL Server persistence

---

## ?? Усунення неполадок

### БД не оновилася
```bash
drop database PharmacyChain
Update-Database
```

### 404 помилки
- Перевірте маршрути в `Program.cs`
- Використовуйте `asp-controller` та `asp-action`

### Unauthorized (401)
- Заголіки/Аліби
- Логін з admin обліком

### Database locked
- Закрийте всі з'єднання
- Перезавантажте Visual Studio

---

## ?? Документація

| Файл | Мета |
|------|------|
| `README.md` | Основна інструкція |
| `AUDIT_REPORT.md` | Детальний аудит |
| `TEST_SCENARIOS.md` | Тестові сценарії |
| `FINAL_REPORT.md` | Звіт про ЛР 5 |
| `CHECKLIST.md` | Чеклист вимог |
| `PROJECT_SUMMARY.md` | Резюме проекту |
| `QUICK_START.md` | Цей файл |

---

## ?? Контакт

Розроблено для ЛР 1-5 курсу ООП

---

## ? Ключові цифри

```
? 11 CRUD контролерів
? 11 моделей даних
? 50+ представлень
? 3 ролі доступу
? 6 SQL запитів
? 100% ACID
? 2000+ ліній коду
? 100% готовності
```

---

**Ready to demo! ??**

