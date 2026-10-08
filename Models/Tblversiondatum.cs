using System;
using System.Collections.Generic;

namespace VAapiNestle.Models;

public partial class Tblversiondatum
{
    public int Versionid { get; set; }

    public int? Enqid { get; set; }

    public int? Itemnumber { get; set; }

    public int? Versionno { get; set; }

    public decimal? InitUnitprice { get; set; }

    public decimal? NegoUnitprice { get; set; }

    public decimal? InitQuoteamount { get; set; }

    public decimal? NegoQuoteamount { get; set; }

    public decimal? UnitpricewithMf { get; set; }

    public decimal? Sellprice { get; set; }

    public int? Supplierid { get; set; }

    public bool? Selected { get; set; }

    public DateTime? Modifydate { get; set; }

    public int? Qty { get; set; }
}
