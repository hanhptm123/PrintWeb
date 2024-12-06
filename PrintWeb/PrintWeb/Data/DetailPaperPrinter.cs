using System;
using System.Collections.Generic;

namespace PrintWeb.Data;

public partial class DetailPaperPrinter
{
    public int PaperTypeId { get; set; }

    public string PrinterId { get; set; } = null!;

    public int Quantity { get; set; }

    public virtual PaperType PaperType { get; set; } = null!;

    public virtual Printer Printer { get; set; } = null!;
}
