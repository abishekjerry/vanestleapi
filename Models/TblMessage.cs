using System;
using System.Collections.Generic;

namespace VAapiNestle.Models;

public partial class TblMessage
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public int? FromId { get; set; }

    public int? ToId { get; set; }

    public string? Message { get; set; }

    public DateTime? Sent { get; set; }

    public DateTime? Received { get; set; }

    public bool? ReadStatus { get; set; }

    public bool? Status { get; set; }
}
