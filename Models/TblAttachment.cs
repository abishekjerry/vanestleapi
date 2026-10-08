using System;
using System.Collections.Generic;

namespace VAapiNestle.Models;

public partial class TblAttachment
{
    public int Id { get; set; }

    public int? EnqId { get; set; }

    public string? Type { get; set; }

    public string? Size { get; set; }

    public DateTime? UploadDate { get; set; }

    public string? Note { get; set; }

    public string? Status { get; set; }

    public int? Rowid { get; set; }

    public string? FileName { get; set; }

    public string? FileN { get; set; }

    public int? FileSize { get; set; }

    public string? CreatedBy { get; set; }

    public int? SupplierId { get; set; }

    public string? UserId { get; set; }
}
