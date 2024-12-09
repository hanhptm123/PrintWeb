using System;
using System.Collections.Generic;

namespace PrintWeb.Data;

public partial class PaymentRecord
{
    public string PaymentId { get; set; } = null!;

    public string? StudentId { get; set; }

    public decimal? Amount { get; set; }

    public int? PaymentMethod { get; set; }

    public DateTime? PaymentDate { get; set; }

    public virtual PaymentMethod? PaymentMethodNavigation { get; set; }

    public virtual Student? Student { get; set; }
}
