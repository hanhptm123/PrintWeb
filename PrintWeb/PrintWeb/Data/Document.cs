using System;
using System.Collections.Generic;

namespace PrintWeb.Data;

public partial class Document
{
    public int DocumentId { get; set; }

    public string StudentId { get; set; } = null!;

    public string FileName { get; set; } = null!;

    public string FileType { get; set; } = null!;

    public DateTime UploadedAt { get; set; }

    public virtual ICollection<PrintingLog> PrintingLogs { get; set; } = new List<PrintingLog>();

    public virtual Student Student { get; set; } = null!;
}
