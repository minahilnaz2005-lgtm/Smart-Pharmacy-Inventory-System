using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SPIEMS.DAL.Entities;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int DefaultShelfLifeDays { get; set; } = 90;
    public ICollection<Medicine> Medicines { get; set; } = new List<Medicine>();
}
