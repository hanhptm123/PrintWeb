using System;
using System.Collections.Generic;

namespace PrintWeb.Data;

public partial class Student
{
    public string StudentId { get; set; } = null!;

    public string? StudentName { get; set; }

    public string? Email { get; set; }

    public decimal? AccountBalance { get; set; }

    public virtual Account? Account { get; set; }

    public virtual ICollection<BuyPaperLog> BuyPaperLogs { get; set; } = new List<BuyPaperLog>();

    public virtual ICollection<DetailPaperStudent> DetailPaperStudents { get; set; } = new List<DetailPaperStudent>();

    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();

    public virtual ICollection<PaymentRecord> PaymentRecords { get; set; } = new List<PaymentRecord>();

    public virtual ICollection<PrintingLog> PrintingLogs { get; set; } = new List<PrintingLog>();
}
