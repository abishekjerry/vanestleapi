using System;
using System.Collections.Generic;

namespace VAapiNestle.Models;

public partial class TblJobHistory
{
    public long Id { get; set; }

    public int? EnqId { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public int? UserId { get; set; }

    public string? Field { get; set; }

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }
}
