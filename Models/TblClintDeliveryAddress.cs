using System;
using System.Collections.Generic;

namespace VAapiNestle.Models;

public partial class TblClintDeliveryAddress
{
    public int Id { get; set; }

    public int? Enqid { get; set; }

    public string? CompanyName { get; set; }

    public string? AdressLine1 { get; set; }

    public string? AdressLine2 { get; set; }

    public string? AdressLine3 { get; set; }

    public string? NameorDept { get; set; }

    public string? ContactNo { get; set; }

    public string? Remarks { get; set; }
}
