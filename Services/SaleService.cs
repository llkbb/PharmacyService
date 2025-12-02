using PharmacyChain.Data;
using PharmacyChain.Models;
using Microsoft.EntityFrameworkCore;

namespace PharmacyChain.Services
{
    public class SalesService
    {
        private readonly ApplicationDbContext _db;

        public SalesService(ApplicationDbContext db)
        {
            _db = db;
        }

        public bool ProcessSale(int pharmacyId, int customerId, List<SaleLine> lines)
        {
            foreach (var line in lines)
            {
                var drug = _db.Drugs.Find(line.DrugId);
                var inv = _db.InventoryItems.FirstOrDefault(i =>
                    i.PharmacyId == pharmacyId && i.DrugId == line.DrugId);

                if (inv == null || inv.Quantity < line.Quantity)
                    return false;

                if (drug.PrescriptionRequired)
                {
                    if (!ValidatePrescription(customerId, drug.Id))
                        return false;
                }
            }

            // створення шапки
            var sale = new Sale
            {
                CreatedAt = DateTime.Now,
                CustomerId = customerId,
                PharmacyId = pharmacyId
            };

            _db.Sales.Add(sale);
            _db.SaveChanges();

            // додавання ліній
            foreach (var line in lines)
            {
                line.SaleId = sale.Id;
                _db.SaleLines.Add(line);

                var inv = _db.InventoryItems.First(i =>
                    i.PharmacyId == pharmacyId && i.DrugId == line.DrugId);

                inv.Quantity -= line.Quantity;
            }

            _db.SaveChanges();
            return true;
        }

        private bool ValidatePrescription(int customerId, int drugId)
        {
            var rx = _db.Prescriptions
                .FirstOrDefault(x => x.CustomerId == customerId && x.DrugId == drugId);

            return rx != null && rx.DateIssued.AddDays(rx.ValidDays) >= DateTime.Now;
        }
    }
}
