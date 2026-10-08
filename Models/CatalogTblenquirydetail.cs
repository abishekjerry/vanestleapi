using System;
using System.Collections.Generic;

namespace VAapiNestle.Models;

public partial class CatalogTblenquirydetail
{
    public int Id { get; set; }

    public int? EnquiryId { get; set; }

    public int? ItemNumber { get; set; }

    public string? ItemName { get; set; }

    public string? ItemDescription { get; set; }

    public decimal? BenchmarkPrice { get; set; }

    public int? ProductCategoryId { get; set; }

    public string? ProductionMethod { get; set; }

    public string? CopySpeFrom { get; set; }

    public string? QuoteType { get; set; }

    public string? QuoteQtyOrSize { get; set; }

    public string? FlatSizeLength { get; set; }

    public string? FlatSizeWidth { get; set; }

    public string? TotalFlatSize { get; set; }

    public string? TotalQtySize { get; set; }

    public string? QuoteFileList { get; set; }

    public string? SelectedSuppliers { get; set; }

    public string? QuoteStartDate { get; set; }

    public string? QuoteEndDate { get; set; }

    public string? ProofStartDate { get; set; }

    public string? ProofEndDate { get; set; }

    public string? ProductionStartDate { get; set; }

    public string? ProductionEndDate { get; set; }

    public string? FileCopiesStartDate { get; set; }

    public string? FileCopiesEndDate { get; set; }

    public string? InvoicingStartDate { get; set; }

    public string? InvoicingEndDate { get; set; }

    public int? ClientSupplierId { get; set; }

    public int? SupplierIdprive { get; set; }

    public int? SupplierId { get; set; }

    public decimal? ClientItemSellPrice { get; set; }

    public decimal? ItemSellPricedis { get; set; }

    public decimal? ItemSellPrice { get; set; }

    public decimal? QuotePrice { get; set; }

    public decimal? BaseLinePrice { get; set; }

    public string? ManagementFeeMethod { get; set; }

    public decimal? ManagementFeeUnit { get; set; }

    public decimal? TotalManagementFee { get; set; }

    public decimal? TaxPercentage { get; set; }

    public decimal? TotalTaxAmount { get; set; }

    public decimal? TotalQuoteAmount { get; set; }

    public int? CreatedOrModifiedBy { get; set; }

    public int? Status { get; set; }

    public DateTime? ServerTime { get; set; }

    public string? ProductType { get; set; }

    public string? PrintingMethod { get; set; }

    public string? PrintingOther { get; set; }

    public string? Suplier { get; set; }

    public decimal? Sst { get; set; }

    public int? QuoteTypeindex { get; set; }

    public string? Urgent { get; set; }

    public string? Printornonprint { get; set; }

    public string? Supplier2Name { get; set; }

    public string? Supplier2amount { get; set; }

    public string? Supplier3Name { get; set; }

    public string? Supplier3amount { get; set; }

    public string? Supplier1amount { get; set; }

    public string? Printornonprintother { get; set; }

    public string? Supplier1Intilamount { get; set; }

    public string? Supplier2Intilamount { get; set; }

    public string? Supplier3Intilamount { get; set; }

    public string? FlatSizeDandH { get; set; }

    public decimal? EuroRate { get; set; }

    public decimal? EurototalManagmentFee { get; set; }

    public decimal? EurototalQuotePrice { get; set; }

    public decimal? EurototalTaxamount { get; set; }

    public decimal? EuroTotalQuoteAmount { get; set; }

    public string? SavingsCal { get; set; }

    public decimal? NegQuoteAmount { get; set; }

    public string? SubCategory { get; set; }

    public string? Recycled { get; set; }

    public string? ContainPlastic { get; set; }

    public string? RecycledPlastic { get; set; }

    public string? SustainableMaterial { get; set; }

    public string? LocalRateCard { get; set; }

    public string? PromoOssorderWindows { get; set; }

    public string? ActualQuoteStartDate { get; set; }

    public string? ActualQuoteEndDate { get; set; }

    public string? ActualProofStartDate { get; set; }

    public string? ActualProofEndDate { get; set; }

    public string? ActualProductionStartDate { get; set; }

    public string? ActualProductionEndDate { get; set; }

    public string? ActualFileCopiesStartDate { get; set; }

    public string? ActualFileCopiesEndDate { get; set; }

    public string? ActualInvoicingStartDate { get; set; }

    public string? ActualInvoicingEndDate { get; set; }

    public string? SpecNote { get; set; }

    public string? Snote { get; set; }

    public int? Version { get; set; }

    public string? SubcatId { get; set; }

    public string? Podate { get; set; }

    public string? PrintedorproducedonFscmeterial { get; set; }

    public string? SustainableOptionthatwasrejected { get; set; }

    public string? WasthisitemdesignedtoreducedPlastic { get; set; }

    public string? Isthisitemdesignedtobereused { get; set; }

    public string? SupplierType { get; set; }

    public string? Smetaaccredited { get; set; }

    public string? Gmpaccredited { get; set; }

    public string? Natureofsupplier { get; set; }

    public string? Materialused { get; set; }

    public string? HarmonizedOrder { get; set; }

    public string? CatalogueUsage { get; set; }

    public string? DigitalInnovation { get; set; }

    public string? Innovation { get; set; }

    public string? Sourcinglocation { get; set; }

    public string? Typeofitem { get; set; }

    public string? EnvironmentalPaperName { get; set; }

    public string? EnvironmentalPaperGrade { get; set; }

    public string? EnvironmentalPaperQuantity { get; set; }

    public string? EnvironmentalPaperUnit { get; set; }

    public string? EnvironmentalPaperRecycled { get; set; }

    public string? EnvironmentalWood { get; set; }

    public string? EnvironmentalEnergy { get; set; }

    public string? EnvironmentalGhg { get; set; }

    public string? EnvironmentalWater { get; set; }

    public string? CompareEnvironmentalPaperName { get; set; }

    public string? CompareEnvironmentalPaperGrade { get; set; }

    public string? CompareEnvironmentalPaperQuantity { get; set; }

    public string? CompareEnvironmentalPaperUnit { get; set; }

    public string? CompareEnvironmentalPaperRecycled { get; set; }

    public string? CompareEnvironmentalWood { get; set; }

    public string? CompareEnvironmentalEnergy { get; set; }

    public string? CompareEnvironmentalGhg { get; set; }

    public string? CompareEnvironmentalWater { get; set; }

    public string? Incoterm { get; set; }

    public string? Eauction { get; set; }

    public string? CatalogCategory { get; set; }

    public string? Variation { get; set; }

    public string? SupplierCurrency { get; set; }

    public string? ProductCode { get; set; }
}
