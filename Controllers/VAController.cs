using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Newtonsoft.Json.Linq;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Xml.Linq;
using VAapiNestle.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static VAapiNestle.CustomModels.Base;
using static VAapiNestle.Models.MasterDbContext;

namespace VAapiNestle.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VAController : ControllerBase
    {
        private readonly AppDbContext instancedb;
        private readonly MasterDbContext _masterdb;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IConfiguration _configuration;

        public VAController(AppDbContext db, MasterDbContext masterdb, IWebHostEnvironment hostingEnvironment, IConfiguration configuration)
        {
            instancedb = db;
            _masterdb = masterdb;
            _hostingEnvironment = hostingEnvironment;
            _configuration = configuration;
        }

        #region Usersignon
        [HttpPost("UserSignon")]
        public Result UserSignon(UserRequest User)
        {
            Result rslt = new Result();
            try
            {

                rslt.Status = false;
                if (instancedb.TblUsers.Where(x => x.UserName == User.Username).Count() > 0)
                {
                    if (instancedb.TblUsers.Where(x => x.UserName == User.Username && x.Password == User.Password).Count() > 0)
                    {
                        rslt.Status = true;
                        User usr = new User();
                        var tblusers = instancedb.TblUsers.Where(x => x.UserName == User.Username && x.Password == User.Password).FirstOrDefault();
                        usr.Username = tblusers.UserName;
                        usr.password = tblusers.Password;
                        usr.Usertype = tblusers.UserType;
                        int fkid = tblusers.FkId;
                        usr.UserID = tblusers.Id;
                        usr.FkID = tblusers.FkId;

                        if (usr.Usertype == "Agency")
                        {
                            var agencyusers = instancedb.TblAgencyUsers.Where(x => x.Id == fkid).FirstOrDefault();
                            usr.Country = agencyusers.Country;
                            usr.Email = agencyusers.Email;
                            usr.Role = agencyusers.JobRole;
                            //usr.Usertype = "1";
                        }
                        else if (usr.Usertype == "Supplier")
                        {
                            var supplierusers = instancedb.TblSupplierUsers.Where(x => x.Id == fkid).FirstOrDefault();
                            usr.Email = supplierusers.EmailAddressLive;
                            int suppid = Convert.ToInt32(supplierusers.SupplierId);
                            var supplierslist = instancedb.TblSuppliersLists.Where(x => x.Id == suppid).FirstOrDefault();
                            usr.Country = supplierslist.Country;
                            usr.Currency = supplierslist.Currency;
                            //usr.Usertype = "2";
                        }
                        else
                        {
                            var clientusers = instancedb.TblClients.Where(x => x.Id == fkid).FirstOrDefault();
                            usr.Country = clientusers.Country;
                            usr.Email = clientusers.Email;

                            //usr.Usertype = "3";
                        }
                        usr.Currency = instancedb.TblCountries.Where(x => x.Country == usr.Country).Select(s => s.Currency).FirstOrDefault();
                        usr.CountryId = instancedb.TblCountries.Where(x => x.Country == usr.Country).Select(s => s.Id).FirstOrDefault();
                        rslt.Data = usr;
                    }
                    else
                        rslt.Status = false;
                        rslt.Data = " Username and password not matched";
                }
                else
                    rslt.Status = false;
                    rslt.Data = " Invalid Username ";

            }
            catch (Exception ex)
            {
                rslt.Status = false;
                rslt.Data = ex.Message;
            }
            return rslt;
        }
        #endregion

        #region ChangePassword
        [HttpPost("ChangePassword")]
        public Result ChangePassword(UserRequest request)
        {
            Result rslt = new Result();

            try
            {
                var user = instancedb.TblUsers.FirstOrDefault(x => x.UserName == request.Username && x.Password == request.Password);
                if (user == null)
                {
                    rslt.Status = false;
                    rslt.Data = "Current password is incorrect";
                    return rslt;
                }

                var lastFive = instancedb.Tblpwdhistories.Where(x => x.Userid == user.Id)
                    .OrderByDescending(x => x.Id).Take(5).Select(x => x.Password);

                if (lastFive.Contains(request.NewPassword.Trim()))
                {
                    rslt.Status = false;
                    rslt.Data = "Can't use the last 5 passwords";
                    return rslt;
                }
                user.Password = request.NewPassword.Trim();
                instancedb.SaveChanges();

                rslt.Status = true;
                rslt.Data = "Password changed successfully";

            }
            catch (Exception ex)
            {
                rslt.Status = false;
                rslt.Data = ex.Message;
            }
            return rslt;
        }
        #endregion

        #region Multilangual
        [HttpPost("Language")]
        public async Task<Result> Language([FromBody] UserRequest request)
        {

            Result rslt = new Result();

            var result = _masterdb.TblLanguages
             .Where(x => x.Status == true)
             .AsEnumerable()
             .ToDictionary(
                 x => x.ResourceKey,
                 x => x.GetType().GetProperty(request.Lang)?.GetValue(x)?.ToString()
             );
            if (result.Count > 0)
            {
                rslt.Status = true;
                rslt.Data = result;
            }
            else
            {
                rslt.Status = false;
                rslt.Data = _masterdb.TblLanguages
                 .Where(x => x.Status == true)
                 .AsEnumerable()
                 .ToDictionary(
                     x => x.ResourceKey,
                     x => x.GetType().GetProperty("English")?.GetValue(x)?.ToString()
                 );
            }
            return rslt;
        }
        #endregion

        #region Dashboard
        [HttpPost("ViewDashboard")]
        public async Task<Result> ViewDashboard([FromBody] EnquiryFilterRequest req)
        {
            Result rslt = new Result();

            try
            {
                if (req == null)
                {
                    rslt.Status = false;
                    rslt.Data = "Request is null";
                    return rslt;
                }
                var res = GetProjects(req);
                var processedData = res.Select(pi => new
                {
                    Id = pi.Id,
                    EnquiryId = pi.EnquiryId,
                    ProjectNo = pi.ProjectNo,
                    ProjectDesc = pi.ProjectDesc,
                    ServerTime = pi.ServerTime,
                    Status = pi.Status,
                    StatusName = pi.StatusName,
                    PmgEntity = pi.PmgEntity,
                    BiddingStartDate = pi.BiddingStartDate,
                    BiddingEndDate = pi.BiddingEndDate,
                    TimeOffset = pi.TimeOffset,
                    Duration = pi.Duration,
                    StepId = pi.StepId,
                    ClientId = pi.ClientId,
                    Orderid = pi.Orderid,
                    QuoteEndDate = pi.QuoteEndDate,
                    serchjobs = pi.serchjobs,
                    Attachments = pi.Attachments,
                    CreatedBy = pi.CreatedBy,
                    surveystatus = pi.surveystatus,
                    Submitedby = pi.Submitedby,
                    Surveycount = pi.Surveycount,
                    OldsurveyENQ = pi.OldsurveyENQ,
                    Ponumber = pi.Ponumber,
                    SurveyStatusName = GetSurveyStatusName(pi),
                    Country = pi.Country
                    // Include other properties if needed
                }).ToList().OrderByDescending(x => x.EnquiryId);
                var statusSummary = processedData
                .GroupBy(x => new { x.Status, x.StatusName })
                .Select(g => new GetJobStatus
                {
                    StatusId = g.Key.Status,
                    StatusName = g.Key.StatusName,
                    EnquiryCount = g.Count()
                })
                .OrderByDescending(x => x.EnquiryCount)
                .ToList();
                var model = new GetJobCount
                {
                    JobStatus = statusSummary,
                    active = processedData.Where(x => x.Status == 1).Select(x => x.Id).Distinct().Count(),
                    approval = processedData.Where(x => x.Status == 3).Select(x => x.Id).Distinct().Count(),
                    awarded = processedData.Where(x => x.Status >= 6 && x.Status != 24).Select(x => x.Id).Distinct().Count(),
                    completed = processedData.Where(x => x.Status == 24).Select(x => x.Id).Distinct().Count()
                };
                rslt.Status = true;
                rslt.Data = new
                {
                    Summary = model,
                    Detailed = processedData
                };
            }
            catch (Exception ex)
            {
                rslt.Status = false;
                rslt.Data = ex.Message;
            }

            return rslt;
        }
        [NonAction]
        public string GetMenuName(int? Menuid)
        {
            return Menuid switch
            {
                2 => "E-Bidding",
                3 => "Catalog",
                _ => "Enquiry"
            };
        }
        [NonAction]
        public List<ProjectInformation> GetProjects(EnquiryFilterRequest req)
        {

            req.CreatedName = (from tu in instancedb.TblUsers
                               where tu.UserName == req.Username
                               select tu.Id).FirstOrDefault();
            var model = new List<ProjectInformation>();
            try
            {
                var now = DateTime.Now;
                var last12Months = now.AddMonths(-12);
                var query = (from tm in instancedb.TblEnquiryMasters
                             join tdGroup in instancedb.TblEnquiryDetails
                             on tm.Id equals tdGroup.EnquiryId into tdg
                             let td = tdg.OrderBy(x => x.Id).FirstOrDefault()
                             join tblJobStatus in instancedb.TblJobStatuses on td.Status equals tblJobStatus.StatusId
                             join tblCountry in instancedb.TblCountries on tm.PmgEntity equals tblCountry.Id
                             join ta in instancedb.TblAgencyUsers on tm.CreatedBy equals ta.Userid
                             where
                             //tm.Status == true
                             //      && tm.JobType == GetMenuName(req.Menuid)
                             //      && (td.Status >= 1 && td.Status <= 25 && tm.PmgEntity == req.UserCountryId && tm.Status == true)
                             //   || (tm.CreatedBy == req.CreatedName && td.Status == 0 && tm.PmgEntity == req.UserCountryId && tm.Status == true)

                              tm.Status == true && tm.JobType == GetMenuName(req.Menuid) && tm.PmgEntity == req.UserCountryId && ((td.Status >= 1 && td.Status <= 25) || (tm.CreatedBy == req.CreatedName && td.Status == 0))
                              && ((!string.IsNullOrEmpty(req.StartDate) && !string.IsNullOrEmpty(req.EndDate) && tm.ServerTime >= Convert.ToDateTime(req.StartDate) && tm.ServerTime < Convert.ToDateTime(req.EndDate).AddDays(1))
                              || (string.IsNullOrEmpty(req.StartDate) && string.IsNullOrEmpty(req.EndDate) && tm.ServerTime >= last12Months && tm.ServerTime <= now))

                             //  && EF.Functions.Like(tm.EnqUid, "%ENQ%")
                             orderby tm.Id descending
                             select new ProjectInformation
                             {
                                 Id = tm.Id,
                                 EnquiryId = tm.EnqUid,
                                 ProjectNo = tm.ProjectNo ?? "",
                                 ProjectDesc = tm.ProjectDesc ?? "",
                                 ServerTime = tm.ServerTime.ToString("dd/MM/yyyy"),
                                 Status = td.Status,
                                 StatusName = tblJobStatus.StatusName,
                                 PmgEntity = tm.PmgEntity,
                                 BiddingStartDate = tm.BiddingStartDate,
                                 BiddingEndDate = tm.BiddingEndDate,
                                 TimeOffset = tblCountry.TimeOffset,
                                 Duration = tm.Duration,
                                 StepId = tm.StepId,
                                 ClientId = tm.ClientId,
                                 Orderid = tm.Orderid ?? "",
                                 QuoteEndDate = td.QuoteEndDate,
                                 serchjobs = td.Status == 1 ? 30 : td.Status == 3 ? 31 : td.Status >= 6 ? 32 : td.Status,
                                 Attachments = instancedb.TblAttachments.Count(a => a.EnqId == tm.Id),
                                 CreatedBy = ta.FirstName + " " + ta.LastName,
                                 surveystatus = tm.Surveynotification == "YES" ? 1 : 0,
                                 Submitedby = (instancedb.TblSurveys.Where(s => s.EnqId == tm.Id).OrderBy(s => s.SubmitedBy).Select(s => s.SubmitedBy).FirstOrDefault()) == "Client" ? "Client" : "Default",
                                 Surveycount = instancedb.TblSurveys.Count(s => s.EnqId == tm.Id),
                                 OldsurveyENQ = instancedb.TblJobHistories.Any(jh => jh.EnqId == tm.Id && jh.NewValue == "24" && jh.Field == "Status") ? "NO" : "YES",
                                 Ponumber = tm.Ponumber ?? "",
                                 Country = tblCountry.Country

                             }).Distinct().ToList();
                return query;

            }
            catch (Exception ex)
            {
                Trace.TraceError($"An error occurred: {ex.InnerException}");
            }
            return model;
        }
        [NonAction]
        private string? GetSurveyStatusName(ProjectInformation pi)
        {
            if (pi.surveystatus == 1 && pi.OldsurveyENQ == "NO")
            {
                pi.SurveyStatusName = "Survey email sent to Client";
            }
            else if (pi.StatusName == "Completed" && pi.Surveycount > 0 && pi.OldsurveyENQ == "NO" && pi.Submitedby == "Client")
            {
                pi.SurveyStatusName = "Survey Taken";
            }
            else if (pi.StatusName == "Completed" && pi.surveystatus == 1 && pi.OldsurveyENQ == "YES")
            {
                pi.SurveyStatusName = "";
            }
            else
            {
                pi.SurveyStatusName = "";
            }

            return pi.SurveyStatusName;
        }
        #endregion

        #region Dropdowns
        [HttpPost("MasterDropdowns")]
        public async Task<MasterDropdowns> MasterDropdowns(EnquiryFilterRequest req)
        {
            var isAdmin = string.Equals(req.Role, "Administration", StringComparison.OrdinalIgnoreCase);
            var countryname = instancedb.TblCountries.Where(s => s.Id == req.UserCountryId).Select(s => s.Country).FirstOrDefault();
            var response = new MasterDropdowns
            {
                Country = await instancedb.TblCountries.Where(s => s.Status == true
                //&& (isAdmin || s.Id == req.UserCountryId)
                )
                    .Select(x => new DropdownItem
                    {
                        Value = x.Id,
                        Label = x.Country ?? "",
                        Selected = false
                    }).ToListAsync(),

                Client = await instancedb.TblClientDetails.Where(s => s.Status == true && (isAdmin || s.Country == countryname)
                ).Select(x => new DropdownItem
                {
                    Value = x.Id,
                    Label = x.ClientName ?? "",
                    //Selected = true,
                }).ToListAsync(),

                User = await instancedb.TblAgencyUsers.Where(s => s.Status == true && (s.Id == req.UserId)
                ).Select(x => new DropdownItem
                {
                    Value = x.Id,
                    Label = x.FirstName + " " + x.LastName ?? ""
                }).ToListAsync(),

                Status = await instancedb.TblJobStatuses
                 .Select(x => new DropdownItem
                 {
                     Value = x.StatusId,
                     Label = x.StatusName ?? "",
                     Selected = false
                 }).ToListAsync(),

                ManagementFeetype = await instancedb.Tblmanagmenttypes.Where(s => s.Status == "1")
                    .Select(x => new DropdownItem
                    {
                        Value = x.Id,
                        Label = x.Managementfeetypename ?? "",
                        Selected = false
                    }).OrderByDescending(s => s.Value).ToListAsync(),

                Printcapabilities = await instancedb.TblPrintCapabilities
                    .Select(x => new DropdownItem
                    {
                        Value = x.CapabilityId,
                        Label = x.Capability ?? "",
                        Selected = false
                    }).ToListAsync(),

                Division = await (
                        from d in instancedb.TblDivisions
                        join c in instancedb.TblClientDetails on d.ClientId equals c.Id
                        join s in instancedb.TblCountries on d.Country equals s.Country
                        where c.Status == true && d.Status == true && (isAdmin || s.Id == req.UserCountryId)

                        group d by new
                        {
                            d.EntityName,
                            d.Country,
                            d.BussinessUnit,
                            d.ClientName
                        } into g

                        select new DropdownItem
                        {
                            Value = g.Min(x => x.Id), // same as SQL MIN(ID)
                            Selected = false,
                            Label = (g.Key.ClientName ?? "") + " > " +
                                    (g.Key.Country ?? "") + " > " +
                                    (g.Key.EntityName ?? "") + " > " +
                                    (g.Key.BussinessUnit ?? "")
                        }
                 ).OrderBy(x => x.Label).ToListAsync(),

                Sla = await instancedb.TblSlas.Where(s => s.Status == true && s.Country == countryname).OrderBy(s => s.Description)
                .Select(x => new DropdownItem
                {
                    Value = x.Id,
                    Label = x.Description ?? "",
                    Selected = x.IsDefault
                }).ToListAsync(),

                Year = new List<DropdownItem>
                {
                    new DropdownItem { Value = 1, Label = "Year 1", Selected=false },
                    new DropdownItem { Value = 2, Label = "Year 2", Selected=false },
                    new DropdownItem { Value = 3, Label = "Year 3" , Selected=false},
                    new DropdownItem { Value = 4, Label = "Prev_Contract", Selected=false }
                },

                TypeofJob = new List<DropdownItem>
                {
                    new DropdownItem { Value = 1, Label = "Print" , Selected=false},
                    new DropdownItem { Value = 2, Label = "POSM" , Selected=false},
                    new DropdownItem { Value = 3, Label = "Promo" , Selected = false}
                    //,new DropdownItem { Value = 4, Label = "Others" , Selected = false}
                },
                ProjectAttribute = new List<DropdownItem>
                {
                    new DropdownItem { Value = 1, Label = "Regular Job" , Selected = false},
                    new DropdownItem { Value = 2, Label = "Use & Re-Use" , Selected = false},
                    new DropdownItem { Value = 3, Label = "Re-Engineering" , Selected = false},
                    new DropdownItem { Value = 4, Label = "Harmonisation Program"  , Selected=false },
                    new DropdownItem { Value = 5, Label = "Global Order Window" , Selected = false},
                    new DropdownItem { Value = 6, Label = "Regional Order Window" , Selected = false},
                    new DropdownItem { Value = 7, Label = "Volume Aggregation" , Selected = false},
                    new DropdownItem { Value = 8, Label = "Local Catalogs" , Selected = false},
                    new DropdownItem { Value = 9, Label = "Not Applicable" , Selected = false},
                    new DropdownItem { Value = 10, Label = "Missed Opportunity" , Selected = false}
                }


            };

            return response;
        }

        [HttpPost("GetSlatemplate")]
        public async Task<EnquirySlaDto?> GetSlatemplate(EnquiryFilterRequest req)
        {
            EnquirySlaDto? sla;

            if (req.EnqId.HasValue)
            {
                // Step 1: Check if TemplateId exists
                var templateId = await instancedb.TblEnquiryMasters
                    .Where(m => m.Id == req.EnqId)
                    .Select(m => m.TemplateId)
                    .FirstOrDefaultAsync();

                if (templateId != null)
                {
                    // ✅ Case 1: Template exists → join all tables
                    sla = await (
                        from t in instancedb.TblEnquiryDetails
                        join m in instancedb.TblEnquiryMasters on t.EnquiryId equals m.Id
                        join s in instancedb.TblSlas on m.TemplateId equals s.Id
                        where m.Id == req.EnqId && s.Status
                        select new EnquirySlaDto
                        {
                            Quote = t.ActualQuote ?? s.Quote,
                            Proof = t.ActualProof ?? s.Proof,
                            Production = t.ACtualProduction ?? s.Production,
                            FileCopies = t.ActualFilecopies ?? s.FileCopies,
                            Invoicing = t.ActualInvoicing ?? s.Invoicing,
                            Id = s.Id,
                            Description = s.Description,
                            Status = s.Status,
                            IsDefault = s.IsDefault,
                            DefQuote = s.Quote,
                            DefProof = s.Proof,
                            DefProduction = s.Production,
                            DefFileCopies = s.FileCopies,
                            DefInvoices = s.Invoicing
                        })
                        .AsNoTracking()
                        .FirstOrDefaultAsync();
                }
                else
                {
                    // ✅ Case 2: No Template → fallback to SLA only
                    sla = await instancedb.TblSlas
                        .Where(s => s.Id == req.SlaId && s.Status)
                        .Select(s => new EnquirySlaDto
                        {
                            Quote = s.Quote,
                            Proof = s.Proof,
                            Production = s.Production,
                            FileCopies = s.FileCopies,
                            Invoicing = s.Invoicing,
                            Id = s.Id,
                            Description = s.Description,
                            Status = s.Status,
                            IsDefault = s.IsDefault,
                            DefQuote = s.Quote,
                            DefProof = s.Proof,
                            DefProduction = s.Production,
                            DefFileCopies = s.FileCopies,
                            DefInvoices = s.Invoicing
                        })
                        .AsNoTracking()
                        .FirstOrDefaultAsync();
                }
            }
            else
            {
                // ✅ Case 3: No Enquiry → direct SLA
                sla = await instancedb.TblSlas
                    .Where(s => s.Id == req.SlaId && s.Status)
                    .Select(s => new EnquirySlaDto
                    {
                        Quote = s.Quote,
                        Proof = s.Proof,
                        Production = s.Production,
                        FileCopies = s.FileCopies,
                        Invoicing = s.Invoicing,
                        Id = s.Id,
                        Description = s.Description,
                        Status = s.Status,
                        IsDefault = s.IsDefault,
                        DefQuote = s.Quote,
                        DefProof = s.Proof,
                        DefProduction = s.Production,
                        DefFileCopies = s.FileCopies,
                        DefInvoices = s.Invoicing
                    })
                    .AsNoTracking()
                    .FirstOrDefaultAsync();
            }

            return sla;
        }

        [HttpPost("EnqClientinfoDropdowns")]
        public async Task<MasterDropdowns> EnqClientinfoDropdowns(UserRequest req)
        {
            int Divisionid = req.Divisionid;
            var division = instancedb.TblDivisions.Where(s => s.Id == Divisionid).FirstOrDefault();
            var response = new MasterDropdowns
            {
                Division = await instancedb.TblDivisions.Where(s => s.Country == division.Country && s.BussinessUnit == division.BussinessUnit && s.Status == true)
                .OrderBy(s => s.GlobalBumapping)
                    .Select(x => new DropdownItem
                    {
                        Value = x.Id,
                        Label = x.GlobalBumapping ?? "",
                        Selected = false
                    }).ToListAsync(),

                Brands = await instancedb.TblDivisionBrandsorproducts.Where(s => s.Divisionid == Divisionid)
                .OrderBy(s => s.Brandorproduct)
                    .Select(x => new DropdownItem
                    {
                        Value = x.Id,
                        Label = x.Brandorproduct ?? "",
                        Selected = false
                    }).ToListAsync(),

                Client = await (
                from c in instancedb.TblClients
                join u in instancedb.TblUsers on c.Id equals u.FkId   // 🔁 adjust key if different
                where c.Department == Divisionid && c.Status == true && u.UserType == "Client"
                orderby (c.FirstName + " " + c.LastName)
                select new DropdownItem
                {
                    Value = c.Id,
                    Label = (c.FirstName + " " + c.LastName) + " (" + u.UserName + ")",
                    Selected = false
                }).ToListAsync(),

            };

            return response;
        }

        [HttpPost("EnqLineItemsDropdowns")]
        public async Task<MasterDropdowns> EnqLineItemsDropdowns(UserRequest req)
        {
            var response = new MasterDropdowns
            {
                ItemCategory = (await instancedb.TblProductCategories
                .Where(s => s.Status == true && s.Categorytype != "Catalog")
                .OrderBy(s => s.Category)
                .ToListAsync())
                .DistinctBy(s => s.Category)
                .Select(x => new DropdownItem
                {
                    Value = x.Id,
                    Label = req.Lang == "English" ? (x.Category ?? "") :
                            req.Lang == "Indonesia" ? (x.Indonesia ?? "") :
                            req.Lang == "Japanese" ? (x.Japanese ?? "") :
                            req.Lang == "Korea" ? (x.Korean ?? "") :
                            req.Lang == "Chinese" ? (x.ChineseTraditional ?? "") :
                            req.Lang == "Taiwan" ? (x.ChineseSimplified ?? "") :
                            (x.Category ?? ""),
                    Selected = false
                })
                .ToList(),

                SubCategory = (await instancedb.TblSubCategories.Where(s => s.Status == true && s.Typeofjob == req.TypeOfJob)
                .OrderBy(s => s.SubCategory).ToListAsync())
                .DistinctBy(s => s.SubCategory)
                .Select(x => new DropdownItem
                    {
                        Value = x.Id,
                        Label = x.SubCategory ?? "",
                        Selected = false
                    }).ToList(),

                GlobalOrder = await instancedb.TblPromoOssorderWindows
                .OrderBy(s => s.PromoOssorderWindows)
                .Select(x => new DropdownItem
                    {
                        Value = x.Id,
                        Label = x.PromoOssorderWindows ?? "",
                        Selected = (x.PromoOssorderWindows ?? "") == "N/A" ? true : false
                    }).ToListAsync(),

                ExceptionReason = await instancedb.Tblexceptionreasons.Where(s => s.Status == true)
                .OrderBy(s => s.Exceptionreason)
                .Select(x => new DropdownItem
                {
                    Value = x.Id,
                    Label = x.Exceptionreason ?? "",
                    Selected = false
                }).ToListAsync(),

                LocalCatalog = await instancedb.TblCatalogueUsages.Where(s => s.Status == true)
                .OrderBy(s => s.CatalogueUsage)
                .Select(x => new DropdownItem
                {
                    Value = x.Id,
                    Label = x.CatalogueUsage ?? "",
                    Selected = (x.CatalogueUsage ?? "") == "N/A" ? true : false
                }).ToListAsync(),

                SourcingLocation = await instancedb.TblSourcinglocations.Where(s => s.Status == true)
                .OrderBy(s => s.Sourcinglocation)
                .Select(x => new DropdownItem
                {
                    Value = x.Id,
                    Label = x.Sourcinglocation ?? "",
                    Selected = (x.Sourcinglocation ?? "") == "N/A" ? true : false
                }).ToListAsync(),

                RegionalOrder = await instancedb.TblRegionalcatalognames.Where(s => s.Status == true)
                .OrderBy(s => s.RegionalName)
                .Select(x => new DropdownItem
                {
                    Value = x.Id,
                    Label = x.RegionalName ?? "",
                    Selected = (x.RegionalName ?? "") == "N/A" ? true : false
                }).ToListAsync(),

                TypeOfItem = await GetTypeOfItems(req.TypeOfJob, req.Subitemid),

                SavingsReason = await instancedb.TblSavingReasons.Where(s => s.Status == true && s.Savingstype == req.Savingstype)
                .OrderBy(s => s.SavingReson)
                .Select(x => new DropdownItem
                {
                    Value = x.Id,
                    Label = x.SavingReson ?? "",
                    Selected = false
                }).ToListAsync(),

                PrintingMethod = await instancedb.TblProductMethods.Where(s => s.Status == true)
                .OrderBy(s => s.ProductionMethod)
                .Select(x => new DropdownItem
                {
                    Value = x.Id,
                    Label = req.Lang == "English" ? (x.ProductionMethod ?? "") :
                    req.Lang == "Indonesia" ? (x.Indonesia ?? "") :
                    req.Lang == "Japanese" ? (x.Japanese ?? "") :
                    req.Lang == "Korea" ? (x.Korean ?? "") :
                    req.Lang == "Chinese" ? (x.ChineseTraditional ?? "") :
                    req.Lang == "Taiwan" ? (x.ChineseSimplified ?? "") : (x.ProductionMethod ?? ""),
                    Selected = (x.ProductionMethod ?? "") == "N/A" || (x.ProductionMethod ?? "") == "N/A" ? true : false
                }).ToListAsync(),

                SavingsType = new List<DropdownItem>
                {
                    new DropdownItem { Value = 1, Label = "Cost Avoidance" , Selected=false},
                    new DropdownItem { Value = 2, Label = "Cost Reduction", Selected = false },
                    new DropdownItem { Value = 3, Label = "Non Addressable", Selected = false }
                }
            };

            return response;
        }

        [NonAction]
        public async Task<List<DropdownItem>> GetTypeOfItems(string jobtype, int subitemid)
        {
            var query = instancedb.TblTypeofItems.AsQueryable();

            // 🔹 Step 1: Apply JobType filter
            if (jobtype == "POSM")
            {
                query = query.Where(x => x.Status == true && (x.TypeofJob == "Both" || x.TypeofJob == "POSM"));
            }
            else if (jobtype == "Promo")
            {
                query = query.Where(x => x.Status == true && x.TypeofJob == "Promo");
            }
            else if (jobtype == "Print")
            {
                query = query.Where(x => x.Status == true && (x.TypeofJob == "Both" || x.TypeofJob == "Print"));
            }
            else
            {
                query = query.Where(x => x.Status == true);
            }

            // 🔹 Step 2: Check MarkedTypeofItem from SubCategory
            if (subitemid > 0)
            {
                var marked = await instancedb.TblSubCategories
                    .Where(x => x.Id == subitemid)
                    .Select(x => x.MarkedTypeofItem ?? false)
                    .FirstOrDefaultAsync();

                if (marked)
                {
                    List<string> jobTypes;

                    if (jobtype == "POSM" || jobtype == "Print")
                        jobTypes = new List<string> { "Both", "POSM" };
                    else
                        jobTypes = new List<string> { "Promo" };

                    query = instancedb.TblTypeofItems
                        .Where(x => x.Status == true &&
                                    jobTypes.Contains(x.TypeofJob) &&
                                    x.SubitemId == subitemid);
                }
            }

            // 🔹 Step 3: Final Projection + Sorting
            var dbList = await query
            .OrderBy(x => x.TypeofItem)
            .Select(x => new DropdownItem
            {
                Value = x.Id,
                Label = x.TypeofItem,
                Selected = false
            })
            .ToListAsync();

            // 🔹 Add "N/A" at first position
            dbList.Insert(0, new DropdownItem
            {
                Value = 0,          // or -1 based on your UI handling
                Label = "N/A",
                Selected = true     // optional: make default selected
            });

            return dbList;
        }

        [HttpPost("GetSupplier")]
        public async Task<List<EnqSupplierinfo>?> GetSupplier(UserRequest req)
        {
            var supplier = (await instancedb.TblSuppliersLists.Where(s => s.Currency == req.Currency && s.Status == true).ToListAsync()).OrderBy(s => s.SupplierName)
                .Select(x => new EnqSupplierinfo
                {
                    SupplierId = x.Id,
                    Suppliername = x.SupplierName,
                    Country = x.Country,
                    CountryId = instancedb.TblCountries.Where(s => s.Country == x.Country).Select(s => s.Id).FirstOrDefault(),
                    Currency = x.Currency,
                    Selected = false,
                    Suppliercode = x.Suppliercode ?? "",
                    PrintCapability = x.Productioncapability
                }).ToList();

            return supplier;
        }

        [HttpPost("AddSuppliers")]
        public async Task<Result> AddSuppliers(SupplierInfo req)
        {
            Result rslt = new Result();
            try
            {
                if (req.EnqDetailsId?.Length > 0)
                {
                    for (int i = 0; i < req.EnqDetailsId.Length; i++)
                    {
                        await instancedb.Database.ExecuteSqlInterpolatedAsync($@"EXEC AddNewSuppliersToProject @enqId={req.EnqId}, @enqDetailsId={req.EnqDetailsId[i]}, @newSuppliers={req.NewSelectedSuppliers},
                        @itemNo={i + 1}, @suppliersList={req.SelectedSuppliers}");
                    }
                }
                rslt.Status = true;
                rslt.Data = "Add supplier successfully";
            }
            catch (Exception ex)
            {
                rslt.Status = false;
                rslt.Data = ex.Message;
            }
            return rslt;
        }
        #endregion

        #region Add client
        [HttpPost("AddnewClient")]
        public Result AddnewClient([FromBody] ClientModel req)
        {
            Result rslt = new Result();
            if (ModelState.IsValid)
            {
                var division = instancedb.TblDivisions.Where(s => s.Id == req.Divisionid).FirstOrDefault();
                if (division != null)
                {
                    using (var transaction = instancedb.Database.BeginTransaction())
                    {
                        try
                        {
                            var isExists = instancedb.TblUsers.Any(x => x.UserName == req.Logonid);
                            if (isExists)
                            {
                                rslt.Status = false;
                                rslt.Data = "Username already exists";
                                return rslt;
                            }
                            var isClientExists = instancedb.TblClients.Any(x => x.FirstName == req.FirstName && x.Email == req.Email);
                            if (isClientExists)
                            {
                                rslt.Status = false;
                                rslt.Data = "Client with same firstname and emailid already exists";
                                return rslt;
                            }

                            TblClient tbclient = new TblClient();
                            tbclient.FirstName = req.FirstName;
                            tbclient.LastName = req.LastName;
                            tbclient.JobTitle = req.Jobtitle;
                            tbclient.Jobposition = req.JobPosition;
                            tbclient.Email = req.Email;
                            tbclient.Phone = req.Phone;
                            tbclient.ReceiveNotification = req.Receivenotification;
                            tbclient.Status = true;
                            tbclient.Department = division.Id;
                            tbclient.Gbui = division.GlobalBumapping;
                            tbclient.Country = division.Country;
                            instancedb.TblClients.Add(tbclient);
                            instancedb.SaveChanges();
                            TblUser tbuser = new TblUser();
                            tbuser.UserName = req.Logonid;
                            tbuser.Password = "password";
                            tbuser.FkId = tbclient.Id;
                            tbuser.UserType = "Client";
                            tbuser.Status = true;
                            instancedb.TblUsers.Add(tbuser);
                            instancedb.SaveChanges();
                            tbclient.Userid = tbuser.Id;
                            instancedb.SaveChanges();
                            transaction.Commit();
                            rslt.Status = true;
                            rslt.Data = "Client added successfully";

                        }
                        catch (Exception ex)
                        {
                            rslt.Status = false;
                            rslt.Data = ex.Message;
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
                else
                {
                    rslt.Status = false;
                    rslt.Data = "Division is not valid";
                }

            }
            else
            {
                rslt.Status = false;
                rslt.Data = ModelState.Values
                          .SelectMany(v => v.Errors)
                          .Select(e => e.ErrorMessage);
            }
            return rslt;
        }
        #endregion

        #region Add Brands
        [HttpPost("AddnewBrands")]
        public Result AddnewBrands([FromBody] BrandModel req)
        {
            Result rslt = new Result();
            if (ModelState.IsValid)
            {
                var division = instancedb.TblDivisions.Where(s => s.Id == req.Divisionid).FirstOrDefault();
                if (division != null)
                {
                    var isExists = instancedb.TblDivisionBrandsorproducts.Any(x => x.Divisionid == req.Divisionid && x.Brandorproduct == req.Brand);
                    if (isExists)
                    {
                        rslt.Status = false;
                        rslt.Data = "Brand already exists for this division";
                        return rslt;
                    }
                    TblDivisionBrandsorproduct brnd = new TblDivisionBrandsorproduct();
                    brnd.Divisionid = division.Id;
                    brnd.Brandorproduct = req.Brand;
                    instancedb.TblDivisionBrandsorproducts.Add(brnd);
                    instancedb.SaveChangesAsync();
                    rslt.Status = true;
                    rslt.Data = "Brands added successfully";
                }
                else
                {
                    rslt.Status = false;
                    rslt.Data = "Division is not valid";
                }

            }
            else
            {
                rslt.Status = false;
                rslt.Data = ModelState.Values
                          .SelectMany(v => v.Errors)
                          .Select(e => e.ErrorMessage);
            }

            return rslt;
        }
        #endregion

        #region CheckforUsername
        [HttpPost("CheckforUsername")]
        public Result CheckforUsername(UserRequest req)
        {
            Result rslt = new Result();
            var isExists = instancedb.TblUsers.Any(x => x.UserName == req.Username);
            if (isExists)
            {
                rslt.Status = false;
                rslt.Data = "Username already exists";
            }
            else
            {
                rslt.Status = true;
                rslt.Data = "Username is available";
            }
            return rslt;
        }
        #endregion

        #region DuplicateEntry
        [HttpPost("EB_Duplicate")]
        public async Task<Result> EB_Duplicate([FromBody] EnquiryItem req)
        {
            var rslt = new Result();

            try
            {
                var rows = await instancedb.Database.ExecuteSqlInterpolatedAsync($@"EXEC EB_DuplicateItem  @Createdby={req.ModifiedBy}, @EnqDetailsID={req.EnqdetailsId}, @EnquiryID={req.EnqId}");
                rslt.Status = true;
                rslt.Data = "Duplicated Item successfully";
            }
            catch (Exception ex)
            {
                rslt.Status = false;
                rslt.Data = ex.InnerException?.Message ?? ex.Message;
            }

            return rslt;
        }
        #endregion

        #region SummaryView
        [HttpPost("GetDetails")]
        public GetDetails GetDetails(UserRequest req)
        {
            GetDetails rslt = new GetDetails();
            try
            {
                int Enquiryid = req.Enquiryid;
                var data = (from em in instancedb.TblEnquiryMasters
                            join d in instancedb.TblDivisions on em.DivisionId equals d.Id
                            join c in instancedb.TblClients on em.ClientId equals c.Id
                            join a in instancedb.TblAgencyUsers on em.CreatedBy equals a.Id
                            join U in instancedb.TblUsers on a.Id equals U.FkId
                            join Cl in instancedb.TblClientDetails on d.ClientId equals Cl.Id
                            join E in instancedb.TblEnquiryDetails on em.Id equals E.EnquiryId into ed
                            where em.Id == req.Enquiryid
                            select new
                            {
                                em,
                                d,
                                c,
                                a,
                                U,
                                Cl,
                                ed
                            }).FirstOrDefault();
                if (data != null)
                {
                    var status = data.ed.Select(s => s.Status).FirstOrDefault();
                    var jobstatus = instancedb.TblJobStatuses.Where(s => s.StatusId == status).FirstOrDefault();
                    rslt.StatusId = jobstatus.StatusId;
                    rslt.Jobstatus = jobstatus.StatusName;
                    rslt.StepId = data.em.StepId;
                    rslt.enqClientinfo = new EnqClientinfo
                    {
                        EnqUId = data.em.EnqUid,
                        EnqId = data.em.Id,
                        Divisionid = data.d.Id,
                        CreatedBy = data.em.CreatedBy,
                        ModifiedBy = data.em.ModifiedBy,
                        ClientContactId = data.em.ClientContactId,
                        DeliveryCountryId = data.em.DeliveryCountry,
                        PMGEntity = data.em.PmgEntity,
                        Brand = data.em.Brandorproduct ?? "",
                        AboveorAtmarket = data.em.AboveorAtmarket ?? "",
                        Client = data.Cl.ClientName,
                        Entityname = data.d.EntityName ?? "",
                        BussinessUnit = data.d.BussinessUnit ?? "",
                        Divisionname = data.d.ClientName + " > " + data.d.Country + " > " + data.d.EntityName + " > " + data.d.BussinessUnit,
                        GlobalBussinessUnit = data.d.GlobalBumapping ?? "",
                        CreatedByUser = data.a.FirstName + " " + data.a.LastName + (" (" + data.U.UserName + ")"),
                        ClientContact = data.c.FirstName + " " + data.c.LastName + (" (" + instancedb.TblUsers.Where(s => s.FkId == data.em.ClientContactId && s.UserType == "Client").Select(s => s.UserName).FirstOrDefault() + ")"),
                        PMGEntityname = instancedb.TblCountries.Where(s => s.Id == data.em.PmgEntity).Select(s => s.Country).FirstOrDefault() ?? "",
                        DeliveryCountryname = instancedb.TblCountries.Where(s => s.Id == data.em.DeliveryCountry).Select(s => s.Country).FirstOrDefault() ?? "",
                        Country = instancedb.TblCountries.Where(s => s.Id == data.em.PmgEntity).Select(s => s.Country).FirstOrDefault() ?? "",
                        InvoiceNumber = data.em.InvoiceNumber ?? "",
                        PoNumber = data.em.Ponumber ?? "",
                        RaisedDate = data.em.PoorderraisedDate ?? "",
                        CreatedDate = data.em.EstDate ?? "",
                        UserName = data.U.UserName ?? "",
                        InvoiceDate = data.em.InvoicedDate ?? "",
                        ManagementFee = GetMfee(req.Enquiryid, data.ed.FirstOrDefault()?.ManagementFeeUnit ?? 0),
                        Actualdeliverydate = data.em.Actualdeliverydate != null && data.em.Actualdeliverydate != null ? data.em.Actualdeliverydate : data.em.EstDate,
                    };

                    rslt.enqProjectinfo = new EnqProjectinfo
                    {
                        ProjectDesc = data.em.ProjectDesc ?? "",
                        EnqId = data.em.Id,
                        EnqUId = data.em.EnqUid,
                        ProjectNo = data.em.ProjectNo ?? "",
                        Estdate = data.em.EstDate ?? "",
                        Briefdate = data.em.Briefdate ?? "",
                        ModifiedBy = data.em.ModifiedBy,
                        QuoteBy = data.em.SupplierQuoteAmountPriceOrQuantity,
                        SlaId = data.em.TemplateId,
                        ManagementfeetypeId = data.em.Managementfeetypeid,
                        SlaTemplatename = instancedb.TblSlas.Where(x => x.Id == data.em.TemplateId).Select(s => s.Description).FirstOrDefault(),
                        ManagementFeetype = instancedb.Tblmanagmenttypes.Where(x => x.Id == data.em.Managementfeetypeid).Select(s => s.Managementfeetypename).FirstOrDefault(),
                        HybridModel = data.em.Hybridmodel,
                        Attribute = data.em.Projectattribute,
                        Year = data.em.Year,
                        Quotestartdate = data.ed.Select(s => s.QuoteStartDate).FirstOrDefault() ?? "",
                        Quoteenddate = data.ed.Select(s => s.QuoteEndDate).FirstOrDefault() ?? "",
                        Proofstartdate = data.ed.Select(s => s.ProofStartDate).FirstOrDefault() ?? "",
                        Proofenddate = data.ed.Select(s => s.ProofEndDate).FirstOrDefault() ?? "",
                        Productionstartdate = data.ed.Select(s => s.ProductionStartDate).FirstOrDefault() ?? "",
                        Productionenddate = data.ed.Select(s => s.ProductionEndDate).FirstOrDefault() ?? "",
                        Filecopiesstartdate = data.ed.Select(s => s.FileCopiesStartDate).FirstOrDefault() ?? "",
                        Filecopiesenddate = data.ed.Select(s => s.FileCopiesEndDate).FirstOrDefault() ?? "",
                        Invoicestartdate = data.ed.Select(s => s.InvoicingStartDate).FirstOrDefault() ?? "",
                        Invoiceenddate = data.ed.Select(s => s.InvoicingEndDate).FirstOrDefault() ?? "",
                        //ProjectQuotetype = data.em.SupplierQuoteAmountPriceOrQuantity == 1 ? "Quote By Total Price" : "Quote By Unit Price"
                        ProjectQuotetype = data.em.SupplierQuoteAmountPriceOrQuantity switch
                        {
                            1 => "Quote By Total Price",
                            2 => "Quote By Unit Price",
                            _ => ""
                        }
                    };
                    rslt.enqlineItems = data.ed.Select(ed => new EnquiryItem
                    {
                        EnqId = ed.EnquiryId,
                        EnqdetailsId = ed.Id,
                        ItemNumber = ed.ItemNumber,
                        Printornonprint = ed.Printornonprint,
                        ItemName = ed.ItemName,
                        ItemDescription = ed.ItemDescription,
                        ProductCategoryId = ed.ProductCategoryId,
                        Productcategory = instancedb.TblProductCategories.Where(pc => pc.Id == ed.ProductCategoryId).Select(pc => pc.Category).FirstOrDefault() ?? "",
                        ProductionMethod = ed.ProductionMethod,
                        QuoteType = ed.QuoteType,
                        QuoteQtyOrSize = ed.QuoteQtyOrSize,
                        FlatSizeDandH = ed.FlatSizeDandH,
                        FlatSizeLength = ed.FlatSizeLength,
                        FlatSizeWidth = ed.FlatSizeWidth,
                        TotalFlatSize = ed.TotalFlatSize,
                        TotalQtySize = ed.TotalQtySize,
                        QuoteFileList = ed.QuoteFileList,
                        ModifiedBy = ed.CreatedOrModifiedBy,
                        ItemSellPrice = ed.ItemSellPrice,
                        BaseLinePrice = ed.BaseLinePrice,
                        Status = ed.Status,
                        QuoteTypeindex = ed.QuoteTypeindex,
                        SubCategory = ed.SubCategory,
                        Recycled = ed.Recycled,
                        ContainPlastic = ed.ContainPlastic,
                        RecycledPlastic = ed.RecycledPlastic,
                        SustainableMaterial = ed.SustainableMaterial,
                        LocalRateCard = ed.LocalRateCard,
                        PromoOSSOrderWindows = ed.PromoOssorderWindows,
                        SpecNote = ed.SpecNote,
                        SNote = ed.Snote,
                        Version = ed.Version,
                        printedorproducedonFSCmeterial = ed.PrintedorproducedonFscmeterial,
                        SustainableOptionthatwasrejected = ed.SustainableOptionthatwasrejected,
                        WasthisitemdesignedtoreducedPlastic = ed.WasthisitemdesignedtoreducedPlastic,
                        Isthisitemdesignedtobereused = ed.Isthisitemdesignedtobereused,
                        HarmonizedOrder = ed.HarmonizedOrder,
                        CatalogueUsage = ed.CatalogueUsage,
                        DigitalInnovation = ed.DigitalInnovation,
                        Innovation = ed.Innovation,
                        Sourcinglocation = ed.Sourcinglocation,
                        Typeofitem = ed.Typeofitem,
                        Eauction = ed.Eauction,
                        SMETAorEquivalentDocuments = ed.SmetaorEquivalentDocuments,
                        GMPorEquivalentDocuments = ed.GmporEquivalentDocuments,
                        usingFSCMaterial = ed.UsingFscmaterial,
                        OEKOTEXCertification = ed.Oekotexcertification,
                        designedforrecycling = ed.Designedforrecycling,
                        plasticapartfromPLA = ed.PlasticapartfromPla,
                        Plastictonnageweight = ed.Plastictonnageweight,
                        proposedwithsustainabilityoption = ed.Proposedwithsustainabilityoption,
                        containrecycledmaterial = ed.Containrecycledmaterial,
                        containrecycledplastic = ed.Containrecycledplastic,
                        Weightageofrecycledmaterial = ed.Weightageofrecycledmaterial,
                        RateCard = ed.RateCard,
                        VolumeAggregation = ed.VolumeAggregation,
                        CATABC = ed.Catabc,
                        OWlink = ed.Owlink,
                        TOJABC = ed.Tojabc,
                        Competbidmandate = ed.Competbidmandate,
                        Competbidcomplaint = ed.Competbidcomplaint,
                        Competbidexception = ed.Competbidexception,
                        Simplex = ed.Simplex,
                        TCOapproval = ed.Tcoapproval,
                        TCOapproved = ed.Tcoapproved,
                        Exceptionreason = ed.Exceptionreason,
                        Regionalname = ed.Regionalname,
                        savingstype = ed.Savingstype,
                        savingsreason = ed.Savingsreason,
                        CompetetiveWinningSupplier = ed.CompetetiveWinningSupplier,
                        Noofmaterials = ed.Noofmaterials,
                        Itemtype = ed.ProductType,
                        Incoterm = ed.Incoterm,
                        PrintingMethod = ed.PrintingMethod
                    }).ToList();

                    var selectedSuppliers = data.ed.Select(s => s.SelectedSuppliers).FirstOrDefault();
                    if (!string.IsNullOrEmpty(selectedSuppliers))
                    {
                        List<int> supplierIds = !string.IsNullOrEmpty(selectedSuppliers)
                            ? selectedSuppliers.Split(',').Select(int.Parse).ToList()
                            : new List<int>();

                        rslt.supplierinfo = instancedb.TblSuppliersLists
                            .Where(s => supplierIds.Contains(s.Id)).
                            Select(x => new SupplierDetails
                            {
                                SupplierID = x.Id,
                                Suppliername = x.SupplierName
                            })
                            .ToList();
                    }
                }
            }
            catch (Exception ex)
            { }
            return rslt;
        }
        #endregion

        #region Insertstep1
        [HttpPost("EnqClientinfo")]
        public Result EnqClientinfo(EnqClientinfo req)
        {
            Result rslt = new Result();
            if (ModelState.IsValid)
            {
                var division = instancedb.TblDivisions.Where(s => s.Id == req.Divisionid).FirstOrDefault();
                if (division != null)
                {
                    var clientdetails = instancedb.TblClientDetails.Where(s => s.Id == division.ClientId).FirstOrDefault();
                    var isClientExists = instancedb.TblClients.Any(x => x.Id == req.ClientContactId);
                    if (!isClientExists)
                    {
                        rslt.Status = false;
                        rslt.Data = "Client Id not matched with Division";
                        return rslt;
                    }
                    var isBrandExists = instancedb.TblDivisionBrandsorproducts.Any(x => x.Brandorproduct == req.Brand);
                    if (!isBrandExists)
                    {
                        rslt.Status = false;
                        rslt.Data = "Brand not matched with Division";
                        return rslt;
                    }
                    using (var transaction = instancedb.Database.BeginTransaction())
                    {
                        try
                        {
                            if (req.Action == "Update")
                            {
                                var enq = instancedb.TblEnquiryMasters.Where(x => x.Id == req.EnqId).FirstOrDefault();
                                if (enq != null)
                                {
                                    enq.DivisionId = division.Id;
                                    enq.ClientId = division.ClientId;
                                    enq.ClientContactId = req.ClientContactId;
                                    enq.DeliveryCountry = req.DeliveryCountryId;
                                    enq.PmgEntity = req.PMGEntity;
                                    enq.JobType = GetMenuName(req.Menuid);
                                    enq.CreatedBy = req.CreatedBy;
                                    enq.ModifiedBy = req.ModifiedBy;
                                    enq.Brandorproduct = req.Brand;
                                    enq.AboveorAtmarket = req.AboveorAtmarket;
                                    enq.StepId = 1;
                                    enq.Gbui = division.GlobalBumapping;
                                    enq.Clientprintpercentage = clientdetails.Printpercentage;
                                    enq.ClientPromoPercentage = clientdetails.PromoPercentage;
                                    enq.ClientPosmpercentage = clientdetails.Posmpercentage;
                                    enq.ClientPmggainSharePercentage = clientdetails.PmggainShare;
                                    enq.ClientVanestleGainSharePercentage = clientdetails.VanestleGainShare;
                                    instancedb.SaveChanges();
                                    transaction.Commit();
                                    rslt.Status = true;
                                    rslt.Data = new
                                    {
                                        EnqId = enq.Id,
                                        StepId = enq.StepId,
                                        Message = "Enquiry updated successfully"
                                    };
                                }
                            }
                            else
                            {
                                TblEnquiryMaster enq = new TblEnquiryMaster();
                                enq.DivisionId = division.Id;
                                enq.ClientId = division.ClientId;
                                enq.ClientContactId = req.ClientContactId;
                                enq.DeliveryCountry = req.DeliveryCountryId;
                                enq.PmgEntity = req.PMGEntity;
                                enq.JobType = GetMenuName(req.Menuid);
                                enq.CreatedBy = req.CreatedBy;
                                enq.ModifiedBy = req.ModifiedBy;
                                enq.Brandorproduct = req.Brand;
                                enq.AboveorAtmarket = req.AboveorAtmarket;
                                enq.StepId = 1;
                                enq.Gbui = division.GlobalBumapping;
                                enq.Clientprintpercentage = clientdetails.Printpercentage;
                                enq.ClientPromoPercentage = clientdetails.PromoPercentage;
                                enq.ClientPosmpercentage = clientdetails.Posmpercentage;
                                enq.ClientPmggainSharePercentage = clientdetails.PmggainShare;
                                enq.ClientVanestleGainSharePercentage = clientdetails.VanestleGainShare;
                                instancedb.TblEnquiryMasters.Add(enq);
                                instancedb.SaveChanges();
                                TblEnquiryDetail endet = new TblEnquiryDetail();
                                endet.EnquiryId = enq.Id;
                                endet.Status = 0;
                                instancedb.TblEnquiryDetails.Add(endet);
                                instancedb.SaveChanges();
                                enq.EnqUid = req.Menuid == 3 ? "CAT" + enq.Id.ToString("D6") : req.Menuid == 2 ? "BID" + enq.Id.ToString("D6") : "ENQ" + enq.Id.ToString("D6");
                                enq.Status = true;
                                instancedb.SaveChanges();
                                transaction.Commit();
                                rslt.Status = true;
                                rslt.Data = new
                                {
                                    EnqId = enq.Id,
                                    StepId = enq.StepId,
                                    Message = "Enquiry created successfully"
                                };
                            }

                        }
                        catch (Exception ex)
                        {
                            rslt.Status = false;
                            rslt.Data = ex.Message;
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
                else
                {
                    rslt.Status = false;
                    rslt.Data = "Division is not valid";
                }
            }
            else
            {
                rslt.Status = false;
                rslt.Data = ModelState.Values
                          .SelectMany(v => v.Errors)
                          .Select(e => e.ErrorMessage);
            }
            return rslt;
        }
        #endregion

        #region Insertstep2
        [HttpPost("EnqProjectinfo")]
        public Result EnqProjectinfo(EnqProjectinfo req)
        {
            Result rslt = new Result();
            if (ModelState.IsValid)
            {
                using (var transaction = instancedb.Database.BeginTransaction())
                {
                    try
                    {
                        var enqmaster = instancedb.TblEnquiryMasters.Where(x => x.Id == req.EnqId).FirstOrDefault();
                        enqmaster.ProjectNo = req.ProjectNo;
                        enqmaster.ProjectDesc = req.ProjectDesc;
                        enqmaster.EstDate = req.Estdate;
                        enqmaster.Briefdate = req.Briefdate;
                        enqmaster.ModifiedBy = req.ModifiedBy;
                        enqmaster.SupplierQuoteAmountPriceOrQuantity = req.QuoteBy;
                        enqmaster.TemplateId = req.SlaId;
                        enqmaster.Managementfeetypeid = req.ManagementfeetypeId;
                        enqmaster.Hybridmodel = req.HybridModel;
                        enqmaster.Year = req.Year;
                        enqmaster.Projectattribute = req.Attribute;
                        enqmaster.StepId = 2;
                        instancedb.SaveChanges();
                        var enqdetail = instancedb.TblEnquiryDetails.Where(x => x.EnquiryId == req.EnqId).FirstOrDefault();
                        enqdetail.QuoteStartDate = req.Quotestartdate;
                        enqdetail.QuoteEndDate = req.Quoteenddate;
                        enqdetail.ProofStartDate = req.Proofstartdate;
                        enqdetail.ProofEndDate = req.Proofenddate;
                        enqdetail.ProductionStartDate = req.Productionstartdate;
                        enqdetail.ProductionEndDate = req.Productionenddate;
                        enqdetail.FileCopiesStartDate = req.Filecopiesstartdate;
                        enqdetail.FileCopiesEndDate = req.Filecopiesenddate;
                        enqdetail.InvoicingStartDate = req.Invoicestartdate;
                        enqdetail.InvoicingEndDate = req.Invoiceenddate;
                        enqdetail.ActualQuoteStartDate = req.Quotestartdate;
                        enqdetail.ActualQuoteEndDate = req.Quoteenddate;
                        enqdetail.ActualProofStartDate = req.Proofstartdate;
                        enqdetail.ActualProofEndDate = req.Proofenddate;
                        enqdetail.ActualProductionStartDate = req.Productionstartdate;
                        enqdetail.ActualProductionEndDate = req.Productionenddate;
                        enqdetail.ActualFileCopiesStartDate = req.Filecopiesstartdate;
                        enqdetail.ActualFileCopiesEndDate = req.Filecopiesenddate;
                        enqdetail.ActualInvoicingStartDate = req.Invoicestartdate;
                        enqdetail.ActualInvoicingEndDate = req.Invoiceenddate;
                        enqdetail.ActualQuote = req.ModifiedQuote;
                        enqdetail.ActualProof = req.ModifiedProof;
                        enqdetail.ACtualProduction = req.ModifiedProduction;
                        enqdetail.ActualFilecopies = req.ModifiedFilecopies;
                        enqdetail.ActualInvoicing = req.ModifiedInvoice;
                        instancedb.SaveChanges();
                        transaction.Commit();
                        rslt.Status = true;
                        rslt.Data = new
                        {
                            EnqId = enqmaster.Id,
                            StepId = enqmaster.StepId,
                            Message = "Project Information for Enquiry created successfully"
                        };
                    }
                    catch (Exception ex)
                    {
                        rslt.Status = false;
                        rslt.Data = ex.Message;
                        transaction.Rollback();
                        throw;
                    }
                }

            }
            else
            {
                rslt.Status = false;
                rslt.Data = ModelState.Values
                          .SelectMany(v => v.Errors)
                          .Select(e => e.ErrorMessage);
            }
            return rslt;
        }
        #endregion

        #region Insertstep3
        [HttpPost("EnqLineItemDetails")]
        public Result EnqLineItemDetails(EnquiryItem req)
        {
            Result rslt = new Result();
            if (ModelState.IsValid)
            {
                using (var transaction = instancedb.Database.BeginTransaction())
                {
                    try
                    {
                        if (req.EnqdetailsId.HasValue)
                        {
                            var enqdata = instancedb.TblEnquiryDetails.Where(x => x.EnquiryId == req.EnqId && x.Id == req.EnqdetailsId).FirstOrDefault();
                            enqdata.ItemName = req.ItemName;
                            enqdata.ItemDescription = req.ItemDescription;
                            enqdata.PromoOssorderWindows = req.PromoOSSOrderWindows;
                            enqdata.SpecNote = req.SpecNote;
                            enqdata.Snote = req.SNote;
                            enqdata.Version = req.Version;
                            enqdata.CatalogueUsage = req.CatalogueUsage;
                            enqdata.DigitalInnovation = req.DigitalInnovation;
                            enqdata.Innovation = req.Innovation;
                            enqdata.Sourcinglocation = req.Sourcinglocation;
                            enqdata.PrintingMethod = req.PrintingMethod;
                            enqdata.QuoteQtyOrSize = req.QuoteQtyOrSize;
                            enqdata.CreatedOrModifiedBy = req.ModifiedBy;
                            enqdata.HarmonizedOrder = req.HarmonizedOrder;
                        }
                        else
                        {
                            TblEnquiryDetail item = new TblEnquiryDetail();
                            var enqdetail = instancedb.TblEnquiryDetails.Where(x => x.EnquiryId == req.EnqId).ToList();
                            var enqdata = enqdetail.First();
                            if (enqdetail.Count == 1 && string.IsNullOrEmpty(enqdata.ItemName) && enqdata.ItemNumber == null)
                            {
                                enqdata = AssignData(enqdata, req);

                            }
                            else
                            {
                                item = new TblEnquiryDetail
                                {
                                    EnquiryId = req.EnqId,
                                    ItemNumber = enqdetail.Count + 1,
                                    Printornonprint = req.Printornonprint,
                                    ItemName = req.ItemName,
                                    ItemDescription = req.ItemDescription,
                                    ProductCategoryId = req.ProductCategoryId,
                                    ProductionMethod = req.ProductionMethod,
                                    QuoteType = req.QuoteType,
                                    QuoteQtyOrSize = req.QuoteQtyOrSize,
                                    FlatSizeDandH = req.FlatSizeDandH,
                                    FlatSizeLength = req.FlatSizeLength,
                                    FlatSizeWidth = req.FlatSizeWidth,
                                    TotalFlatSize = req.TotalFlatSize,
                                    TotalQtySize = req.TotalQtySize,
                                    QuoteFileList = req.QuoteFileList,
                                    CreatedOrModifiedBy = req.ModifiedBy,
                                    ItemSellPrice = req.ItemSellPrice,
                                    BaseLinePrice = req.BaseLinePrice,
                                    Status = 0,
                                    QuoteTypeindex = req.QuoteTypeindex,
                                    SubcatId = req.SubcatID.ToString(),
                                    Recycled = req.Recycled,
                                    ContainPlastic = req.ContainPlastic,
                                    RecycledPlastic = req.RecycledPlastic,
                                    SustainableMaterial = req.SustainableMaterial,
                                    LocalRateCard = req.LocalRateCard,
                                    PromoOssorderWindows = req.PromoOSSOrderWindows,
                                    SpecNote = req.SpecNote,
                                    Snote = req.SNote,
                                    Version = req.Version,
                                    PrintedorproducedonFscmeterial = req.printedorproducedonFSCmeterial,
                                    SustainableOptionthatwasrejected = req.SustainableOptionthatwasrejected,
                                    WasthisitemdesignedtoreducedPlastic = req.WasthisitemdesignedtoreducedPlastic,
                                    Isthisitemdesignedtobereused = req.Isthisitemdesignedtobereused,
                                    HarmonizedOrder = req.HarmonizedOrder,
                                    CatalogueUsage = req.CatalogueUsage,
                                    DigitalInnovation = req.DigitalInnovation,
                                    Innovation = req.Innovation,
                                    Sourcinglocation = req.Sourcinglocation,
                                    Typeofitem = req.Typeofitem,
                                    Eauction = req.Eauction,
                                    SmetaorEquivalentDocuments = req.SMETAorEquivalentDocuments,
                                    GmporEquivalentDocuments = req.GMPorEquivalentDocuments,
                                    UsingFscmaterial = req.usingFSCMaterial,
                                    Oekotexcertification = req.OEKOTEXCertification,
                                    Designedforrecycling = req.designedforrecycling,
                                    PlasticapartfromPla = req.plasticapartfromPLA,
                                    Plastictonnageweight = req.Plastictonnageweight,
                                    Proposedwithsustainabilityoption = req.proposedwithsustainabilityoption,
                                    Containrecycledmaterial = req.containrecycledmaterial,
                                    Containrecycledplastic = req.containrecycledplastic,
                                    Weightageofrecycledmaterial = req.Weightageofrecycledmaterial,
                                    RateCard = req.RateCard,
                                    VolumeAggregation = req.VolumeAggregation,
                                    Catabc = req.CATABC,
                                    Owlink = req.OWlink,
                                    Tojabc = req.TOJABC,
                                    Competbidmandate = req.Competbidmandate,
                                    Competbidcomplaint = req.Competbidcomplaint,
                                    Competbidexception = req.Competbidexception,
                                    Simplex = req.Simplex,
                                    Tcoapproval = req.TCOapproval,
                                    Tcoapproved = req.TCOapproved,
                                    Exceptionreason = req.Exceptionreason,
                                    Regionalname = req.Regionalname,
                                    Savingstype = req.savingstype,
                                    Savingsreason = req.savingsreason,
                                    CompetetiveWinningSupplier = req.CompetetiveWinningSupplier,
                                    Noofmaterials = req.Noofmaterials,
                                    ProductType = req.Itemtype,
                                    Incoterm = req.Incoterm,
                                    PrintingMethod = req.PrintingMethod,
                                    SubCategory = instancedb.TblSubCategories.Where(s => s.Id == req.SubcatID).Select(s => s.SubCategory).FirstOrDefault()
                                };
                                if (item != null)
                                    instancedb.TblEnquiryDetails.Add(item);
                            }
                        }

                        var enqmaster = instancedb.TblEnquiryMasters.Where(x => x.Id == req.EnqId).FirstOrDefault();
                        if (enqmaster != null)
                        {
                            if (enqmaster.StepId < 3)
                            {
                                enqmaster.StepId = 3;
                            }
                        }
                        instancedb.SaveChanges();
                        transaction.Commit();
                        rslt.Status = true;
                        rslt.Data = new
                        {
                            EnqId = enqmaster.Id,
                            StepId = enqmaster.StepId,
                            Message = req.EnqdetailsId.HasValue ? "Item updated in VA" : "Item created in VA"
                        };

                    }
                    catch (Exception ex)
                    {
                        rslt.Status = false;
                        rslt.Data = ex.Message;
                        if (transaction != null)
                        {
                            try
                            {
                                transaction.Rollback();
                            }
                            catch
                            {
                                // already completed, ignore
                            }
                        }
                        throw;
                    }
                }

            }
            else
            {
                rslt.Status = false;
                rslt.Data = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            }
            return rslt;
        }
        [NonAction]
        public TblEnquiryDetail AssignData(TblEnquiryDetail enqdata, EnquiryItem req)
        {
            enqdata.EnquiryId = req.EnqId;
            enqdata.ItemNumber = 1;
            enqdata.ItemName = req.ItemName;
            enqdata.ItemDescription = req.ItemDescription;
            enqdata.ProductCategoryId = req.ProductCategoryId;
            enqdata.ProductionMethod = req.ProductionMethod;
            enqdata.QuoteType = req.QuoteType;
            enqdata.QuoteQtyOrSize = req.QuoteQtyOrSize;
            enqdata.Printornonprint = req.Printornonprint;
            enqdata.FlatSizeDandH = req.FlatSizeDandH;
            enqdata.FlatSizeLength = req.FlatSizeLength;
            enqdata.FlatSizeWidth = req.FlatSizeWidth;
            enqdata.TotalFlatSize = req.TotalFlatSize;
            enqdata.TotalQtySize = req.TotalQtySize;
            enqdata.QuoteFileList = req.QuoteFileList;
            enqdata.ItemSellPrice = req.ItemSellPrice;
            enqdata.BaseLinePrice = req.BaseLinePrice;
            enqdata.Status = 0;
            enqdata.QuoteTypeindex = req.QuoteTypeindex;
            enqdata.SubcatId = req.SubcatID.ToString();
            enqdata.SubCategory = instancedb.TblSubCategories.Where(s => s.Id == req.SubcatID).Select(s => s.SubCategory).FirstOrDefault();
            enqdata.Recycled = req.Recycled;
            enqdata.ContainPlastic = req.ContainPlastic;
            enqdata.RecycledPlastic = req.RecycledPlastic;
            enqdata.SustainableMaterial = req.SustainableMaterial;
            enqdata.LocalRateCard = req.LocalRateCard;
            enqdata.PromoOssorderWindows = req.PromoOSSOrderWindows;
            enqdata.SpecNote = req.SpecNote;
            enqdata.Snote = req.SNote;
            enqdata.Version = req.Version;
            enqdata.PrintedorproducedonFscmeterial = req.printedorproducedonFSCmeterial;
            enqdata.SustainableOptionthatwasrejected = req.SustainableOptionthatwasrejected;
            enqdata.WasthisitemdesignedtoreducedPlastic = req.WasthisitemdesignedtoreducedPlastic;
            enqdata.Isthisitemdesignedtobereused = req.Isthisitemdesignedtobereused;
            enqdata.HarmonizedOrder = req.HarmonizedOrder;
            enqdata.CatalogueUsage = req.CatalogueUsage;
            enqdata.DigitalInnovation = req.DigitalInnovation;
            enqdata.Innovation = req.Innovation;
            enqdata.Sourcinglocation = req.Sourcinglocation;
            enqdata.Typeofitem = req.Typeofitem;
            enqdata.Eauction = req.Eauction;
            enqdata.SmetaorEquivalentDocuments = req.SMETAorEquivalentDocuments;
            enqdata.GmporEquivalentDocuments = req.GMPorEquivalentDocuments;
            enqdata.UsingFscmaterial = req.usingFSCMaterial;
            enqdata.Oekotexcertification = req.OEKOTEXCertification;
            enqdata.Designedforrecycling = req.designedforrecycling;
            enqdata.PlasticapartfromPla = req.plasticapartfromPLA;
            enqdata.Plastictonnageweight = req.Plastictonnageweight;
            enqdata.Proposedwithsustainabilityoption = req.proposedwithsustainabilityoption;
            enqdata.Containrecycledmaterial = req.containrecycledmaterial;
            enqdata.Containrecycledplastic = req.containrecycledplastic;
            enqdata.Weightageofrecycledmaterial = req.Weightageofrecycledmaterial;
            enqdata.RateCard = req.RateCard;
            enqdata.VolumeAggregation = req.VolumeAggregation;
            enqdata.Catabc = req.CATABC;
            enqdata.Owlink = req.OWlink;
            enqdata.Tojabc = req.TOJABC;
            enqdata.Competbidmandate = req.Competbidmandate;
            enqdata.Competbidcomplaint = req.Competbidcomplaint;
            enqdata.Competbidexception = req.Competbidexception;
            enqdata.Simplex = req.Simplex;
            enqdata.Tcoapproval = req.TCOapproval;
            enqdata.Tcoapproved = req.TCOapproved;
            enqdata.Exceptionreason = req.Exceptionreason;
            enqdata.Regionalname = req.Regionalname;
            enqdata.Savingstype = req.savingstype;
            enqdata.Savingsreason = req.savingsreason;
            enqdata.CompetetiveWinningSupplier = req.CompetetiveWinningSupplier;
            enqdata.Noofmaterials = req.Noofmaterials;
            enqdata.ProductType = req.Itemtype;
            enqdata.Incoterm = req.Incoterm;
            enqdata.PrintingMethod = req.PrintingMethod;
            return enqdata;
        }
        [NonAction]
        public char InsertEnqLineItemAttachment(int enqId, int itemNumber, string fileList, int modifiedBy)
        {
            char rslt = 'F';
            var enqdetail = instancedb.TblEnquiryDetails.Where(x => x.EnquiryId == enqId && x.ItemNumber == itemNumber).FirstOrDefault();
            if (enqdetail != null)
            {
                enqdetail.QuoteFileList = fileList;
                enqdetail.CreatedOrModifiedBy = modifiedBy;
                instancedb.SaveChanges();
                rslt = 'S';
            }
            return rslt;
        }
        #endregion

        #region Insertstep4
        [HttpPost("EnqSupplierInfo")]
        public Result EnqSupplierInfo(SupplierInfo req)
        {
            Result rslt = new Result();
            if (ModelState.IsValid)
            {
                using (var transaction = instancedb.Database.BeginTransaction())
                {
                    try
                    {
                        var enqmaster = instancedb.TblEnquiryMasters.Where(x => x.Id == req.EnqId).FirstOrDefault();
                        if (enqmaster != null)
                        {
                            var enqdetails = instancedb.TblEnquiryDetails.Where(s => s.EnquiryId == req.EnqId).ToList();
                            if (enqdetails != null)
                            {
                                instancedb.TblEnquiryDetails.Where(s => s.EnquiryId == req.EnqId)
                                .ExecuteUpdate(s => s.SetProperty(x => x.CreatedOrModifiedBy, req.ModifiedBy)
                                                     .SetProperty(x => x.SelectedSuppliers, req.SelectedSuppliers));

                                if (enqmaster.StepId < 4)
                                {
                                    enqmaster.StepId = 4;
                                    instancedb.SaveChanges();
                                }

                                transaction.Commit();
                                rslt.Status = true;
                                rslt.Data = new
                                {
                                    EnqId = enqmaster.Id,
                                    StepId = enqmaster.StepId,
                                    Message = "Supplier added in VA"
                                };
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        rslt.Status = false;
                        rslt.Data = ex.Message;
                        transaction.Rollback();
                        throw;
                    }
                }

            }
            else
            {
                rslt.Status = false;
                rslt.Data = ModelState.Values
                          .SelectMany(v => v.Errors)
                          .Select(e => e.ErrorMessage);
            }
            return rslt;
        }
        #endregion

        #region Insertstep5
        [HttpPost("EnqReview")]
        public async Task<Result> EnqReview(int enqId, int createdBy)
        {
            Result rslt = new Result();

            try
            {
                if (enqId == 0)
                {
                    rslt.Status = false;
                    rslt.Data = "Invalid enquiry.";
                    return rslt;
                }
                string selectedSuppliers = await instancedb.TblEnquiryDetails.Where(x => x.EnquiryId == enqId).OrderBy(x => x.ItemNumber).Select(x => x.SelectedSuppliers).FirstOrDefaultAsync();

                // Calculate total Item Sell Price
                decimal totalSellPrice = await instancedb.TblEnquiryDetails.Where(x => x.EnquiryId == enqId).SumAsync(x => x.ItemSellPrice ?? 0);

                // Execute Stored Procedure
                await instancedb.Database.ExecuteSqlRawAsync(@"EXEC EN_InsertEnquiryMasterDetailsStep5 @EnquiryID = {0}, @SelectedSuppliers = {1}, @ItemSellPrice = {2}, @jobtype = {3}, @Createdby = {4}"
                                                             , enqId, selectedSuppliers, totalSellPrice, "New Item", createdBy);

                // Send notification
                //SendMail();

                rslt.Status = true;
                rslt.Data = "Enquiry submitted successfully.";
            }
            catch (Exception ex)
            {
                rslt.Status = false;
                rslt.Data = ex.Message;
            }

            return rslt;
        }
        #endregion

        #region ProjectEnquirySummary
        [HttpPost("GetProjectDetails")]
        public GetProjectDetails GetProjectDetails(UserRequest req)
        {
            var rslt = new GetProjectDetails();
            try
            {
                int enquiryId = req.Enquiryid;

                rslt.historySearches = ([.. instancedb.TblNewSavingsData.Where(x => x.HistoricalPriceLcl != null && x.Curr == req.Currency && x.Country == req.Country)
                .Select(x => new HistorySearch
                {
                    EnquriyID = x.JobRef,
                    Qty = x.Qty,
                    Country = x.Country,
                    Specifications = x.FullSpec,
                    ReferencePrice = x.HistoricalPriceLcl,
                    MaterialUsed = x.MaterialUsed,
                    PoNumber = x.CustomerPo,
                    SubCategory = x.SubCategory,
                    Brand = x.Brand
                })]);

                rslt.historyLogs = [.. (from jh in instancedb.TblJobHistories
                        join u in instancedb.TblUsers on jh.UserId equals u.Id into ug from u in ug.DefaultIfEmpty()
                        join oldStatus in instancedb.TblJobStatuses on jh.OldValue equals oldStatus.StatusId.ToString() into oldStatusGroup from oldStatus in oldStatusGroup.DefaultIfEmpty()
                        join newStatus in instancedb.TblJobStatuses on jh.NewValue equals newStatus.StatusId.ToString() into newStatusGroup from newStatus in newStatusGroup.DefaultIfEmpty()
                        where jh.EnqId == enquiryId && !string.IsNullOrEmpty(jh.NewValue)
                        orderby jh.ModifiedDate descending
                        select new HistoryLogs
                        {
                            ModifiedDate = jh.ModifiedDate.Value.ToString("dd/MM/yyyy hh:mm:ss tt") ?? "",
                            UserName = u != null ? u.UserName : "",
                            Field = jh.Field,
                            OldValue = jh.Field == "Status" ? oldStatus.StatusName : jh.OldValue,
                            NewValue = jh.Field == "Status" ? newStatus.StatusName : jh.NewValue
                        })];

                rslt.lineItemLogs = [.. (from jh in instancedb.TblLineitemsHistories
                                     join u in instancedb.TblUsers on jh.Userid equals u.Id into ug
                                     from u in ug.DefaultIfEmpty()
                                     where jh.Enqid == enquiryId && !string.IsNullOrEmpty(jh.NewValue) && !string.IsNullOrEmpty(jh.OldValue) && jh.OldValue != "0" && jh.OldValue != "-2"
                                     && jh.OldValue != "-3" && jh.OldValue != "undefined" && jh.OldValue != "Null" && jh.OldValue != jh.NewValue && jh.NewValue != "undefined"
                                     orderby jh.ModifiedDate descending
                                     select new LineItemsLogs
                                     {
                                         ModifiedDate = jh.ModifiedDate.Value.ToString("dd/MM/yyyy hh:mm:ss tt") ?? "",
                                         UserName = u != null ? u.UserName : "",
                                         Field = jh.Field,
                                         OldValue = jh.OldValue,
                                         NewValue = jh.NewValue,
                                         ItemNumber = jh.ItemNumber
                                     })];

                rslt.revisedQuotes = [.. (
                                from h in instancedb.TblSupplierQuoteHistories
                                join ed in instancedb.TblEnquiryDetails on h.EnquiryDetailsId equals ed.Id
                                join s in instancedb.TblSuppliersLists on h.SupplierId equals s.Id
                                where h.EnquiryId == enquiryId
                                select new
                                {
                                    h.Id,
                                    h.EnquiryId,
                                    h.EnquiryDetailsId,
                                    h.SupplierId,
                                    ItemName = "Item " + h.ItemNumber + " - " + (ed.ItemName ?? ""),
                                    s.SupplierName,
                                    SupplierPrice = h.Amount,
                                    ItemNote = h.SupplierNotes,
                                    DateOfChange = h.ServerTime,
                                    ItemNumber = h.ItemNumber
                                }).AsEnumerable().GroupBy(x => new { x.ItemName, x.SupplierId })
                                .SelectMany(g => g.OrderByDescending(x => x.DateOfChange).Take(3)).OrderBy(x => x.ItemName).ThenByDescending(x => x.SupplierName).ThenByDescending(x => x.DateOfChange)
                                .Select(x => new RevisedQuotes
                                {
                                    ItemNumber = x.ItemNumber,
                                    ItemName = x.ItemName,
                                    SupplierName = x.SupplierName,
                                    SupplierPrice = x.SupplierPrice,
                                    ItemNote = x.ItemNote,
                                    DateOfChange = x.DateOfChange.Value.ToString("dd/MM/yyyy hh:mm:ss tt") ?? "",
                                })];

                var query = (from h in instancedb.TblSupplierQuotes
                    join ed in instancedb.TblEnquiryDetails on h.EnquiryDetailsId equals ed.Id
                    join s in instancedb.TblSuppliersLists on h.SupplierId equals s.Id
                    where h.EnquiryId == enquiryId
                    select new{ h, ed, s }).ToList();

                rslt.requestQuotes = [.. query.AsEnumerable().Select(x => new RequestQuotes
                {
                    ItemNumber = x.h.ItemNumber,
                    ItemName = "Item " + x.h.ItemNumber + " - " + (x.ed.ItemName ?? ""),
                    SupplierName = x.s.SupplierName,
                    SupplierQuotesId = x.h.Id,
                    EnquiryId = x.h.EnquiryId,
                    EnquiryDetailsId = x.h.EnquiryDetailsId,
                    SupplierId = x.h.SupplierId,
                    InitialQuote = Math.Round(Convert.ToDecimal(x.h.InitAmount), 2),
                    NegQuote = Math.Round(Convert.ToDecimal(x.h.Amount), 2),
                    Quantity = Convert.ToInt32(x.ed.QuoteQtyOrSize),
                    IniUnitPrice =  Math.Round(Convert.ToDecimal(x.h.InitAmount /  Convert.ToDecimal(x.ed.QuoteQtyOrSize)), 2),
                    NegUnitPrice =  Math.Round(Convert.ToDecimal(x.h.Amount /  Convert.ToDecimal(x.ed.QuoteQtyOrSize)), 2),
                    NegUnitPriceFee = Math.Round(Convert.ToDecimal(x.h.Amount /  Convert.ToDecimal(x.ed.QuoteQtyOrSize)) * ((GetMfee(req.Enquiryid, x.ed.ManagementFeeUnit ?? 0)) / 100), 2),
                    PmgSellPrice = x.ed.ItemSellPrice,
                    IsCalculateId = x.ed.SupplierId,
                })];

                var calculationDetails = (from h in instancedb.TblSupplierQuotes
                                          join ed in instancedb.TblEnquiryDetails on h.EnquiryDetailsId equals ed.Id
                                          where h.EnquiryId == enquiryId && h.QuoteStatus == 2 && ed.SupplierId == h.SupplierId
                                          group new { h, ed } by 1 into g
                                          select new
                                          {
                                              Cost = g.Sum(x => (decimal?)x.h.Amount) ?? 0,
                                              Sell = g.Sum(x => (decimal?)x.ed.ItemSellPrice) ?? 0
                                          }).FirstOrDefault();

                var totalCost = calculationDetails?.Cost ?? 0;
                var totalSell = calculationDetails?.Sell ?? 0;
                var totalMargin = totalSell - totalCost;

                rslt.calculationDetails = calculationDetails == null ? []
                    : [new() {
                        Cost = Math.Round(totalCost, 2),
                        Sell = Math.Round(totalSell, 2),
                        Margin = Math.Round(totalMargin, 2),
                        MarkupPercent = Math.Round(totalCost > 0 ? (totalMargin / totalCost) * 100 : 0, 2),
                        MarginPercent = Math.Round(totalSell > 0 ? (totalMargin / totalSell) * 100 : 0, 2)
                    }];

                rslt.calculationSupplierlogs = [..(from ed in instancedb.TblEnquiryDetails
                           join sl in instancedb.TblSuppliersLists on ed.SupplierId equals sl.Id
                           where ed.EnquiryId == enquiryId
                           select new CalculationSupplierlogs
                           {
                               Id = ed.Id,
                               ItemNumber = ed.ItemNumber,
                               ItemName = ed.ItemName,
                               SMETAAccredited = ed.Smetaaccredited,
                               GMPAccredited = ed.Gmpaccredited,
                               Natureofsupplier = ed.Natureofsupplier,
                               SupplierName = sl.SupplierName,
                               SupplierType = sl.SupplierType
                           })];

                var count = instancedb.TblEnquiryDetails.Where(x => x.EnquiryId == enquiryId).GroupBy(x => new { x.Savingstype, x.Savingsreason }).Count();

                rslt.savingReasons = [.. instancedb.TblEnquiryDetails.Where(x => x.EnquiryId == enquiryId).Select(x => new SavingReasons
                {
                    ItemName = count == 1 ? "All items" : x.ItemName,
                    Id = count == 1 ? 0 : x.Id,
                    SavingsType = x.Savingstype,
                    SavingsReason = x.Savingsreason
                }).Distinct()];

                rslt.SavingsResponseDto = GetSavings(enquiryId);

                rslt.DeliveryOrder = [.. instancedb.TblClintDeliveryAddresses.Where(x => x.Enqid == enquiryId).Select(x => new DeliveryOrder
                {
                    Enqid = x.Enqid,
                    AddressLineOne = x.AdressLine1,
                    AddressLineTwo = x.AdressLine2,
                    AddressLineThree = x.AdressLine3,
                    CompanyName = x.CompanyName,
                    ContactNo  = x.ContactNo,
                    NameorDept = x.NameorDept,
                    Id = x.Id,
                    Remarks= x.Remarks
                })];

                rslt.ProjectStatus = GetProjectStatus(enquiryId);

                rslt.PreviewQuotes = [.. instancedb.TblEnquiryDetails.Where(x => x.EnquiryId == enquiryId).Select(x => new PreviewQuotes
                {
                    ItemNumber = x.ItemNumber,
                    ItemName = x.ItemName,
                    SupplierA = x.Suplier,
                    SupplierB = x.Supplier2Name,
                    SupplierC = x.Supplier3Name,
                    SupplierAInitAmount = x.Supplier1Intilamount,
                    SupplierBInitAmount = x.Supplier2Intilamount,
                    SupplierCInitAmount = x.Supplier3Intilamount,
                    SupplierANegAmount = x.Supplier1amount,
                    SupplierBNegAmount = x.Supplier2amount,
                    SupplierCNegAmount = x.Supplier3amount,
                })];

            }
            catch (Exception ex)
            {
                // Log exception
            }
            return rslt;
        }

        #region Getsavings
        [HttpGet("GetSavings")]
        public SavingsResponseDto GetSavings(int enquiryId)
        {
            var response = new SavingsResponseDto();
            // var savings = instancedb.TblSavingsDetails
            //   .Where(x => x.EnquiryId == enquiryId)
            // .ToList();

            var savings = (from s in instancedb.TblSavingsDetails
                           join d in instancedb.TblEnquiryDetails on s.EnquiryDetailsId equals d.Id
                           where s.EnquiryId == enquiryId
                           select new
                           {
                               ItemName = d.ItemName,
                               s
                           }).ToList();


            if (!savings.Any()) return response;

            var enquiryDetails = instancedb.TblEnquiryDetails.Where(x => x.EnquiryId == enquiryId).ToList();

            var result = (from m in instancedb.TblEnquiryMasters
                          join d in instancedb.TblEnquiryDetails on m.Id equals d.EnquiryId
                          join c in instancedb.TblCountries on m.PmgEntity equals c.Id
                          where m.Id == enquiryId && (d.ManagementFeeUnit ?? 0) != 0
                          select new
                          {
                              TaxPercentage = c.TaxPercentage,
                              ManagementFeeUnit = d.ManagementFeeUnit,
                              ManagementFeeMethod = d.ManagementFeeMethod
                          }).FirstOrDefault();


            // Item-wise Summary
            response.ItemWiseSummary = savings
                .GroupBy(x => x.s.ItemNumber)
                .Select(g =>
                {
                    decimal totalPrevious = g.Where(x => !string.IsNullOrEmpty(x.s.PreviousPrice) && x.s.PreviousPrice != "N/A")
                    .Sum(x =>
                    {
                        decimal.TryParse(x.s.PreviousPrice, out decimal val);
                        return val;
                    });

                    decimal TaxPercentage = 0;
                    decimal taxamount = 0;
                    decimal totalsellpricewithtax = 0;
                    decimal totalSell = g.Sum(x => x.s.ItemSellPrice ?? 0);
                    decimal totalNegSell = g.Sum(x => x.s.Negitemsellprice ?? 0);
                    decimal managementFee = g.FirstOrDefault()?.s.ManagementFee ?? 0;
                    decimal saving = totalPrevious - (totalSell + managementFee);
                    decimal savingPercent = totalPrevious == 0 ? 0 : (saving / totalPrevious) * 100;
                    decimal savingExclfee = totalPrevious - (totalNegSell);
                    decimal savingExclfeePercent = totalPrevious == 0 ? 0 : (savingExclfee / totalPrevious) * 100;

                    if (result != null)
                    {
                        TaxPercentage = Convert.ToDecimal(result.TaxPercentage);
                        //ManagementFeeUnit = result.ManagementFeeUnit ?? 0;
                        //ManagementFeeMethod = result.ManagementFeeMethod;
                    }
                    taxamount = (totalSell + managementFee) * ((TaxPercentage) / 100);
                    totalsellpricewithtax = (totalSell + managementFee) + taxamount;

                    return new SavingsItemDto
                    {
                        ItemNumber = g.Key,
                        ItemName = g.FirstOrDefault()?.ItemName,
                        TotalPreviousPrice = totalPrevious,
                        TotalSellPrice = totalSell,
                        TotalNegSellPrice = totalNegSell,
                        ManagementFee = managementFee,
                        Saving = saving,
                        SavingPercent = savingPercent,
                        SavingExclfee = savingExclfee,
                        SavingExcelfeePercent = savingExclfeePercent,
                        Taxpercentage = TaxPercentage,
                        Taxamount = taxamount,
                        Totalsellpricewithtax = totalsellpricewithtax
                    };
                }).OrderBy(x => x.ItemNumber).ToList();

            // Detail rows
            response.Details = savings.Select(x =>
            {
                var enquiry = enquiryDetails.FirstOrDefault(e => e.ItemNumber == x.s.ItemNumber);

                decimal.TryParse(x.s.PreviousPrice, out decimal previousPrice);

                return new SavingsDetailsDto
                {
                    Id = x.s.Id,
                    EnquiryId = x.s.EnquiryId,
                    EnquiryDetailsId = x.s.EnquiryDetailsId,
                    ItemNumber = x.s.ItemNumber,
                    ItemName = enquiry.ItemName,
                    ReasonForSaving = x.s.ReasonForSaving,
                    PreviousSupplier = x.s.PreviousSupplier,
                    PreviousPrice = previousPrice,
                    ItemSellPrice = x.s.ItemSellPrice ?? 0,
                    NegItemSellPrice = x.s.Negitemsellprice ?? 0,
                    ManagementFee = x.s.ManagementFee ?? 0,
                    //BaselineQuantity = x.s.BaselineQuntity ?? 0,
                    //BaselineUnitPrice = x.s.Baselineunitprice ?? 0,
                    //BaselinePrice = x.s.Baselineprice ?? 0,
                    //BaselineSaving = x.s.Savings ?? 0,
                    //GainShare = x.s.GainShareforeachpart ?? 0,
                    BaselineQuantity = x.s.BaselineQuntity ?? "0",
                    BaselineUnitPrice = Convert.ToDecimal(x.s.Baselineunitprice ?? "0"),
                    BaselinePrice = Convert.ToDecimal(x.s.Baselineprice ?? "0"),
                    BaselineSaving = Convert.ToDecimal(x.s.Savings ?? "0"),
                    GainShare = Convert.ToDecimal(x.s.GainShareforeachpart ?? "0"),
                    QuoteQtyOrSize = enquiry?.QuoteQtyOrSize ?? "",
                    SavingType = enquiry?.Savingstype,
                    MFee = (x.s.Negitemsellprice ?? 0) * ((enquiry?.ManagementFeeUnit ?? 0) / 100)
                };
            }).ToList();

            // Overall Totals
            response.TotalPreviousPrice = response.ItemWiseSummary.Sum(x => x.TotalPreviousPrice);
            response.TotalSellPrice = response.ItemWiseSummary.Sum(x => x.TotalSellPrice);
            response.TotalNegSellPrice = response.ItemWiseSummary.Sum(x => x.TotalNegSellPrice);
            response.TotalManagementFee = response.ItemWiseSummary.Sum(x => x.ManagementFee);
            //response.TotalSaving = response.ItemWiseSummary.Sum(x => x.Saving);
            //response.TotalSavingPercent = response.TotalPreviousPrice == 0 ? 0 : (response.TotalSaving / response.TotalPreviousPrice) * 100;
            response.TotalSaving = Math.Max(0, response.ItemWiseSummary.Sum(x => x.Saving));
            response.TotalSavingPercent = response.TotalPreviousPrice <= 0 || response.TotalSaving <= 0 ? 0 : (response.TotalSaving / response.TotalPreviousPrice) * 100;
            return response;
        }
        #endregion

        #region GetProjectStatus

        [HttpGet("GetProjectStatus")]
        public List<ProjectStatus> GetProjectStatus(int enqId)
        {

            int? status = instancedb.TblEnquiryDetails.Where(x => x.EnquiryId == enqId).OrderByDescending(x => x.Id).Select(x => x.Status)
            .FirstOrDefault();
            List<int> statusIds = status switch
            {
                int s when s <= 6 => [25],
                7 or 8 => [9, 11, 16, 25],
                9 => [10, 25],
                10 => [11, 25],
                11 => [13, 25],
                13 => [15, 25],
                15 => [16, 25],
                16 => [17, 18, 25],
                17 => [18, 25],
                18 => [19, 22],
                19 => [22],
                22 => [23],
                23 => [24],
                _ => [],
            };
            return [.. instancedb.TblJobStatuses.Where(x => statusIds.Contains(x.StatusId)) .Select(x => new ProjectStatus
            {
                Value = x.StatusId,
                Label = x.StatusName
            })];

        }

        #endregion

        #endregion

        #region DeliveryAddress
        [HttpPost("CheckDeliveryAddress")]
        public Result CheckDeliveryAddress(DeliveryOrder req)
        {
            Result rslt = new Result();

            var data = instancedb.TblClintDeliveryAddresses
                .FirstOrDefault(x => x.CompanyName == req.CompanyName);

            if (data != null)
            {
                rslt.Status = true;
                rslt.Data = data; // Returns the first matching record
            }
            else
            {
                rslt.Status = false;
                rslt.Data = "No delivery address found";
            }

            return rslt;
        }

        [HttpPost("AddUpdateDeliveryAddress")]
        public Result AddUpdateDeliveryAddress(DeliveryOrder req)
        {
            Result rslt = new Result();
            try
            {
                TblClintDeliveryAddress address;
                if (req.Id == 0)
                {
                    address = new TblClintDeliveryAddress();
                    instancedb.TblClintDeliveryAddresses.Add(address);
                }
                else
                {
                    address = instancedb.TblClintDeliveryAddresses.FirstOrDefault(x => x.Id == req.Id);

                    if (address == null)
                    {
                        rslt.Status = false;
                        rslt.Data = "Delivery address not found.";
                        return rslt;
                    }
                }
                address.Enqid = req.Enqid;
                address.CompanyName = req.CompanyName;
                address.AdressLine1 = req.AddressLineOne;
                address.AdressLine2 = req.AddressLineTwo;
                address.AdressLine3 = req.AddressLineThree;
                address.NameorDept = req.NameorDept;
                address.ContactNo = req.ContactNo;
                address.Remarks = req.Remarks;
                instancedb.SaveChanges();
                rslt.Status = true;
                rslt.Data = req.Id == 0 ? "Delivery address added successfully." : "Delivery address updated successfully.";
            }
            catch (Exception ex)
            {
                rslt.Status = false;
                rslt.Data = ex.Message;
            }

            return rslt;
        }

        #endregion

        #region RFQ
        [HttpPost("PostSupplierQuotes")]
        public async Task<Result> PostSupplierQuotes(List<RFQSupplierQuotes> quotes)
        {
            Result rslt = new Result();
            try
            {
                decimal finalPrice = 0;
                decimal finalPrice1 = 0;
                decimal priceDiff = 0;
                string? price = "";
                string? price1 = "";
                foreach (var item in quotes)
                {
                    if (item.SupplierQuoteAmountPriceOrQuantity == 1)
                    {
                        if (item.Intialprice != null || item.Negoprice != null)
                        {
                            price = item.Intialprice?.ToString();
                            price1 = (item.Negoprice ?? item.Intialprice)?.ToString();
                        }
                    }
                    else
                    {
                        double? tempPrice = Convert.ToDouble(item.InitialUnitprice) * item.Qty;
                        double? tempNegoPrice = Convert.ToDouble(item.NegoUnitprice ?? item.InitialUnitprice) * item.Qty;
                        price = string.Format(new System.Globalization.CultureInfo("en-US"), "{0:##,##0.00}", tempPrice);
                        price1 = string.Format(new System.Globalization.CultureInfo("en-US"), "{0:##,##0.00}", tempNegoPrice);

                    }
                    decimal.TryParse(string.IsNullOrWhiteSpace(price) ? price1 : price, out finalPrice);
                    decimal.TryParse(string.IsNullOrWhiteSpace(price1) ? price : price1, out finalPrice1);

                    if (finalPrice1 > finalPrice)
                    {
                        priceDiff = finalPrice1 - finalPrice;
                    }

                    await instancedb.Database.ExecuteSqlInterpolatedAsync($@"EXEC EN_UpdateSupplierQuoteByAgency @Price={finalPrice1}, @Price1={finalPrice}, @Pricediff={priceDiff}, @Id={item.QuoteId}");
                }
                await UpdateJobStatus(new Jobstatus { EnqId = quotes.FirstOrDefault()?.EnqId, ModifiedBy = quotes.FirstOrDefault()?.ModifiedBy, Status = 2 });
                rslt.Status = true;
                rslt.Data = "Quotes fetched successfully";
            }
            catch (Exception ex)
            {
                rslt.Status = false;
                rslt.Data = ex.InnerException?.Message ?? ex.Message;
            }

            return rslt;
        }
        #endregion

        #region UpdateJobStatus
        [HttpPost("UpdateJobStatus")]
        public async Task<Result> UpdateJobStatus([FromBody] Jobstatus req)
        {
            var rslt = new Result();
            try
            {
                var rows = await instancedb.Database.ExecuteSqlInterpolatedAsync($@"EXEC UpdateJobStatus @EnquiryID={req.EnqId}, @ModifiedBy={req.ModifiedBy}, @Status={req.Status}");
                rslt.Status = true;
                rslt.Data = "Job Status updated successfully";
            }
            catch (Exception ex)
            {
                rslt.Status = false;
                rslt.Data = ex.InnerException?.Message ?? ex.Message;
            }

            return rslt;
        }
        #endregion

        [NonAction]
        public decimal GetMfee(int enqid, decimal Mfee)
        {
            var tblenqmaster = instancedb.TblEnquiryMasters.Where(x => x.Id == enqid).FirstOrDefault();
            var ManagementFeeUnit = Mfee != null && Mfee != 0 ? Mfee
                         : tblenqmaster.Managementfeetypeid != null ? instancedb.Tblmanagmenttypes
                         .Where(x => x.Id == tblenqmaster.Managementfeetypeid).Select(x => x.Managementtypevalue).FirstOrDefault()
                         : Convert.ToDecimal(instancedb.TblClientDetails.Where(x => x.Id == tblenqmaster.ClientId).Select(x => x.DefaultSalesPercentage).FirstOrDefault());
            return ManagementFeeUnit ?? 0;
        }

        #region CalculateSavings
        [HttpPost("CalculateSavings")]
        public Result CalculateSavings(Calculate_supplierQuotes req)
        {
            Result rslt = new Result();
            decimal ErouRate = 0; decimal? totalerouiteamprice = 0; string europrice; decimal? EurototalQuotePrice = 0;
            decimal? eurototalManagmentFee = 0; decimal? totalTaxamount = 0; decimal? EurototalTaxamount = 0; decimal? EuroTotalQuoteAmount = 0;
            decimal? Quoteprice = 0;
            string ManagementFeeMethod = "";
            decimal? ManagementFeeUnit = 0;
            decimal TotalManagementFee = 0;
            decimal? TaxPercentage = 0;
            decimal? TotalTaxAmount = 0;
            decimal? TotalQuoteAmount = 0;
            try
            {
                ErouRate = Convert.ToDecimal(instancedb.TblEnquiryMasters.Where(e => e.Id == req.Enqid)
                    .Join(instancedb.TblClientDetails, e => e.ClientId, c => c.Id, (e, c) => c.EuroRate).FirstOrDefault());

                TotalManagementFee = 0;

                var result = (from m in instancedb.TblEnquiryMasters
                              join d in instancedb.TblEnquiryDetails on m.Id equals d.EnquiryId
                              join c in instancedb.TblCountries on m.PmgEntity equals c.Id
                              where m.Id == req.Enqid
                              select new
                              {
                                  TaxPercentage = c.TaxPercentage,
                                  ManagementFeeUnit = d.ManagementFeeUnit,
                                  ManagementFeeMethod = d.ManagementFeeMethod
                              }).FirstOrDefault();
                //&& (d.ManagementFeeUnit ?? 0) != 0

                if (result != null)
                {
                    TaxPercentage = Convert.ToDecimal(result.TaxPercentage);
                    ManagementFeeUnit = GetMfee(req.Enqid, result.ManagementFeeUnit ?? 0);
                    ManagementFeeMethod = result.ManagementFeeMethod;
                }

                string connnString = instancedb.Database.GetConnectionString();
                using (SqlConnection con = new SqlConnection(connnString))
                {
                    con.Open();

                    SqlTransaction trans = con.BeginTransaction();

                    try
                    {
                        Quoteprice = 0;
                        foreach (var item in req.Ls_getsupplierQuotes)
                        {
                            Quoteprice = +Convert.ToDecimal(item.NegotiatePrice);
                            var initialprice = item.InitialPrice + ((item.InitialPrice * ManagementFeeUnit) / 100);
                            using (SqlCommand cmd = new SqlCommand("UpdateSupplierForItem", con, trans))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;

                                cmd.Parameters.AddWithValue("@ItemPrice", initialprice);
                                cmd.Parameters.AddWithValue("@itempriceNEG", item.NegotiatePrice);
                                cmd.Parameters.AddWithValue("@EnquiryId", req.Enqid);
                                cmd.Parameters.AddWithValue("@ItemNumber", item.Itemnumber);
                                cmd.Parameters.AddWithValue("@SupplierId", item.SupplierID);
                                cmd.Parameters.AddWithValue("@totalerouiteamprice", ErouRate);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        Quoteprice = Quoteprice + (Quoteprice * (ManagementFeeUnit / 100));
                        TotalTaxAmount = Quoteprice * (TaxPercentage / 100);
                        TotalQuoteAmount = Quoteprice + TotalTaxAmount;

                        eurototalManagmentFee = TotalManagementFee * ErouRate;
                        EurototalQuotePrice = Quoteprice * ErouRate;
                        EurototalTaxamount = TotalTaxAmount * ErouRate;
                        EuroTotalQuoteAmount = TotalQuoteAmount * ErouRate;
                        using (SqlCommand cmd = new SqlCommand("UpdateClientQuoteToEnqDetails", con, trans))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@EnquiryId", req.Enqid);
                            cmd.Parameters.AddWithValue("@QuotePrice", Quoteprice);
                            cmd.Parameters.AddWithValue("@ManagementFeeMethod", ManagementFeeMethod);
                            cmd.Parameters.AddWithValue("@ManagementFeeUnit", ManagementFeeUnit);
                            cmd.Parameters.AddWithValue("@TotalManagementFee", TotalManagementFee);
                            cmd.Parameters.AddWithValue("@TaxPercentage", TaxPercentage);
                            cmd.Parameters.AddWithValue("@TotalTaxAmount", TotalTaxAmount);
                            cmd.Parameters.AddWithValue("@TotalQuoteAmount", TotalQuoteAmount);
                            cmd.Parameters.AddWithValue("@Statuscul", "cul");
                            cmd.Parameters.AddWithValue("@Status", 2);
                            cmd.Parameters.AddWithValue("@eurototalManagmentFee", eurototalManagmentFee);
                            cmd.Parameters.AddWithValue("@EurototalQuotePrice", EurototalQuotePrice);
                            cmd.Parameters.AddWithValue("@EurototalTaxamount", EurototalTaxamount);
                            cmd.Parameters.AddWithValue("@EuroTotalQuoteAmount", EuroTotalQuoteAmount);
                            cmd.ExecuteNonQuery();
                        }
                        trans.Commit();
                        rslt.Status = true;
                        rslt.Data = "Savings calculated successfully.";
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();

                        rslt.Status = false;
                        rslt.Data = ex.Message;
                    }
                }
            }
            catch (Exception ex)
            {
                rslt.Status = false;
                rslt.Data = ex.Message;
            }

            return rslt;
        }
        #endregion

        #region Post Reference price
        [HttpPost("PostRefprice")]
        public async Task<Result> PostRefprice(List<Ls_Store_Refprice> quotes)
        {
            Result rslt = new Result();
            try
            {
                foreach (var item in quotes)
                {
                    var saving = await instancedb.TblSavingsDetails
                        .FirstOrDefaultAsync(x => x.Id == item.Id);

                    if (saving != null)
                    {
                        saving.PreviousSupplier = item.previousSupplier;
                        saving.PreviousPrice = item.previousPrice.ToString();
                        saving.PreviousPriceReports = item.previousPrice.ToString();
                        saving.Note = item.note;
                        saving.BaselineQuntity = item.BaselineQuantity;
                    }
                }
                await instancedb.SaveChangesAsync();
                rslt.Status = true;
                rslt.Data = "Reference prices updated successfully.";
            }
            catch (Exception ex)
            {
                rslt.Status = false;
                rslt.Data = ex.Message;
            }
            return rslt;
        }
        #endregion

        #region Update SavingsReasons
        [HttpPost("UpdateSavingsReasons")]
        public async Task<Result> UpdateSavingsReasons(List<SavingReasons> savingReasons)
        {
            Result rslt = new Result();

            try
            {
                foreach (var item in savingReasons)
                {
                    if (item.Id == 0)
                    {
                        // Update all items for the enquiry
                        var savings = await instancedb.TblEnquiryDetails
                            .Where(x => x.EnquiryId == item.EnquiryId)
                            .ToListAsync();

                        foreach (var saving in savings)
                        {
                            saving.Savingsreason = item.SavingsReason;
                            saving.Savingstype = item.SavingsType;
                        }

                        var savingsDetails = await instancedb.TblSavingsDetails
                           .Where(x => x.EnquiryId == item.EnquiryId)
                           .ToListAsync();

                        foreach (var savingDetail in savingsDetails)
                        {
                            savingDetail.ReasonForSaving = item.SavingsReason;
                        }
                    }
                    else
                    {
                        // Update only the selected item
                        var saving = await instancedb.TblEnquiryDetails
                            .FirstOrDefaultAsync(x => x.Id == item.Id);

                        if (saving != null)
                        {
                            saving.Savingsreason = item.SavingsReason;
                            saving.Savingstype = item.SavingsType;
                        }

                        var savingsDetails = await instancedb.TblSavingsDetails
                            .FirstOrDefaultAsync(x => x.EnquiryDetailsId == item.Id);

                        if (savingsDetails != null)
                        {
                            savingsDetails.ReasonForSaving = item.SavingsReason;
                        }
                    }
                }

                await instancedb.SaveChangesAsync();

                rslt.Status = true;
                rslt.Data = "Savings Reason updated successfully.";
            }
            catch (Exception ex)
            {
                rslt.Status = false;
                rslt.Data = ex.Message;
            }

            return rslt;
        }
        #endregion

        #region UpdateJobSummary
        [HttpPost("UpdateJobSummary")]
        public async Task<Result> UpdateJobSummary(UpdateJobSummary req)
        {
            Result rslt = new Result();
            var data = "";
            if (req.Action == "Update")
            {
                var enq = instancedb.TblEnquiryMasters.Where(x => x.Id == req.EnqId).FirstOrDefault();
                if (enq != null)
                {
                    enq.ClientContactId = req.ClientContactId;
                    enq.InvoiceNumber = req.InvoiceNumber;
                    enq.InvoicedDate = req.InvoiceDate;
                    enq.Ponumber = req.PoNumber;
                    enq.Actualdeliverydate = req.Actualdeliverydate;
                    enq.PoorderraisedDate = req.RaisedDate;
                    enq.ModifiedBy = req.ModifiedBy;
                    enq.CreatedBy = req.CreatedBy;
                    enq.ProjectNo = req.ProjectNo;
                    enq.ProjectDesc = req.ProjectDesc;
                    enq.EstDate = req.Estdate;
                    enq.Briefdate = req.Briefdate;
                }
                await instancedb.SaveChangesAsync();
                await UpdateJobStatus(new Jobstatus { EnqId = req.EnqId, ModifiedBy = req.ModifiedBy, Status = req.StatusId });
                rslt.Status = true;
                data = req.StatusId == 7 ? "Po number created successfully" : "Job Summary updated successfully";
            }
            else if (req.Action == "fee")
            {
                var enq = instancedb.TblEnquiryDetails.Where(x => x.EnquiryId == req.EnqId).FirstOrDefault();
                if (enq != null)
                {
                    enq.ManagementFeeUnit = req.ManagementFee;
                }
                await instancedb.SaveChangesAsync();
                await UpdateJobStatus(new Jobstatus { EnqId = req.EnqId, ModifiedBy = req.ModifiedBy, Status = req.StatusId });
                rslt.Status = true;
                data = "ManagementFee updated successfully";
            }
            else
            {
                rslt.Status = false;
                data = "Data not found";
            }
            rslt.Data = data;
            return rslt;
        }
        #endregion

        #region UpdatePreviewQuotes
        [HttpPost("UpdatePreviewQuotes")]
        public async Task<Result> UpdatePreviewQuotes(PreviewQuotes req)
        {
            Result rslt = new Result();
            var enq = instancedb.TblEnquiryDetails.Where(x => x.Id == req.Id).FirstOrDefault();
            if (enq != null)
            {
                enq.Supplier2Name = req.SupplierB;
                enq.Supplier2amount = req.SupplierBNegAmount;
                enq.Supplier2Intilamount = req.SupplierBInitAmount;
                enq.Supplier3Name = req.SupplierC;
                enq.Supplier3amount = req.SupplierCNegAmount;
                enq.Supplier3Intilamount = req.SupplierCInitAmount;
            }
            await instancedb.SaveChangesAsync();
            rslt.Status = true;
            rslt.Data = "Supplier B&C updated successfully";
            return rslt;
        }
        #endregion

        #region GetPreviewQuotes
        [HttpPost("GetPreviewQuotes")]
        public async Task<IActionResult> GetPreviewQuotes(UserRequest req)
        {

            try
            {
                // 1. Get selected enquiry detail
                var enq = await instancedb.TblEnquiryDetails.FirstOrDefaultAsync(x => x.Id == req.Enquiryid);

                // 2. Get Company Details
                var companyDetails = await (from em in instancedb.TblEnquiryMasters
                                            join division in instancedb.TblDivisions on em.DivisionId equals division.Id
                                            join clientDetails in instancedb.TblClientDetails on em.ClientId equals clientDetails.Id
                                            join client in instancedb.TblClients on em.ClientContactId equals client.Id
                                            join pmgCountry in instancedb.TblCountries on em.PmgEntity equals pmgCountry.Id
                                            join user in instancedb.TblAgencyUsers on em.CreatedBy equals user.Id
                                            where em.Id == enq.Id && em.JobType == GetMenuName(1)
                                            select new
                                            {
                                                em.ProjectNo,
                                                em.ProjectDesc,
                                                em.PmgEntity,
                                                CompanyEntityName = division.EntityName,
                                                division.AddressLine1,
                                                division.AddressLine2,
                                                division.AddressLine3,
                                                AddressLine4 = "",
                                                division.City,
                                                clientDetails.ClientName,
                                                clientDetails.Country,
                                                clientDetails.PaymentTerm,
                                                ClientContactName = client.FirstName + " " + client.LastName,
                                                PmgCountry = pmgCountry.Country,
                                                AgencyUserName = user.FirstName + " " + user.LastName,
                                                AgencyUserEmail = user.Email,
                                                AgencyUserPhone = user.Phone,
                                                Currency = pmgCountry.RegionalSetting,
                                            }
                ).FirstOrDefaultAsync();

                // 3. Get quotation items
                var items = await instancedb.TblEnquiryDetails.Where(x => x.EnquiryId == enq.Id && x.SupplierId != 0).OrderBy(x => x.Id).ToListAsync();

                // 4. Build items HTML
                StringBuilder itemHtml = new StringBuilder();
                itemHtml.Append(@" <table width='100%' border='1' cellspacing='0' cellpadding='0'
                           style='font-family:Franklin Gothic Book,Arial;font-size:12px;color:black;'>
                    <tbody>");

                foreach (var item in items)
                {
                    decimal qty = Convert.ToDecimal(item.QuoteQtyOrSize ?? "0");
                    decimal price = item.ItemSellPrice ?? 0;
                    decimal unitPrice = qty > 0 ? Math.Round(price / qty, 4) : 0;

                    itemHtml.Append($@"
                    <tr>
                        <td width='65%' align='left' style='border:1px solid black; padding:3px;'>
                            {item.ItemDescription}
                        </td>
                        <td width='10%' align='right' style='border:1px solid black; padding:3px;'>
                            {qty:#,##0}
                        </td>
                        <td width='10%' align='right' style='border:1px solid black; padding:3px;'>
                            {unitPrice:#,##0.0000}
                        </td>
                        <td width='15%' align='right' style='border:1px solid black; padding:3px;'>
                            {price:#,##0.00}
                        </td>
                    </tr>");
                }
                itemHtml.Append("</tbody></table>");

                // 5. Get totals
                var firstItem = items.FirstOrDefault();

                decimal quotePrice = firstItem?.QuotePrice ?? 0;
                decimal managementFee = firstItem?.TotalManagementFee ?? 0;
                decimal taxAmount = firstItem?.TotalTaxAmount ?? 0;
                decimal totalAmount = firstItem?.TotalQuoteAmount ?? 0;
                decimal subTotal = quotePrice + managementFee;

                // 6. Select quotation template
                string country = companyDetails.Country ?? "";
                string template = country switch
                {
                    "Malaysia" => "Quatation_Malaysia.html",
                    "Thailand" => "Quataion_Thailand.html",
                    "Philippines" => "Quatation_PH.html",
                    "Vietnam" => "Quataion_Vietnam.html",
                    "Singapore" => "Quatation_SG.html",
                    "Japan" => "Quatation_JP.html",
                    "New Zealand" => "Quatation_NZL.html",
                    "Australia" => "Quatation_AUS.html",
                    _ => "Quatation.html"
                };

                // 7. Read HTML
                string templatePath = Path.Combine(_hostingEnvironment.ContentRootPath, "PrintTemplates", template);
                string html = await System.IO.File.ReadAllTextAsync(templatePath);

                // 8. Replace quotation values
                html = html.Replace("[CompanyEntityName]", companyDetails.CompanyEntityName ?? "")
                    .Replace("[ClientContact]", companyDetails.ClientContactName ?? "")
                    .Replace("[Address1]", companyDetails.AddressLine1 ?? "")
                    .Replace("[Address2]", companyDetails.AddressLine2 ?? "")
                    .Replace("[Address3]", companyDetails.AddressLine3 ?? "")
                    .Replace("[Address4]", companyDetails.AddressLine4 ?? "")
                    .Replace("[EntityCity]", companyDetails.City ?? "")
                    .Replace("[EntityCountry]", country)
                    .Replace("[ProjectName]", companyDetails.ProjectDesc ?? "")
                    .Replace("[ProjectNo]", companyDetails.ProjectNo ?? "")
                    .Replace("[PaymentTerms]", companyDetails.PaymentTerm ?? "")
                    .Replace("[PMGENTITYNAME]", companyDetails.PmgCountry ?? "")
                    .Replace("[PMGCREATORUSER]", companyDetails.AgencyUserName ?? "")
                    .Replace("[PMGCREATORUSEREMAIL]", companyDetails.AgencyUserEmail ?? "")
                    .Replace("[PMGCREATORUSERPHONECONTACT]", companyDetails.AgencyUserPhone ?? "")
                    .Replace("[QUOTESUBBMITEDDATE]", "")
                    .Replace("[ITEMS1]", itemHtml.ToString())
                    .Replace("[TOTALITEMSPRICE]", quotePrice.ToString("#,##0.00"))
                    .Replace("[TOTALAGENCYFEE]", managementFee.ToString("#,##0.00"))
                    .Replace("[SUBTOTALAMOUNT]", subTotal.ToString("#,##0.00"))
                    .Replace("[TAXAMOUNT]", taxAmount.ToString("#,##0.00"))
                    .Replace("[TOTALAMOUNT]", totalAmount.ToString("#,##0.00"));

                // 9. Currency symbol
                try
                {
                    var region = new RegionInfo(companyDetails.Currency ?? "en-US");
                    html = html.Replace("[CurrencySymbol]", region.CurrencySymbol.ToString());
                }
                catch
                {
                    html = html.Replace("[CurrencySymbol]", "");
                }

                // 10. Generate PDF
                var converter = new NReco.PdfGenerator.HtmlToPdfConverter();
                byte[] pdf = converter.GeneratePdf(html);
                return File(pdf, "application/pdf", $"Quotation_{enq}.pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Status = false,
                    Message = ex.Message
                });
            }
        }
        #endregion

        #region GetReport
        [HttpPost("GetReport")]
        public IActionResult GetReport(GetReport request)
        {
            try
            {
                string procedureName;
                if (request.Tco == "Yes")
                {
                    procedureName = "EB_NewGenerateReport_TCOexport";
                }
                else if (request.UserType == "Client")
                {
                    procedureName = "EB_NewGenerateReport_clientportal";
                }
                else if (request.ClientName == "All")
                {
                    procedureName = "EB_NewGenerateReport";
                }
                else
                {
                    procedureName = "EB_NewGenerateReport_client";
                }

                DataSet ds = new DataSet();
                string connectionString = _configuration["ConnectionStrings:VADbConnectionString"];
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(procedureName, con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 300;
                        cmd.Parameters.AddWithValue("@StartDate", request.StartDate);
                        cmd.Parameters.AddWithValue("@EndDate", request.EndDate);
                        cmd.Parameters.AddWithValue("@Role", request.Role);
                        cmd.Parameters.AddWithValue("@Type", request.ReportType);
                        //cmd.Parameters.AddWithValue("@StatusFrom", request.StatusFrom);
                        //cmd.Parameters.AddWithValue("@StatusTo", request.StatusTo);
                        cmd.Parameters.AddWithValue("@clientname", request.ClientName);
                        if ((request.Role == "Administration" || request.Role == "ClientServices") && request.UserClientId > 0)
                        {
                            cmd.Parameters.AddWithValue("@UserClientId", request.UserClientId);
                        }

                        if (request.UserType == "Client")
                        {
                            cmd.Parameters.AddWithValue("@countryId", request.CountryId);
                            cmd.Parameters.AddWithValue("@Businessunit", request.BusinessUnit);
                        }
                        using SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                        adapter.Fill(ds);
                    }
                }

                using XLWorkbook workbook = new XLWorkbook();
                string? reportName = request.ClientName == "All" ? "P&G" : request.ClientName;
                string dateTime = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string fileName = $"{reportName}_Report_{dateTime}.xlsx";

                string[] sheetNames =
                {
                        "Savings Data",
                        "Job Cancelled",
                        "Performance Survey",
                        "Performance Survey New"
                };

                string[] sheetTitles =
                {
                        $"SAVINGS REPORT ({reportName})",
                        $"Job Cancelled ({reportName})",
                        $"Performance Survey ({reportName})",
                        $"Performance Survey New ({reportName})"
                };

                for (int tableIndex = 0; tableIndex < ds.Tables.Count && tableIndex < sheetNames.Length; tableIndex++)
                {
                    DataTable source = ds.Tables[tableIndex];
                    if (source == null)
                    {
                        continue;
                    }


                    IXLWorksheet worksheet = workbook.Worksheets.Add(sheetNames[tableIndex]);
                    DataTable exportTable = source.Clone();

                    foreach (DataRow sourceRow in source.Rows)
                    {
                        DataRow exportRow = exportTable.NewRow();
                        foreach (DataColumn column in exportTable.Columns)
                        {
                            exportRow[column.ColumnName] = sourceRow[column.ColumnName];
                        }
                        exportTable.Rows.Add(exportRow);
                    }

                    if (exportTable.Rows.Count == 0 && exportTable.Columns.Count > 0)
                    {
                        DataRow row = exportTable.NewRow();
                        row[0] = "No data available";
                        exportTable.Rows.Add(row);
                    }

                    // Top title
                    worksheet.Range(1, 1, 1, exportTable.Columns.Count).Merge();
                    worksheet.Cell("A1").Value = sheetTitles[tableIndex];
                    worksheet.Cell("A1").Style.Font.FontSize = 16;
                    worksheet.Cell("A1").Style.Font.Bold = true;
                    worksheet.Cell("A1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell("A1").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Row(1).Height = 25;

                    // Existing table alignment — unchanged
                    if (exportTable.Columns.Count > 0)
                    {
                        worksheet.Cell(3, 1).InsertTable(exportTable);
                        worksheet.Row(3).Style.Font.SetBold();
                        if (source.Rows.Count == 0)
                        {
                            worksheet.Range(4, 1, 4, exportTable.Columns.Count).Merge();
                            worksheet.Cell(4, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            worksheet.Cell(4, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            worksheet.Cell("A1").Style.Font.FontSize = 12;
                            worksheet.Cell("A1").Style.Font.Bold = true;
                            worksheet.Row(4).Height = 25;
                        }

                        for (int i = 0; i < exportTable.Columns.Count; i++)
                        {
                            IXLColumn column = worksheet.Column(i + 1);

                            string columnName = exportTable.Columns[i].ColumnName;

                            if (
                                columnName.Contains("Amount", StringComparison.OrdinalIgnoreCase) || columnName.Contains("Saving", StringComparison.OrdinalIgnoreCase) ||
                                columnName.Contains("Price", StringComparison.OrdinalIgnoreCase)
                            )
                            {
                                column.Style.NumberFormat.Format = "##,##0.00";
                            }

                            // Keep your existing alignment
                            column.Style.Alignment.SetWrapText(true);
                            column.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Top);
                        }

                        worksheet.Columns().AdjustToContents();
                        foreach (IXLColumn column in worksheet.Columns())
                        {
                            if (column.Width > 40)
                            {
                                column.Width = 40;
                            }
                        }
                    }
                }

                using MemoryStream stream = new MemoryStream();
                workbook.SaveAs(stream);
                byte[] file = stream.ToArray();
                return File(file, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Status = false,
                    Message = ex.Message
                });
            }
        }
        #endregion
    }

}
