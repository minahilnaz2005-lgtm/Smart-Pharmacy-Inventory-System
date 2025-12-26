namespace SPIEMS.Web.Models;

public class CartItemVm
{
    public int MedicineId { get; set; }
    public string MedicineName { get; set; }
    public int Quantity { get; set; }
    public decimal EstimatedPrice { get; set; }
}
