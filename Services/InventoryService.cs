using Microsoft.EntityFrameworkCore;
using PharmacyChain.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PharmacyChain.Data
{
    public class InventoryService : IInventoryService
    {
        private readonly ApplicationDbContext _db;
        public InventoryService(ApplicationDbContext db) => _db = db;

        public async Task<bool> TryReserveAsync(int pharmacyId, int drugId, int qty)
        {
            var item = await _db.InventoryItems
                .SingleOrDefaultAsync(i => i.PharmacyId == pharmacyId && i.DrugId == drugId);
            if (item == null || item.Quantity < qty) return false;
            item.Quantity -= qty;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task AddStockAsync(int pharmacyId, int drugId, int qty, decimal? newPrice = null)
        {
            var item = await _db.InventoryItems
                .SingleOrDefaultAsync(i => i.PharmacyId == pharmacyId && i.DrugId == drugId);
            if (item == null)
            {
                item = new InventoryItem { PharmacyId = pharmacyId, DrugId = drugId, Quantity = 0, UnitPrice = 0m };
                await _db.InventoryItems.AddAsync(item);
            }
            item.Quantity += qty;
            if (newPrice.HasValue) item.UnitPrice = newPrice.Value;
            await _db.SaveChangesAsync();
        }

        public async Task<List<InventoryItem>> GetLowStockAsync()
        {
            return await _db.InventoryItems
                .Include(i => i.Drug)
                .Where(i => i.Quantity <= i.Drug.ReorderLevel)
                .ToListAsync();
        }
    }
}
