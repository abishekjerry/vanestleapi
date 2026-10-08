using System;
using System.Collections.Generic;

namespace VAapiNestle.Models;

public partial class TblCategoryGroup
{
    public int GroupId { get; set; }

    public string? GroupName { get; set; }

    public string? Indonesia { get; set; }

    public string? ChineseSimplified { get; set; }

    public string? ChineseTraditional { get; set; }

    public string? Japanese { get; set; }

    public string? Korean { get; set; }

    public int? ProductCategoryId { get; set; }

    public int? OrderById { get; set; }

    public bool? Status { get; set; }
}
