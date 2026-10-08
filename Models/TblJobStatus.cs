using System;
using System.Collections.Generic;

namespace VAapiNestle.Models;

public partial class TblJobStatus
{
    public int StatusId { get; set; }

    public string StatusName { get; set; } = null!;

    public string? VisibleSuppliersStatus { get; set; }
}
