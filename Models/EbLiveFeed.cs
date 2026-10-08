using System;
using System.Collections.Generic;

namespace VAapiNestle.Models;

public partial class EbLiveFeed
{
    public int Id { get; set; }

    public int EnquryId { get; set; }

    public string Feed { get; set; } = null!;

    public string FeedType { get; set; } = null!;

    public DateTime CreatedDate { get; set; }
}
