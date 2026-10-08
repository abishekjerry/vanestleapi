using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace VAapiNestle.Models;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CatalogTblenquirydetail> CatalogTblenquirydetails { get; set; }

    public virtual DbSet<EbLiveFeed> EbLiveFeeds { get; set; }

    public virtual DbSet<EbTblPauseExtend> EbTblPauseExtends { get; set; }

    public virtual DbSet<EbtblJobStatus> EbtblJobStatuses { get; set; }

    public virtual DbSet<TblAgencyUser> TblAgencyUsers { get; set; }

    public virtual DbSet<TblAttachment> TblAttachments { get; set; }

    public virtual DbSet<TblCatalogMailDraft> TblCatalogMailDrafts { get; set; }

    public virtual DbSet<TblCatalogueUsage> TblCatalogueUsages { get; set; }

    public virtual DbSet<TblCategoryField> TblCategoryFields { get; set; }

    public virtual DbSet<TblCategoryGroup> TblCategoryGroups { get; set; }

    public virtual DbSet<TblClient> TblClients { get; set; }


    public virtual DbSet<TblClientDepartment> TblClientDepartments { get; set; }

    public virtual DbSet<TblClientDetail> TblClientDetails { get; set; }

    public virtual DbSet<TblClientValue> TblClientValues { get; set; }

    public virtual DbSet<TblClintDeliveryAddress> TblClintDeliveryAddresses { get; set; }

    public virtual DbSet<TblCountry> TblCountries { get; set; }

    public virtual DbSet<TblDivision> TblDivisions { get; set; }

    public virtual DbSet<TblDivisionBrandsorproduct> TblDivisionBrandsorproducts { get; set; }

    public virtual DbSet<TblEnquiryDetail> TblEnquiryDetails { get; set; }

    public virtual DbSet<TblEnquiryMaster> TblEnquiryMasters { get; set; }

    public virtual DbSet<TblEnquirySpecification> TblEnquirySpecifications { get; set; }

    public virtual DbSet<TblEnvironmentEmission> TblEnvironmentEmissions { get; set; }

    public virtual DbSet<TblEnvironmentresult> TblEnvironmentresults { get; set; }

    public virtual DbSet<TblFieldType> TblFieldTypes { get; set; }

    public virtual DbSet<TblFieldValue> TblFieldValues { get; set; }

    public virtual DbSet<TblJobHistory> TblJobHistories { get; set; }

    public virtual DbSet<TblJobStatus> TblJobStatuses { get; set; }

    public virtual DbSet<TblLineitemsHistory> TblLineitemsHistories { get; set; }

    public virtual DbSet<TblLog> TblLogs { get; set; }

    public virtual DbSet<TblMaterialused> TblMaterialuseds { get; set; }

    public virtual DbSet<TblMessage> TblMessages { get; set; }

    public virtual DbSet<TblNewSavingsdata> TblNewSavingsData { get; set; }

    public virtual DbSet<TblPrintCapability> TblPrintCapabilities { get; set; }

    public virtual DbSet<TblPrintCapability1> TblPrintCapabilities1 { get; set; }

    public virtual DbSet<TblProductCatalogue> TblProductCatalogues { get; set; }

    public virtual DbSet<TblProductCatalogueNew> TblProductCatalogueNews { get; set; }

    public virtual DbSet<TblProductCategory> TblProductCategories { get; set; }

    public virtual DbSet<TblProductMethod> TblProductMethods { get; set; }

    public virtual DbSet<TblProductSpecification> TblProductSpecifications { get; set; }

    public virtual DbSet<TblProductionMethod> TblProductionMethods { get; set; }

    public virtual DbSet<TblProductionMethodGroup> TblProductionMethodGroups { get; set; }

    public virtual DbSet<TblPromoOssorderWindow> TblPromoOssorderWindows { get; set; }

    public virtual DbSet<TblRegionalcatalogname> TblRegionalcatalognames { get; set; }

    public virtual DbSet<TblSavingReason> TblSavingReasons { get; set; }

    public virtual DbSet<TblSavingsDetail> TblSavingsDetails { get; set; }

    public virtual DbSet<TblSla> TblSlas { get; set; }

    public virtual DbSet<TblSourcinglocation> TblSourcinglocations { get; set; }

    public virtual DbSet<TblSubCategory> TblSubCategories { get; set; }

    public virtual DbSet<TblSupplierQuote> TblSupplierQuotes { get; set; }

    public virtual DbSet<TblSupplierQuoteHistory> TblSupplierQuoteHistories { get; set; }

    public virtual DbSet<TblSupplierQuotesforfirstround> TblSupplierQuotesforfirstrounds { get; set; }

    public virtual DbSet<TblSupplierUser> TblSupplierUsers { get; set; }

    public virtual DbSet<TblSuppliersList> TblSuppliersLists { get; set; }

    public virtual DbSet<TblSurvey> TblSurveys { get; set; }

    public virtual DbSet<TblTemp> TblTemps { get; set; }

    public virtual DbSet<TblTypeofItem> TblTypeofItems { get; set; }

    public virtual DbSet<TblTypeofjob> TblTypeofjobs { get; set; }

    public virtual DbSet<TblUser> TblUsers { get; set; }

    public virtual DbSet<TblWebAdmin> TblWebAdmins { get; set; }


    public virtual DbSet<TblcatalogUser> TblcatalogUsers { get; set; }

    public virtual DbSet<Tblcataloglogindetail> Tblcataloglogindetails { get; set; }

    public virtual DbSet<Tblcurrency> Tblcurrencies { get; set; }

    public virtual DbSet<Tbldivisionold> Tbldivisionolds { get; set; }

    public virtual DbSet<Tblenvironmentalimpact> Tblenvironmentalimpacts { get; set; }

    public virtual DbSet<Tblenvironmentcomparison> Tblenvironmentcomparisons { get; set; }

    public virtual DbSet<Tblexceptionreason> Tblexceptionreasons { get; set; }

    public virtual DbSet<Tblmanagmenttype> Tblmanagmenttypes { get; set; }

    public virtual DbSet<Tblpwdhistory> Tblpwdhistories { get; set; }

    public virtual DbSet<Tblversiondatum> Tblversiondata { get; set; }

    
    public virtual DbSet<ViewLogonHistory> ViewLogonHistories { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    { }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        
        modelBuilder.Entity<CatalogTblenquirydetail>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("catalog_tblenquirydetails");

            entity.Property(e => e.BaseLinePrice).HasColumnType("decimal(24, 2)");
            entity.Property(e => e.BenchmarkPrice).HasColumnType("decimal(24, 2)");
            entity.Property(e => e.CatalogCategory)
                .HasMaxLength(100)
                .HasColumnName("catalog_Category");
            entity.Property(e => e.ClientItemSellPrice).HasColumnType("decimal(24, 2)");
            entity.Property(e => e.ContainPlastic).HasMaxLength(50);
            entity.Property(e => e.Eauction)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("EAUCTION");
            entity.Property(e => e.EnquiryId).HasColumnName("EnquiryID");
            entity.Property(e => e.EuroRate)
                .HasColumnType("decimal(24, 2)")
                .HasColumnName("euroRate");
            entity.Property(e => e.EuroTotalQuoteAmount).HasColumnType("decimal(24, 2)");
            entity.Property(e => e.EurototalManagmentFee)
                .HasColumnType("decimal(24, 2)")
                .HasColumnName("eurototalManagmentFee");
            entity.Property(e => e.EurototalQuotePrice).HasColumnType("decimal(24, 2)");
            entity.Property(e => e.EurototalTaxamount).HasColumnType("decimal(24, 2)");
            entity.Property(e => e.Gmpaccredited).HasColumnName("GMPAccredited");
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Incoterm)
                .HasMaxLength(100)
                .HasColumnName("INCOTERM");
            entity.Property(e => e.ItemSellPrice).HasColumnType("decimal(24, 2)");
            entity.Property(e => e.ItemSellPricedis).HasColumnType("decimal(24, 2)");
            entity.Property(e => e.LocalRateCard).HasMaxLength(50);
            entity.Property(e => e.ManagementFeeUnit).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.NegQuoteAmount).HasColumnType("decimal(24, 2)");
            entity.Property(e => e.Podate).HasColumnName("PODate");
            entity.Property(e => e.PrintedorproducedonFscmeterial).HasColumnName("printedorproducedonFSCmeterial");
            entity.Property(e => e.PrintingMethod).HasMaxLength(50);
            entity.Property(e => e.PrintingOther).HasMaxLength(50);
            entity.Property(e => e.Printornonprint).HasMaxLength(50);
            entity.Property(e => e.Printornonprintother).HasMaxLength(50);
            entity.Property(e => e.ProductCode)
                .HasMaxLength(50)
                .HasColumnName("Product_code");
            entity.Property(e => e.ProductType).HasMaxLength(50);
            entity.Property(e => e.ProductionMethod).HasMaxLength(50);
            entity.Property(e => e.PromoOssorderWindows)
                .HasMaxLength(50)
                .HasColumnName("PromoOSSOrderWindows");
            entity.Property(e => e.QuotePrice).HasColumnType("decimal(24, 2)");
            entity.Property(e => e.Recycled).HasMaxLength(50);
            entity.Property(e => e.RecycledPlastic).HasMaxLength(50);
            entity.Property(e => e.SavingsCal).HasMaxLength(50);
            entity.Property(e => e.ServerTime).HasColumnType("datetime");
            entity.Property(e => e.Smetaaccredited).HasColumnName("SMETAAccredited");
            entity.Property(e => e.Snote).HasColumnName("SNote");
            entity.Property(e => e.Sst)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("SST");
            entity.Property(e => e.SubCategory).HasColumnName("subCategory");
            entity.Property(e => e.SubcatId).HasColumnName("SubcatID");
            entity.Property(e => e.Suplier)
                .HasMaxLength(50)
                .HasColumnName("suplier");
            entity.Property(e => e.Supplier1Intilamount)
                .HasMaxLength(50)
                .HasColumnName("supplier1Intilamount");
            entity.Property(e => e.Supplier1amount)
                .HasMaxLength(50)
                .HasColumnName("supplier1amount");
            entity.Property(e => e.Supplier2Intilamount)
                .HasMaxLength(50)
                .HasColumnName("supplier2Intilamount");
            entity.Property(e => e.Supplier2Name).HasMaxLength(50);
            entity.Property(e => e.Supplier2amount)
                .HasMaxLength(50)
                .HasColumnName("supplier2amount");
            entity.Property(e => e.Supplier3Intilamount)
                .HasMaxLength(50)
                .HasColumnName("supplier3Intilamount");
            entity.Property(e => e.Supplier3Name)
                .HasMaxLength(50)
                .HasColumnName("supplier3Name");
            entity.Property(e => e.Supplier3amount)
                .HasMaxLength(50)
                .HasColumnName("supplier3amount");
            entity.Property(e => e.SupplierIdprive).HasColumnName("SupplierIDprive");
            entity.Property(e => e.SustainableMaterial).HasMaxLength(50);
            entity.Property(e => e.TaxPercentage).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.TotalManagementFee).HasColumnType("decimal(24, 2)");
            entity.Property(e => e.TotalQuoteAmount).HasColumnType("decimal(24, 2)");
            entity.Property(e => e.TotalTaxAmount).HasColumnType("decimal(24, 2)");
            entity.Property(e => e.Urgent).HasMaxLength(50);
        });

        modelBuilder.Entity<EbLiveFeed>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("EB_LiveFeed");

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FeedType).HasMaxLength(50);
        });

        modelBuilder.Entity<EbTblPauseExtend>(entity =>
        {
            entity.ToTable("EB_tblPauseExtend");

            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.Type).HasMaxLength(50);
        });

        modelBuilder.Entity<EbtblJobStatus>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("EBtblJobStatus");

            entity.Property(e => e.StatusName).HasMaxLength(50);
            entity.Property(e => e.VisibleSuppliersStatus).HasMaxLength(255);
        });

        modelBuilder.Entity<TblAgencyUser>(entity =>
        {
            entity.ToTable("tblAgencyUsers");

            entity.Property(e => e.Country).HasMaxLength(255);
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FirstName).HasMaxLength(255);
            entity.Property(e => e.JobPosition).HasMaxLength(255);
            entity.Property(e => e.JobRole).HasMaxLength(255);
            entity.Property(e => e.JobTitle).HasMaxLength(255);
            entity.Property(e => e.LastName).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(255);
            entity.Property(e => e.Reportenabled).HasDefaultValue(false);
        });

        modelBuilder.Entity<TblAttachment>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("tblAttachments");

            entity.Property(e => e.CreatedBy).HasMaxLength(10);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Size).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.Type).HasMaxLength(50);
            entity.Property(e => e.UploadDate).HasColumnType("datetime");
            entity.Property(e => e.UserId)
                .HasMaxLength(50)
                .HasColumnName("UserID");
        });

        modelBuilder.Entity<TblCatalogMailDraft>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("tblCatalog_MailDraft");

            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.JobType)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Market)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ProjectManager).HasMaxLength(250);
            entity.Property(e => e.TeamLead).HasMaxLength(250);
        });

        modelBuilder.Entity<TblCatalogueUsage>(entity =>
        {
            entity.ToTable("tblCatalogueUsage");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Status)
                .HasDefaultValue(true)
                .HasColumnName("status");
        });

        modelBuilder.Entity<TblCategoryField>(entity =>
        {
            entity.HasKey(e => e.FieldId);

            entity.ToTable("tblCategoryFields");

            entity.Property(e => e.FieldId).HasColumnName("FieldID");
            entity.Property(e => e.GroupId).HasColumnName("GroupID");
        });

        modelBuilder.Entity<TblCategoryGroup>(entity =>
        {
            entity.HasKey(e => e.GroupId).HasName("PK_Sheet1$_2");

            entity.ToTable("tblCategoryGroups");

            entity.Property(e => e.GroupId).HasColumnName("GroupID");
            entity.Property(e => e.ProductCategoryId).HasColumnName("ProductCategoryID");
        });

        modelBuilder.Entity<TblClient>(entity =>
        {
            entity.ToTable("tblClient");

            entity.Property(e => e.CatalogEntity)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("Catalog_Entity");
            entity.Property(e => e.Country).HasMaxLength(150);
            entity.Property(e => e.Email).HasMaxLength(50);
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.Gbui)
                .HasMaxLength(150)
                .HasColumnName("GBUI");
            entity.Property(e => e.JobTitle).HasMaxLength(50);
            entity.Property(e => e.Jobposition).HasMaxLength(50);
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.Status).HasDefaultValue(true);
        });

        modelBuilder.Entity<TblClient28072016>(entity =>
        {
            entity.ToTable("tblClient_28072016");

            entity.Property(e => e.Email).HasMaxLength(50);
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.JobTitle).HasMaxLength(50);
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.Status).HasDefaultValue(true);
        });

        modelBuilder.Entity<TblClientDepartment>(entity =>
        {
            entity.ToTable("tblClientDepartment");
        });

        modelBuilder.Entity<TblClientDetail>(entity =>
        {
            entity.ToTable("tblClientDetails");

            entity.Property(e => e.Changingdate).HasColumnType("datetime");
            entity.Property(e => e.City).HasMaxLength(50);
            entity.Property(e => e.Clientcode)
                .HasMaxLength(200)
                .HasColumnName("clientcode");
            entity.Property(e => e.Currency).HasMaxLength(50);
            entity.Property(e => e.DefaultSalesPercentage).HasMaxLength(50);
            entity.Property(e => e.EuroRate).HasMaxLength(50);
            entity.Property(e => e.Language).HasMaxLength(50);
            entity.Property(e => e.NeweuroRate).HasMaxLength(50);
            entity.Property(e => e.PaymentTermcode).HasMaxLength(200);
            entity.Property(e => e.PmggainShare)
                .HasMaxLength(50)
                .HasColumnName("PMGGainShare");
            entity.Property(e => e.Posmpercentage)
                .HasMaxLength(50)
                .HasColumnName("POSMPercentage");
            entity.Property(e => e.Printpercentage)
                .HasMaxLength(50)
                .HasColumnName("printpercentage");
            entity.Property(e => e.PromoPercentage).HasMaxLength(50);
            entity.Property(e => e.SalesPercentage).HasMaxLength(50);
            entity.Property(e => e.Sst)
                .HasMaxLength(50)
                .HasColumnName("SST");
            entity.Property(e => e.VanestleGainShare)
                .HasMaxLength(50)
                .HasColumnName("VANestleGainShare");
        });

        modelBuilder.Entity<TblClientValue>(entity =>
        {
            entity.HasKey(e => e.ClientId).HasName("PK_tblclientvalues");

            entity.ToTable("tblClientValues");

            entity.Property(e => e.ClientId)
                .ValueGeneratedNever()
                .HasColumnName("ClientID");
            entity.Property(e => e.ClientName).HasMaxLength(50);
        });

        modelBuilder.Entity<TblClintDeliveryAddress>(entity =>
        {
            entity.ToTable("tblClintDeliveryAddress");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.ContactNo).HasColumnName("ContactNO");
        });

        modelBuilder.Entity<TblCountry>(entity =>
        {
            entity.ToTable("tblCountry");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Code).HasMaxLength(10);
            entity.Property(e => e.Country).HasMaxLength(50);
            entity.Property(e => e.Currency).HasMaxLength(50);
            entity.Property(e => e.Language).HasMaxLength(50);
            entity.Property(e => e.RegionalSetting).HasMaxLength(50);
            entity.Property(e => e.Sapneeded)
                .HasDefaultValue(false)
                .HasColumnName("SAPNeeded");
            entity.Property(e => e.TaxPercentage).HasMaxLength(50);
        });

        modelBuilder.Entity<TblDivision>(entity =>
        {
            entity.ToTable("tblDivision");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.BussinessUnit)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ClientId).HasColumnName("ClientID");
            entity.Property(e => e.ClientName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Country)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.DepartmentOrProduct)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.EntityName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.GlobalBumapping)
                .HasMaxLength(150)
                .HasColumnName("GlobalBUMapping");
            entity.Property(e => e.Status).HasDefaultValue(true);
        });

        modelBuilder.Entity<TblDivisionBrandsorproduct>(entity =>
        {
            entity.ToTable("tblDivisionBrandsorproducts");

            entity.HasKey(e => e.Id); // ✅ Define primary key

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("ID");

            entity.Property(e => e.Divisionid);
            entity.Property(e => e.Brandorproduct);
        });

        modelBuilder.Entity<TblEnquiryDetail>(entity =>
        {
            entity.ToTable("tblEnquiryDetails", tb =>
                {
                    tb.HasTrigger("trgOnStatusChange");
                    tb.HasTrigger("trg_StatusUpdate_APIQueue");
                });

            entity.Property(e => e.Addressable).HasDefaultValue(false);
            entity.Property(e => e.BaseLinePrice).HasColumnType("decimal(24, 2)");
            entity.Property(e => e.BenchmarkPrice).HasColumnType("decimal(24, 2)");
            entity.Property(e => e.Catabc)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("CATABC");
            entity.Property(e => e.CatalogCategory)
                .HasMaxLength(100)
                .HasColumnName("catalog_Category");
            entity.Property(e => e.ClientItemSellPrice).HasColumnType("decimal(24, 2)");
            entity.Property(e => e.Competbidcomplaint).HasMaxLength(100);
            entity.Property(e => e.Competbidexception).HasMaxLength(100);
            entity.Property(e => e.Competbidmandate).HasMaxLength(100);
            entity.Property(e => e.CompetetiveWinningSupplier).HasMaxLength(300);
            entity.Property(e => e.ContainPlastic).HasMaxLength(50);
            entity.Property(e => e.Containrecycledmaterial).HasColumnName("containrecycledmaterial");
            entity.Property(e => e.Containrecycledplastic).HasColumnName("containrecycledplastic");
            entity.Property(e => e.Designedforrecycling).HasColumnName("designedforrecycling");
            entity.Property(e => e.Eauction).HasMaxLength(100);
            entity.Property(e => e.EchoProductcode).HasColumnName("Echo_productcode");
            entity.Property(e => e.EnquiryId).HasColumnName("EnquiryID");
            entity.Property(e => e.EnvironmentalWooddensity).HasMaxLength(100);
            entity.Property(e => e.EuroRate)
                .HasColumnType("decimal(24, 2)")
                .HasColumnName("euroRate");
            entity.Property(e => e.EuroTotalQuoteAmount).HasColumnType("decimal(24, 2)");
            entity.Property(e => e.EurototalManagmentFee)
                .HasColumnType("decimal(24, 2)")
                .HasColumnName("eurototalManagmentFee");
            entity.Property(e => e.EurototalQuotePrice).HasColumnType("decimal(24, 2)");
            entity.Property(e => e.EurototalTaxamount).HasColumnType("decimal(24, 2)");
            entity.Property(e => e.Exceptionreason).HasMaxLength(100);
            entity.Property(e => e.Gmpaccredited).HasColumnName("GMPAccredited");
            entity.Property(e => e.GmporEquivalentDocuments).HasColumnName("GMPorEquivalentDocuments");
            entity.Property(e => e.Incoterm)
                .HasMaxLength(100)
                .HasColumnName("INCOTERM");
            entity.Property(e => e.IsCustomize).HasColumnName("is_customize");
            entity.Property(e => e.ItemSellPrice).HasColumnType("decimal(24, 2)");
            entity.Property(e => e.ItemSellPricedis).HasColumnType("decimal(24, 2)");
            entity.Property(e => e.LocalRateCard).HasMaxLength(50);
            entity.Property(e => e.ManagementFeeUnit).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.NegQuoteAmount).HasColumnType("decimal(24, 2)");
            entity.Property(e => e.Oekotexcertification).HasColumnName("OEKOTEXCertification");
            entity.Property(e => e.Owlink)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("OWlink");
            entity.Property(e => e.PlasticapartfromPla).HasColumnName("plasticapartfromPLA");
            entity.Property(e => e.Podate).HasColumnName("PODate");
            entity.Property(e => e.PrintedorproducedonFscmeterial).HasColumnName("printedorproducedonFSCmeterial");
            entity.Property(e => e.PrintingMethod).HasMaxLength(50);
            entity.Property(e => e.PrintingOther).HasMaxLength(50);
            entity.Property(e => e.Printornonprint).HasMaxLength(50);
            entity.Property(e => e.Printornonprintother).HasMaxLength(50);
            entity.Property(e => e.ProductType).HasMaxLength(50);
            entity.Property(e => e.ProductionMethod).HasMaxLength(50);
            entity.Property(e => e.PromoOssorderWindows)
                .HasMaxLength(50)
                .HasColumnName("PromoOSSOrderWindows");
            entity.Property(e => e.Proposedwithsustainabilityoption).HasColumnName("proposedwithsustainabilityoption");
            entity.Property(e => e.QuotePrice).HasColumnType("decimal(24, 2)");
            entity.Property(e => e.RangeTo).HasMaxLength(100);
            entity.Property(e => e.Rangefrom).HasMaxLength(100);
            entity.Property(e => e.RateCard).HasMaxLength(10);
            entity.Property(e => e.Recycled).HasMaxLength(50);
            entity.Property(e => e.RecycledPlastic).HasMaxLength(50);
            entity.Property(e => e.Regionalname).HasMaxLength(500);
            entity.Property(e => e.SapLineItem).HasColumnName("SAP_LineItem");
            entity.Property(e => e.SapSync).HasColumnName("SAP_Sync");
            entity.Property(e => e.SavingsCal).HasMaxLength(50);
            entity.Property(e => e.Savingsreason)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("savingsreason");
            entity.Property(e => e.Savingstype)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("savingstype");
            entity.Property(e => e.ServerTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Simplex).HasMaxLength(100);
            entity.Property(e => e.Smetaaccredited).HasColumnName("SMETAAccredited");
            entity.Property(e => e.SmetaorEquivalentDocuments).HasColumnName("SMETAorEquivalentDocuments");
            entity.Property(e => e.Snote).HasColumnName("SNote");
            entity.Property(e => e.Sst)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("SST");
            entity.Property(e => e.SubCategory).HasColumnName("subCategory");
            entity.Property(e => e.SubcatId).HasColumnName("SubcatID");
            entity.Property(e => e.Suplier)
                .HasMaxLength(50)
                .HasColumnName("suplier");
            entity.Property(e => e.Supplier1Intilamount)
                .HasMaxLength(50)
                .HasColumnName("supplier1Intilamount");
            entity.Property(e => e.Supplier1amount)
                .HasMaxLength(50)
                .HasColumnName("supplier1amount");
            entity.Property(e => e.Supplier2Intilamount)
                .HasMaxLength(50)
                .HasColumnName("supplier2Intilamount");
            entity.Property(e => e.Supplier2Name).HasMaxLength(500);
            entity.Property(e => e.Supplier2amount)
                .HasMaxLength(50)
                .HasColumnName("supplier2amount");
            entity.Property(e => e.Supplier3Intilamount)
                .HasMaxLength(50)
                .HasColumnName("supplier3Intilamount");
            entity.Property(e => e.Supplier3Name)
                .HasMaxLength(500)
                .HasColumnName("supplier3Name");
            entity.Property(e => e.Supplier3amount)
                .HasMaxLength(50)
                .HasColumnName("supplier3amount");
            entity.Property(e => e.SupplierIdprive).HasColumnName("SupplierIDprive");
            entity.Property(e => e.SustainableMaterial).HasMaxLength(50);
            entity.Property(e => e.TaxPercentage).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.Tcoapproval)
                .HasMaxLength(100)
                .HasColumnName("TCOapproval");
            entity.Property(e => e.Tcoapproved)
                .HasMaxLength(100)
                .HasColumnName("TCOapproved");
            entity.Property(e => e.Tojabc)
                .HasMaxLength(100)
                .HasColumnName("TOJABC");
            entity.Property(e => e.TotalManagementFee).HasColumnType("decimal(24, 2)");
            entity.Property(e => e.TotalQuoteAmount).HasColumnType("decimal(24, 2)");
            entity.Property(e => e.TotalTaxAmount).HasColumnType("decimal(24, 2)");
            entity.Property(e => e.Urgent).HasMaxLength(50);
            entity.Property(e => e.UsingFscmaterial).HasColumnName("usingFSCMaterial");
            entity.Property(e => e.Version).HasDefaultValue(1);
            entity.Property(e => e.VolumeAggregation).HasMaxLength(10);
        });

        modelBuilder.Entity<TblEnquiryMaster>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_tblEnquiry");

            entity.ToTable("tblEnquiryMaster", tb =>
                {
                    tb.HasTrigger("trgOnJobUpdate");
                    tb.HasTrigger("trg_StatusUpdate");
                });

            entity.Property(e => e.AboveorAtmarket).HasMaxLength(50);
            entity.Property(e => e.Actualdeliverydate).HasMaxLength(50);
            entity.Property(e => e.BiddingEndDate).HasColumnType("datetime");
            entity.Property(e => e.BiddingStartDate).HasColumnType("datetime");
            entity.Property(e => e.Brandorproduct).HasMaxLength(50);
            entity.Property(e => e.Briefdate)
                .HasMaxLength(100)
                .HasColumnName("BRIEFDATE");
            entity.Property(e => e.CancelDate).HasColumnType("datetime");
            entity.Property(e => e.CatalogEntity).HasColumnName("CATALOG_ENTITY");
            entity.Property(e => e.ClientPmggainSharePercentage)
                .HasMaxLength(50)
                .HasColumnName("ClientPMGGainSharePercentage");
            entity.Property(e => e.ClientPosmpercentage)
                .HasMaxLength(50)
                .HasColumnName("ClientPOSMPercentage");
            entity.Property(e => e.ClientPromoPercentage).HasMaxLength(50);
            entity.Property(e => e.ClientVanestleGainSharePercentage)
                .HasMaxLength(50)
                .HasColumnName("ClientVANestleGainSharePercentage");
            entity.Property(e => e.Clientprintpercentage).HasMaxLength(50);
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.DeletDate).HasColumnType("datetime");
            entity.Property(e => e.DeletReason).HasMaxLength(50);
            entity.Property(e => e.DeleteDate).HasColumnType("datetime");
            entity.Property(e => e.Deletreasontext).HasMaxLength(50);
            entity.Property(e => e.EnqUid)
                .HasMaxLength(50)
                .HasColumnName("EnqUId");
            entity.Property(e => e.EstDate).HasMaxLength(50);
            entity.Property(e => e.Extendduration).HasMaxLength(50);
            entity.Property(e => e.Gbui)
                .HasMaxLength(50)
                .HasColumnName("GBUI");
            entity.Property(e => e.Hybridmodel)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.InvoiceNumber)
                .HasMaxLength(50)
                .HasColumnName("invoiceNumber");
            entity.Property(e => e.IsFirstRemainderSent).HasDefaultValue(false);
            entity.Property(e => e.IsSecondRemainderSent).HasDefaultValue(false);
            entity.Property(e => e.JobType).HasMaxLength(50);
            entity.Property(e => e.Managementfeetypeid).HasColumnName("MANAGEMENTFEETYPEID");
            entity.Property(e => e.Orderid)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Ponumber)
                .HasMaxLength(250)
                .HasColumnName("PONumber");
            entity.Property(e => e.PoorderraisedDate).HasColumnName("POOrderraisedDate");
            entity.Property(e => e.Portal)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ProjectDesc).HasMaxLength(250);
            entity.Property(e => e.ProjectNo).HasMaxLength(50);
            entity.Property(e => e.Projectattribute)
                .HasMaxLength(250)
                .HasColumnName("projectattribute");
            entity.Property(e => e.SapSync).HasColumnName("SAP_Sync");
            entity.Property(e => e.SapelementUuid).HasColumnName("SAPElementUUID");
            entity.Property(e => e.Sapneeded).HasColumnName("SAPNeeded");
            entity.Property(e => e.SapprojectNo).HasColumnName("SAPProjectNo");
            entity.Property(e => e.Sapuuid).HasColumnName("SAPUUID");
            entity.Property(e => e.Savingreson).HasMaxLength(50);
            entity.Property(e => e.Savingstype)
                .HasMaxLength(250)
                .HasColumnName("savingstype");
            entity.Property(e => e.ServerTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Status).HasDefaultValue(true);
            entity.Property(e => e.Surveynotification).HasMaxLength(50);
            entity.Property(e => e.Year)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("year");
        });

        modelBuilder.Entity<TblEnquirySpecification>(entity =>
        {
            entity.ToTable("tblEnquirySpecification");

            entity.Property(e => e.EnqDetailsId).HasColumnName("EnqDetailsID");
            entity.Property(e => e.EnquiryId).HasColumnName("EnquiryID");
            entity.Property(e => e.FieldId).HasColumnName("FieldID");
            entity.Property(e => e.GroupId).HasColumnName("GroupID");
        });

        modelBuilder.Entity<TblEnvironmentEmission>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("tblEnvironmentEmission");

            entity.Property(e => e.Country)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Gef).HasMaxLength(250);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Status).HasColumnName("status");
        });

        modelBuilder.Entity<TblEnvironmentresult>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("tblEnvironmentresult");

            entity.Property(e => e.CompareEnvReason).HasMaxLength(500);
            entity.Property(e => e.CompareEnvironmentalEnergy).HasMaxLength(200);
            entity.Property(e => e.CompareEnvironmentalGhg).HasMaxLength(200);
            entity.Property(e => e.CompareEnvironmentalPaperGrade).HasMaxLength(200);
            entity.Property(e => e.CompareEnvironmentalPaperName).HasMaxLength(200);
            entity.Property(e => e.CompareEnvironmentalPaperQuantity).HasMaxLength(200);
            entity.Property(e => e.CompareEnvironmentalPaperRecycled).HasMaxLength(200);
            entity.Property(e => e.CompareEnvironmentalPaperUnit).HasMaxLength(200);
            entity.Property(e => e.CompareEnvironmentalWater).HasMaxLength(200);
            entity.Property(e => e.CompareEnvironmentalWood).HasMaxLength(200);
            entity.Property(e => e.EnvironmentalEnergy).HasMaxLength(200);
            entity.Property(e => e.EnvironmentalGhg).HasMaxLength(200);
            entity.Property(e => e.EnvironmentalPaperGrade).HasMaxLength(200);
            entity.Property(e => e.EnvironmentalPaperName).HasMaxLength(200);
            entity.Property(e => e.EnvironmentalPaperQuantity).HasMaxLength(200);
            entity.Property(e => e.EnvironmentalPaperRecycled).HasMaxLength(200);
            entity.Property(e => e.EnvironmentalPaperUnit).HasMaxLength(200);
            entity.Property(e => e.EnvironmentalWater).HasMaxLength(200);
            entity.Property(e => e.EnvironmentalWood).HasMaxLength(200);
            entity.Property(e => e.EnvironmentalWooddensity).HasMaxLength(200);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Materialused).HasMaxLength(200);
            entity.Property(e => e.Type)
                .HasMaxLength(10)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TblFieldType>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("tblFieldTypes");

            entity.Property(e => e.FieldTypeId)
                .ValueGeneratedOnAdd()
                .HasColumnName("FieldTypeID");
            entity.Property(e => e.Status).HasDefaultValue(true);
        });

        modelBuilder.Entity<TblFieldValue>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("tblFieldValues");

            entity.Property(e => e.FieldId).HasColumnName("FieldID");
            entity.Property(e => e.FieldValueId).HasColumnName("FieldValueID");
        });

        modelBuilder.Entity<TblJobHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_tblJobHistory1");

            entity.ToTable("tblJobHistory");

            entity.Property(e => e.ModifiedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<TblJobStatus>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("tblJobStatus");

            entity.Property(e => e.StatusName).HasMaxLength(255);
            entity.Property(e => e.VisibleSuppliersStatus).HasMaxLength(255);
        });

        modelBuilder.Entity<TblLineitemsHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__tblLinei__3213E83FADDB4838");

            entity.ToTable("tblLineitemsHistory");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Field).HasColumnName("field");
            entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<TblLog>(entity =>
        {
            entity.ToTable("tblLog");

            entity.Property(e => e.Ipaddress)
                .HasMaxLength(50)
                .HasColumnName("IPAddress");
            entity.Property(e => e.Location).HasMaxLength(50);
            entity.Property(e => e.LoginDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<TblMaterialused>(entity =>
        {
            entity.ToTable("tblMaterialused");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Status)
                .HasDefaultValue(true)
                .HasColumnName("status");
        });

        modelBuilder.Entity<TblMessage>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("tblMessage");

            entity.Property(e => e.FromId).HasColumnName("From_id");
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("id");
            entity.Property(e => e.Message)
                .HasColumnType("text")
                .HasColumnName("message");
            entity.Property(e => e.ReadStatus).HasColumnName("read_status");
            entity.Property(e => e.Received)
                .HasColumnType("datetime")
                .HasColumnName("received");
            entity.Property(e => e.Sent)
                .HasColumnType("datetime")
                .HasColumnName("sent");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.ToId).HasColumnName("To_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
        });

        modelBuilder.Entity<TblNewSavingsdata>(entity =>
        {
            entity.HasNoKey().ToTable("TblNewSavingsdata");
            entity.Property(e => e.JobRef).HasColumnName("job ref");
            entity.Property(e => e.FullSpec).HasColumnName("Full Spec");
            entity.Property(e => e.HistoricalPriceLcl).HasColumnName("Historical Price (Lcl)");
            entity.Property(e => e.MaterialUsed).HasColumnName("Material Used");
            entity.Property(e => e.CustomerPo).HasColumnName("Customer po");
            entity.Property(e => e.SubCategory).HasColumnName("Sub category");
            entity.Property(e => e.Brand).HasColumnName("Brand");
        });

        modelBuilder.Entity<TblPrintCapability>(entity =>
        {
            entity.HasKey(e => e.CapabilityId).HasName("PK__tblPrint__9A6A78D6297E2100");

            entity.ToTable("tblPrintCapabilities");

            entity.Property(e => e.CapabilityId).ValueGeneratedNever();
            entity.Property(e => e.Capability).HasMaxLength(50);
            entity.Property(e => e.ChineseSimplified).HasMaxLength(50);
            entity.Property(e => e.ChineseTraditional).HasMaxLength(50);
            entity.Property(e => e.Indonesia).HasMaxLength(50);
            entity.Property(e => e.Japanese).HasMaxLength(50);
            entity.Property(e => e.Korean).HasMaxLength(50);
        });

        modelBuilder.Entity<TblPrintCapability1>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_tblPrintCategory");

            entity.ToTable("tblPrintCapability");

            entity.Property(e => e._10Digital)
                .HasMaxLength(255)
                .HasColumnName("10-Digital");
            entity.Property(e => e._10SheetfedLitho)
                .HasMaxLength(255)
                .HasColumnName("10-SheetfedLitho");
            entity.Property(e => e._11All)
                .HasMaxLength(255)
                .HasColumnName("11-All");
            entity.Property(e => e._12All)
                .HasMaxLength(255)
                .HasColumnName("12-All");
            entity.Property(e => e._13All)
                .HasMaxLength(255)
                .HasColumnName("13-All");
            entity.Property(e => e._14All)
                .HasMaxLength(255)
                .HasColumnName("14-All");
            entity.Property(e => e._15All)
                .HasMaxLength(255)
                .HasColumnName("15-All");
            entity.Property(e => e._16Digital)
                .HasMaxLength(255)
                .HasColumnName("16-Digital");
            entity.Property(e => e._16SheetfedLitho)
                .HasMaxLength(255)
                .HasColumnName("16-SheetfedLitho");
            entity.Property(e => e._16Web)
                .HasMaxLength(255)
                .HasColumnName("16-Web");
            entity.Property(e => e._17All)
                .HasMaxLength(255)
                .HasColumnName("17-All");
            entity.Property(e => e._18All)
                .HasMaxLength(255)
                .HasColumnName("18-All");
            entity.Property(e => e._19All)
                .HasMaxLength(255)
                .HasColumnName("19-All");
            entity.Property(e => e._1All)
                .HasMaxLength(255)
                .HasColumnName("1-All");
            entity.Property(e => e._20All)
                .HasMaxLength(255)
                .HasColumnName("20-All");
            entity.Property(e => e._21All)
                .HasMaxLength(255)
                .HasColumnName("21-All");
            entity.Property(e => e._22All)
                .HasMaxLength(255)
                .HasColumnName("22-All");
            entity.Property(e => e._23All)
                .HasMaxLength(255)
                .HasColumnName("23-All");
            entity.Property(e => e._2Digital)
                .HasMaxLength(255)
                .HasColumnName("2-Digital");
            entity.Property(e => e._2SheetfedLitho)
                .HasMaxLength(255)
                .HasColumnName("2-SheetfedLitho");
            entity.Property(e => e._3Digital)
                .HasMaxLength(255)
                .HasColumnName("3-Digital");
            entity.Property(e => e._3SheetfedLitho)
                .HasMaxLength(255)
                .HasColumnName("3-SheetfedLitho");
            entity.Property(e => e._3Web)
                .HasMaxLength(255)
                .HasColumnName("3-Web");
            entity.Property(e => e._4Continuous)
                .HasMaxLength(255)
                .HasColumnName("4-Continuous");
            entity.Property(e => e._4Digital)
                .HasMaxLength(255)
                .HasColumnName("4-Digital");
            entity.Property(e => e._4SheetfedLitho)
                .HasMaxLength(255)
                .HasColumnName("4-SheetfedLitho");
            entity.Property(e => e._4Web)
                .HasMaxLength(255)
                .HasColumnName("4-Web");
            entity.Property(e => e._5FlexoEnvelopes)
                .HasMaxLength(255)
                .HasColumnName("5-FlexoEnvelopes");
            entity.Property(e => e._5LithoEnvelopes)
                .HasMaxLength(255)
                .HasColumnName("5-LithoEnvelopes");
            entity.Property(e => e._6Continuous)
                .HasMaxLength(255)
                .HasColumnName("6-Continuous");
            entity.Property(e => e._6Digital)
                .HasMaxLength(255)
                .HasColumnName("6-Digital");
            entity.Property(e => e._6Flexo)
                .HasMaxLength(255)
                .HasColumnName("6-Flexo");
            entity.Property(e => e._6Screen)
                .HasMaxLength(255)
                .HasColumnName("6-Screen");
            entity.Property(e => e._6SheetfedLitho)
                .HasMaxLength(255)
                .HasColumnName("6-SheetfedLitho");
            entity.Property(e => e._7Continuous)
                .HasMaxLength(255)
                .HasColumnName("7-Continuous");
            entity.Property(e => e._8Digital)
                .HasMaxLength(255)
                .HasColumnName("8-Digital");
            entity.Property(e => e._8SheetfedLitho)
                .HasMaxLength(255)
                .HasColumnName("8-SheetfedLitho");
            entity.Property(e => e._9Continuous)
                .HasMaxLength(255)
                .HasColumnName("9-Continuous");
            entity.Property(e => e._9Digital)
                .HasMaxLength(255)
                .HasColumnName("9-Digital");
            entity.Property(e => e._9SheetfedLitho)
                .HasMaxLength(255)
                .HasColumnName("9-SheetfedLitho");
        });

        modelBuilder.Entity<TblProductCatalogue>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("tblProductCatalogue");

            entity.Property(e => e.FinishedSize).HasMaxLength(255);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.PrintingProcess).HasMaxLength(50);
            entity.Property(e => e.ProductName).HasMaxLength(255);
            entity.Property(e => e.ProductSpecification).HasMaxLength(255);
            entity.Property(e => e.Quote).HasMaxLength(50);
            entity.Property(e => e.Thickness).HasMaxLength(255);
            entity.Property(e => e.Weight).HasMaxLength(255);
        });

        modelBuilder.Entity<TblProductCatalogueNew>(entity =>
        {
            entity.ToTable("tblProductCatalogueNew");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.ProductCategoryId).HasColumnName("ProductCategoryID");
            entity.Property(e => e.Savings).HasMaxLength(50);
            entity.Property(e => e.Supllier)
                .HasMaxLength(50)
                .HasColumnName("supllier");
        });

        modelBuilder.Entity<TblProductCategory>(entity =>
        {
            entity.ToTable("tblProductCategory");

            entity.Property(e => e.Category).HasMaxLength(50);
            entity.Property(e => e.CategoryJob)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.Categorytype)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.ChineseSimplified).HasMaxLength(50);
            entity.Property(e => e.ChineseTraditional).HasMaxLength(50);
            entity.Property(e => e.Indonesia).HasMaxLength(50);
            entity.Property(e => e.Japanese).HasMaxLength(50);
            entity.Property(e => e.Korean).HasMaxLength(50);
        });

        modelBuilder.Entity<TblProductMethod>(entity =>
        {
            entity.ToTable("tblProductMethods");

            entity.Property(e => e.ChineseSimplified).HasMaxLength(50);
            entity.Property(e => e.ChineseTraditional).HasMaxLength(50);
            entity.Property(e => e.Indonesia).HasMaxLength(50);
            entity.Property(e => e.Japanese).HasMaxLength(50);
            entity.Property(e => e.Korean).HasMaxLength(50);
            entity.Property(e => e.ProductionMethod).HasMaxLength(50);
        });

        modelBuilder.Entity<TblProductSpecification>(entity =>
        {
            entity.ToTable("tblProductSpecification");

            entity.Property(e => e.CatalogueId).HasColumnName("CatalogueID");
            entity.Property(e => e.FieldId).HasColumnName("FieldID");
            entity.Property(e => e.GroupId).HasColumnName("GroupID");
        });

        modelBuilder.Entity<TblProductionMethod>(entity =>
        {
            entity.ToTable("tblProductionMethod");

            entity.Property(e => e.ChineseSimplified).HasMaxLength(50);
            entity.Property(e => e.ChineseTraditional).HasMaxLength(50);
            entity.Property(e => e.Indonesia).HasMaxLength(50);
            entity.Property(e => e.Japanese).HasMaxLength(50);
            entity.Property(e => e.Korean).HasMaxLength(50);
            entity.Property(e => e.ProductCatecoryId).HasMaxLength(50);
            entity.Property(e => e.ProductionMethod).HasMaxLength(50);
        });

        modelBuilder.Entity<TblProductionMethodGroup>(entity =>
        {
            entity.ToTable("tblProductionMethodGroup");

            entity.Property(e => e.ChineseSimplified).HasMaxLength(50);
            entity.Property(e => e.ChineseTraditional).HasMaxLength(50);
            entity.Property(e => e.GroupName).HasMaxLength(50);
            entity.Property(e => e.Indonesia).HasMaxLength(50);
            entity.Property(e => e.Japanese).HasMaxLength(50);
            entity.Property(e => e.Korean).HasMaxLength(50);
        });

        modelBuilder.Entity<TblPromoOssorderWindow>(entity =>
        {
            entity.ToTable("tblPromoOSSOrderWindows");

            entity.Property(e => e.PromoOssorderWindows).HasColumnName("PromoOSSOrderWindows");
        });

        modelBuilder.Entity<TblRegionalcatalogname>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("tblRegionalcatalogname");

            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.RegionalName).HasMaxLength(500);
        });

        modelBuilder.Entity<TblSavingReason>(entity =>
        {
            entity.ToTable("tblSavingReasons");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.SavingReson).HasMaxLength(500);
            entity.Property(e => e.Savingstype)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Status).HasDefaultValue(true);
        });

        modelBuilder.Entity<TblSavingsDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_tblSavingsDetails_1");

            entity.ToTable("tblSavingsDetails");

            entity.HasIndex(e => e.EnquiryDetailsId, "IX_tblSavingsDetails").IsUnique();

            entity.Property(e => e.Baselineprice).HasColumnName("baselineprice");
            entity.Property(e => e.GauranteedPmgsavings).HasColumnName("GauranteedPMGSavings");
            entity.Property(e => e.GauranteedsavingPmgsellprice).HasColumnName("GauranteedsavingPMGSellprice");
            entity.Property(e => e.GauranteedsavingPmgunitprice).HasColumnName("GauranteedsavingPMGUnitprice");
            entity.Property(e => e.ItemSellPrice).HasColumnType("decimal(24, 2)");
            entity.Property(e => e.ManagementFee).HasColumnType("decimal(24, 2)");
            entity.Property(e => e.Negitemsellprice)
                .HasColumnType("decimal(24, 2)")
                .HasColumnName("negitemsellprice");
            entity.Property(e => e.ServerTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TotalPmggainShareSellPriceandnegamountandmfee).HasColumnName("TotalPMGGainShareSellPriceandnegamountandmfee");
            entity.Property(e => e.TotalPmggainShareUnitPriceandnegamountandmfee).HasColumnName("TotalPMGGainShareUnitPriceandnegamountandmfee");
            entity.Property(e => e.TotalPmggainUnitPrice).HasColumnName("TotalPMGGainUnitPrice");
        });

        modelBuilder.Entity<TblSla>(entity =>
        {
            entity.ToTable("tblSLA");

            entity.Property(e => e.ServerTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<TblSourcinglocation>(entity =>
        {
            entity.ToTable("tblSourcinglocation");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Status)
                .HasDefaultValue(true)
                .HasColumnName("status");
        });

        modelBuilder.Entity<TblSubCategory>(entity =>
        {
            entity.ToTable("tblSubCategory");

            entity.Property(e => e.CatAbc)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("CatABC");
            entity.Property(e => e.Typeofjob)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Zsim).HasColumnName("ZSIM");
        });

        modelBuilder.Entity<TblSupplierQuote>(entity =>
        {
            entity.ToTable("tblSupplierQuotes", tb => tb.HasTrigger("trgOnQuotePriceChange"));

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.EnquiryDetailsId).HasColumnName("EnquiryDetailsID");
            entity.Property(e => e.EnquiryId).HasColumnName("EnquiryID");
            entity.Property(e => e.InitAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.QuoteStatus).HasDefaultValue(1);
            entity.Property(e => e.ResponseDate).HasColumnType("datetime");
            entity.Property(e => e.SaveAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ServerTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.SupplierId).HasColumnName("SupplierID");
            entity.Property(e => e.SupplierNotes).HasMaxLength(250);
            entity.Property(e => e.SupplierUserId).HasColumnName("SupplierUserID");
        });

        modelBuilder.Entity<TblSupplierQuoteHistory>(entity =>
        {
            entity.ToTable("tblSupplierQuoteHistory");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.EnquiryDetailsId).HasColumnName("EnquiryDetailsID");
            entity.Property(e => e.EnquiryId).HasColumnName("EnquiryID");
            entity.Property(e => e.QuoteStatus).HasDefaultValue(1);
            entity.Property(e => e.ServerTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Status).HasDefaultValue(1);
            entity.Property(e => e.SupplierId).HasColumnName("SupplierID");
            entity.Property(e => e.SupplierNotes).HasMaxLength(250);
            entity.Property(e => e.SupplierUserId).HasColumnName("SupplierUserID");
        });

        modelBuilder.Entity<TblSupplierQuotesforfirstround>(entity =>
        {
            entity.ToTable("tblSupplierQuotesforfirstround");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.EnquiryDetailsId).HasColumnName("EnquiryDetailsID");
            entity.Property(e => e.EnquiryId).HasColumnName("EnquiryID");
            entity.Property(e => e.InitAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.QuoteStatus).HasDefaultValue(1);
            entity.Property(e => e.ResponseDate).HasColumnType("datetime");
            entity.Property(e => e.SaveAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ServerTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.SupplierId).HasColumnName("SupplierID");
            entity.Property(e => e.SupplierNotes).HasMaxLength(250);
            entity.Property(e => e.SupplierUserId).HasColumnName("SupplierUserID");
        });

        modelBuilder.Entity<TblSupplierUser>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Sheet1$");

            entity.ToTable("tblSupplierUsers");

            entity.Property(e => e.EmailAddressLive)
                .HasMaxLength(255)
                .HasColumnName("EmailAddressLIVE");
            entity.Property(e => e.EmailAddressUat)
                .HasMaxLength(255)
                .HasColumnName("EmailAddressUAT");
            entity.Property(e => e.FirstName).HasMaxLength(255);
            entity.Property(e => e.JobTitle).HasMaxLength(255);
            entity.Property(e => e.LastName).HasMaxLength(255);
            entity.Property(e => e.PhoneNumber).HasMaxLength(255);
            entity.Property(e => e.Servertime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<TblSuppliersList>(entity =>
        {
            entity.ToTable("tblSuppliersList");

            entity.Property(e => e.AddressLine1).HasMaxLength(250);
            entity.Property(e => e.AddressLine2).HasMaxLength(250);
            entity.Property(e => e.AddressLine3).HasMaxLength(250);
            entity.Property(e => e.AddressLine4).HasMaxLength(250);
            entity.Property(e => e.City).HasMaxLength(50);
            entity.Property(e => e.ClientName).HasMaxLength(50);
            entity.Property(e => e.Country).HasMaxLength(50);
            entity.Property(e => e.Currency).HasMaxLength(50);
            entity.Property(e => e.Gmpaccredited)
                .HasMaxLength(250)
                .HasColumnName("GMPAccredited");
            entity.Property(e => e.Language).HasMaxLength(50);
            entity.Property(e => e.Natureofsupplier).HasMaxLength(250);
            entity.Property(e => e.Productioncapability).HasDefaultValue(1);
            entity.Property(e => e.Smetaaccredited)
                .HasMaxLength(250)
                .HasColumnName("SMETAAccredited");
            entity.Property(e => e.Status).HasDefaultValue(true);
            entity.Property(e => e.SupplierName).HasMaxLength(250);
            entity.Property(e => e.SupplierNameEn)
                .HasMaxLength(250)
                .HasColumnName("SupplierNameEN");
            entity.Property(e => e.SupplierType).HasMaxLength(250);
            entity.Property(e => e.Suppliercode).HasMaxLength(200);
        });

        modelBuilder.Entity<TblSurvey>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("tblSurvey");

            entity.Property(e => e.Accuracy).HasMaxLength(10);
            entity.Property(e => e.AccuracyComments).HasMaxLength(250);
            entity.Property(e => e.Country).HasMaxLength(50);
            entity.Property(e => e.Deliverytarget).HasMaxLength(20);
            entity.Property(e => e.DeliverytargetComments).HasMaxLength(250);
            entity.Property(e => e.EnqUid).HasMaxLength(50);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.OvarallExp).HasMaxLength(10);
            entity.Property(e => e.OvarallExpComments).HasMaxLength(250);
            entity.Property(e => e.Quality).HasMaxLength(10);
            entity.Property(e => e.QualityComments).HasMaxLength(250);
            entity.Property(e => e.Responsive).HasMaxLength(10);
            entity.Property(e => e.SubmitedBy).HasMaxLength(20);
            entity.Property(e => e.SurveyDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<TblTemp>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("tblTemp");

            entity.Property(e => e.EndDateTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.StartDateTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<TblTypeofItem>(entity =>
        {
            entity.ToTable("tblTypeofItem");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Status).HasDefaultValue(true);
        });

        modelBuilder.Entity<TblTypeofjob>(entity =>
        {
            entity.ToTable("tblTypeofjob");

            entity.Property(e => e.ChineseSimplified).HasMaxLength(50);
            entity.Property(e => e.ChineseTraditional).HasMaxLength(50);
            entity.Property(e => e.Indonesia).HasMaxLength(50);
            entity.Property(e => e.Japanese).HasMaxLength(50);
            entity.Property(e => e.Korean).HasMaxLength(50);
            entity.Property(e => e.Typeofjob)
                .HasMaxLength(50)
                .HasColumnName("typeofjob");
        });

        modelBuilder.Entity<TblUser>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_tblUser_1");

            entity.ToTable("tblUser", tb => tb.HasTrigger("TR_IUD_UserInsert"));

            entity.Property(e => e.ClientMembers).HasMaxLength(50);
            entity.Property(e => e.Ip).HasMaxLength(255);
            entity.Property(e => e.It).HasColumnName("IT");
            entity.Property(e => e.LastLogon).HasMaxLength(255);
            entity.Property(e => e.Papm)
                .HasDefaultValue(false)
                .HasColumnName("PAPM");
            entity.Property(e => e.PapmToken)
                .HasMaxLength(100)
                .HasColumnName("PAPM_Token");
            entity.Property(e => e.Password).HasMaxLength(255);
            entity.Property(e => e.Portal)
                .IsUnicode(false)
                .HasColumnName("portal");
            entity.Property(e => e.Status).HasDefaultValue(true);
            entity.Property(e => e.UniqueId)
                .HasMaxLength(255)
                .HasColumnName("UniqueID");
            entity.Property(e => e.UserName).HasMaxLength(255);
            entity.Property(e => e.UserType).HasMaxLength(255);
        });

        modelBuilder.Entity<TblWebAdmin>(entity =>
        {
            entity.ToTable("tblWebAdmin");
        });

        modelBuilder.Entity<TblWebAdminOld09052024>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("tblWebAdmin_old_09052024");

            entity.Property(e => e.Id).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<TblcatalogUser>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("tblcatalogUsers");

            entity.Property(e => e.Country)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Currency)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("id");
            entity.Property(e => e.Username)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("username");
        });

        modelBuilder.Entity<Tblcataloglogindetail>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("TBLCATALOGLOGINDETAILS");

            entity.Property(e => e.Crdate)
                .HasColumnType("smalldatetime")
                .HasColumnName("CRDATE");
            entity.Property(e => e.Emailid)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("EMAILID");
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("ID");
            entity.Property(e => e.Password)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("PASSWORD");
            entity.Property(e => e.Type)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("TYPE");
            entity.Property(e => e.Uniquetoken)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("UNIQUETOKEN");
            entity.Property(e => e.Username)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("USERNAME");
        });

        modelBuilder.Entity<Tblcurrency>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("tblcurrency");

            entity.Property(e => e.Crdate)
                .HasColumnType("smalldatetime")
                .HasColumnName("crdate");
            entity.Property(e => e.Currency)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("ID");
            entity.Property(e => e.Value).HasColumnType("numeric(15, 2)");
        });

        modelBuilder.Entity<Tbldivisionold>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("tbldivisionold");

            entity.Property(e => e.BussinessUnit)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ClientId).HasColumnName("ClientID");
            entity.Property(e => e.ClientName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Country)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.DepartmentOrProduct)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.EntityName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.GlobalBumapping)
                .HasMaxLength(150)
                .HasColumnName("GlobalBUMapping");
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("ID");
        });

        modelBuilder.Entity<Tblenvironmentalimpact>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("tblenvironmentalimpact");

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("id");
            entity.Property(e => e.MaterialUsed).HasMaxLength(200);
            entity.Property(e => e.Status).HasColumnName("status");
        });

        modelBuilder.Entity<Tblenvironmentcomparison>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("tblenvironmentcomparison");

            entity.Property(e => e.CEmbodiedCarbon).HasColumnName("C_EmbodiedCarbon");
            entity.Property(e => e.CEnergyConsumption).HasColumnName("C_EnergyConsumption");
            entity.Property(e => e.CWaterUsage).HasColumnName("C_WaterUsage");
            entity.Property(e => e.CWoodUse).HasColumnName("C_WoodUse");
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("id");
        });

        modelBuilder.Entity<Tblexceptionreason>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("TBLExceptionreason");

            entity.Property(e => e.Exceptionreason).HasMaxLength(250);
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("id");
            entity.Property(e => e.Status).HasColumnName("status");
        });

        modelBuilder.Entity<Tblmanagmenttype>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("TBLMANAGMENTTYPE");

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("ID");
            entity.Property(e => e.Managementfeetypename)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("MANAGEMENTFEETYPENAME");
            entity.Property(e => e.Managementtypevalue)
                .HasColumnType("numeric(15, 2)")
                .HasColumnName("MANAGEMENTTYPEVALUE");
            entity.Property(e => e.Servertime)
                .HasColumnType("smalldatetime")
                .HasColumnName("SERVERTIME");
            entity.Property(e => e.Status)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasColumnName("STATUS");
        });

        modelBuilder.Entity<Tblpwdhistory>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("tblpwdhistory", tb => tb.HasTrigger("trg_user_password_histories_insert"));

            entity.HasIndex(e => new { e.Userid, e.Password }, "pwd_is_not_reused").IsUnique();

            entity.Property(e => e.Createddate)
                .HasColumnType("smalldatetime")
                .HasColumnName("createddate");
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("id");
            entity.Property(e => e.Ip).HasMaxLength(50);
            entity.Property(e => e.Password)
                .HasMaxLength(100)
                .HasColumnName("password");
            entity.Property(e => e.Portal)
                .HasMaxLength(20)
                .HasColumnName("portal");
            entity.Property(e => e.Userid).HasColumnName("userid");
            entity.Property(e => e.Username)
                .HasMaxLength(100)
                .HasColumnName("username");
            entity.Property(e => e.Usertype)
                .HasMaxLength(20)
                .HasColumnName("usertype");
        });

        modelBuilder.Entity<Tblversiondatum>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("tblversiondata");

            entity.Property(e => e.InitQuoteamount).HasColumnType("numeric(18, 4)");
            entity.Property(e => e.InitUnitprice).HasColumnType("numeric(18, 4)");
            entity.Property(e => e.Modifydate).HasColumnType("smalldatetime");
            entity.Property(e => e.NegoQuoteamount).HasColumnType("numeric(18, 4)");
            entity.Property(e => e.NegoUnitprice).HasColumnType("numeric(18, 4)");
            entity.Property(e => e.Sellprice).HasColumnType("numeric(18, 4)");
            entity.Property(e => e.UnitpricewithMf).HasColumnType("numeric(18, 4)");
            entity.Property(e => e.Versionid).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<TempProductcategory20220826>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("temp_productcategory_20220826");

            entity.Property(e => e.Category).HasMaxLength(50);
            entity.Property(e => e.CategoryJob)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.Categorytype)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.ChineseSimplified).HasMaxLength(50);
            entity.Property(e => e.ChineseTraditional).HasMaxLength(50);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Indonesia).HasMaxLength(50);
            entity.Property(e => e.Japanese).HasMaxLength(50);
            entity.Property(e => e.Korean).HasMaxLength(50);
        });

        modelBuilder.Entity<Tempdivision>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("tempdivision");

            entity.Property(e => e.BussinessUnit)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ClientId).HasColumnName("ClientID");
            entity.Property(e => e.ClientName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Country)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.DepartmentOrProduct)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.EntityName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.GlobalBumapping)
                .HasMaxLength(150)
                .HasColumnName("GlobalBUMapping");
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("ID");
        });

        modelBuilder.Entity<Tttblproductcategory>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("tttblproductcategory");

            entity.Property(e => e.Category).HasMaxLength(50);
            entity.Property(e => e.CategoryJob)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.Categorytype)
                .HasMaxLength(25)
                .IsUnicode(false);
            entity.Property(e => e.ChineseSimplified).HasMaxLength(50);
            entity.Property(e => e.ChineseTraditional).HasMaxLength(50);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Indonesia).HasMaxLength(50);
            entity.Property(e => e.Japanese).HasMaxLength(50);
            entity.Property(e => e.Korean).HasMaxLength(50);
        });

        modelBuilder.Entity<ViewLogonHistory>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("ViewLogonHistory");

            entity.Property(e => e.Ipaddress)
                .HasMaxLength(50)
                .HasColumnName("IPAddress");
            entity.Property(e => e.Location).HasMaxLength(50);
            entity.Property(e => e.LoginDate).HasColumnType("datetime");
            entity.Property(e => e.UserName).HasMaxLength(255);
            entity.Property(e => e.UserType).HasMaxLength(255);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
