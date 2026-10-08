using System;
using System.Collections.Generic;

namespace VAapiNestle.Models;

public partial class TblEnquiryMaster
{
    public int Id { get; set; }

    public string? EnqUid { get; set; }

    public int? ClientId { get; set; }

    public int DivisionId { get; set; }

    public int ClientContactId { get; set; }

    public int DeliveryCountry { get; set; }

    public int PmgEntity { get; set; }

    public string? JobType { get; set; }

    public string? ProjectNo { get; set; }

    public string? ProjectDesc { get; set; }

    public string? EstDate { get; set; }

    public string? Ponumber { get; set; }

    public int CreatedBy { get; set; }

    public int ModifiedBy { get; set; }

    public DateTime ServerTime { get; set; }

    public bool Status { get; set; }

    public DateTime? DeleteDate { get; set; }

    public string? Reason { get; set; }

    public DateTime? BiddingStartDate { get; set; }

    public DateTime? BiddingEndDate { get; set; }

    public int? Duration { get; set; }

    public bool? IsFirstRemainderSent { get; set; }

    public bool? IsSecondRemainderSent { get; set; }

    public int? TemplateId { get; set; }

    public int? StepId { get; set; }

    public string? CancelReason { get; set; }

    public DateTime? CancelDate { get; set; }

    public int? PauseDuration { get; set; }

    public int PauseTime { get; set; }

    public int TotalPauseDuration { get; set; }

    public int? CancelatStatus { get; set; }

    public string? DeletReason { get; set; }

    public string? Deletreasontext { get; set; }

    public DateTime? DeletDate { get; set; }

    public string? Savingreson { get; set; }

    public string? Extendduration { get; set; }

    public int? Resumed { get; set; }

    public string? InvoiceNumber { get; set; }

    public int CountForPause { get; set; }

    public int CountForExtend { get; set; }

    public int? SupplierQuoteAmountPriceOrQuantity { get; set; }

    public string? Actualdeliverydate { get; set; }

    public string? Brandorproduct { get; set; }

    public string? Gbui { get; set; }

    public string? AboveorAtmarket { get; set; }

    public string? Clientprintpercentage { get; set; }

    public string? ClientPosmpercentage { get; set; }

    public string? ClientPromoPercentage { get; set; }

    public string? ClientPmggainSharePercentage { get; set; }

    public string? ClientVanestleGainSharePercentage { get; set; }

    public string? TotalCostprice { get; set; }

    public string? Surveynotification { get; set; }

    public string? PoorderraisedDate { get; set; }

    public string? WithinTolerance { get; set; }

    public string? InvoicedDate { get; set; }

    public string? QuotationDate { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? Briefdate { get; set; }

    public int? Managementfeetypeid { get; set; }

    public string? Portal { get; set; }

    public string? Orderid { get; set; }

    public string? CatalogEntity { get; set; }

    public string? UseReuse { get; set; }

    public string? Year { get; set; }

    public string? Savingstype { get; set; }

    public string? Projectattribute { get; set; }

    public string? Hybridmodel { get; set; }

    public bool? HasReviewed { get; set; }

    public string? Salesorderno { get; set; }

    public bool? SapSync { get; set; }

    public string? Sapuuid { get; set; }

    public bool? Sapneeded { get; set; }

    public string? SapprojectNo { get; set; }

    public bool? Disablepage { get; set; }

    public string? SapelementUuid { get; set; }
}
