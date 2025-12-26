using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPIEMS.DAL.Entities;

public class Medicine
{
    public int Id { get; set; }
    public string GenericName { get; set; } = "";
    public string BrandName { get; set; } = "";
    public string Company { get; set; } = "";
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
    public int? DefaultShelfLifeDays { get; set; }
    public int ReorderLevel { get; set; } = 10;
    public ICollection<MedicineBatch> Batches { get; set; } = new List<MedicineBatch>();
}
