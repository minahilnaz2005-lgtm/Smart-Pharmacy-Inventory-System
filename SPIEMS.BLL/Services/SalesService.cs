using Microsoft.EntityFrameworkCore;
using SPIEMS.DAL;
using SPIEMS.DAL.Entities;

namespace SPIEMS.BLL.Services;

public class SalesService
{
    private readonly SPIEMSDbContext _db;

    public SalesService(SPIEMSDbContext db)
    {
        _db = db;
    }

    // STAFF SALE → DIRECT SOLD
    public async Task<int> ProcessSaleAsync(int medicineId, int quantity, int userId)
    {
        if (quantity <= 0)
            throw new Exception("Invalid sale quantity.");

        var batches = await _db.MedicineBatches
            .Include(b => b.Medicine)
            .Where(b => b.MedicineId == medicineId && b.Quantity > 0)
            .OrderBy(b => b.ExpiryDate)
            .ToListAsync();

        if (batches.Sum(b => b.Quantity) < quantity)
            throw new Exception("Insufficient stock.");

        var sale = new Sale
        {
            UserId = userId,
            SaleDate = DateTime.Now,
            Status = "SoldOut"
        };

        int remaining = quantity;
        decimal total = 0;

        foreach (var batch in batches)
        {
            if (remaining == 0) break;

            int take = Math.Min(batch.Quantity, remaining);
            remaining -= take;
            batch.Quantity -= take;

            decimal lineTotal = batch.SalePrice * take;

            sale.Lines.Add(new SaleLine
            {
                MedicineBatchId = batch.Id,
                Quantity = take,
                UnitPrice = batch.SalePrice,
                LineTotal = lineTotal
            });

            total += lineTotal;
        }

        sale.TotalAmount = total;
        _db.Sales.Add(sale);
        await _db.SaveChangesAsync();

        return sale.Id;
    }

    // CUSTOMER CHECKOUT → PENDING
    public async Task<int> ProcessOrderAsync(List<(int medicineId, int quantity)> items, int userId)
    {
        if (items == null || !items.Any())
            throw new Exception("Cart is empty.");

        var sale = new Sale
        {
            UserId = userId,
            SaleDate = DateTime.Now,
            Status = "Checkout"
        };

        decimal total = 0;

        foreach (var item in items)
        {
            var batches = await _db.MedicineBatches
                .Include(b => b.Medicine)
                .Where(b => b.MedicineId == item.medicineId && b.Quantity > 0)
                .OrderBy(b => b.ExpiryDate)
                .ToListAsync();

            if (batches.Sum(b => b.Quantity) < item.quantity)
                throw new Exception("Insufficient stock.");

            int remaining = item.quantity;

            foreach (var batch in batches)
            {
                if (remaining == 0) break;

                int take = Math.Min(batch.Quantity, remaining);
                remaining -= take;
                batch.Quantity -= take;

                decimal lineTotal = batch.SalePrice * take;

                sale.Lines.Add(new SaleLine
                {
                    MedicineBatchId = batch.Id,
                    Quantity = take,
                    UnitPrice = batch.SalePrice,
                    LineTotal = lineTotal
                });

                total += lineTotal;
            }
        }

        sale.TotalAmount = total;
        _db.Sales.Add(sale);
        await _db.SaveChangesAsync();

        return sale.Id;
    }
}
