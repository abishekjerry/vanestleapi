using System;
using System.Collections.Generic;

namespace VAapiNestle.Models;

public partial class TblLog
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public DateTime LoginDate { get; set; }

    public string? Location { get; set; }

    public string? Ipaddress { get; set; }
}
