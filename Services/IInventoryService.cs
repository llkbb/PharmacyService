using PharmacyChain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PharmacyChain.Data
{
    public interface IInventoryService
    {
        Task<bool> TryReserveAsync(int pharmacyId, int drugId, int qty);
        Task AddStockAsync(int pharmacyId, int drugId, int qty, decimal? newPrice = null);
        Task<List<InventoryItem>> GetLowStockAsync();
    }
}
