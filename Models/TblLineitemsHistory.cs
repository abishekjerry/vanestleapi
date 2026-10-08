using System;
using System.Collections.Generic;

namespace VAapiNestle.Models;

public partial class TblLineitemsHistory
{
    public long Id { get; set; }

    public int? Enqid { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public int? Userid { get; set; }

    public string? Field { get; set; }

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    public int? ItemNumber { get; set; }
}
