using System;
using System.Collections.Generic;

namespace VAapiNestle.Models;

public partial class Tempdivision
{
    public int Id { get; set; }

    public int ClientId { get; set; }

    public string ClientName { get; set; } = null!;

    public string Country { get; set; } = null!;

    public string EntityName { get; set; } = null!;

    public string BussinessUnit { get; set; } = null!;

    public string DepartmentOrProduct { get; set; } = null!;

    public bool Status { get; set; }

    public string? GlobalBumapping { get; set; }
}
