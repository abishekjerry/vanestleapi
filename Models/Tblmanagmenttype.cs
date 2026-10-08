using System;
using System.Collections.Generic;

namespace VAapiNestle.Models;

public partial class Tblmanagmenttype
{
    public int Id { get; set; }

    public string? Managementfeetypename { get; set; }

    public decimal? Managementtypevalue { get; set; }

    public DateTime? Servertime { get; set; }

    public string? Status { get; set; }
}
