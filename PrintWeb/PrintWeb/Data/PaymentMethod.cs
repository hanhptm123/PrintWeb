using System;
using System.Collections.Generic;

namespace PrintWeb.Data;

public partial class PaymentMethod
{
    public int PaymentMethodId { get; set; }

    public string PaymentName { get; set; } = null!;

    public virtual ICollection<PaymentRecord> PaymentRecords { get; set; } = new List<PaymentRecord>();
}
