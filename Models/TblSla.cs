using System;
using System.Collections.Generic;

namespace VAapiNestle.Models;

public partial class TblSla
{
    public int Id { get; set; }

    public string Description { get; set; } = null!;

    public int Quote { get; set; }

    public int Proof { get; set; }

    public int Production { get; set; }

    public int FileCopies { get; set; }

    public int Invoicing { get; set; }

    public bool IsDefault { get; set; }

    public bool Status { get; set; }

    public DateTime ServerTime { get; set; }
    public string Country { get; set; } 
}
