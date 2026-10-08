using System;
using System.Collections.Generic;

namespace VAapiNestle.Models;

public partial class TblMaterialused
{
    public int Id { get; set; }

    public string? Materialused { get; set; }

    public bool? Status { get; set; }

    public string? TypeofJob { get; set; }

    public string? Envimp { get; set; }

    public string? ComEnvimp { get; set; }
}
