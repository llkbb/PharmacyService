using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmacyChain.Data;
using PharmacyChain.Models;
using System.Diagnostics;

namespace PharmacyChain.Controllers
{
    [Authorize(Roles = "Admin")]
    public class PerformanceTestController : Controller
    {
        private readonly ApplicationDbContext _db;

        public PerformanceTestController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            return View();
        }

        // Завдання 1-2: Послідовний запит на невеликій вибірці
        [HttpPost]
        public async Task<IActionResult> TestSequential(int recordCount = 20)
        {
            var sw = Stopwatch.StartNew();

            var results = await _db.Sales
                .Include(s => s.Pharmacy)
                .Include(s => s.Customer)
                .Include(s => s.Lines)
                .ThenInclude(l => l.Drug)
                .Take(recordCount)
                .ToListAsync();

            sw.Stop();

            ViewBag.Method = "Послідовний (Sequential)";
            ViewBag.RecordCount = results.Count;
            ViewBag.ElapsedMs = sw.ElapsedMilliseconds;
            ViewBag.ElapsedTicks = sw.ElapsedTicks;

            return View("TestResult", results);
        }

        // Завдання 1-2: Паралельний запит на невеликій вибірці
        [HttpPost]
        public async Task<IActionResult> TestParallel(int recordCount = 20)
        {
            var sw = Stopwatch.StartNew();

            // ВИПРАВЛЕННЯ: Послідовно завантажуємо дані з БД
            var sales = await _db.Sales.Take(recordCount).ToListAsync();
            var pharmacies = await _db.Pharmacies.ToListAsync();
            var customers = await _db.Customers.ToListAsync();
            var drugs = await _db.Drugs.ToListAsync();

            var saleIds = sales.Select(s => s.Id).ToList();
            var saleLines = await _db.SaleLines
                .Where(sl => saleIds.Contains(sl.SaleId))
                .ToListAsync();

            // Тепер ПАРАЛЕЛЬНА обробка в пам'яті
            var saleLinesDict = saleLines.GroupBy(sl => sl.SaleId)
                .ToDictionary(g => g.Key, g => g.ToList());

            Parallel.ForEach(sales, sale =>
            {
                sale.Pharmacy = pharmacies.FirstOrDefault(p => p.Id == sale.PharmacyId);
                sale.Customer = customers.FirstOrDefault(c => c.Id == sale.CustomerId);

                if (saleLinesDict.ContainsKey(sale.Id))
                {
                    sale.Lines = saleLinesDict[sale.Id];
                    foreach (var line in sale.Lines)
                    {
                        line.Drug = drugs.FirstOrDefault(d => d.Id == line.DrugId);
                    }
                }
            });

            sw.Stop();

            ViewBag.Method = "Паралельний (Parallel)";
            ViewBag.RecordCount = sales.Count;
            ViewBag.ElapsedMs = sw.ElapsedMilliseconds;
            ViewBag.ElapsedTicks = sw.ElapsedTicks;

            return View("TestResult", sales);
        }

