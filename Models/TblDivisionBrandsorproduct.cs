using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VAapiNestle.Models;

public partial class TblDivisionBrandsorproduct
{
    [Key]
    public int Id { get; set; }

    public int? Divisionid { get; set; }

    public string? Brandorproduct { get; set; }
}
