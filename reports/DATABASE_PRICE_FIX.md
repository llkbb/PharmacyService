# Інструкція: Оновлення бази даних з правильними цінами

## Проблема
Ціни препаратів в базі даних = 0.00 грн

## Рішення

### Варіант 1: Видалити базу даних та створити нову

```powershell
# 1. Зупинити додаток (Ctrl+C)

# 2. Видалити базу даних
dotnet ef database drop --force

# 3. Застосувати міграції знову
dotnet ef database update

# 4. Запустити додаток (автоматично запуститься seeder)
dotnet run
```

### Варіант 2: Оновити ціни вручну через SQL

```sql
UPDATE Drugs SET Price = 20.00 WHERE Name LIKE '%Парацетамол%';
UPDATE Drugs SET Price = 30.00 WHERE Name LIKE '%Ібупрофен%';
UPDATE Drugs SET Price = 70.00 WHERE Name LIKE '%Амоксицилін%';
UPDATE Drugs SET Price = 15.00 WHERE Name LIKE '%Активоване%';
UPDATE Drugs SET Price = 25.00 WHERE Name LIKE '%Аскорбінова%';
UPDATE Drugs SET Price = 45.00 WHERE Name LIKE '%Німесулід%';
UPDATE Drugs SET Price = 55.00 WHERE Name LIKE '%Омепразол%';
UPDATE Drugs SET Price = 35.00 WHERE Name LIKE '%Лоратадин%';
UPDATE Drugs SET Price = 120.00 WHERE Name LIKE '%Полівітаміни%';
UPDATE Drugs SET Price = 90.00 WHERE Name LIKE '%Цефтріаксон%';

-- Оновити також ціни в InventoryItems
UPDATE InventoryItems SET UnitPrice = d.Price
FROM InventoryItems i
INNER JOIN Drugs d ON i.DrugId = d.Id;
```

### Варіант 3: Видалити тільки дані, не міграції

```powershell
# Виконати в Package Manager Console або терміналі:
dotnet ef database update 0  # Відкат всіх міграцій
dotnet ef database update    # Застосувати знову
```

## Після оновлення

Перезапустіть додаток:

```powershell
dotnet run
```

Увійдіть як клієнт:
- **Email:** user@pharmacy.com
- **Password:** User123!

Ціни мають відображатися коректно.

## Що було змінено в DbSeeder.cs

- Додано 10 українських препаратів з правильними цінами (від 15 до 120 грн)
- Додано 5 аптек з українськими адресами
- Автоматично створено inventory для кожної аптеки
- Ціна в `InventoryItems.UnitPrice` = `Drug.Price`

## Перевірка

Після запуску перейдіть на:
- `/CustomerPortal/Index` - каталог препаратів
- Ціни мають відображатися як "20.00 грн", "30.00 грн", тощо
