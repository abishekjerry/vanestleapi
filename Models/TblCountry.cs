using System;
using System.Collections.Generic;

namespace VAapiNestle.Models;

public partial class TblCountry
{
    public int Id { get; set; }

    public string Country { get; set; } = null!;

    public string Code { get; set; } = null!;

    public string Currency { get; set; } = null!;

    public string Language { get; set; } = null!;

    public string RegionalSetting { get; set; } = null!;

    public int? TimeOffset { get; set; }

    public string TaxPercentage { get; set; } = null!;

    public bool Status { get; set; }

    public bool? Sapneeded { get; set; }
}
