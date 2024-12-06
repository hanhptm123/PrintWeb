using System;
using System.Collections.Generic;

namespace PrintWeb.Data;

public partial class PaperType
{
    public int PaperTypeId { get; set; }

    public string PaperTypeName { get; set; } = null!;

    public decimal Price { get; set; }

    public virtual ICollection<DetailBuyPaperLog> DetailBuyPaperLogs { get; set; } = new List<DetailBuyPaperLog>();

    public virtual ICollection<DetailPaperPrinter> DetailPaperPrinters { get; set; } = new List<DetailPaperPrinter>();

    public virtual ICollection<DetailPaperStudent> DetailPaperStudents { get; set; } = new List<DetailPaperStudent>();

    public virtual ICollection<PrintingLog> PrintingLogs { get; set; } = new List<PrintingLog>();
}
