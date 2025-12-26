namespace SPIEMS.DAL.Entities;

public class SaleLine
{
    public int Id { get; set; }

    public int SaleId { get; set; }
    public Sale? Sale { get; set; }

    public int MedicineBatchId { get; set; }
    public MedicineBatch? MedicineBatch { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}