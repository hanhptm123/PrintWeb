using System;
using System.Collections.Generic;

namespace PrintWeb.Data;

public partial class Printer
{
    public string PrinterId { get; set; } = null!;

    public string Brand { get; set; } = null!;

    public string Model { get; set; } = null!;

    public string? Description { get; set; }

    public string CampusName { get; set; } = null!;

    public string BuildingName { get; set; } = null!;

    public int RoomNumber { get; set; }

    public virtual ICollection<DetailPaperPrinter> DetailPaperPrinters { get; set; } = new List<DetailPaperPrinter>();

    public virtual ICollection<PrintingLog> PrintingLogs { get; set; } = new List<PrintingLog>();
}
