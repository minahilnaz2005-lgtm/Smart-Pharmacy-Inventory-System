using Microsoft.EntityFrameworkCore;
using SPIEMS.DAL;
using SPIEMS.DAL.Entities;

namespace SPIEMS.BLL.Services;

public class ReturnToSupplierService
{
    private readonly SPIEMSDbContext _db;

    public ReturnToSupplierService(SPIEMSDbContext db)
    {
        _db = db;
    }

    public async Task ReturnAsync(int supplierId, int batchId, int quantity, string? reason = null)
    {
        if (quantity <= 0)
            throw new Exception("Quantity must be greater than 0.");

        var supplier = await _db.Suppliers.FirstOrDefaultAsync(s => s.Id == supplierId && s.IsActive);
        if (supplier == null)
            throw new Exception("Supplier not found or inactive.");

        var batch = await _db.MedicineBatches
            .Include(b => b.Medicine)
            .FirstOrDefaultAsync(b => b.Id == batchId);

        if (batch == null)
            throw new Exception("Batch not found.");

        if (batch.Quantity < quantity)
            throw new Exception("Not enough quantity in this batch.");

        // deduct stock
        batch.Quantity -= quantity;

        // log return
        _db.SupplierReturns.Add(new SupplierReturn
        {
            SupplierId = supplierId,
            MedicineBatchId = batchId,
            Quantity = quantity,
            ReturnDate = DateTime.Now,
            Reason = string.IsNullOrWhiteSpace(reason) ? "Near Expiry" : reason
        });

        await _db.SaveChangesAsync();
    }

    public async Task<List<SupplierReturn>> GetAllReturnsAsync()
    {
        return await _db.SupplierReturns
            .AsNoTracking()
            .Include(r => r.Supplier)
            .Include(r => r.MedicineBatch)
                .ThenInclude(b => b.Medicine)
            .OrderByDescending(r => r.ReturnDate)
            .ToListAsync();
    }

    public async Task<List<MedicineBatch>> GetNearExpiryBatchesAsync(int days = 30)
    {
        var today = DateTime.Today;
        var until = today.AddDays(days);

        return await _db.MedicineBatches
            .AsNoTracking()
            .Include(b => b.Medicine)
            .Where(b => b.Quantity > 0 &&
                        b.ExpiryDate != null &&
                        b.ExpiryDate.Value >= today &&
                        b.ExpiryDate.Value <= until)
            .OrderBy(b => b.ExpiryDate)
            .ToListAsync();
    }
}