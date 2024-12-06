using System;
using System.Collections.Generic;

namespace PrintWeb.Data;

public partial class DetailPaperStudent
{
    public int PaperTypeId { get; set; }

    public string StudentId { get; set; } = null!;

    public int Quantity { get; set; }

    public virtual PaperType PaperType { get; set; } = null!;

    public virtual Student Student { get; set; } = null!;
}
