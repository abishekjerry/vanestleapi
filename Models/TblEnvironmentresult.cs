using System;
using System.Collections.Generic;

namespace VAapiNestle.Models;

public partial class TblEnvironmentresult
{
    public int Id { get; set; }

    public int? Enqid { get; set; }

    public int? EnqDetailsid { get; set; }

    public int? Itemnumber { get; set; }

    public string? Type { get; set; }

    public string? Materialused { get; set; }

    public string? EnvironmentalPaperName { get; set; }

    public string? EnvironmentalPaperGrade { get; set; }

    public string? EnvironmentalPaperQuantity { get; set; }

    public string? EnvironmentalPaperUnit { get; set; }

    public string? EnvironmentalPaperRecycled { get; set; }

    public string? EnvironmentalWood { get; set; }

    public string? EnvironmentalEnergy { get; set; }

    public string? EnvironmentalGhg { get; set; }

    public string? EnvironmentalWater { get; set; }

    public string? CompareEnvironmentalPaperName { get; set; }

    public string? CompareEnvironmentalPaperGrade { get; set; }

    public string? CompareEnvironmentalPaperQuantity { get; set; }

    public string? CompareEnvironmentalPaperUnit { get; set; }

    public string? CompareEnvironmentalPaperRecycled { get; set; }

    public string? CompareEnvironmentalWood { get; set; }

    public string? CompareEnvironmentalEnergy { get; set; }

    public string? CompareEnvironmentalGhg { get; set; }

    public string? CompareEnvironmentalWater { get; set; }

    public string? EnvironmentalWooddensity { get; set; }

    public string? CompareEnvReason { get; set; }
}
