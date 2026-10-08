using System;
using System.Collections.Generic;

namespace VAapiNestle.Models;

public partial class Tblpwdhistory
{
    public int Id { get; set; }

    public int? Userid { get; set; }

    public string? Username { get; set; }

    public string? Password { get; set; }

    public string? Usertype { get; set; }

    public DateTime? Createddate { get; set; }

    public string? Portal { get; set; }

    public string? Ip { get; set; }
}
