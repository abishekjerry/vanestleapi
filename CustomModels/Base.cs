using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace VAapiNestle.CustomModels
{
    // Nullability conventions used by the models below (they keep the existing JSON contract unchanged):
    //  - "string?"      : optional value / response field that may be absent (null fields are omitted from JSON)
    //  - "= null!"      : request field that model validation requires ([Required] or implicitly required),
    //                     so it is never null once the action runs
    //  - "= []"         : collection that is always returned (empty instead of null)
    public class Base
    {
        public class NullToEmptyStringConverter : JsonConverter<string>
        {
            public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                return reader.GetString() ?? "";
            }

            public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value ?? "");
            }
        }
        // Email sending moved to VAapiNestle.Services.EmailService (credentials now come from configuration)
        public class User
        {
            public int UserID { get; set; }
            public int FkID { get; set; }
            public int CountryId { get; set; }
            public string? Role { get; set; }
            public string? Username { get; set; }
            public string? Email { get; set; }
            public string? Usertype { get; set; }
            public string? Country { get; set; }
            public string? Currency { get; set; }
            public string? password { get; set; } // never populated in responses
            public string? Token { get; set; }
            public string? Enquiryid { get; set; }

        }
        public class UserRequest
        {

            public string? Username { get; set; }
            public string? Lang { get; set; }
            public string? Password { get; set; }
            public string? NewPassword { get; set; }
            public string? TypeOfJob { get; set; }
            public string? Country { get; set; }
            public string? Currency { get; set; }
            public string? Savingstype { get; set; }
            public int Divisionid { get; set; }
            public int Enquiryid { get; set; }
            public int Subitemid { get; set; }
            public int CountryId { get; set; }
        }
        public class DropdownItem
        {
            public int Value { get; set; }
            public string Label { get; set; } = "";
            public bool Selected { get; set; } = false;
        }
        // Each endpoint fills only the lists it serves; unset lists are omitted from the response
        public class MasterDropdowns
        {
            public List<DropdownItem>? Country { get; set; }
            public List<DropdownItem>? Status { get; set; }
            public List<DropdownItem>? Client { get; set; }
            public List<DropdownItem>? User { get; set; }
            public List<DropdownItem>? TypeofJob { get; set; }
            public List<DropdownItem>? Division { get; set; }
            public List<DropdownItem>? ManagementFeetype { get; set; }
            public List<DropdownItem>? Year { get; set; }
            public List<DropdownItem>? ProjectAttribute { get; set; }
            public List<DropdownItem>? Brands { get; set; }
            public List<DropdownItem>? Sla { get; set; }
            public List<DropdownItem>? ItemCategory { get; set; }
            public List<DropdownItem>? SubCategory { get; set; }
            public List<DropdownItem>? ExceptionReason { get; set; }
            public List<DropdownItem>? TypeOfItem { get; set; }
            public List<DropdownItem>? GlobalOrder { get; set; }
            public List<DropdownItem>? RegionalOrder { get; set; }
            public List<DropdownItem>? LocalCatalog { get; set; }
            public List<DropdownItem>? PrintingMethod { get; set; }
            public List<DropdownItem>? SourcingLocation { get; set; }
            public List<DropdownItem>? SavingsReason { get; set; }
            public List<DropdownItem>? SavingsType { get; set; }
            public List<DropdownItem>? Printcapabilities { get; set; }
        }

        public class Result
        {
            public Result()
            {
            }

            public Result(bool status, object? data)
            {
                Status = status;
                Data = data;
            }

            public bool Status { get; set; }
            public dynamic? Data { get; set; }

            public static Result Ok(object? data) => new(true, data);
            public static Result Fail(object? data) => new(false, data);
        }
        #region dashboard
        public class EnquiryFilterRequest
        {
            public int? Menuid { get; set; }
            public int? UserCountryId { get; set; }
            public int? SlaId { get; set; }
            public int? CreatedName { get; set; }
            public int? EnqId { get; set; }
            public string? EnqUId { get; set; }
            public string? ProjectNo { get; set; }
            public string? StartDate { get; set; }
            public string? EndDate { get; set; }
            public string? StatusId { get; set; }
            public string? Role { get; set; }
            public string? Jobposition { get; set; }
            public string? Client { get; set; }
            public string? Username { get; set; }
            public int? UserId { get; set; }
        }

        public class GetJobCount
        {
            public int proof { get; set; }
            public int artwork { get; set; }
            public int production { get; set; }
            public int active { get; set; }
            public int approval { get; set; }
            public int forapproval { get; set; }
            public int awarded { get; set; }
            public int completed { get; set; }
            public int newrfq { get; set; }
            public int quoteproposed { get; set; }
            public int invites { get; set; }
            public List<GetJobStatus> JobStatus { get; set; } = [];

        }
        public class GetJobStatus
        {
            public int? StatusId { get; set; }
            public string? StatusName { get; set; }
            public int EnquiryCount { get; set; }
        }
        public class ProjectInformation
        {
            [Key]
            public int Id { get; set; }
            public string? EnquiryId { get; set; }
            public string? ProjectNo { get; set; }
            public string? ProjectDesc { get; set; }
            public string? ServerTime { get; set; }
            public int? Status { get; set; }
            public string? StatusName { get; set; }
            public string? Country { get; set; }
            public int PmgEntity { get; set; }
            public DateTime? BiddingStartDate { get; set; }
            public DateTime? BiddingEndDate { get; set; }
            public int? TimeOffset { get; set; }
            public int? Duration { get; set; }
            public int? StepId { get; set; }
            public int? ClientId { get; set; }
            public string? Orderid { get; set; }
            public string? QuoteEndDate { get; set; }
            public int? serchjobs { get; set; }
            public int? Attachments { get; set; }
            public string? CreatedBy { get; set; }
            public int? surveystatus { get; set; }
            public string? Submitedby { get; set; }
            public int? Surveycount { get; set; }
            public string? OldsurveyENQ { get; set; }
            public string? Ponumber { get; set; }
            public string? SurveyStatusName { get; set; }
            public string? BusinessUnit { get; set; }
            public int ClientContactId { get; set; }
        }

        #endregion

        public class ClientModel
        {
            [Required(ErrorMessage = "First Name is required")]
            [RegularExpression(@"\S+", ErrorMessage = "First Name cannot be empty or whitespace")]
            public string FirstName { get; set; } = null!;

            public string LastName { get; set; } = null!;

            [Required(ErrorMessage = "Logon ID is required")]
            [RegularExpression(@"\S+", ErrorMessage = "Logon ID cannot be empty or whitespace")]
            public string Logonid { get; set; } = null!;

            public string Jobtitle { get; set; } = null!;

            [Required(ErrorMessage = "Email is required")]
            [EmailAddress(ErrorMessage = "Invalid Email format")]
            public string Email { get; set; } = null!;


            public string Phone { get; set; } = null!;

            [Required(ErrorMessage = "Job Position is required")]
            public string JobPosition { get; set; } = null!;

            [Required(ErrorMessage = "Receive Notification flag is required")]
            public bool Receivenotification { get; set; }

            [Required(ErrorMessage = "Division ID is required")]
            [Range(1, int.MaxValue, ErrorMessage = "Division ID must be greater than 0")]
            public int Divisionid { get; set; }
        }
        public class BrandModel : IValidatableObject
        {
            [Required]
            public int Divisionid { get; set; }
            [Required(ErrorMessage = "Brand is required")]
            public string Brand { get; set; } = null!;
            public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            {
                if (string.IsNullOrWhiteSpace(Brand))
                {
                    yield return new ValidationResult("Brand cannot be empty or whitespace", new[] { nameof(Brand) });
                }
            }
        }
        public class EnqClientinfo
        {
            public string? EnqUId { get; set; }
            [Required(ErrorMessage = "The Division field is required.")]
            public int Divisionid { get; set; }
            [Required(ErrorMessage = "The Client contact field is required.")]
            public int ClientContactId { get; set; }
            public int CreatedBy { get; set; }
            public int ModifiedBy { get; set; }
            [Required(ErrorMessage = "Brands are required.")]
            public string Brand { get; set; } = null!;
            public int DeliveryCountryId { get; set; }
            public int PMGEntity { get; set; }
            public int EnqId { get; set; }
            public int? Menuid { get; set; }

            [Required(ErrorMessage = "The Above or At Market is required.")]
            public string AboveorAtmarket { get; set; } = null!;
            public string Action { get; set; } = null!;
            public string? Divisionname { get; set; }
            public string? Client { get; set; }
            public string? Entityname { get; set; }
            public string? BussinessUnit { get; set; }
            public string? GlobalBussinessUnit { get; set; }
            public string? Departmentorproduct { get; set; }
            public string? CreatedByUser { get; set; }
            public string? ClientContact { get; set; }
            public string? Country { get; set; }
            public string? PMGEntityname { get; set; }
            public string? DeliveryCountryname { get; set; }
            public string? InvoiceNumber { get; set; }
            public string? Actualdeliverydate { get; set; }
            public string? RaisedDate { get; set; }
            public string? PoNumber { get; set; }
            public string? CreatedDate { get; set; }
            public string? UserName { get; set; }
            public string? InvoiceDate { get; set; }
            public decimal? ManagementFee { get; set; }
        }
        public class EnqProjectinfo
        {
            public string? EnqUId { get; set; }
            public int EnqId { get; set; }

            [Required(ErrorMessage = "ProjectNo are required.")]
            public string ProjectNo { get; set; } = null!;

            [Required(ErrorMessage = "ProjectDesc are required.")]
            public string ProjectDesc { get; set; } = null!;

            [Required(ErrorMessage = "Estimate Date are required.")]
            public string Estdate { get; set; } = null!;
            [Required(ErrorMessage = "Brief Date are required.")]
            public string Briefdate { get; set; } = null!;
            public int ModifiedBy { get; set; }
            public int? QuoteBy { get; set; }
            public int? SlaId { get; set; }
            public int? ManagementfeetypeId { get; set; }

            [Required(ErrorMessage = "The Hybrid Model is required.")]
            public string? HybridModel { get; set; }
            public string? Attribute { get; set; }

            [Required(ErrorMessage = "Year are required.")]
            public string? Year { get; set; }
            public string Quotestartdate { get; set; } = null!;
            public string Quoteenddate { get; set; } = null!;
            public string Proofstartdate { get; set; } = null!;
            public string Proofenddate { get; set; } = null!;
            public string Productionstartdate { get; set; } = null!;
            public string Productionenddate { get; set; } = null!;
            public string Filecopiesstartdate { get; set; } = null!;
            public string Filecopiesenddate { get; set; } = null!;
            public string Invoicestartdate { get; set; } = null!;
            public string Invoiceenddate { get; set; } = null!;

            public int? ModifiedQuote { get; set; }
            public int? ModifiedProof { get; set; }
            public int? ModifiedProduction { get; set; }
            public int? ModifiedFilecopies { get; set; }
            public int? ModifiedInvoice { get; set; }

            public string? SlaTemplatename { get; set; }
            public string? ManagementFeetype { get; set; }
            public string? ProjectQuotetype { get; set; }

        }
        public class EnquiryItem
        {
            public int? EnqId { get; set; }
            public int? EnqdetailsId { get; set; }
            public int? ItemNumber { get; set; }
            public string? ItemName { get; set; }
            public string? ItemDescription { get; set; }

            public int? ProductCategoryId { get; set; }
            public string? Productcategory { get; set; }
            public string? ProductionMethod { get; set; }
            public string? QuoteType { get; set; }
            public string? QuoteQtyOrSize { get; set; }
            public string? FlatSizeDandH { get; set; }
            public string? FlatSizeLength { get; set; }
            public string? FlatSizeWidth { get; set; }
            public string? TotalFlatSize { get; set; }
            public string? TotalQtySize { get; set; }

            public string? QuoteFileList { get; set; }

            public int? ModifiedBy { get; set; }

            public decimal? ItemSellPrice { get; set; }
            public decimal? BaseLinePrice { get; set; }

            public int? Status { get; set; }

            public int? QuoteTypeindex { get; set; }
            public string? SubCategory { get; set; }
            public int? SubcatID { get; set; }
            public string? Recycled { get; set; }
            public string? ContainPlastic { get; set; }
            public string? RecycledPlastic { get; set; }
            public string? SustainableMaterial { get; set; }
            public string? LocalRateCard { get; set; }
            public string? PromoOSSOrderWindows { get; set; }
            public string? SpecNote { get; set; }
            public string? SNote { get; set; }
            public int? Version { get; set; }

            public string? printedorproducedonFSCmeterial { get; set; }
            public string? SustainableOptionthatwasrejected { get; set; }
            public string? WasthisitemdesignedtoreducedPlastic { get; set; }
            public string? Isthisitemdesignedtobereused { get; set; }
            public string? Dictated { get; set; }
            public string? Itemtype { get; set; }
            public string? Incoterm { get; set; }
            public string? HarmonizedOrder { get; set; }
            public string? CatalogueUsage { get; set; }
            public string? DigitalInnovation { get; set; }
            public string? Innovation { get; set; }
            public string? Sourcinglocation { get; set; }
            public string? Typeofitem { get; set; }
            public string? Eauction { get; set; }
            public string? SMETAorEquivalentDocuments { get; set; }
            public string? GMPorEquivalentDocuments { get; set; }
            public string? usingFSCMaterial { get; set; }
            public string? OEKOTEXCertification { get; set; }
            public string? designedforrecycling { get; set; }
            public string? plasticapartfromPLA { get; set; }
            public string? Plastictonnageweight { get; set; }
            public string? proposedwithsustainabilityoption { get; set; }
            public string? containrecycledmaterial { get; set; }
            public string? containrecycledplastic { get; set; }
            public string? Weightageofrecycledmaterial { get; set; }
            public string? RateCard { get; set; }
            public string? VolumeAggregation { get; set; }
            public string? CATABC { get; set; }
            public string? OWlink { get; set; }
            public string? TOJABC { get; set; }
            public string? Competbidmandate { get; set; }
            public string? Competbidcomplaint { get; set; }
            public string? Competbidexception { get; set; }
            public string? Simplex { get; set; }
            public string? TCOapproval { get; set; }
            public string? TCOapproved { get; set; }
            public string? Exceptionreason { get; set; }
            public string? Regionalname { get; set; }
            public string? savingstype { get; set; }
            public string? savingsreason { get; set; }
            public string? CompetetiveWinningSupplier { get; set; }
            public string? Printornonprint { get; set; }
            public string? PrintingMethod { get; set; }
            public int? Noofmaterials { get; set; }
        }
        public class EnqSupplierinfo
        {
            public string? Suppliername { get; set; }
            public string? Currency { get; set; }
            public string? Country { get; set; }
            public int? SupplierId { get; set; }
            public int? PrintCapability { get; set; }
            public int? CountryId { get; set; }
            public string? Suppliercode { get; set; }
            public bool? Selected { get; set; }
        }
        public class SupplierInfo
        {
            public string? SelectedSuppliers { get; set; }
            public int? EnqId { get; set; }
            public int? ModifiedBy { get; set; }
            public int[]? EnqDetailsId { get; set; }
            public string? NewSelectedSuppliers { get; set; }
        }
        public class Jobstatus
        {
            public int? ModifiedBy { get; set; }
            public int? EnqId { get; set; }
            public int? Status { get; set; }

        }
        public class SupplierDetails
        {
            public string? Suppliername { get; set; }
            public int? EnqId { get; set; }
            public int? SupplierID { get; set; }

        }

        public class HistoryLogs
        {
            public string? ModifiedDate { get; set; }
            public string? UserName { get; set; }
            public string? Field { get; set; }
            public string? OldValue { get; set; }
            public string? NewValue { get; set; }
        }

        public class LineItemsLogs
        {
            public string? ModifiedDate { get; set; }
            public string? UserName { get; set; }
            public string? Field { get; set; }
            public string? OldValue { get; set; }
            public string? NewValue { get; set; }
            public int? ItemNumber { get; set; }
        }

        public class HistorySearch
        {
            public string? EnquriyID { get; set; }
            public double? Qty { get; set; }
            public string? Country { get; set; }
            public string? Specifications { get; set; }
            public double? ReferencePrice { get; set; }
            public string? MaterialUsed { get; set; }
            public string? PoNumber { get; set; }
            public string? SubCategory { get; set; }
            public string? Brand { get; set; }
        }

        public class RFQSupplierQuotes
        {
            public int? EnqId { get; set; }
            public int? QuoteId { get; set; }
            public int? SupplierQuoteAmountPriceOrQuantity { get; set; }
            public int? ItemNumber { get; set; }
            public int? Qty { get; set; }
            public int? ModifiedBy { get; set; }
            public string? Intialprice { get; set; }
            public string? Negoprice { get; set; }
            public string? InitialUnitprice { get; set; }
            public string? NegoUnitprice { get; set; }
        }
        public class RevisedQuotes
        {
            public int? ItemNumber { get; set; }
            public string? ItemName { get; set; }
            public string? SupplierName { get; set; }
            public decimal? SupplierPrice { get; set; }
            public string? ItemNote { get; set; }
            public string? DateOfChange { get; set; }
        }

        public class RequestQuotes
        {
            public int? ItemNumber { get; set; }
            public string? ItemName { get; set; }
            public string? SupplierName { get; set; }
            public int? SupplierQuotesId { get; set; }
            public int? EnquiryId { get; set; }
            public int? EnquiryDetailsId { get; set; }
            public int? SupplierId { get; set; }
            public int? Quantity { get; set; }
            public decimal? InitialQuote { get; set; }
            public decimal? NegQuote { get; set; }
            public decimal IniUnitPrice { get; set; }
            public decimal? NegUnitPrice { get; set; }
            public decimal? NegUnitPriceFee { get; set; }
            public decimal? PmgSellPrice { get; set; }
            public int? IsCalculateId { get; set; }
        }

        public class CalculationDetails
        {
            public decimal? Cost { get; set; }
            public decimal? Sell { get; set; }
            public decimal? Margin { get; set; }
            public decimal? MarkupPercent { get; set; }
            public decimal? MarginPercent { get; set; }
        }

        public class CalculationSupplierlogs
        {
            public int Id { get; set; }
            public int? ItemNumber { get; set; }
            public string? ItemName { get; set; }
            public string? SMETAAccredited { get; set; }
            public string? GMPAccredited { get; set; }
            public string? Natureofsupplier { get; set; }
            public string? SupplierName { get; set; }
            public string? SupplierType { get; set; }

        }

        public class DeliveryOrder
        {
            public int? Id { get; set; }
            public int? Enqid { get; set; }
            public string? CompanyName { get; set; }
            public string? AddressLineOne { get; set; }
            public string? AddressLineTwo { get; set; }
            public string? AddressLineThree { get; set; }
            public string? NameorDept { get; set; }
            public string? ContactNo { get; set; }
            public string? Remarks { get; set; }

        }
        public class SavingReasons
        {
            public int? EnquiryId { get; set; }
            public int Id { get; set; }
            public string? ItemName { get; set; }
            public string? SavingsType { get; set; }
            public string? SavingsReason { get; set; }

        }

        public class UpdateJobSummary
        {
            public string? InvoiceNumber { get; set; }
            public string? Actualdeliverydate { get; set; }
            public string? RaisedDate { get; set; }
            public string? PoNumber { get; set; }
            public string? ProjectNo { get; set; }
            public string? ProjectDesc { get; set; }
            public string? Estdate { get; set; }
            public string? Briefdate { get; set; }
            public int ModifiedBy { get; set; }
            public int CreatedBy { get; set; }
            public int ClientContactId { get; set; }
            public string Action { get; set; } = null!;
            public int EnqId { get; set; }
            public int StatusId { get; set; }
            public string? InvoiceDate { get; set; }
            public decimal? ManagementFee { get; set; }
        }

        public class PreviewQuotes
        {
            public int? Id { get; set; }
            public int? ItemNumber { get; set; }
            public string? ItemName { get; set; }
            public string? SupplierA { get; set; }
            public string? SupplierB { get; set; }
            public string? SupplierC { get; set; }
            public string? SupplierAInitAmount { get; set; }
            public string? SupplierBInitAmount { get; set; }
            public string? SupplierCInitAmount { get; set; }
            public string? SupplierANegAmount { get; set; }
            public string? SupplierBNegAmount { get; set; }
            public string? SupplierCNegAmount { get; set; }

        }
        public class GetProjectDetails
        {
            public List<HistorySearch> historySearches { get; set; } = [];
            public List<HistoryLogs> historyLogs { get; set; } = [];
            public List<LineItemsLogs> lineItemLogs { get; set; } = [];
            public List<RevisedQuotes> revisedQuotes { get; set; } = [];
            public List<RequestQuotes> requestQuotes { get; set; } = [];
            public List<CalculationDetails>? calculationDetails { get; set; }
            public List<CalculationSupplierlogs> calculationSupplierlogs { get; set; } = [];
            public List<SavingReasons> savingReasons { get; set; } = [];
            public SavingsResponseDto? SavingsResponseDto { get; set; }
            public List<DeliveryOrder>? DeliveryOrder { get; set; }
            public List<ProjectStatus>? ProjectStatus { get; set; }
            public List<PreviewQuotes>? PreviewQuotes { get; set; }
        }

        public class ProjectStatus
        {
            public int Value { get; set; }
            public string? Label { get; set; }
        }
        public class SavingsResponseDto
        {
            public decimal TotalPreviousPrice { get; set; }
            public decimal TotalSellPrice { get; set; }
            public decimal TotalNegSellPrice { get; set; }
            public decimal TotalManagementFee { get; set; }
            public decimal TotalSaving { get; set; }
            public decimal TotalSavingPercent { get; set; }
            public List<SavingsItemDto> ItemWiseSummary { get; set; } = [];
            public List<SavingsDetailsDto> Details { get; set; } = [];
        }
        public class SavingsItemDto
        {
            public int? ItemNumber { get; set; }
            public string? ItemName { get; set; }
            public decimal TotalPreviousPrice { get; set; }
            public decimal TotalSellPrice { get; set; }
            public decimal TotalNegSellPrice { get; set; }
            public decimal ManagementFee { get; set; }
            public decimal Saving { get; set; }
            public decimal SavingPercent { get; set; }
            public string SavingDisplay => Saving <= 0 ? "0" : Saving.ToString("0.00");
            public string SavingPercentDisplay => (SavingPercent <= 0 ? 0 : SavingPercent).ToString("0.00");
            public decimal SavingExclfee { get; set; }
            public decimal SavingExcelfeePercent { get; set; }
            public string SavingExclfeeDisplay => SavingExclfee <= 0 ? "0" : SavingExclfee.ToString("0.00");
            public string SavingExclfeePercentDisplay => (SavingExcelfeePercent <= 0 ? 0 : SavingExcelfeePercent).ToString("0.00");
            public decimal Taxpercentage { get; set; }
            public decimal Taxamount { get; set; }
            public decimal Totalsellpricewithtax { get; set; }
        }
        public class SavingsDetailsDto
        {
            public int Id { get; set; }
            public int EnquiryId { get; set; }
            public int EnquiryDetailsId { get; set; }
            public int? ItemNumber { get; set; }
            public string? ItemName { get; set; }
            public string? ReasonForSaving { get; set; }
            public string? PreviousSupplier { get; set; }
            public decimal PreviousPrice { get; set; }
            public decimal ItemSellPrice { get; set; }
            public decimal NegItemSellPrice { get; set; }
            public decimal ManagementFee { get; set; }
            public string? BaselineQuantity { get; set; }
            public decimal BaselineUnitPrice { get; set; }
            public decimal BaselinePrice { get; set; }
            public decimal BaselineSaving { get; set; }
            public decimal GainShare { get; set; }
            public string? QuoteQtyOrSize { get; set; }
            public string? SavingType { get; set; }
            public decimal MFee { get; set; }

        }
        public class GetDetails
        {
            public EnqClientinfo? enqClientinfo { get; set; }
            public EnqProjectinfo? enqProjectinfo { get; set; }
            public List<EnquiryItem> enqlineItems { get; set; } = [];
            public List<SupplierDetails> supplierinfo { get; set; } = [];
            public string? Jobstatus { get; set; }
            public int? StatusId { get; set; }
            public int? StepId { get; set; }
        }

        [Keyless]
        public class EnquirySlaDto
        {
            public decimal? Quote { get; set; }
            public decimal? Proof { get; set; }
            public decimal? Production { get; set; }
            public decimal? FileCopies { get; set; }
            public decimal? Invoicing { get; set; }

            public int Id { get; set; }
            public string? Description { get; set; }
            public bool Status { get; set; }
            public bool? IsDefault { get; set; }

            public decimal? DefQuote { get; set; }
            public decimal? DefProof { get; set; }
            public decimal? DefProduction { get; set; }
            public decimal? DefFileCopies { get; set; }
            public decimal? DefInvoices { get; set; }
        }
        public class Calculate_supplierQuotes
        {
            public List<Ls_Calculate_SupplierQuotesd> Ls_getsupplierQuotes { get; set; } = [];
            public int Enqid { get; set; }

        }

        public class Ls_Calculate_SupplierQuotesd
        {

            public int Itemnumber { get; set; }
            public int SupplierID { get; set; }
            public int SupplierQuotesId { get; set; }
            public decimal InitialPrice { get; set; }
            public double NegotiatePrice { get; set; }
        }
        public class Ls_Store_Refprice
        {
            public string previousSupplier { get; set; } = null!;
            public decimal previousPrice { get; set; }
            public string note { get; set; } = null!;
            public int Id { get; set; }
            public string? BaselineQuantity { get; set; }
        }
        public class GetReport
        {
            public string? StartDate { get; set; }
            public string? EndDate { get; set; }
            public string? Role { get; set; }
            public int UserClientId { get; set; }
            public int CountryId { get; set; }
            public string? ReportType { get; set; }
            public string? ClientName { get; set; }
            public string? BusinessUnit { get; set; }
            public string? StatusFrom { get; set; }
            public string? StatusTo { get; set; }
            public string? Tco { get; set; }
            public string? UserType { get; set; }

        }

        public class SendSurveyMailRequest
        {
            public int EnqId { get; set; }
            public string? Country { get; set; }
            public string? RootPath { get; set; }
            public string? ProjectNumber { get; set; }
            public string? ProjectDescription { get; set; }
            public string? EnquiryNumber { get; set; }
            public string? ClientName { get; set; }
            public string? EmailAddress { get; set; }
        }

    }
}
