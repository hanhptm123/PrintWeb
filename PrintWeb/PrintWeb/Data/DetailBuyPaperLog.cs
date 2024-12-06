using System;
using System.Collections.Generic;

namespace PrintWeb.Data;

public partial class DetailBuyPaperLog
{
    public int BuyPaperLogId { get; set; }

    public int PaperTypeId { get; set; }

    public int PaperBuy { get; set; }

    public virtual BuyPaperLog BuyPaperLog { get; set; } = null!;

    public virtual PaperType PaperType { get; set; } = null!;
}
