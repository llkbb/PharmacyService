using PharmacyChain.Models;

namespace PharmacyChain.Data
{
    public static class DbSeeder
    {
        public static void Seed(ApplicationDbContext db)
        {
            db.Database.EnsureCreated();

            // Якщо вже є дані — не повторюємо
            if (db.Drugs.Any()) return;

            // ---- Додаємо українські препарати з цінами ----
            var drugs = new List<Drug>
            {
                new Drug { Name = "Парацетамол 500 мг", Description = "Знеболювальний та жарознижувальний", Category = "Знеболювальні", Manufacturer = "FarmCorp", Price = 20.00m, ReorderLevel = 50, PrescriptionRequired = false },
                new Drug { Name = "Ібупрофен 200 мг", Description = "Протизапальний та знеболювальний", Category = "Знеболювальні", Manufacturer = "HealthMed", Price = 30.00m, ReorderLevel = 40, PrescriptionRequired = false },
                new Drug { Name = "Амоксицилін 500 мг", Description = "Антибіотик широкого спектру", Category = "Антибіотики", Manufacturer = "MediPharm", Price = 70.00m, ReorderLevel = 30, PrescriptionRequired = true },
                new Drug { Name = "Активоване вугілля", Description = "Ентеросорбент", Category = "Сорбенти", Manufacturer = "Carbonix", Price = 15.00m, ReorderLevel = 100, PrescriptionRequired = false },
                new Drug { Name = "Аскорбінова к-та", Description = "Вітамін C", Category = "Вітаміни", Manufacturer = "VitaminUa", Price = 25.00m, ReorderLevel = 60, PrescriptionRequired = false },
                new Drug { Name = "Німесулід 100 мг", Description = "Протизапальний препарат", Category = "Знеболювальні", Manufacturer = "PainOff", Price = 45.00m, ReorderLevel = 35, PrescriptionRequired = true },
                new Drug { Name = "Омепразол 20 мг", Description = "Препарат від виразки", Category = "Гастроентерологія", Manufacturer = "GastroMed", Price = 55.00m, ReorderLevel = 25, PrescriptionRequired = true },
                new Drug { Name = "Лоратадин 10 мг", Description = "Антигістамінний препарат", Category = "Алергія", Manufacturer = "Allergo", Price = 35.00m, ReorderLevel = 45, PrescriptionRequired = false },
                new Drug { Name = "Полівітаміни", Description = "Комплекс вітамінів", Category = "Вітаміни", Manufacturer = "VitAll", Price = 120.00m, ReorderLevel = 20, PrescriptionRequired = false },
                new Drug { Name = "Цефтріаксон 1 г", Description = "Антибіотик", Category = "Антибіотики", Manufacturer = "MediPharm", Price = 90.00m, ReorderLevel = 15, PrescriptionRequired = true }
            };

            db.Drugs.AddRange(drugs);
            db.SaveChanges();

            // ---- Додаємо аптеки ----
            var pharmacies = new List<Pharmacy>
            {
                new Pharmacy { Name = "Аптека №1", Address = "вул. Хрещатик, 1", Phone = "+380441234567" },
                new Pharmacy { Name = "Аптека №5", Address = "пр. Перемоги, 42", Phone = "+380441234568" },
                new Pharmacy { Name = "Аптека №2", Address = "вул. Лесі Українки, 10", Phone = "+380441234569" },
                new Pharmacy { Name = "Аптека №7", Address = "пр. Героїв Сталінграду, 5", Phone = "+380441234570" },
                new Pharmacy { Name = "Аптека №3", Address = "вул. Грушевського, 3", Phone = "+380441234571" }
            };

            db.Pharmacies.AddRange(pharmacies);
            db.SaveChanges();

            // ---- Додаємо складські залишки ----
            var inventory = new List<InventoryItem>();
            var random = new Random();

            foreach (var pharmacy in pharmacies)
            {
                foreach (var drug in drugs)
                {
                    // Не всі препарати є у всіх аптеках
                    if (random.Next(0, 3) > 0) // 66% ймовірність
                    {
                        inventory.Add(new InventoryItem
                        {
                            DrugId = drug.Id,
                            PharmacyId = pharmacy.Id,
                            Quantity = random.Next(0, 200), // Деякі можуть бути 0 (низькі залишки)
                            UnitPrice = drug.Price
                        });
                    }
                }
            }

            db.InventoryItems.AddRange(inventory);
            db.SaveChanges();

            // ---- Додаємо клієнтів ----
            var customers = new List<Customer>
            {
                new Customer { FullName = "Іван Петренко", Email = "ivan@example.com", Phone = "+380501111111" },
                new Customer { FullName = "Марія Коваленко", Email = "maria@example.com", Phone = "+380502222222" },
                new Customer { FullName = "Олександр Шевченко", Email = "alex@example.com", Phone = "+380503333333" },
                new Customer { FullName = "Наталія Мельник", Email = "natalia@example.com", Phone = "+380504444444" }
            };

            db.Customers.AddRange(customers);
            db.SaveChanges();

            // ---- Додаємо постачальників ----
            var suppliers = new List<Supplier>
            {
                new Supplier { Name = "ФармОпт", Address = "Промзона 5", Phone = "+380441111111", Email = "info@pharmopt.ua" },
                new Supplier { Name = "МедПостач", Address = "Торговий центр 10", Phone = "+380442222222", Email = "sales@medpostach.ua" },
                new Supplier { Name = "HealthSupply", Address = "вул. Індустріальна, 15", Phone = "+380443333333", Email = "contact@healthsupply.ua" }
            };

            db.Suppliers.AddRange(suppliers);
            db.SaveChanges();

            // ---- Додаємо працівників ----
            var employees = new List<Employee>();
            
            foreach (var pharmacy in pharmacies)
            {
                employees.Add(new Employee { FullName = $"Провізор {pharmacy.Name}", PharmacyId = pharmacy.Id, Role = "Провізор" });
                employees.Add(new Employee { FullName = $"Касир {pharmacy.Name}", PharmacyId = pharmacy.Id, Role = "Касир" });
            }

            db.Employees.AddRange(employees);
            db.SaveChanges();
        }
    }
}
