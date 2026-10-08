using System;
using System.Collections.Generic;

namespace VAapiNestle.Models;

public partial class TblUser
{
    public int Id { get; set; }

    public int FkId { get; set; }

    public string UserName { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string UserType { get; set; } = null!;

    public string? LastLogon { get; set; }

    public string? UniqueId { get; set; }

    public string? Ip { get; set; }

    public bool Status { get; set; }

    public string? ClientMembers { get; set; }

    public string? Portal { get; set; }

    public bool? It { get; set; }

    public bool? Papm { get; set; }

    public string? PapmToken { get; set; }
}
