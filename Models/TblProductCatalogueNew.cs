using System;
using System.Collections.Generic;

namespace VAapiNestle.Models;

public partial class TblProductCatalogueNew
{
    public int Id { get; set; }

    public int ProductCategoryId { get; set; }

    public string ProductName { get; set; } = null!;

    public int BaselinePrice { get; set; }

    public int RatePrice { get; set; }

    public string QuoteType { get; set; } = null!;

    public string? Savings { get; set; }

    public string? Supllier { get; set; }
}
