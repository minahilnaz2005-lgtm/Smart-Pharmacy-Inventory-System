using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SPIEMS.DAL;

public class ChartService
{
    private readonly SPIEMSDbContext _db;

    public ChartService(SPIEMSDbContext db)
    {
        _db = db;
    }

    // =========================
    // EXPIRY TREND (FIXED)
    // =========================
    public async Task<(List<string> labels, List<int> values)> GetExpiryTrendAsync(int months = 6)
    {
        var from = DateTime.Today.AddMonths(-(months - 1));
        from = new DateTime(from.Year, from.Month, 1);

        // STEP 1: fetch minimal data from SQL
        var raw = await _db.MedicineBatches
            .AsNoTracking()
            .Where(b => b.ExpiryDate != null && b.ExpiryDate.Value >= from)
            .Select(b => b.ExpiryDate!.Value)
            .ToListAsync();

        // STEP 2: group in memory (safe)
        var grouped = raw
            .GroupBy(d => new DateTime(d.Year, d.Month, 1))
            .Select(g => new
            {
                Month = g.Key,
                Count = g.Count()
            })
            .OrderBy(x => x.Month)
            .ToList();

        var labels = new List<string>();
        var values = new List<int>();

        for (int i = 0; i < months; i++)
        {
            var m = from.AddMonths(i);
            labels.Add(m.ToString("MMM yyyy"));
            values.Add(grouped.FirstOrDefault(x => x.Month == m)?.Count ?? 0);
        }

        return (labels, values);
    }

    // =========================
    // STOCK BY MEDICINE (OK)
    // =========================
    public async Task<(List<string> labels, List<int> values)> GetStockByMedicineAsync()
    {
        var data = await _db.MedicineBatches
            .AsNoTracking()
            .Include(b => b.Medicine)
            .Where(b => b.Quantity > 0)
            .GroupBy(b => b.Medicine != null ? b.Medicine.BrandName : "Unknown")
            .Select(g => new
            {
                Medicine = g.Key,
                Qty = g.Sum(x => x.Quantity)
            })
            .OrderByDescending(x => x.Qty)
            .ToListAsync();

        return (
            data.Select(x => x.Medicine).ToList(),
            data.Select(x => x.Qty).ToList()
        );
    }

    // =========================
    // PROFIT TREND (ADMIN ONLY)
    // =========================
    public async Task<(List<string> labels, List<decimal> values)> GetProfitTrendAsync(int months = 6)
    {
        var from = DateTime.Today.AddMonths(-(months - 1));
        from = new DateTime(from.Year, from.Month, 1);

        // STEP 1: fetch minimal rows
        var raw = await _db.SaleLines
            .AsNoTracking()
            .Include(l => l.Sale)
            .Include(l => l.MedicineBatch)
            .Where(l =>
                l.Sale != null &&
                l.MedicineBatch != null &&
                l.Sale.SaleDate >= from)
            .Select(l => new
            {
                l.Sale!.SaleDate,
                Profit = (l.UnitPrice - l.MedicineBatch!.CostPrice) * l.Quantity
            })
            .ToListAsync();

        // STEP 2: group in memory
        var grouped = raw
            .GroupBy(x => new DateTime(x.SaleDate.Year, x.SaleDate.Month, 1))
            .Select(g => new
            {
                Month = g.Key,
                Profit = g.Sum(x => x.Profit)
            })
            .OrderBy(x => x.Month)
            .ToList();

        var labels = new List<string>();
        var values = new List<decimal>();

        for (int i = 0; i < months; i++)
        {
            var m = from.AddMonths(i);
            labels.Add(m.ToString("MMM yyyy"));
            values.Add(grouped.FirstOrDefault(x => x.Month == m)?.Profit ?? 0m);
        }

        return (labels, values);
    }
}
