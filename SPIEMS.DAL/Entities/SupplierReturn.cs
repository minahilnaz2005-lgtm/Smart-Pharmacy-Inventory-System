namespace SPIEMS.DAL.Entities;

public class SupplierReturn
{
    public int Id { get; set; }

    public int SupplierId { get; set; }
    public Supplier? Supplier { get; set; }

    public int MedicineBatchId { get; set; }
    public MedicineBatch? MedicineBatch { get; set; }

    public int Quantity { get; set; }
    public DateTime ReturnDate { get; set; } = DateTime.Now;

    public string Reason { get; set; } = "Near Expiry";
}