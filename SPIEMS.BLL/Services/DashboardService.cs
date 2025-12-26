using Microsoft.EntityFrameworkCore;
using SPIEMS.DAL;

namespace SPIEMS.BLL.Services;

public class DashboardService
{
    private readonly SPIEMSDbContext _db;
    public DashboardService(SPIEMSDbContext db) => _db = db;

    public async Task<int> NearExpiryCountAsync(int days = 30)
    {
        var today = DateTime.Today;
        var until = today.AddDays(days);

        return await _db.MedicineBatches.CountAsync(b =>
            b.Quantity > 0 &&
            b.ExpiryDate != null &&
            b.ExpiryDate.Value >= today &&
            b.ExpiryDate.Value <= until);
    }

    // ✅ Better logic:
    // Count medicines that have EVER had batches, and are currently below reorder level.
    // This avoids "LowStock == TotalMedicines" when many medicines have no batches.
    public async Task<int> LowStockCountAsync()
    {
        var meds = await _db.Medicines.AsNoTracking().ToListAsync();

        var stock = await _db.MedicineBatches
            .GroupBy(b => b.MedicineId)
            .Select(g => new
            {
                MedicineId = g.Key,
                HasAnyBatch = g.Count() > 0,
                Total = g.Sum(x => x.Quantity)
            })
            .ToListAsync();

        var dict = stock.ToDictionary(x => x.MedicineId, x => x);

        return meds.Count(m =>
        {
            if (!dict.TryGetValue(m.Id, out var s)) return false; // never stocked
            return s.HasAnyBatch && s.Total < m.ReorderLevel;     // strictly below
        });
    }
}