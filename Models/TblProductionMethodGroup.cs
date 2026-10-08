using System;
using System.Collections.Generic;

namespace VAapiNestle.Models;

public partial class TblProductionMethodGroup
{
    public int Id { get; set; }

    public int ProductCatecoryId { get; set; }

    public string GroupName { get; set; } = null!;

    public string Indonesia { get; set; } = null!;

    public string ChineseSimplified { get; set; } = null!;

    public string ChineseTraditional { get; set; } = null!;

    public string Japanese { get; set; } = null!;

    public string Korean { get; set; } = null!;
}
