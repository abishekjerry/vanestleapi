using System;
using System.Collections.Generic;

namespace VAapiNestle.Models;

public partial class TblCategoryField
{
    public int FieldId { get; set; }

    public string? FieldName { get; set; }

    public string? Indonesia { get; set; }

    public string? ChineseSimplified { get; set; }

    public string? ChineseTraditional { get; set; }

    public string? Japanese { get; set; }

    public string? Korean { get; set; }

    public int? GroupId { get; set; }

    public string? FieldType { get; set; }

    public bool? IsMandatory { get; set; }
}
