using System;
using System.Collections.Generic;

namespace VAapiNestle.Models;

public partial class TblClientDepartment
{
    public int Id { get; set; }

    public int ClientId { get; set; }

    public int DivisionId { get; set; }

    public bool Status { get; set; }
}
