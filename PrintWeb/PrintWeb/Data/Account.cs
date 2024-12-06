using System;
using System.Collections.Generic;

namespace PrintWeb.Data;

public partial class Account
{
    public string AccountId { get; set; } = null!;

    public string Password { get; set; } = null!;

    public virtual Student AccountNavigation { get; set; } = null!;
}
