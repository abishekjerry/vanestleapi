using System;
using System.Collections.Generic;

namespace VAapiNestle.Models;

public partial class TblProductMethod
{
    public int Id { get; set; }

    public string ProductionMethod { get; set; } = null!;

    public string Indonesia { get; set; } = null!;

    public string ChineseSimplified { get; set; } = null!;

    public string ChineseTraditional { get; set; } = null!;

    public string Japanese { get; set; } = null!;

    public string Korean { get; set; } = null!;

    public bool? Status { get; set; }
}
