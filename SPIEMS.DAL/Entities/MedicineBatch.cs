using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPIEMS.DAL.Entities;

public class MedicineBatch
{
    public int Id { get; set; }
    public int MedicineId { get; set; }
    public Medicine? Medicine { get; set; }
    public string BatchNo { get; set; } = "";
    public DateTime PurchaseDate { get; set; } = DateTime.Today;
    public DateTime? ExpiryDate { get; set; }
    public int Quantity { get; set; }
    public decimal CostPrice { get; set; }
    public decimal SalePrice { get; set; }

    public int? SupplierId { get; set; }
    public Supplier? Supplier { get; set; }
}
