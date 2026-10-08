using System;
using System.Collections.Generic;

namespace VAapiNestle.Models;

public partial class Tblcataloglogindetail
{
    public int Id { get; set; }

    public string? Username { get; set; }

    public string? Password { get; set; }

    public string? Type { get; set; }

    public string? Emailid { get; set; }

    public DateTime? Crdate { get; set; }

    public string? Uniquetoken { get; set; }
}
