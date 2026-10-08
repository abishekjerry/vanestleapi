using System;
using System.Collections.Generic;

namespace VAapiNestle.Models;

public partial class TblProductCategory
{
    public int Id { get; set; }

    public string Category { get; set; } = null!;

    public string Indonesia { get; set; } = null!;

    public string ChineseSimplified { get; set; } = null!;

    public string ChineseTraditional { get; set; } = null!;

    public string Japanese { get; set; } = null!;

    public string Korean { get; set; } = null!;

    public bool? Status { get; set; }

    public string? Categorytype { get; set; }

    public string? CategoryJob { get; set; }

    public string? Country { get; set; }

    public string? Materialcode { get; set; }
}
