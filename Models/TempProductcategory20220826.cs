using System;
using System.Collections.Generic;

namespace VAapiNestle.Models;

public partial class TempProductcategory20220826
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
}
