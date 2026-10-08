using System;
using System.Collections.Generic;

namespace VAapiNestle.Models;

public partial class TblSupplierQuotesforfirstround
{
    public int Id { get; set; }

    public int? EnquiryId { get; set; }

    public int? EnquiryDetailsId { get; set; }

    public int? SupplierId { get; set; }

    public int? SupplierUserId { get; set; }

    public int? ItemNumber { get; set; }

    public decimal? Amount { get; set; }

    public int? QuoteStatus { get; set; }

    public DateTime? ServerTime { get; set; }

    public string? SupplierNotes { get; set; }

    public bool? SupplierResponse { get; set; }

    public DateTime? ResponseDate { get; set; }

    public decimal? InitAmount { get; set; }

    public decimal? SaveAmount { get; set; }
}
