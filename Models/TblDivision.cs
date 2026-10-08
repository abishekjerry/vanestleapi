using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace VAapiNestle.Models;

public partial class TblDivision
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

    public string? Clientcode { get; set; }

    [NotMapped]
    public string? AddressLine1 { get; set; }

    [NotMapped]
    public string? AddressLine2 { get; set; }

    [NotMapped]
    public string? AddressLine3 { get; set; }

    [NotMapped]
    public string? City { get; set; }
}
