using System;
using System.Collections.Generic;

namespace VAapiNestle.Models;

public partial class TblSupplierUser
{
    public int Id { get; set; }

    public int? SupplierId { get; set; }

    public string FirstName { get; set; } = null!;

    public string? LastName { get; set; }

    public string? JobTitle { get; set; }

    public string? EmailAddressUat { get; set; }

    public string EmailAddressLive { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public bool ReceiveNotification { get; set; }

    public bool Status { get; set; }

    public DateTime? Servertime { get; set; }
}
