using System;
using System.Collections.Generic;

namespace PrintWeb.Data;

public partial class BuyPaperLog
{
    public int BuyPaperLogId { get; set; }

    public string StudentId { get; set; } = null!;

    public DateTime DateBuy { get; set; }

    public decimal TotalBuy { get; set; }

    public virtual ICollection<DetailBuyPaperLog> DetailBuyPaperLogs { get; set; } = new List<DetailBuyPaperLog>();

    public virtual Student Student { get; set; } = null!;
}
