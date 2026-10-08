using System;
using System.Collections.Generic;

namespace VAapiNestle.Models;

public partial class TblSavingsDetail
{
    public int Id { get; set; }

    public int EnquiryId { get; set; }

    public int EnquiryDetailsId { get; set; }

    public string? ReasonForSaving { get; set; }

    public int? ItemNumber { get; set; }

    public string? PreviousSupplier { get; set; }

    public string? PreviousPrice { get; set; }

    public string? Note { get; set; }

    public decimal? ItemSellPrice { get; set; }

    public decimal? ManagementFee { get; set; }

    public DateTime? ServerTime { get; set; }

    public string? PreviousPriceReports { get; set; }

    public string? BaselineQuntity { get; set; }

    public string? Baselineunitprice { get; set; }

    public string? Baselineprice { get; set; }

    public string? Savings { get; set; }

    public decimal? Negitemsellprice { get; set; }

    public string? TotalNestleGainUnitPrice { get; set; }

    public string? TotalPmggainUnitPrice { get; set; }

    public string? TotalPmggainShareUnitPriceandnegamountandmfee { get; set; }

    public string? TotalPmggainShareSellPriceandnegamountandmfee { get; set; }

    public string? Gauranteedsavingsunitprice { get; set; }

    public string? GauranteedsavingPmgsellprice { get; set; }

    public string? GauranteedsavingPmgunitprice { get; set; }

    public string? TotalGainShareUnitPrice { get; set; }

    public string? GrossSavings { get; set; }

    public string? GauranteedPmgsavings { get; set; }

    public string? ManagementFeeAmt { get; set; }

    public string? GainShareforeachpart { get; set; }
}
