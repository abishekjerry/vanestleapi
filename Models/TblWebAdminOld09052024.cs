using System;
using System.Collections.Generic;

namespace VAapiNestle.Models;

public partial class TblWebAdminOld09052024
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string UserType { get; set; } = null!;
}
