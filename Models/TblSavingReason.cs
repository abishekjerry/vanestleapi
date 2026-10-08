using System;
using System.Collections.Generic;

namespace VAapiNestle.Models;

public partial class TblSavingReason
{
    public int Id { get; set; }

    public string? SavingReson { get; set; }

    public bool Status { get; set; }

    public string? Savingstype { get; set; }
}
