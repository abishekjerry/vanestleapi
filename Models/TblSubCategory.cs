using System;
using System.Collections.Generic;

namespace VAapiNestle.Models;

public partial class TblSubCategory
{
    public int Id { get; set; }

    public string? SubCategory { get; set; }

    public bool Status { get; set; }

    public string? Japanese { get; set; }

    public string? Zsim { get; set; }

    public bool? MarkedTypeofItem { get; set; }

    public string? CatAbc { get; set; }

    public string? Typeofjob { get; set; }

    public int? ProductId { get; set; }

    public string? SapMaterialcode { get; set; }
}
