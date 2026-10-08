using System;
using System.Collections.Generic;

namespace VAapiNestle.Models;

public partial class TblProductCatalogue
{
    public int Id { get; set; }

    public int? ProductCategoryId { get; set; }

    public string? ProductName { get; set; }

    public string? ProductSpecification { get; set; }

    public string? Weight { get; set; }

    public string? Thickness { get; set; }

    public string? FinishedSize { get; set; }

    public string? PrintingProcess { get; set; }

    public string? Quote { get; set; }

    public double? BaselinePricePerUnit { get; set; }

    public double? RatePricePerUnit { get; set; }
}
