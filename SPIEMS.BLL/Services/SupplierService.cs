using Microsoft.EntityFrameworkCore;
using SPIEMS.DAL;
using SPIEMS.DAL.Entities;

namespace SPIEMS.BLL.Services;

public class SupplierService
{
    private readonly SPIEMSDbContext _db;
    public SupplierService(SPIEMSDbContext db) => _db = db;

    public Task<List<Supplier>> GetAllAsync(bool includeInactive = true)
    {
        var q = _db.Suppliers.AsNoTracking();
        if (!includeInactive) q = q.Where(s => s.IsActive);
        return q.OrderBy(s => s.Name).ToListAsync();
    }

    public Task<Supplier?> GetByIdAsync(int id) =>
        _db.Suppliers.FirstOrDefaultAsync(s => s.Id == id);

    public async Task AddAsync(Supplier supplier)
    {
        supplier.IsActive = true;
        _db.Suppliers.Add(supplier);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Supplier supplier)
    {
        _db.Suppliers.Update(supplier);
        await _db.SaveChangesAsync();
    }

    public async Task DeactivateAsync(int id)
    {
        var s = await _db.Suppliers.FirstOrDefaultAsync(x => x.Id == id);
        if (s == null) throw new Exception("Supplier not found.");
        s.IsActive = false;
        await _db.SaveChangesAsync();
    }

    public async Task ActivateAsync(int id)
    {
        var s = await _db.Suppliers.FirstOrDefaultAsync(x => x.Id == id);
        if (s == null) throw new Exception("Supplier not found.");
        s.IsActive = true;
        await _db.SaveChangesAsync();
    }
}