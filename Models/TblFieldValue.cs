using System;
using System.Collections.Generic;

namespace VAapiNestle.Models;

public partial class TblFieldValue
{
    public int FieldValueId { get; set; }

    public string? FieldValue { get; set; }

    public string? Indonesia { get; set; }

    public string? ChineseSimplified { get; set; }

    public string? ChineseTraditional { get; set; }

    public string? Japanese { get; set; }

    public string? Korean { get; set; }

    public int FieldId { get; set; }
}
