using System;
using System.Collections.Generic;

namespace VAapiNestle.Models;

public partial class TblEnvironmentEmission
{
    public int Id { get; set; }

    public string? Country { get; set; }

    public string? Gef { get; set; }

    public bool? Status { get; set; }
}
