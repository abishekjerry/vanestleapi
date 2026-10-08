using System;
using System.Collections.Generic;

namespace VAapiNestle.Models;

public partial class TblSurvey
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Email { get; set; }

    public string? Responsive { get; set; }

    public string? Accuracy { get; set; }

    public string? Quality { get; set; }

    public string? OvarallExp { get; set; }

    public string? Comments { get; set; }

    public int? EnqId { get; set; }

    public string? EnqUid { get; set; }

    public string? Country { get; set; }

    public DateTime? SurveyDate { get; set; }

    public string? SubmitedBy { get; set; }

    public string? Deliverytarget { get; set; }

    public string? QualityComments { get; set; }

    public string? AccuracyComments { get; set; }

    public string? OvarallExpComments { get; set; }

    public string? DeliverytargetComments { get; set; }
}
