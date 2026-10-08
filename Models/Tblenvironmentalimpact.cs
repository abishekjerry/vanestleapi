using System;
using System.Collections.Generic;

namespace VAapiNestle.Models;

public partial class Tblenvironmentalimpact
{
    public int Id { get; set; }

    public string? Material { get; set; }

    public string? Type { get; set; }

    public string? EmbodiedCarbon { get; set; }

    public string? WaterUsage { get; set; }

    public string? EnergyConsumption { get; set; }

    public string? WoodUse { get; set; }

    public bool? Status { get; set; }

    public string? MaterialUsed { get; set; }
}
