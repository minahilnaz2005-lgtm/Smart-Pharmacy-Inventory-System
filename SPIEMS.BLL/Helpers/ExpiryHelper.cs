using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPIEMS.BLL.Helpers;

public static class ExpiryHelper
{
    public static int DaysToExpiry(DateTime? expiryDate)
    {
        if (expiryDate == null)
            return int.MaxValue;

        return (expiryDate.Value.Date - DateTime.Today).Days;
    }

    public static string GetStatus(int daysLeft)
    {
        if (daysLeft <= 7) return "Critical";
        if (daysLeft <= 30) return "Warning";
        return "Safe";
    }
}
