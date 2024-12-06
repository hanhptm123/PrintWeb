using System;
using System.Collections.Generic;

namespace PrintWeb.Data;

public partial class PaymentRecord
{
    public string PaymentId { get; set; } = null!;

    public string StudentId { get; set; } = null!;

    public decimal Amount { get; set; }

    public int PaymentMethod { get; set; }

    public DateTime PaymentDate { get; set; }

    public virtual PaymentMethod PaymentMethodNavigation { get; set; } = null!;

    public virtual Student Student { get; set; } = null!;
}
