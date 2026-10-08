using System;
using System.Collections.Generic;

namespace VAapiNestle.Models;

public partial class TblCatalogMailDraft
{
    public int Id { get; set; }

    public string? Market { get; set; }

    public string? JobType { get; set; }

    public string? ProjectManager { get; set; }

    public string? TeamLead { get; set; }

    public bool? Status { get; set; }
}
