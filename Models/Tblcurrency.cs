using System;
using System.Collections.Generic;

namespace VAapiNestle.Models;

public partial class Tblcurrency
{
    public int Id { get; set; }

    public string? Currency { get; set; }

    public decimal? Value { get; set; }

    public DateTime? Crdate { get; set; }
}
