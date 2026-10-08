using System;
using System.Collections.Generic;

namespace VAapiNestle.Models;

public partial class TblAgencyUser
{
    public int Id { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? JobTitle { get; set; }

    public string? Country { get; set; }

    public string? JobRole { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public bool? ReceiveQuoteNotification { get; set; }

    public bool? ReceiveOrderNotification { get; set; }

    public bool? Status { get; set; }

    public string? JobPosition { get; set; }

    public int? Userid { get; set; }

    public bool? Reportenabled { get; set; }
}
