using System.ComponentModel.DataAnnotations;

namespace SPIEMS.DAL.Entities;

public class Sale
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    public DateTime SaleDate { get; set; } = DateTime.Now;

    // 🔴 IMPORTANT: this already exists in DB
    [Required]
    public string Status { get; set; } = "Checkout";

    public decimal TotalAmount { get; set; }

    // ✅ ONLY NEW PROPERTY
    public string? ProcessedBy { get; set; }

    public ICollection<SaleLine> Lines { get; set; } = new List<SaleLine>();
}