        // Завдання 3: Генерація 100,000 записів у таблицю Sales
        [HttpPost]
        public async Task<IActionResult> GenerateLargeDataset()
        {
            var sw = Stopwatch.StartNew();

            // Отримуємо існуючі дані
            var pharmacies = await _db.Pharmacies.ToListAsync();
            var customers = await _db.Customers.ToListAsync();
            var drugs = await _db.Drugs.ToListAsync();

            if (!pharmacies.Any() || !customers.Any() || !drugs.Any())
            {
                TempData["Error"] = "Спочатку додайте аптеки, клієнтів та препарати!";
                return RedirectToAction(nameof(Index));
            }

            var random = new Random();
            var batchSize = 1000;
            var totalRecords = 100000;
            var batches = totalRecords / batchSize;

            for (int batch = 0; batch < batches; batch++)
            {
                var sales = new List<Sale>();

                for (int i = 0; i < batchSize; i++)
                {
                    var sale = new Sale
                    {
                        PharmacyId = pharmacies[random.Next(pharmacies.Count)].Id,
                        CustomerId = customers[random.Next(customers.Count)].Id,
                        CreatedAt = DateTime.UtcNow.AddDays(-random.Next(365)),
                        PaymentMethod = random.Next(2) == 0 ? "Cash" : "Card",
                        PaymentStatus = "Paid",
                        PaidAt = DateTime.UtcNow.AddDays(-random.Next(365))
                    };

                    // Додаємо 1-3 позиції до кожного продажу
                    var lineCount = random.Next(1, 4);
                    for (int j = 0; j < lineCount; j++)
                    {
                        var drug = drugs[random.Next(drugs.Count)];
                        sale.Lines.Add(new SaleLine
                        {
                            DrugId = drug.Id,
                            Quantity = random.Next(1, 10),
                            UnitPrice = drug.Price + random.Next(-50, 50)
                        });
                    }

                    sales.Add(sale);
                }

                await _db.Sales.AddRangeAsync(sales);
                await _db.SaveChangesAsync();

                // Очищення контексту для уникнення перевантаження пам'яті
                _db.ChangeTracker.Clear();
            }

            sw.Stop();

            TempData["Success"] = $"Згенеровано {totalRecords:N0} записів за {sw.ElapsedMilliseconds:N0} мс ({sw.Elapsed.TotalSeconds:F2} сек)";
            return RedirectToAction(nameof(Index));
        }

        // Завдання 4: Тестування на великій вибірці (послідовно)
        [HttpPost]
        public async Task<IActionResult> TestLargeSequential(int recordCount = 10000)
        {
            var sw = Stopwatch.StartNew();

            var results = await _db.Sales
                .Include(s => s.Pharmacy)
                .Include(s => s.Customer)
                .Include(s => s.Lines)
                .ThenInclude(l => l.Drug)
                .Take(recordCount)
                .ToListAsync();

            sw.Stop();

            ViewBag.Method = "Послідовний (Sequential) - Велика вибірка";
            ViewBag.RecordCount = results.Count;
            ViewBag.ElapsedMs = sw.ElapsedMilliseconds;
            ViewBag.ElapsedTicks = sw.ElapsedTicks;
            ViewBag.ElapsedSeconds = sw.Elapsed.TotalSeconds;

            return View("TestResultLarge", results);
        }

        // Завдання 4: Тестування на великій вибірці (паралельно)
        [HttpPost]
        public async Task<IActionResult> TestLargeParallel(int recordCount = 10000)
        {
            var sw = Stopwatch.StartNew();

            // ВИПРАВЛЕННЯ: Послідовно завантажуємо дані (DbContext не thread-safe навіть для async)
            var sales = await _db.Sales.Take(recordCount).ToListAsync();
            var pharmacies = await _db.Pharmacies.ToDictionaryAsync(p => p.Id);
            var customers = await _db.Customers.ToDictionaryAsync(c => c.Id);
            var drugs = await _db.Drugs.ToDictionaryAsync(d => d.Id);

            var saleIds = sales.Select(s => s.Id).ToList();
            var saleLines = await _db.SaleLines
                .Where(sl => saleIds.Contains(sl.SaleId))
                .ToListAsync();

            // Тепер ПАРАЛЕЛЬНА обробка даних в пам'яті (БЕЗ DbContext)
            var saleLinesDict = saleLines.GroupBy(sl => sl.SaleId)
                .ToDictionary(g => g.Key, g => g.ToList());

            Parallel.ForEach(sales, sale =>
            {
                if (pharmacies.ContainsKey(sale.PharmacyId))
                    sale.Pharmacy = pharmacies[sale.PharmacyId];

                if (sale.CustomerId.HasValue && customers.ContainsKey(sale.CustomerId.Value))
                    sale.Customer = customers[sale.CustomerId.Value];

                if (saleLinesDict.ContainsKey(sale.Id))
                {
                    sale.Lines = saleLinesDict[sale.Id];
                    foreach (var line in sale.Lines)
                    {
                        if (drugs.ContainsKey(line.DrugId))
                            line.Drug = drugs[line.DrugId];
                    }
                }
            });

            sw.Stop();

            ViewBag.Method = "Паралельний (Parallel) - Велика вибірка";
            ViewBag.RecordCount = sales.Count;
            ViewBag.ElapsedMs = sw.ElapsedMilliseconds;
            ViewBag.ElapsedTicks = sw.ElapsedTicks;
            ViewBag.ElapsedSeconds = sw.Elapsed.TotalSeconds;

            return View("TestResultLarge", sales);
        }

