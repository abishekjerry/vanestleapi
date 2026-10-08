using System;
using System.Collections.Generic;

namespace VAapiNestle.Models;

public partial class ViewLogonHistory
{
    public string UserName { get; set; } = null!;

    public string UserType { get; set; } = null!;

    public DateTime LoginDate { get; set; }

    public string? Location { get; set; }

    public string? Ipaddress { get; set; }

    public int UserId { get; set; }

    public int Id { get; set; }
}
