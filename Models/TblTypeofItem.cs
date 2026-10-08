using System;
using System.Collections.Generic;

namespace VAapiNestle.Models;

public partial class TblTypeofItem
{
    public int Id { get; set; }

    public string? TypeofItem { get; set; }

    public string? TypeofJob { get; set; }

    public bool? Status { get; set; }

    public int? SubitemId { get; set; }
}
