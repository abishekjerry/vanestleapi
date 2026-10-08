using System;
using System.Collections.Generic;

namespace VAapiNestle.Models;

public partial class TblSuppliersList
{
    public int Id { get; set; }

    public int? PrintCapability { get; set; }

    public string SupplierName { get; set; } = null!;

    public string? SupplierNameEn { get; set; }

    public string AddressLine1 { get; set; } = null!;

    public string? AddressLine2 { get; set; }

    public string? AddressLine3 { get; set; }

    public string? AddressLine4 { get; set; }

    public string City { get; set; } = null!;

    public string? ClientName { get; set; }

    public string Country { get; set; } = null!;

    public string Language { get; set; } = null!;

    public string Currency { get; set; } = null!;

    public bool Status { get; set; }

    public int Productioncapability { get; set; }

    public string? Natureofsupplier { get; set; }

    public string? SupplierType { get; set; }

    public string? Smetaaccredited { get; set; }

    public string? Gmpaccredited { get; set; }

    public string? Suppliercode { get; set; }
}
