# ?? Виправлення DbContext Concurrency Exception

## ? Проблема:

```
InvalidOperationException: A second operation was started on this context 
instance before a previous operation completed. This is usually caused by 
different threads concurrently using the same instance of DbContext.
```

## ?? Причина:

**DbContext НЕ є потокобезпечним (not thread-safe)!**

Навіть async операції (`Task.WhenAll`) не можна виконувати паралельно на одному екземплярі DbContext.

---

## ? Що було (НЕПРАВИЛЬНО):

### Варіант 1: Паралельні async запити
```csharp
// ? ПОМИЛКА: Task.WhenAll використовує один _db з кількох Task
var salesTask = _db.Sales.Take(recordCount).ToListAsync();
var pharmaciesTask = _db.Pharmacies.ToListAsync();
var customersTask = _db.Customers.ToListAsync();
var drugsTask = _db.Drugs.ToListAsync();

await Task.WhenAll(salesTask, pharmaciesTask, customersTask, drugsTask);
// Exception: DbContext вже зайнятий іншим запитом!
```

### Варіант 2: Parallel.ForEach з DbContext
```csharp
// ? ПОМИЛКА: Доступ до _db з кількох потоків
Parallel.ForEach(sales, sale =>
{
    sale.Pharmacy = _db.Pharmacies.Find(sale.PharmacyId); // Crash!
});
```

---

## ? Що стало (ПРАВИЛЬНО):

### Концепція: **2 етапи**

#### **Етап 1**: Завантажити ВСІ дані з БД (ПОСЛІДОВНО)
```csharp
// ? Послідовне завантаження (один запит за раз)
var sales = await _db.Sales.Take(recordCount).ToListAsync();
var pharmacies = await _db.Pharmacies.ToDictionaryAsync(p => p.Id);
var customers = await _db.Customers.ToDictionaryAsync(c => c.Id);
var drugs = await _db.Drugs.ToDictionaryAsync(d => d.Id);
var saleLines = await _db.SaleLines
    .Where(sl => saleIds.Contains(sl.SaleId))
    .ToListAsync();
```

#### **Етап 2**: Обробити дані паралельно (БЕЗ DbContext)
```csharp
// ? Паралельна обробка в пам'яті (БЕЗ звернень до БД)
Parallel.ForEach(sales, sale =>
{
    sale.Pharmacy = pharmacies[sale.PharmacyId]; // Словник thread-safe
    sale.Customer = customers[sale.CustomerId.Value];
    // ...
});
```

---

## ?? Виправлені методи:

### 1. `TestParallel()` - невелика вибірка
**Було:**
```csharp
var salesTask = _db.Sales.Take(recordCount).ToListAsync();
var pharmaciesTask = _db.Pharmacies.ToListAsync();
await Task.WhenAll(salesTask, pharmaciesTask, ...); // ?
```

**Стало:**
```csharp
var sales = await _db.Sales.Take(recordCount).ToListAsync();
var pharmacies = await _db.Pharmacies.ToListAsync();
// Потім паралельна обробка без _db ?
Parallel.ForEach(sales, sale => { ... });
```

---

### 2. `TestLargeParallel()` - велика вибірка
**Було:**
```csharp
await Task.WhenAll(salesTask, pharmaciesTask, ...); // ?
foreach (var sale in sales) { ... } // Послідовна обробка
```

**Стало:**
```csharp
// Послідовне завантаження
var sales = await _db.Sales.Take(recordCount).ToListAsync();
var pharmacies = await _db.Pharmacies.ToDictionaryAsync(p => p.Id);
// ...

// Паралельна обробка
Parallel.ForEach(sales, sale => { ... }); // ?
```

---

### 3. `TestParallelForEach()` - Parallel.ForEach
**Було:**
```csharp
var sales = await _db.Sales.Take(recordCount).ToListAsync();
var pharmacies = await _db.Pharmacies.ToDictionaryAsync(p => p.Id);
// Все ОК ? (вже було правильно)
```

---

### 4. `TestTPL()` - Task Parallel Library
**Було:**
```csharp
var sales = await _db.Sales.Take(recordCount).ToListAsync();
var pharmacies = await _db.Pharmacies.ToDictionaryAsync(p => p.Id);
// Все ОК ? (вже було правильно)
```

---

