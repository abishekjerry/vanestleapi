using System;
using System.Collections.Generic;

namespace VAapiNestle.Models;

public partial class TblEnquirySpecification
{
    public int Id { get; set; }

    public int? GroupId { get; set; }

    public int? FieldId { get; set; }

    public int? EnqDetailsId { get; set; }

    public int? EnquiryId { get; set; }

    public string? Value { get; set; }
}
