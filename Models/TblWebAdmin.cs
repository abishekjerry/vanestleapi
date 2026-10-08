using System;
using System.Collections.Generic;

namespace VAapiNestle.Models;

public partial class TblWebAdmin
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string UserType { get; set; } = null!;
}