        // Завдання 5: Розпаралелювання з Parallel.ForEach
        [HttpPost]
        public async Task<IActionResult> TestParallelForEach(int recordCount = 10000)
        {
            var sw = Stopwatch.StartNew();

            // ВИПРАВЛЕННЯ: Завантажуємо ВСІ дані ПЕРЕД паралельною обробкою
            var sales = await _db.Sales.Take(recordCount).ToListAsync();
            var pharmacies = await _db.Pharmacies.ToDictionaryAsync(p => p.Id);
            var customers = await _db.Customers.ToDictionaryAsync(c => c.Id);
            var drugs = await _db.Drugs.ToDictionaryAsync(d => d.Id);

            var saleIds = sales.Select(s => s.Id).ToList();
            var saleLines = await _db.SaleLines
                .Where(sl => saleIds.Contains(sl.SaleId))
                .ToListAsync();

            var saleLinesDict = saleLines.GroupBy(sl => sl.SaleId)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Тепер паралельна обробка БЕЗ DbContext
            Parallel.ForEach(sales, sale =>
            {
                if (pharmacies.ContainsKey(sale.PharmacyId))
                    sale.Pharmacy = pharmacies[sale.PharmacyId];

                if (sale.CustomerId.HasValue && customers.ContainsKey(sale.CustomerId.Value))
                    sale.Customer = customers[sale.CustomerId.Value];

                if (saleLinesDict.ContainsKey(sale.Id))
                {
                    sale.Lines = saleLinesDict[sale.Id];
                    foreach (var line in sale.Lines)
                    {
                        if (drugs.ContainsKey(line.DrugId))
                            line.Drug = drugs[line.DrugId];
                    }
                }
            });

            sw.Stop();

            ViewBag.Method = "Parallel.ForEach";
            ViewBag.RecordCount = sales.Count;
            ViewBag.ElapsedMs = sw.ElapsedMilliseconds;
            ViewBag.ElapsedTicks = sw.ElapsedTicks;
            ViewBag.ElapsedSeconds = sw.Elapsed.TotalSeconds;

            return View("TestResultLarge", sales);
        }

        // Завдання 5: Розпаралелювання з TPL (Task Parallel Library)
        [HttpPost]
        public async Task<IActionResult> TestTPL(int recordCount = 10000)
        {
            var sw = Stopwatch.StartNew();

            // ВИПРАВЛЕННЯ: Завантажуємо ВСІ дані ПЕРЕД паралельною обробкою
            var sales = await _db.Sales.Take(recordCount).ToListAsync();

            var pharmacies = await _db.Pharmacies.ToDictionaryAsync(p => p.Id);
            var customers = await _db.Customers.ToDictionaryAsync(c => c.Id);
            var drugs = await _db.Drugs.ToDictionaryAsync(d => d.Id);

            var saleIds = sales.Select(s => s.Id).ToList();
            var saleLines = await _db.SaleLines
                .Where(sl => saleIds.Contains(sl.SaleId))
                .ToListAsync();

            var saleLinesDict = saleLines.GroupBy(sl => sl.SaleId)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Розбиваємо на частини для паралельної обробки
            var chunkSize = Math.Max(1, sales.Count / Environment.ProcessorCount);
            var chunks = sales.Select((sale, index) => new { sale, index })
                .GroupBy(x => x.index / chunkSize)
                .Select(g => g.Select(x => x.sale).ToList())
                .ToList();

            // Паралельна обробка через Task БЕЗ DbContext
            var tasks = chunks.Select(chunk => Task.Run(() =>
            {
                foreach (var sale in chunk)
                {
                    if (pharmacies.ContainsKey(sale.PharmacyId))
                        sale.Pharmacy = pharmacies[sale.PharmacyId];

                    if (sale.CustomerId.HasValue && customers.ContainsKey(sale.CustomerId.Value))
                        sale.Customer = customers[sale.CustomerId.Value];

                    if (saleLinesDict.ContainsKey(sale.Id))
                    {
                        sale.Lines = saleLinesDict[sale.Id];
                        foreach (var line in sale.Lines)
                        {
                            if (drugs.ContainsKey(line.DrugId))
                                line.Drug = drugs[line.DrugId];
                        }
                    }
                }
            })).ToList();

            await Task.WhenAll(tasks);

            sw.Stop();

            ViewBag.Method = "TPL (Task Parallel Library)";
            ViewBag.RecordCount = sales.Count;
            ViewBag.ElapsedMs = sw.ElapsedMilliseconds;
            ViewBag.ElapsedTicks = sw.ElapsedTicks;
            ViewBag.ElapsedSeconds = sw.Elapsed.TotalSeconds;

            return View("TestResultLarge", sales);
        }

