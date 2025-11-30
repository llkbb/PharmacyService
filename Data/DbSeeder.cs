using PharmacyChain.Models;

namespace PharmacyChain.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext db)
        {
            if (!db.Pharmacies.Any())
            {
                db.Pharmacies.AddRange(
                    new Pharmacy { Name = "Аптека №1", Address = "Київ, вул. Хрещатик, 1", Phone = "+380441112233" },
                    new Pharmacy { Name = "Аптека №2", Address = "Львів, просп. Свободи, 10", Phone = "+380322223344" }
                );
            }

            if (!db.Drugs.Any())
            {
                db.Drugs.AddRange(
                    new Drug { Name = "Парацетамол 500 мг", Category = "OTC", Manufacturer = "FarmCorp", PrescriptionRequired = false, ReorderLevel = 50 },
                    new Drug { Name = "Амоксицилін 500 мг", Category = "Rx", Manufacturer = "MediPharm", PrescriptionRequired = true, ReorderLevel = 30 }
                );
            }

            if (!db.Suppliers.Any())
            {
                db.Suppliers.AddRange(
                    new Supplier { Name = "ТОВ \"ФармПостач\"", Email = "supply@farmpostach.ua" },
                    new Supplier { Name = "ТОВ \"МедДистриб\"", Email = "info@meddistr.ua" }
                );
            }

            await db.SaveChangesAsync();

            if (!db.InventoryItems.Any())
            {
                var p1 = db.Pharmacies.First();
                var d1 = db.Drugs.First();
                db.InventoryItems.Add(new InventoryItem { PharmacyId = p1.Id, DrugId = d1.Id, Quantity = 120, UnitPrice = 65m });
                await db.SaveChangesAsync();
            }
        }
    }
}
