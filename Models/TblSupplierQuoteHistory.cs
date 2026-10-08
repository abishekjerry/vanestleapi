using System;
using System.Collections.Generic;

namespace VAapiNestle.Models;

public partial class TblSupplierQuoteHistory
{
    public long Id { get; set; }

    public int? EnquiryId { get; set; }

    public int? EnquiryDetailsId { get; set; }

    public int? SupplierId { get; set; }

    public int? SupplierUserId { get; set; }

    public int? ItemNumber { get; set; }

    public decimal? Amount { get; set; }

    public int? QuoteStatus { get; set; }

    public string? SupplierNotes { get; set; }

    public DateTime? ServerTime { get; set; }

    public int Status { get; set; }
}
