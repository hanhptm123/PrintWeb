using System;
using System.Collections.Generic;

namespace PrintWeb.Data;

public partial class PrintingLog
{
    public int PrintingLogId { get; set; }

    public string? StudentId { get; set; }

    public string? PrinterId { get; set; }

    public int? DocumentId { get; set; }

    public int? PaperTypeId { get; set; }

    public string? Colored { get; set; }

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public int? Quantity { get; set; }

    public bool? IsTwoSide { get; set; }

    public string? Status { get; set; }

    public virtual Document? Document { get; set; }

    public virtual PaperType? PaperType { get; set; }

    public virtual Printer? Printer { get; set; }

    public virtual Student? Student { get; set; }
}
