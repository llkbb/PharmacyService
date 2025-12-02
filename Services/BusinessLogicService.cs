using Microsoft.EntityFrameworkCore;
using PharmacyChain.Data;
using PharmacyChain.Exceptions;

namespace PharmacyChain.Services
{
    /// <summary>
    /// Сервіс для перевірки бізнес-правил перед видаленням даних
    /// </summary>
    public class BusinessLogicService
    {
        private readonly ApplicationDbContext _db;

        public BusinessLogicService(ApplicationDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Перевіряє, чи можна видалити препарат
        /// </summary>
        public async Task ValidateCanDeleteDrugAsync(int drugId)
        {
            // Препарат використовується в інвентарі
            var inInventory = await _db.InventoryItems.AnyAsync(i => i.DrugId == drugId);
            if (inInventory)
                throw new BusinessLogicException($"Препарат не можна видалити, тому що він присутній на складі.");

            // Препарат використовується в продажах
            var inSales = await _db.SaleLines.AnyAsync(l => l.DrugId == drugId);
            if (inSales)
                throw new BusinessLogicException($"Препарат не можна видалити, тому що він має історію продажів.");

            // Препарат пов'язаний із рецептами
            var inPrescriptions = await _db.Prescriptions.AnyAsync(p => p.DrugId == drugId);
            if (inPrescriptions)
                throw new BusinessLogicException($"Препарат не можна видалити, тому що він пов'язаний з рецептами.");

            // Препарат в закупівлях
            var inPurchaseOrders = await _db.PurchaseOrderLines.AnyAsync(p => p.DrugId == drugId);
            if (inPurchaseOrders)
                throw new BusinessLogicException($"Препарат не можна видалити, тому що він є в закупівлях.");
        }

        /// <summary>
        /// Перевіряє, чи можна видалити аптеку
        /// </summary>
        public async Task ValidateCanDeletePharmacyAsync(int pharmacyId)
        {
            // Аптека має працівників
            var hasEmployees = await _db.Employees.AnyAsync(e => e.PharmacyId == pharmacyId);
            if (hasEmployees)
                throw new BusinessLogicException("Аптеку не можна видалити, тому що в ній є працівники.");

            // Аптека має запаси
            var hasInventory = await _db.InventoryItems.AnyAsync(i => i.PharmacyId == pharmacyId);
            if (hasInventory)
                throw new BusinessLogicException("Аптеку не можна видалити, тому що в ній є запаси.");

            // Аптека має продажі
            var hasSales = await _db.Sales.AnyAsync(s => s.PharmacyId == pharmacyId);
            if (hasSales)
                throw new BusinessLogicException("Аптеку не можна видалити, тому що у неї є історія продажів.");

            // Аптека має закупівлі
            var hasPurchaseOrders = await _db.PurchaseOrders.AnyAsync(p => p.PharmacyId == pharmacyId);
            if (hasPurchaseOrders)
                throw new BusinessLogicException("Аптеку не можна видалити, тому що у неї є закупівлі.");
        }

        /// <summary>
        /// Перевіряє, чи можна видалити клієнта
        /// </summary>
        public async Task ValidateCanDeleteCustomerAsync(int customerId)
        {
            // Клієнт має покупки
            var hasSales = await _db.Sales.AnyAsync(s => s.CustomerId == customerId);
            if (hasSales)
                throw new BusinessLogicException("Клієнта не можна видалити, тому що у нього є історія покупок.");

            // Клієнт має рецепти
            var hasPrescriptions = await _db.Prescriptions.AnyAsync(p => p.CustomerId == customerId);
            if (hasPrescriptions)
                throw new BusinessLogicException("Клієнта не можна видалити, тому що він має рецепти.");
        }

        /// <summary>
        /// Перевіряє, чи можна видалити постачальника
        /// </summary>
        public async Task ValidateCanDeleteSupplierAsync(int supplierId)
        {
            // Постачальник має закупівлі
            var hasOrders = await _db.PurchaseOrders.AnyAsync(p => p.SupplierId == supplierId);
            if (hasOrders)
                throw new BusinessLogicException("Постачальника не можна видалити, тому що у нього є закупівлі.");
        }

        /// <summary>
        /// Перевіряє, чи можна видалити запас
        /// </summary>
        public async Task ValidateCanDeleteInventoryItemAsync(int inventoryId)
        {
            var item = await _db.InventoryItems.FindAsync(inventoryId);
            if (item == null)
                throw new BusinessLogicException("Запас не знайдено.");

            // Можна видалити тільки порожні запаси
            if (item.Quantity > 0)
                throw new BusinessLogicException($"Запас не можна видалити, тому що на складі є {item.Quantity} одиниць. Спочатку очистіть склад.");
        }

        /// <summary>
        /// Перевіряє можливість оновлення ціни препарату
        /// </summary>
        public async Task ValidateCanUpdateDrugPriceAsync(int pharmacyId, int drugId, decimal newPrice)
        {
            if (newPrice < 0)
                throw new BusinessLogicException("Ціна не може бути від'ємною.");

            var inventoryItem = await _db.InventoryItems
                .FirstOrDefaultAsync(i => i.PharmacyId == pharmacyId && i.DrugId == drugId);

            if (inventoryItem == null)
                throw new BusinessLogicException("Препарат не знайдено в цій аптеці.");

            if (inventoryItem.Quantity == 0 && newPrice > 0)
                throw new BusinessLogicException("Не можна встановити ціну для препарату, якого немає на складі.");
        }
    }
}
