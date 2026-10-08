using System;
using System.Collections.Generic;

namespace VAapiNestle.Models;

public partial class Tblenvironmentcomparison
{
    public int Id { get; set; }

    public string? CEmbodiedCarbon { get; set; }

    public string? CWaterUsage { get; set; }

    public string? CEnergyConsumption { get; set; }

    public string? CWoodUse { get; set; }

    public string? WoodDensity { get; set; }
}
