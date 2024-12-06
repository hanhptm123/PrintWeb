using System;
using System.Collections.Generic;

namespace PrintWeb.Data;

public partial class PrintingLog
{
    public int PrintingLogId { get; set; }

    public string StudentId { get; set; } = null!;

    public string PrinterId { get; set; } = null!;

    public int DocumentId { get; set; }

    public int PaperTypeId { get; set; }

    public string Colored { get; set; } = null!;

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public int Quantity { get; set; }

    public bool IsTwoSide { get; set; }

    public string Status { get; set; } = null!;

    public virtual Document Document { get; set; } = null!;

    public virtual PaperType PaperType { get; set; } = null!;

    public virtual Printer Printer { get; set; } = null!;

    public virtual Student Student { get; set; } = null!;
}
