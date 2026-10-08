using System;
using System.Collections.Generic;

namespace VAapiNestle.Models;

public partial class TblClient28072016
{
    public int Id { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string? JobTitle { get; set; }

    public string Email { get; set; } = null!;

    public string? Phone { get; set; }

    public int Department { get; set; }

    public bool Status { get; set; }
}
