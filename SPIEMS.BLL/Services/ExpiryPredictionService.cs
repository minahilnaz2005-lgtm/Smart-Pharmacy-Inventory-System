using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SPIEMS.DAL;

namespace SPIEMS.BLL.Services;

public class ExpiryPredictionService
{
    private readonly SPIEMSDbContext _db;

    public ExpiryPredictionService(SPIEMSDbContext db) => _db = db;

    public async Task<DateTime> PredictExpiryAsync(int medicineId, DateTime purchaseDate)
    {
        var med = await _db.Medicines.Include(m => m.Category)
            .FirstOrDefaultAsync(m => m.Id == medicineId);

        var shelfLife = med?.DefaultShelfLifeDays
            ?? med?.Category?.DefaultShelfLifeDays
            ?? 90;

        return purchaseDate.Date.AddDays(shelfLife);
    }
}