### 5. `TestPLINQ()` - Parallel LINQ
**Було:**
```csharp
var sales = await _db.Sales.Take(recordCount).ToListAsync();
var pharmacies = await _db.Pharmacies.ToDictionaryAsync(p => p.Id);
// Все ОК ? (вже було правильно)
```

---

## ?? Правила роботи з DbContext:

### ? МОЖНА:
1. Послідовні async запити:
   ```csharp
   var sales = await _db.Sales.ToListAsync();
   var drugs = await _db.Drugs.ToListAsync();
   ```

2. Паралельна обробка даних БЕЗ DbContext:
   ```csharp
   Parallel.ForEach(sales, sale => { ... });
   ```

3. Використання `Dictionary` для швидкого пошуку:
   ```csharp
   var pharmacies = await _db.Pharmacies.ToDictionaryAsync(p => p.Id);
   sale.Pharmacy = pharmacies[sale.PharmacyId]; // O(1)
   ```

### ? НЕМОЖНА:
1. `Task.WhenAll` з одним DbContext:
   ```csharp
   await Task.WhenAll(
       _db.Sales.ToListAsync(),
       _db.Drugs.ToListAsync()
   ); // ? Exception!
   ```

2. Доступ до `_db` з `Parallel.ForEach`:
   ```csharp
   Parallel.ForEach(sales, sale =>
   {
       sale.Drug = _db.Drugs.Find(sale.DrugId); // ? Exception!
   });
   ```

3. PLINQ з DbContext:
   ```csharp
   sales.AsParallel().Select(s => 
   {
       s.Drug = _db.Drugs.Find(s.DrugId); // ? Exception!
       return s;
   });
   ```

---

## ?? Альтернативні рішення (не використано):

### Рішення 1: DbContext на кожен потік
```csharp
// ? Працює, але складно та неефективно
Parallel.ForEach(sales, sale =>
{
    using var db = new ApplicationDbContext(options);
    sale.Pharmacy = db.Pharmacies.Find(sale.PharmacyId);
});
```
**Проблеми:**
- Багато з'єднань до БД
- Overhead на створення контексту
- Складність передачі `options`

---

### Рішення 2: Скопіювати дані
```csharp
// ? Працює
var salesCopy = sales.Select(s => new Sale { ... }).ToList();
Parallel.ForEach(salesCopy, sale => { ... });
```
**Проблеми:**
- Подвоєння пам'яті
- Складність копіювання

---

## ? Обране рішення:

**2-етапний підхід:**
1. Завантажити ВСІ дані з БД послідовно ? `List`/`Dictionary`
2. Обробити дані паралельно БЕЗ звернень до БД

**Переваги:**
- ? Швидко (Dictionary lookup = O(1))
- ? Безпечно (thread-safe)
- ? Просто (зрозумілий код)
- ? Ефективно (мінімум запитів до БД)

---

## ?? Порівняння:

| Підхід | DbContext безпека | Швидкість | Складність |
|--------|-------------------|-----------|------------|
| Task.WhenAll з одним DbContext | ? Exception | - | Легко |
| Послідовні запити + Parallel обробка | ? OK | ??? | Легко |
| DbContext на потік | ? OK | ? | Складно |
| Копіювання даних | ? OK | ?? | Середньо |

**Обрано:** Послідовні запити + Parallel обробка ?

---

## ?? Перевірка:

Після виправлення всі тести працюють БЕЗ помилок:

1. ? `TestSequential()` - послідовний
2. ? `TestParallel()` - паралельний (мала вибірка)
3. ? `TestLargeSequential()` - послідовний (велика)
4. ? `TestLargeParallel()` - паралельний (велика)
5. ? `TestParallelForEach()` - Parallel.ForEach
6. ? `TestTPL()` - TPL
7. ? `TestPLINQ()` - PLINQ

---

## ?? Висновок:

**Головне правило:**
> DbContext НЕ є потокобезпечним! Завантажуй дані ПОСЛІДОВНО, обробляй ПАРАЛЕЛЬНО!

**Формула успіху:**
```
DbContext (sequential) ? Data in Memory ? Parallel Processing ?
```

**Результат:**
- Немає `InvalidOperationException`
- Швидка обробка даних
- Простий та зрозумілий код

---

**Статус:** ? Виправлено!
