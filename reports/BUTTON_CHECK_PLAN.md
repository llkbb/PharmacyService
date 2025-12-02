# Перевірка всіх кнопок та функціональності по ролях

## ?? Список контролерів та їх доступ

### **Admin Only** (Тільки адміністратор)
| Контролер | Функції CRUD | Статус |
|-----------|-------------|--------|
| `CustomersController` | Create, Edit, Delete, Index, Details | ? Має Authorize(Roles = "Admin") |
| `DrugsController` | Create, Edit, Delete, Index, Details | ? Потрібна перевірка |
| `EmployeesController` | Create, Edit, Delete, Index, Details | ? Має Authorize(Roles = "Admin") |
| `PharmaciesController` | Create, Edit, Delete, Index, Details | ? Потрібна перевірка |
| `PrescriptionsController` | Create, Edit, Delete, Index, Details | ? Потрібна перевірка |
| `PurchaseOrdersController` | Create, Edit, Delete, Index, Details | ? Потрібна перевірка |
| `PurchaseOrderLinesController` | Create, Edit, Delete, Index, Details | ? Потрібна перевірка |
| `SuppliersController` | Create, Edit, Delete, Index, Details | ? Має Authorize(Roles = "Admin") |

### **Admin + Pharmacist** (Адміністратор + Аптекар)
| Контролер | Функції CRUD | Статус |
|-----------|-------------|--------|
| `InventoryItemsController` | Create, Edit, Delete, Index, Details | ? Має Authorize(Roles = "Admin,Pharmacist") |
| `SalesController` | Create, Edit, Delete, Index, Details | ? Має Authorize(Roles = "Admin,Pharmacist") |
| `SaleLinesController` | Create, Edit, Delete, Index, Details | ? Потрібна перевірка |

### **Pharmacist Only** (Тільки аптекар)
| Контролер | Функції | Статус |
|-----------|---------|--------|
| `PharmacistController` | Index (Dashboard), MyInventory, LowStock, ActivePrescriptions | ? Має Authorize(Roles = "Pharmacist") |

### **User Only** (Тільки клієнт)
| Контролер | Функції | Статус |
|-----------|---------|--------|
| `CustomerPortalController` | Index, DrugDetails, Purchase, MyOrders, MyProfile, Pharmacies, PharmacyInventory | ? Потрібна перевірка |
| `CustomerHomeController` | Index (Dashboard) | ? Потрібна перевірка |

### **Public / Authorized** (Для всіх авторизованих)
| Контролер | Функції | Статус |
|-----------|---------|--------|
| `HomeController` | Index, Privacy | ? Має [Authorize] на Index |
| `QueriesController` | RxDrugs, SalesByDate, LowStock, UpdatePrice, DeleteSupplier, InsertSale | ? Потрібна перевірка |
| `AccountController` | Login, Logout, Register | ? AllowAnonymous на Login/Register |

---

## ?? Проблеми що потрібно перевірити

### 1. Контролери без Authorize атрибуту
? **Потенційна проблема безпеки!**

Потрібно перевірити:
- `DrugsController` - чи має `[Authorize(Roles = "Admin")]`?
- `PharmaciesController` - чи має `[Authorize(Roles = "Admin")]`?
- `PrescriptionsController` - хто може керувати рецептами?
- `PurchaseOrdersController` - тільки Admin?
- `PurchaseOrderLinesController` - тільки Admin?
- `SaleLinesController` - Admin + Pharmacist?
- `CustomerPortalController` - тільки User?
- `CustomerHomeController` - тільки User?
- `QueriesController` - хто має доступ?

### 2. Перевірка кнопок у Views

Потрібно перевірити чи всі кнопки мають правильні action:
- **Create** - форма + POST
- **Edit** - форма + POST
- **Delete** - форма + POST
- **Details** - перегляд

### 3. Перевірка запитів до БД

Потрібно перевірити чи всі CRUD операції:
- Використовують `Include()` для навігаційних властивостей
- Мають обробку помилок (try-catch або ModelState)
- Використовують транзакції де потрібно
- Повертають правильні повідомлення (TempData)

---

## ?? План перевірки

### Крок 1: Перевірити Authorize атрибути
- [ ] DrugsController
- [ ] PharmaciesController
- [ ] PrescriptionsController
- [ ] PurchaseOrdersController
- [ ] PurchaseOrderLinesController
- [ ] SaleLinesController
- [ ] CustomerPortalController
- [ ] CustomerHomeController
- [ ] QueriesController

### Крок 2: Перевірити CRUD операції
- [ ] Create - форма існує, POST працює
- [ ] Edit - форма існує, POST працює
- [ ] Delete - форма існує, POST працює
- [ ] Index - список відображається
- [ ] Details - деталі відображаються

### Крок 3: Перевірити запити до БД
- [ ] Include() для зв'язків
- [ ] FirstOrDefaultAsync() для пошуку
- [ ] SaveChangesAsync() для збереження
- [ ] Транзакції для складних операцій
- [ ] Обробка помилок

### Крок 4: Перевірити навігацію
- [ ] Меню для Admin
- [ ] Меню для Pharmacist
- [ ] Меню для User
- [ ] Кнопки "Назад", "Створити", тощо

---

## ?? Наступні кроки

1. Прочитати кожен контролер
2. Додати відсутні `[Authorize]` атрибути
3. Перевірити всі POST методи на наявність обробки помилок
4. Протестувати кожну кнопку в кожній ролі
5. Створити фінальний звіт

---

**Статус:** В процесі перевірки...