        // Завдання 5: Розпаралелювання з PLINQ
        [HttpPost]
        public async Task<IActionResult> TestPLINQ(int recordCount = 10000)
        {
            var sw = Stopwatch.StartNew();

            // ВИПРАВЛЕННЯ: Завантажуємо ВСІ дані ПЕРЕД паралельною обробкою
            var sales = await _db.Sales.Take(recordCount).ToListAsync();
            var pharmacies = await _db.Pharmacies.ToDictionaryAsync(p => p.Id);
            var customers = await _db.Customers.ToDictionaryAsync(c => c.Id);
            var drugs = await _db.Drugs.ToDictionaryAsync(d => d.Id);

            var saleIds = sales.Select(s => s.Id).ToList();
            var saleLines = await _db.SaleLines
                .Where(sl => saleIds.Contains(sl.SaleId))
                .ToListAsync();

            var saleLinesDict = saleLines.GroupBy(sl => sl.SaleId)
                .ToDictionary(g => g.Key, g => g.ToList());

            // PLINQ обробка БЕЗ DbContext
            var processedSales = sales.AsParallel()
                .WithDegreeOfParallelism(Environment.ProcessorCount)
                .Select(sale =>
                {
                    if (pharmacies.ContainsKey(sale.PharmacyId))
                        sale.Pharmacy = pharmacies[sale.PharmacyId];

                    if (sale.CustomerId.HasValue && customers.ContainsKey(sale.CustomerId.Value))
                        sale.Customer = customers[sale.CustomerId.Value];

                    if (saleLinesDict.ContainsKey(sale.Id))
                    {
                        sale.Lines = saleLinesDict[sale.Id];
                        foreach (var line in sale.Lines)
                        {
                            if (drugs.ContainsKey(line.DrugId))
                                line.Drug = drugs[line.DrugId];
                        }
                    }

                    return sale;
                }).ToList();

            sw.Stop();

            ViewBag.Method = "PLINQ (Parallel LINQ)";
            ViewBag.RecordCount = processedSales.Count;
            ViewBag.ElapsedMs = sw.ElapsedMilliseconds;
            ViewBag.ElapsedTicks = sw.ElapsedTicks;
            ViewBag.ElapsedSeconds = sw.Elapsed.TotalSeconds;

            return View("TestResultLarge", processedSales);
        }

        // Очищення великої кількості записів
        [HttpPost]
        public async Task<IActionResult> ClearLargeDataset()
        {
            var sw = Stopwatch.StartNew();

            // Видаляємо SaleLines спочатку (через зовнішній ключ)
            await _db.Database.ExecuteSqlRawAsync("DELETE FROM SaleLines");
            
            // Потім видаляємо Sales
            await _db.Database.ExecuteSqlRawAsync("DELETE FROM Sales");

            sw.Stop();

            TempData["Success"] = $"Видалено всі записи за {sw.ElapsedMilliseconds:N0} мс ({sw.Elapsed.TotalSeconds:F2} сек)";
            return RedirectToAction(nameof(Index));
        }
    }
}
