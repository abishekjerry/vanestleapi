using System;
using System.Collections.Generic;

namespace VAapiNestle.Models;

public partial class EbTblPauseExtend
{
    public int Id { get; set; }

    public int EnquiryId { get; set; }

    public string Type { get; set; } = null!;

    public int Duration { get; set; }

    public string Reason { get; set; } = null!;

    public string? OtherReason { get; set; }

    public DateTime CreatedDate { get; set; }
}
