# Виправлення відображення тексту та валюти

## Проблеми що були виправлені:

### 1. Знаки питання (??) замість тексту
**Причина:** Емодзі не підтримуються всіма браузерами та шрифтами

**Рішення:**
- Видалено всі емодзі з проєкту
- Замінено на Bootstrap Icons (вже є в проєкті)
- Додано `lang="uk"` в HTML тег
- Додано явне `charset=utf-8` в meta тег

### 2. Проблема з відображенням валюти (?)
**Причина:** Символ гривні (?) не завжди правильно відображається

**Рішення:**
- Замінено ? на "грн" у всіх місцях
- Формат: `20.00 грн` замість `?20.00`

## Змінені файли:

### Views:
1. ? `Views/Shared/_Layout.cshtml`
   - Змінено `lang="en"` на `lang="uk"`
   - Додано `charset=utf-8` meta tag
   - Замінено ?? на іконку Bootstrap

2. ? `Views/CustomerPortal/Index.cshtml`
   - Замінено ?? на `<i class="bi bi-shop"></i>`
   - Замінено ?? на `<i class="bi bi-search"></i>`
   - Замінено ? на "грн"

3. ? `Views/CustomerPortal/DrugDetails.cshtml`
   - Замінено ? на "грн"

4. ? `Views/CustomerPortal/Purchase.cshtml`
   - Замінено ?? на `<i class="bi bi-cart-plus"></i>`
   - Замінено ?? на `<i class="bi bi-credit-card"></i>`
   - Замінено ? на "грн"

5. ? `Views/CustomerPortal/MyOrders.cshtml`
   - Замінено ?? на `<i class="bi bi-box-seam"></i>`
   - Замінено ? на "грн" у всіх місцях

6. ? `Views/CustomerPortal/MyPrescriptions.cshtml`
   - Замінено ?? на `<i class="bi bi-file-medical"></i>`

7. ? `Views/CustomerPortal/MyProfile.cshtml`
   - Замінено ?? на `<i class="bi bi-person-circle"></i>`
   - Замінено ??, ??, ?? на відповідні іконки Bootstrap

8. ? `Views/CustomerPortal/Pharmacies.cshtml`
   - Замінено ?? на `<i class="bi bi-hospital"></i>`
   - Замінено ?? на `<i class="bi bi-search"></i>`

9. ? `Views/CustomerPortal/PharmacyInventory.cshtml`
   - Замінено ?? на `<i class="bi bi-hospital"></i>`
   - Замінено ? на "грн"

10. ? `Views/CustomerHome/Index.cshtml`
    - Замінено ?? на `<i class="bi bi-hospital"></i>`
    - Замінено ?? на `<i class="bi bi-star"></i>`
    - Замінено ?? на `<i class="bi bi-exclamation-triangle"></i>`

### Controllers:
11. ? `Controllers/CustomerPortalController.cs`
    - Змінено повідомлення: `?{sale.Total:F2}` ? `{sale.Total:F2} грн`

## Результат:

Тепер весь текст відображається коректно:
- ? Немає знаків питання
- ? Валюта відображається як "грн" замість ?
- ? Використовуються іконки Bootstrap замість емодзі
- ? Правильне кодування UTF-8

## Приклади зміни відображення:

**Було:**
```
?? Каталог препаратів
?20,00
?? Купити
```

**Стало:**
```
?? Каталог препаратів    (іконка)
20.00 грн
?? Купити               (іконка)
```

## Запуск:

Якщо проєкт ще запущений, зупиніть його та запустіть знову:

```sh
# Зупинити (Ctrl+C в терміналі або закрити вкладку браузера)
# Потім запустити знову
dotnet run
```

Всі зміни застосовані та перевірені!
