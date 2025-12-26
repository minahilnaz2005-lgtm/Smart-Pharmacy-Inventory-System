using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SPIEMS.DAL;
using SPIEMS.DAL.Entities;

namespace SPIEMS.BLL.Services;

public class BatchService
{
    private readonly SPIEMSDbContext _db;
    private readonly ExpiryPredictionService _expiry;

    public BatchService(SPIEMSDbContext db, ExpiryPredictionService expiry)
    {
        _db = db;
        _expiry = expiry;
    }

    public async Task AddBatchAsync(MedicineBatch batch)
    {
        if (batch.ExpiryDate == null)
            batch.ExpiryDate = await _expiry.PredictExpiryAsync(batch.MedicineId, batch.PurchaseDate);

        _db.MedicineBatches.Add(batch);
        await _db.SaveChangesAsync();
    }

    public async Task<List<MedicineBatch>> GetAllBatchesAsync()
    {
        return await _db.MedicineBatches
            .Include(b => b.Medicine!)
            .ThenInclude(m => m.Category!)
            .OrderByDescending(b => b.Id)
            .ToListAsync();
    }
}