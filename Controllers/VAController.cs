using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Data;
using System.Globalization;
using System.Net;
using System.Text;
using VAapiNestle.Configuration;
using VAapiNestle.CustomModels;
using VAapiNestle.Models;
using VAapiNestle.Services;
using static VAapiNestle.CustomModels.Base;

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
        private readonly ILogger<VAController> _logger;
        private readonly PaginationSettings _pagination;
        private readonly DatabaseSettings _database;
        private readonly EmailService _emailService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public VAController(AppDbContext db, MasterDbContext masterdb, IWebHostEnvironment hostingEnvironment,
            IConfiguration configuration, ILogger<VAController> logger, IOptions<PaginationSettings> pagination, IOptions<DatabaseSettings> database
            , EmailService emailService, IHttpContextAccessor httpContextAccessor
        )
        {
            instancedb = db ?? throw new ArgumentNullException(nameof(db));
            _masterdb = masterdb ?? throw new ArgumentNullException(nameof(masterdb));
            _hostingEnvironment = hostingEnvironment ?? throw new ArgumentNullException(nameof(hostingEnvironment));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _pagination = pagination?.Value ?? new PaginationSettings();
            _database = database?.Value ?? new DatabaseSettings();
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        /// <summary>
        /// Runs <paramref name="work"/> in a database transaction under the EF execution strategy, so a transient
        /// failure (e.g. this request being chosen as a SQL deadlock victim) rolls back and retries the whole unit.
        /// The transaction commits only when the result reports success; otherwise it rolls back.
        /// The delegate must load everything it changes itself, because it may run more than once.
        /// </summary>
        private Task<Result> InTransactionAsync(Func<Task<Result>> work)
        {
            var strategy = instancedb.Database.CreateExecutionStrategy();
            return strategy.ExecuteAsync(async () =>
            {
                instancedb.ChangeTracker.Clear(); // a retry must not see entities from the failed attempt
                await using var transaction = await instancedb.Database.BeginTransactionAsync();
                var result = await work();
                if (result.Status)
                    await transaction.CommitAsync();
                else
                    await transaction.RollbackAsync();
                return result;
            });
        }


        #region Usersignon

        [HttpPost("UserSignon")]
        [EnableRateLimiting(LoginRateLimitSettings.PolicyName)]
        public async Task<Result> UserSignon(UserRequest User)
        {
            Result rslt = new Result();
            try
            {
                rslt.Status = false;
                var tblusers = await instancedb.TblUsers.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.UserName == User.Username && x.Password == User.Password);

                // Same message for unknown user and wrong password so usernames cannot be enumerated
                if (tblusers == null)
                {
                    rslt.Data = "Invalid username or password";
                    return rslt;
                }

                // Password is intentionally not returned to the client
                User usr = new User
                {
                    Username = tblusers.UserName,
                    Usertype = tblusers.UserType,
                    UserID = tblusers.Id,
                    FkID = tblusers.FkId
                };
                int fkid = tblusers.FkId;

                if (usr.Usertype == "Agency")
                {
                    var agencyusers = await instancedb.TblAgencyUsers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == fkid);
                    usr.Country = agencyusers?.Country ?? "";
                    usr.Email = agencyusers?.Email ?? "";
                    usr.Role = agencyusers?.JobRole ?? "";
                }
                else if (usr.Usertype == "Supplier")
                {
                    var supplierusers = await instancedb.TblSupplierUsers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == fkid);
                    usr.Email = supplierusers?.EmailAddressLive ?? "";
                    int suppid = Convert.ToInt32(supplierusers?.SupplierId);
                    var supplierslist = await instancedb.TblSuppliersLists.AsNoTracking().FirstOrDefaultAsync(x => x.Id == suppid);
                    usr.Country = supplierslist?.Country ?? "";
                    usr.Currency = supplierslist?.Currency ?? "";
                }
                else
                {
                    var clientusers = await instancedb.TblClients.AsNoTracking().FirstOrDefaultAsync(x => x.Id == fkid);
                    usr.Country = clientusers?.Country ?? "";
                    usr.Email = clientusers?.Email ?? "";
                }

                var country = await instancedb.TblCountries.AsNoTracking()
                    .Where(x => x.Country == usr.Country)
                    .Select(s => new { s.Id, s.Currency })
                    .FirstOrDefaultAsync();
                usr.Currency = country?.Currency ?? "";
                usr.CountryId = country?.Id ?? 0;

                rslt.Status = true;
                rslt.Data = usr;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UserSignon failed for {Username}", User.Username);
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

            var rows = await _masterdb.TblLanguages.AsNoTracking()
                .Where(x => x.Status == true)
                .ToListAsync();

            // Only the known language columns may be selected (request.Lang was previously used for unchecked reflection)
            Func<MasterDbContext.TblLanguage, string?>? selector = request.Lang switch
            {
                "English" => x => x.English,
                "Japanese" => x => x.Japanese,
                "Korea" => x => x.Korea,
                "Chinese" => x => x.Chinese,
                "Vietnamese" => x => x.Vietnamese,
                "Taiwan" => x => x.Taiwan,
                "Thai" => x => x.Thai,
                "Indonesia" => x => x.Indonesia,
                _ => null
            };

            rslt.Status = selector != null && rows.Count > 0;
            selector ??= x => x.English;

            var result = new Dictionary<string, string?>();
            foreach (var row in rows)
            {
                if (!string.IsNullOrEmpty(row.ResourceKey))
                    result.TryAdd(row.ResourceKey, selector(row));
            }
            rslt.Data = result;
            return rslt;
        }
        #endregion

        #region Dashboard
        [HttpPost("ViewDashboard")]
        public async Task<Result> ViewDashboard([FromBody] EnquiryFilterRequest req, [FromQuery] PageQuery paging, CancellationToken cancellationToken)
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
                var (res, statusRows, page) = await GetProjectsAsync(req, paging, cancellationToken);
                Response.WriteHeaders(page);
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

                // Summary always covers every matching enquiry (not just the current page). Rows are taken in the same
                // EnquiryId order as the detail list so equal counts keep their established order.
                statusRows = [.. statusRows.OrderByDescending(x => x.EnquiryId)];
                var statusSummary = statusRows
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
                    active = statusRows.Where(x => x.Status == 1).Select(x => x.Id).Distinct().Count(),
                    approval = statusRows.Where(x => x.Status == 3).Select(x => x.Id).Distinct().Count(),
                    awarded = statusRows.Where(x => x.Status >= 6 && x.Status != 24).Select(x => x.Id).Distinct().Count(),
                    completed = statusRows.Where(x => x.Status == 24).Select(x => x.Id).Distinct().Count()
                };
                rslt.Status = true;
                rslt.Data = new
                {
                    Summary = model,
                    Detailed = processedData
                };
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                rslt.Status = false;
                rslt.Data = ex.Message;
            }

            return rslt;
        }

        private sealed class ProjectStatusRow
        {
            public int Id { get; init; }
            public string? EnquiryId { get; init; }
            public int? Status { get; init; }
            public string? StatusName { get; init; }
            public string? CreatedBy { get; init; }
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
        private static string Html(string? value) => WebUtility.HtmlEncode(value ?? "");

        /// <summary>
        /// Dashboard rows (one page when paging is requested, otherwise all) plus the status of every matching
        /// enquiry for the summary counts.
        /// </summary>
        private async Task<(List<ProjectInformation> Rows, List<ProjectStatusRow> StatusRows, PageInfo? Page)> GetProjectsAsync(
            EnquiryFilterRequest req, IPagedRequest paging, CancellationToken cancellationToken)
        {
            req.CreatedName = await (from tu in instancedb.TblUsers
                                     where tu.UserName == req.Username
                                     select tu.Id).FirstOrDefaultAsync(cancellationToken);
            try
            {
                var now = DateTime.Now;
                var last12Months = now.AddMonths(-12);
                var menuName = GetMenuName(req.Menuid);

                // Parse the optional date range up front; previously an empty/invalid date threw inside the query
                // and the dashboard silently came back empty
                bool hasRange = DateTime.TryParse(req.StartDate, out var startDate) & DateTime.TryParse(req.EndDate, out var endDate);
                var rangeStart = hasRange ? startDate : last12Months;
                var rangeEnd = hasRange ? endDate.Date.AddDays(1) : now;

                var filtered = from tm in instancedb.TblEnquiryMasters.AsNoTracking()
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

                                tm.Status == true && tm.JobType == menuName && tm.PmgEntity == req.UserCountryId && ((td.Status >= 1 && td.Status <= 25) || (tm.CreatedBy == req.CreatedName && td.Status == 0))
                                && tm.ServerTime >= rangeStart && tm.ServerTime < rangeEnd

                               //  && EF.Functions.Like(tm.EnqUid, "%ENQ%")
                               select new { tm, td, tblJobStatus, tblCountry, ta };

                var query = filtered.Select(r => new ProjectInformation
                {
                    Id = r.tm.Id,
                    EnquiryId = r.tm.EnqUid,
                    ProjectNo = r.tm.ProjectNo ?? "",
                    ProjectDesc = r.tm.ProjectDesc ?? "",
                    ServerTime = r.tm.ServerTime.ToString("dd/MM/yyyy"),
                    Status = r.td.Status,
                    StatusName = r.tblJobStatus.StatusName,
                    PmgEntity = r.tm.PmgEntity,
                    BiddingStartDate = r.tm.BiddingStartDate,
                    BiddingEndDate = r.tm.BiddingEndDate,
                    TimeOffset = r.tblCountry.TimeOffset,
                    Duration = r.tm.Duration,
                    StepId = r.tm.StepId,
                    ClientId = r.tm.ClientId,
                    Orderid = r.tm.Orderid ?? "",
                    QuoteEndDate = r.td.QuoteEndDate,
                    serchjobs = r.td.Status == 1 ? 30 : r.td.Status == 3 ? 31 : r.td.Status >= 6 ? 32 : r.td.Status,
                    Attachments = instancedb.TblAttachments.Count(a => a.EnqId == r.tm.Id),
                    CreatedBy = r.ta.FirstName + " " + r.ta.LastName,
                    surveystatus = r.tm.Surveynotification == "YES" ? 1 : 0,
                    Submitedby = (instancedb.TblSurveys.Where(s => s.EnqId == r.tm.Id).OrderBy(s => s.SubmitedBy).Select(s => s.SubmitedBy).FirstOrDefault()) == "Client" ? "Client" : "Default",
                    Surveycount = instancedb.TblSurveys.Count(s => s.EnqId == r.tm.Id),
                    OldsurveyENQ = instancedb.TblJobHistories.Any(jh => jh.EnqId == r.tm.Id && jh.NewValue == "24" && jh.Field == "Status") ? "NO" : "YES",
                    Ponumber = r.tm.Ponumber ?? "",
                    Country = r.tblCountry.Country

                }).Distinct();

                var pageRequest = Pagination.Resolve(paging, _pagination.DefaultPageSize);
                if (pageRequest == null)
                {
                    // Unpaged: one query; the summary is derived from the same rows (original behaviour)
                    var rows = await query.ToListAsync(cancellationToken);
                    var statusRows = rows.Select(x => new ProjectStatusRow { Id = x.Id, EnquiryId = x.EnquiryId, Status = x.Status, StatusName = x.StatusName }).ToList();
                    return (rows, statusRows, null);
                }

                // Paged: the summary needs every matching enquiry, but only the light status columns. The distinct key
                // mirrors the columns that can multiply rows in the detail query, so the counts match the unpaged result.
                var allStatusRows = await filtered
                    .Select(r => new ProjectStatusRow
                    {
                        Id = r.tm.Id,
                        EnquiryId = r.tm.EnqUid,
                        Status = r.td.Status,
                        StatusName = r.tblJobStatus.StatusName,
                        CreatedBy = r.ta.FirstName + " " + r.ta.LastName
                    })
                    .Distinct()
                    .ToListAsync(cancellationToken);

                var (page, size) = pageRequest.Value;
                var pageRows = await query
                    .OrderByDescending(x => x.EnquiryId).ThenByDescending(x => x.Id)
                    .Skip((page - 1) * size).Take(size)
                    .ToListAsync(cancellationToken);
                return (pageRows, allStatusRows, new PageInfo(page, size, allStatusRows.Count));
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "GetProjects failed for user {Username}", req.Username);
            }
            return ([], [], null);
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
        public async Task<MasterDropdowns> MasterDropdowns(EnquiryFilterRequest req, CancellationToken cancellationToken)
        {
            var isAdmin = string.Equals(req.Role, "Administration", StringComparison.OrdinalIgnoreCase);
            var countryname = await instancedb.TblCountries.Where(s => s.Id == req.UserCountryId).Select(s => s.Country).FirstOrDefaultAsync(cancellationToken);
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
                    }).ToListAsync(cancellationToken),

                Client = await instancedb.TblClientDetails.Where(s => s.Status == true && (isAdmin || s.Country == countryname)
                ).Select(x => new DropdownItem
                {
                    Value = x.Id,
                    Label = x.ClientName ?? "",
                    //Selected = true,
                }).ToListAsync(cancellationToken),

                User = await instancedb.TblAgencyUsers.Where(s => s.Status == true && (s.Id == req.UserId)
                ).Select(x => new DropdownItem
                {
                    Value = x.Id,
                    Label = x.FirstName + " " + x.LastName ?? ""
                }).ToListAsync(cancellationToken),

                Status = await instancedb.TblJobStatuses
                 .Select(x => new DropdownItem
                 {
                     Value = x.StatusId,
                     Label = x.StatusName ?? "",
                     Selected = false
                 }).ToListAsync(cancellationToken),

                ManagementFeetype = await instancedb.Tblmanagmenttypes.Where(s => s.Status == "1")
                    .Select(x => new DropdownItem
                    {
                        Value = x.Id,
                        Label = x.Managementfeetypename ?? "",
                        Selected = false
                    }).OrderByDescending(s => s.Value).ToListAsync(cancellationToken),

                Printcapabilities = await instancedb.TblPrintCapabilities
                    .Select(x => new DropdownItem
                    {
                        Value = x.CapabilityId,
                        Label = x.Capability ?? "",
                        Selected = false
                    }).ToListAsync(cancellationToken),

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
                 ).OrderBy(x => x.Label).ToListAsync(cancellationToken),

                Sla = await instancedb.TblSlas.Where(s => s.Status == true && s.Country == countryname).OrderBy(s => s.Description)
                .Select(x => new DropdownItem
                {
                    Value = x.Id,
                    Label = x.Description ?? "",
                    Selected = x.IsDefault
                }).ToListAsync(cancellationToken),

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
        public async Task<EnquirySlaDto?> GetSlatemplate(EnquiryFilterRequest req, CancellationToken cancellationToken)
        {
            EnquirySlaDto? sla;

            if (req.EnqId.HasValue)
            {
                // Step 1: Check if TemplateId exists
                var templateId = await instancedb.TblEnquiryMasters
                    .Where(m => m.Id == req.EnqId)
                    .Select(m => m.TemplateId)
                    .FirstOrDefaultAsync(cancellationToken);

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
                        .FirstOrDefaultAsync(cancellationToken);
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
                        .FirstOrDefaultAsync(cancellationToken);
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
                    .FirstOrDefaultAsync(cancellationToken);
            }

            return sla;
        }

        [HttpPost("EnqClientinfoDropdowns")]
        public async Task<MasterDropdowns> EnqClientinfoDropdowns(UserRequest req, CancellationToken cancellationToken)
        {
            int Divisionid = req.Divisionid;
            var division = await instancedb.TblDivisions.AsNoTracking().FirstOrDefaultAsync(s => s.Id == Divisionid, cancellationToken);
            if (division == null)
                return new MasterDropdowns { Division = [], Brands = [], Client = [] };

            var response = new MasterDropdowns
            {
                Division = await instancedb.TblDivisions.Where(s => s.Country == division.Country && s.BussinessUnit == division.BussinessUnit && s.Status == true)
                .OrderBy(s => s.GlobalBumapping)
                    .Select(x => new DropdownItem
                    {
                        Value = x.Id,
                        Label = x.GlobalBumapping ?? "",
                        Selected = false
                    }).ToListAsync(cancellationToken),

                Brands = await instancedb.TblDivisionBrandsorproducts.Where(s => s.Divisionid == Divisionid)
                .OrderBy(s => s.Brandorproduct)
                    .Select(x => new DropdownItem
                    {
                        Value = x.Id,
                        Label = x.Brandorproduct ?? "",
                        Selected = false
                    }).ToListAsync(cancellationToken),

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
                }).ToListAsync(cancellationToken),

            };

            return response;
        }

        [HttpPost("EnqLineItemsDropdowns")]
        public async Task<MasterDropdowns> EnqLineItemsDropdowns(UserRequest req, CancellationToken cancellationToken)
        {
            var response = new MasterDropdowns
            {
                ItemCategory = (await instancedb.TblProductCategories.AsNoTracking()
                .Where(s => s.Status == true && s.Categorytype != "Catalog")
                .OrderBy(s => s.Category)
                .ToListAsync(cancellationToken))
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

                SubCategory = (await instancedb.TblSubCategories.AsNoTracking().Where(s => s.Status == true && s.Typeofjob == req.TypeOfJob)
                .OrderBy(s => s.SubCategory).ToListAsync(cancellationToken))
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
                    }).ToListAsync(cancellationToken),
                ExceptionReason = await instancedb.Tblexceptionreasons.Where(s => s.Status == true)
                .OrderBy(s => s.Exceptionreason)
                    .Select(x => new DropdownItem
                    {
                        Value = x.Id,
                        Label = x.Exceptionreason ?? "",
                        Selected = false
                    }).ToListAsync(cancellationToken),
                LocalCatalog = await instancedb.TblCatalogueUsages.Where(s => s.Status == true)
                .OrderBy(s => s.CatalogueUsage)
                    .Select(x => new DropdownItem
                    {
                        Value = x.Id,
                        Label = x.CatalogueUsage ?? "",
                        Selected = (x.CatalogueUsage ?? "") == "N/A" ? true : false
                    }).ToListAsync(cancellationToken),
                SourcingLocation = await instancedb.TblSourcinglocations.Where(s => s.Status == true)
                .OrderBy(s => s.Sourcinglocation)
                    .Select(x => new DropdownItem
                    {
                        Value = x.Id,
                        Label = x.Sourcinglocation ?? "",
                        Selected = (x.Sourcinglocation ?? "") == "N/A" ? true : false
                    }).ToListAsync(cancellationToken),
                RegionalOrder = await instancedb.TblRegionalcatalognames.Where(s => s.Status == true)
                .OrderBy(s => s.RegionalName)
                    .Select(x => new DropdownItem
                    {
                        Value = x.Id,
                        Label = x.RegionalName ?? "",
                        Selected = (x.RegionalName ?? "") == "N/A" ? true : false
                    }).ToListAsync(cancellationToken),
                TypeOfItem = await GetTypeOfItems(req.TypeOfJob ?? "", req.Subitemid, cancellationToken),
                SavingsReason = await instancedb.TblSavingReasons.Where(s => s.Status == true && s.Savingstype == req.Savingstype)
                .OrderBy(s => s.SavingReson)
                    .Select(x => new DropdownItem
                    {
                        Value = x.Id,
                        Label = x.SavingReson ?? "",
                        Selected = false
                    }).ToListAsync(cancellationToken),

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
                        Selected = (x.ProductionMethod ?? "") == "N/A" || (x.ProductionMethod ?? "") == "NA" ? true : false
                    }).ToListAsync(cancellationToken),
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
        public async Task<List<DropdownItem>> GetTypeOfItems(string jobtype, int subitemid, CancellationToken cancellationToken = default)
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
                    .FirstOrDefaultAsync(cancellationToken);

                if (marked)
                {
                    List<string> jobTypes;

                    if (jobtype == "POSM" || jobtype == "Print")
                        jobTypes = new List<string> { "Both", "POSM" };
                    else
                        jobTypes = new List<string> { "Promo" };

                    query = instancedb.TblTypeofItems
                        .Where(x => x.Status == true &&
                                    x.TypeofJob != null && jobTypes.Contains(x.TypeofJob) &&
                                    x.SubitemId == subitemid);
                }
            }

            // 🔹 Step 3: Final Projection + Sorting
            var dbList = await query
            .OrderBy(x => x.TypeofItem)
            .Select(x => new DropdownItem
            {
                Value = x.Id,
                Label = x.TypeofItem!, // null labels keep being omitted from the JSON, as before
                Selected = false
            })
            .ToListAsync(cancellationToken);

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
        public async Task<List<EnqSupplierinfo>?> GetSupplier(UserRequest req, [FromQuery] PageQuery paging, CancellationToken cancellationToken)
        {
            // Load country ids once instead of one query per supplier
            // (case-insensitive, trailing spaces ignored: same matching rules as the SQL comparison it replaces)
            var countryIds = (await instancedb.TblCountries.AsNoTracking()
                    .OrderBy(s => s.Id)
                    .Select(s => new { s.Id, s.Country })
                    .ToListAsync(cancellationToken))
                .Where(s => s.Country != null)
                .DistinctBy(s => s.Country!.TrimEnd(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(s => s.Country!.TrimEnd(), s => s.Id, StringComparer.OrdinalIgnoreCase);

            var suppliers = instancedb.TblSuppliersLists.AsNoTracking()
                .Where(s => s.Currency == req.Currency && s.Status == true)
                .Select(s => new { s.Id, s.SupplierName, s.Country, s.Currency, s.Suppliercode, s.Productioncapability });

            // Unpaged keeps the original in-memory name ordering; paged requests are ordered and paged in SQL
            var (rows, page) = Pagination.Resolve(paging, _pagination.DefaultPageSize) == null
                ? ((await suppliers.ToListAsync(cancellationToken)).OrderBy(s => s.SupplierName).ToList(), (PageInfo?)null)
                : await suppliers.OrderBy(s => s.SupplierName).ThenBy(s => s.Id)
                    .ToPageAsync(paging, _pagination.DefaultPageSize, cancellationToken);
            Response.WriteHeaders(page);

            return rows.Select(x => new EnqSupplierinfo
            {
                SupplierId = x.Id,
                Suppliername = x.SupplierName,
                Country = x.Country,
                CountryId = x.Country != null && countryIds.TryGetValue(x.Country.TrimEnd(), out var countryId) ? countryId : 0,
                Currency = x.Currency,
                Selected = false,
                Suppliercode = x.Suppliercode ?? "",
                PrintCapability = x.Productioncapability
            }).ToList();
        }
        #endregion

        #region Add client
        [HttpPost("AddnewClient")]
        public async Task<Result> AddnewClient([FromBody] ClientModel req)
        {
            Result rslt = new Result();
            if (ModelState.IsValid)
            {
                var division = await instancedb.TblDivisions.AsNoTracking().FirstOrDefaultAsync(s => s.Id == req.Divisionid);
                if (division != null)
                {
                    rslt = await InTransactionAsync(async () =>
                    {
                        var result = new Result();
                        var isExists = await instancedb.TblUsers.AnyAsync(x => x.UserName == req.Logonid);
                        if (isExists)
                        {
                            result.Status = false;
                            result.Data = "Username already exists";
                            return result;
                        }
                        var isClientExists = await instancedb.TblClients.AnyAsync(x => x.FirstName == req.FirstName && x.Email == req.Email);
                        if (isClientExists)
                        {
                            result.Status = false;
                            result.Data = "Client with same firstname and emailid already exists";
                            return result;
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
                        await instancedb.SaveChangesAsync();
                        TblUser tbuser = new TblUser();
                        tbuser.UserName = req.Logonid;
                        tbuser.Password = "password";
                        tbuser.FkId = tbclient.Id;
                        tbuser.UserType = "Client";
                        tbuser.Status = true;
                        instancedb.TblUsers.Add(tbuser);
                        await instancedb.SaveChangesAsync();
                        tbclient.Userid = tbuser.Id;
                        await instancedb.SaveChangesAsync();
                        return Result.Ok("Client added successfully");
                    });
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
        public async Task<Result> AddnewBrands([FromBody] BrandModel req)
        {
            Result rslt = new Result();
            if (ModelState.IsValid)
            {
                var division = await instancedb.TblDivisions.AsNoTracking().FirstOrDefaultAsync(s => s.Id == req.Divisionid);
                if (division != null)
                {
                    var isExists = await instancedb.TblDivisionBrandsorproducts.AnyAsync(x => x.Divisionid == req.Divisionid && x.Brandorproduct == req.Brand);
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
                    await instancedb.SaveChangesAsync();
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

        #region Add Suppliers

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

        #region CheckforUsername
        [HttpPost("CheckforUsername")]
        public async Task<Result> CheckforUsername(UserRequest req, CancellationToken cancellationToken)
        {
            Result rslt = new Result();
            var isExists = await instancedb.TblUsers.AnyAsync(x => x.UserName == req.Username, cancellationToken);
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
        public async Task<GetDetails> GetDetails(UserRequest req, CancellationToken cancellationToken)
        {
            GetDetails rslt = new GetDetails();
            try
            {
                int Enquiryid = req.Enquiryid;
                var data = await (from em in instancedb.TblEnquiryMasters
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
                                  }).AsNoTracking().FirstOrDefaultAsync(cancellationToken);
                if (data != null)
                {
                    var status = data.ed.Select(s => s.Status).FirstOrDefault();
                    var jobstatus = await instancedb.TblJobStatuses.AsNoTracking().Where(s => s.StatusId == status).FirstOrDefaultAsync(cancellationToken);

                    // Look-ups are loaded up front (they used to be separate synchronous queries inside the object initializers)
                    var clientContactUserName = await instancedb.TblUsers
                        .Where(s => s.FkId == data.em.ClientContactId && s.UserType == "Client")
                        .Select(s => s.UserName).FirstOrDefaultAsync(cancellationToken);
                    var countryNames = await instancedb.TblCountries
                        .Where(s => s.Id == data.em.PmgEntity || s.Id == data.em.DeliveryCountry)
                        .Select(s => new { s.Id, s.Country })
                        .ToDictionaryAsync(s => s.Id, s => s.Country, cancellationToken);
                    var managementFee = await ResolveManagementFeeAsync(
                        data.em.Managementfeetypeid, data.em.ClientId, data.ed.FirstOrDefault()?.ManagementFeeUnit, cancellationToken);
                    var slaTemplateName = await instancedb.TblSlas
                        .Where(x => x.Id == data.em.TemplateId).Select(s => s.Description).FirstOrDefaultAsync(cancellationToken);
                    var managementFeeTypeName = await instancedb.Tblmanagmenttypes
                        .Where(x => x.Id == data.em.Managementfeetypeid).Select(s => s.Managementfeetypename).FirstOrDefaultAsync(cancellationToken);
                    var categoryIds = data.ed.Where(x => x.ProductCategoryId != null).Select(x => x.ProductCategoryId!.Value).Distinct().ToList();
                    var categoryNames = await instancedb.TblProductCategories
                        .Where(pc => categoryIds.Contains(pc.Id))
                        .Select(pc => new { pc.Id, pc.Category })
                        .ToDictionaryAsync(pc => pc.Id, pc => pc.Category, cancellationToken);
                    string? CountryName(int id) => countryNames.TryGetValue(id, out var name) ? name : null;

                    rslt.StatusId = jobstatus?.StatusId ?? status;
                    rslt.Jobstatus = jobstatus?.StatusName ?? "";
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
                        ClientContact = data.c.FirstName + " " + data.c.LastName + (" (" + clientContactUserName + ")"),
                        PMGEntityname = CountryName(data.em.PmgEntity) ?? "",
                        DeliveryCountryname = CountryName(data.em.DeliveryCountry) ?? "",
                        Country = CountryName(data.em.PmgEntity) ?? "",
                        InvoiceNumber = data.em.InvoiceNumber ?? "",
                        PoNumber = data.em.Ponumber ?? "",
                        RaisedDate = data.em.PoorderraisedDate ?? "",
                        CreatedDate = data.em.EstDate ?? "",
                        UserName = data.U.UserName ?? "",
                        InvoiceDate = data.em.InvoicedDate ?? "",
                        ManagementFee = managementFee,
                        Actualdeliverydate = data.em.Actualdeliverydate ?? data.em.EstDate,
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
                        SlaTemplatename = slaTemplateName,
                        ManagementFeetype = managementFeeTypeName,
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
                        Productcategory = (ed.ProductCategoryId != null && categoryNames.TryGetValue(ed.ProductCategoryId.Value, out var category) ? category : null) ?? "",
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
                        // Tolerate blanks/spaces in the stored comma list (int.Parse used to throw on them)
                        List<int> supplierIds = selectedSuppliers
                            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                            .Select(s => int.TryParse(s, out var id) ? id : 0)
                            .Where(id => id > 0)
                            .ToList();

                        rslt.supplierinfo = await instancedb.TblSuppliersLists
                            .Where(s => supplierIds.Contains(s.Id)).
                            Select(x => new SupplierDetails
                            {
                                SupplierID = x.Id,
                                Suppliername = x.SupplierName
                            })
                            .ToListAsync(cancellationToken);
                    }
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "GetDetails failed for enquiry {EnquiryId}", req.Enquiryid);
            }
            return rslt;
        }
        #endregion

        #region Insertstep1
        [HttpPost("EnqClientinfo")]
        public async Task<Result> EnqClientinfo(EnqClientinfo req)
        {
            Result rslt = new Result();
            if (ModelState.IsValid)
            {
                var division = await instancedb.TblDivisions.AsNoTracking().FirstOrDefaultAsync(s => s.Id == req.Divisionid);
                if (division != null)
                {
                    var clientdetails = await instancedb.TblClientDetails.AsNoTracking().FirstOrDefaultAsync(s => s.Id == division.ClientId);
                    if (clientdetails == null)
                    {
                        rslt.Status = false;
                        rslt.Data = "Client details not found for Division";
                        return rslt;
                    }
                    var isClientExists = await instancedb.TblClients.AnyAsync(x => x.Id == req.ClientContactId);
                    if (!isClientExists)
                    {
                        rslt.Status = false;
                        rslt.Data = "Client Id not matched with Division";
                        return rslt;
                    }
                    var isBrandExists = await instancedb.TblDivisionBrandsorproducts.AnyAsync(x => x.Brandorproduct == req.Brand);
                    if (!isBrandExists)
                    {
                        rslt.Status = false;
                        rslt.Data = "Brand not matched with Division";
                        return rslt;
                    }

                    void ApplyClientInfo(TblEnquiryMaster enq)
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
                    }

                    rslt = await InTransactionAsync(async () =>
                    {
                        if (req.Action == "Update")
                        {
                            var enq = await instancedb.TblEnquiryMasters.FirstOrDefaultAsync(x => x.Id == req.EnqId);
                            if (enq == null)
                                return Result.Fail("Enquiry not found");

                            ApplyClientInfo(enq);
                            await instancedb.SaveChangesAsync();
                            return Result.Ok(new
                            {
                                EnqId = enq.Id,
                                StepId = enq.StepId,
                                Message = "Enquiry updated successfully"
                            });
                        }
                        else
                        {
                            TblEnquiryMaster enq = new TblEnquiryMaster();
                            ApplyClientInfo(enq);
                            instancedb.TblEnquiryMasters.Add(enq);
                            await instancedb.SaveChangesAsync();
                            TblEnquiryDetail endet = new TblEnquiryDetail();
                            endet.EnquiryId = enq.Id;
                            endet.Status = 0;
                            instancedb.TblEnquiryDetails.Add(endet);
                            await instancedb.SaveChangesAsync();
                            enq.EnqUid = req.Menuid == 3 ? "CAT" + enq.Id.ToString("D6") : req.Menuid == 2 ? "BID" + enq.Id.ToString("D6") : "ENQ" + enq.Id.ToString("D6");
                            enq.Status = true;
                            await instancedb.SaveChangesAsync();
                            return Result.Ok(new
                            {
                                EnqId = enq.Id,
                                StepId = enq.StepId,
                                Message = "Enquiry created successfully"
                            });
                        }
                    });
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
        public async Task<Result> EnqProjectinfo(EnqProjectinfo req)
        {
            Result rslt = new Result();
            if (ModelState.IsValid)
            {
                rslt = await InTransactionAsync(async () =>
                {
                    var enqmaster = await instancedb.TblEnquiryMasters.FirstOrDefaultAsync(x => x.Id == req.EnqId);
                    var enqdetail = await instancedb.TblEnquiryDetails.FirstOrDefaultAsync(x => x.EnquiryId == req.EnqId);
                    if (enqmaster == null || enqdetail == null)
                        return Result.Fail("Enquiry not found");

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
                    // One save for master + detail: a single short write keeps lock time (and deadlock risk) minimal
                    await instancedb.SaveChangesAsync();
                    return Result.Ok(new
                    {
                        EnqId = enqmaster.Id,
                        StepId = enqmaster.StepId,
                        Message = "Project Information for Enquiry created successfully"
                    });
                });

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
        public async Task<Result> EnqLineItemDetails(EnquiryItem req)
        {
            Result rslt = new Result();
            if (ModelState.IsValid)
            {
                rslt = await InTransactionAsync(async () =>
                {
                    if (req.EnqdetailsId.HasValue)
                    {
                        var enqdata = await instancedb.TblEnquiryDetails.FirstOrDefaultAsync(x => x.EnquiryId == req.EnqId && x.Id == req.EnqdetailsId);
                        if (enqdata == null)
                            return Result.Fail("Enquiry item not found");
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
                        var enqdetail = await instancedb.TblEnquiryDetails.Where(x => x.EnquiryId == req.EnqId).ToListAsync();
                        if (enqdetail.Count == 0)
                            return Result.Fail("Enquiry not found");
                        var subCategoryName = await instancedb.TblSubCategories.Where(s => s.Id == req.SubcatID).Select(s => s.SubCategory).FirstOrDefaultAsync();
                        var enqdata = enqdetail[0];
                        if (enqdetail.Count == 1 && string.IsNullOrEmpty(enqdata.ItemName) && enqdata.ItemNumber == null)
                        {
                            enqdata = AssignData(enqdata, req, subCategoryName);

                        }
                        else
                        {
                            item = new TblEnquiryDetail
                            {
                                EnquiryId = req.EnqId,
                                // MAX + 1 (not COUNT + 1): EN_DeleteItem does not renumber, so COUNT + 1 re-used the
                                // number of an existing item after a delete (quotes/savings are matched by ItemNumber)
                                ItemNumber = (enqdetail.Max(x => x.ItemNumber) ?? 0) + 1,
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
                                SubCategory = subCategoryName
                            };
                            if (item != null)
                                instancedb.TblEnquiryDetails.Add(item);
                        }
                    }

                    var enqmaster = await instancedb.TblEnquiryMasters.FirstOrDefaultAsync(x => x.Id == req.EnqId);
                    if (enqmaster == null)
                        return Result.Fail("Enquiry not found");
                    if (enqmaster.StepId < 3)
                    {
                        enqmaster.StepId = 3;
                    }
                    await instancedb.SaveChangesAsync();
                    return Result.Ok(new
                    {
                        EnqId = enqmaster.Id,
                        StepId = enqmaster.StepId,
                        Message = req.EnqdetailsId.HasValue ? "Item updated in VA" : "Item created in VA"
                    });
                });

            }
            else
            {
                rslt.Status = false;
                rslt.Data = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            }
            return rslt;
        }
        [NonAction]
        public TblEnquiryDetail AssignData(TblEnquiryDetail enqdata, EnquiryItem req, string? subCategoryName)
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
            enqdata.SubCategory = subCategoryName;
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
        public async Task<char> InsertEnqLineItemAttachment(int enqId, int itemNumber, string fileList, int modifiedBy)
        {
            char rslt = 'F';
            var enqdetail = await instancedb.TblEnquiryDetails.FirstOrDefaultAsync(x => x.EnquiryId == enqId && x.ItemNumber == itemNumber);
            if (enqdetail != null)
            {
                enqdetail.QuoteFileList = fileList;
                enqdetail.CreatedOrModifiedBy = modifiedBy;
                await instancedb.SaveChangesAsync();
                rslt = 'S';
            }
            return rslt;
        }
        #endregion

        #region Insertstep4
        [HttpPost("EnqSupplierInfo")]
        public async Task<Result> EnqSupplierInfo(SupplierInfo req)
        {
            Result rslt = new Result();
            if (ModelState.IsValid)
            {
                rslt = await InTransactionAsync(async () =>
                {
                    var enqmaster = await instancedb.TblEnquiryMasters.FirstOrDefaultAsync(x => x.Id == req.EnqId);
                    if (enqmaster == null)
                        return new Result();

                    // Set-based update of all items (runs inside the transaction)
                    await instancedb.TblEnquiryDetails.Where(s => s.EnquiryId == req.EnqId)
                        .ExecuteUpdateAsync(s => s.SetProperty(x => x.CreatedOrModifiedBy, req.ModifiedBy)
                                                  .SetProperty(x => x.SelectedSuppliers, req.SelectedSuppliers));

                    if (enqmaster.StepId < 4)
                    {
                        enqmaster.StepId = 4;
                        await instancedb.SaveChangesAsync();
                    }

                    return Result.Ok(new
                    {
                        EnqId = enqmaster.Id,
                        StepId = enqmaster.StepId,
                        Message = "Supplier added in VA"
                    });
                });
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
                string? selectedSuppliers = await instancedb.TblEnquiryDetails.Where(x => x.EnquiryId == enqId).OrderBy(x => x.ItemNumber).Select(x => x.SelectedSuppliers).FirstOrDefaultAsync();

                // Calculate total Item Sell Price
                decimal totalSellPrice = await instancedb.TblEnquiryDetails.Where(x => x.EnquiryId == enqId).SumAsync(x => x.ItemSellPrice ?? 0);

                // Execute Stored Procedure
                await instancedb.Database.ExecuteSqlRawAsync(@"EXEC EN_InsertEnquiryMasterDetailsStep5 @EnquiryID = {0}, @SelectedSuppliers = {1}, @ItemSellPrice = {2}, @jobtype = {3}, @Createdby = {4}"
                                                             , enqId, selectedSuppliers ?? (object)DBNull.Value, totalSellPrice, "New Item", createdBy);

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
        public async Task<GetProjectDetails> GetProjectDetails(UserRequest req, [FromQuery] PageQuery paging, CancellationToken cancellationToken)
        {
            var rslt = new GetProjectDetails();
            try
            {
                int enquiryId = req.Enquiryid;

                // Historical prices for the whole country/currency: by far the largest list on this screen.
                // Optional paging (?pageNumber=&pageSize=) is applied in SQL; totals are returned in X-* headers.
                var historyQuery = instancedb.TblNewSavingsData.AsNoTracking()
                    .Where(x => x.HistoricalPriceLcl != null && x.Curr == req.Currency && x.Country == req.Country)
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
                    });
                var (historySearches, historyPage) = Pagination.Resolve(paging, _pagination.DefaultPageSize) == null
                    ? (await historyQuery.ToListAsync(cancellationToken), (PageInfo?)null)
                    : await historyQuery
                        .OrderBy(x => x.EnquriyID).ThenBy(x => x.Specifications).ThenBy(x => x.Qty).ThenBy(x => x.ReferencePrice)
                        .ToPageAsync(paging, _pagination.DefaultPageSize, cancellationToken);
                rslt.historySearches = historySearches;
                Response.WriteHeaders(historyPage);

                rslt.historyLogs = await (from jh in instancedb.TblJobHistories
                                          join u in instancedb.TblUsers on jh.UserId equals u.Id into ug
                                          from u in ug.DefaultIfEmpty()
                                          join oldStatus in instancedb.TblJobStatuses on jh.OldValue equals oldStatus.StatusId.ToString() into oldStatusGroup
                                          from oldStatus in oldStatusGroup.DefaultIfEmpty()
                                          join newStatus in instancedb.TblJobStatuses on jh.NewValue equals newStatus.StatusId.ToString() into newStatusGroup
                                          from newStatus in newStatusGroup.DefaultIfEmpty()
                                          where jh.EnqId == enquiryId && !string.IsNullOrEmpty(jh.NewValue)
                                          orderby jh.ModifiedDate descending
                                          select new HistoryLogs
                                          {
                                              ModifiedDate = jh.ModifiedDate!.Value.ToString("dd/MM/yyyy hh:mm:ss tt") ?? "",
                                              UserName = u != null ? u.UserName : "",
                                              Field = jh.Field,
                                              OldValue = jh.Field == "Status" ? oldStatus.StatusName : jh.OldValue,
                                              NewValue = jh.Field == "Status" ? newStatus.StatusName : jh.NewValue
                                          }).ToListAsync(cancellationToken);

                rslt.lineItemLogs = await (from jh in instancedb.TblLineitemsHistories
                                           join u in instancedb.TblUsers on jh.Userid equals u.Id into ug
                                           from u in ug.DefaultIfEmpty()
                                           where jh.Enqid == enquiryId && !string.IsNullOrEmpty(jh.NewValue) && !string.IsNullOrEmpty(jh.OldValue) && jh.OldValue != "0" && jh.OldValue != "-2"
                                           && jh.OldValue != "-3" && jh.OldValue != "undefined" && jh.OldValue != "Null" && jh.OldValue != jh.NewValue && jh.NewValue != "undefined"
                                           orderby jh.ModifiedDate descending
                                           select new LineItemsLogs
                                           {
                                               ModifiedDate = jh.ModifiedDate!.Value.ToString("dd/MM/yyyy hh:mm:ss tt") ?? "",
                                               UserName = u != null ? u.UserName : "",
                                               Field = jh.Field,
                                               OldValue = jh.OldValue,
                                               NewValue = jh.NewValue,
                                               ItemNumber = jh.ItemNumber
                                           }).ToListAsync(cancellationToken);

                var quoteHistory = await (
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
                                }).ToListAsync(cancellationToken);
                rslt.revisedQuotes = [.. quoteHistory.GroupBy(x => new { x.ItemName, x.SupplierId })
                                .SelectMany(g => g.OrderByDescending(x => x.DateOfChange).Take(3)).OrderBy(x => x.ItemName).ThenByDescending(x => x.SupplierName).ThenByDescending(x => x.DateOfChange)
                                .Select(x => new RevisedQuotes
                                {
                                    ItemNumber = x.ItemNumber,
                                    ItemName = x.ItemName,
                                    SupplierName = x.SupplierName,
                                    SupplierPrice = x.SupplierPrice,
                                    ItemNote = x.ItemNote,
                                    DateOfChange = x.DateOfChange!.Value.ToString("dd/MM/yyyy hh:mm:ss tt") ?? "",
                                })];

                // Only the needed columns (TblEnquiryDetails has ~100), read-only
                var query = await (from h in instancedb.TblSupplierQuotes
                                   join ed in instancedb.TblEnquiryDetails on h.EnquiryDetailsId equals ed.Id
                                   join s in instancedb.TblSuppliersLists on h.SupplierId equals s.Id
                                   where h.EnquiryId == enquiryId
                                   select new
                                   {
                                       h.Id,
                                       h.ItemNumber,
                                       h.EnquiryId,
                                       h.EnquiryDetailsId,
                                       h.SupplierId,
                                       h.InitAmount,
                                       h.Amount,
                                       ed.ItemName,
                                       ed.QuoteQtyOrSize,
                                       ed.ManagementFeeUnit,
                                       ed.ItemSellPrice,
                                       EdSupplierId = ed.SupplierId,
                                       s.SupplierName
                                   }).ToListAsync(cancellationToken);

                // Management fee is resolved once per distinct fee unit (it used to run 2-3 queries for every quote row)
                var mfeeByUnit = new Dictionary<decimal, decimal>();
                foreach (var unit in query.Select(x => x.ManagementFeeUnit ?? 0).Distinct())
                    mfeeByUnit[unit] = await GetMfeeAsync(enquiryId, unit, cancellationToken);

                rslt.requestQuotes = [.. query.Select(x =>
                {
                    // Quantity is stored as text; 0/blank/non-numeric previously threw DivideByZero/FormatException
                    // and the whole summary page came back empty
                    decimal.TryParse(x.QuoteQtyOrSize, NumberStyles.Number, CultureInfo.InvariantCulture, out var qty);
                    decimal initAmount = Convert.ToDecimal(x.InitAmount);
                    decimal amount = Convert.ToDecimal(x.Amount);
                    decimal negUnitPrice = qty != 0 ? amount / qty : 0;

                    return new RequestQuotes
                    {
                        ItemNumber = x.ItemNumber,
                        ItemName = "Item " + x.ItemNumber + " - " + (x.ItemName ?? ""),
                        SupplierName = x.SupplierName,
                        SupplierQuotesId = x.Id,
                        EnquiryId = x.EnquiryId,
                        EnquiryDetailsId = x.EnquiryDetailsId,
                        SupplierId = x.SupplierId,
                        InitialQuote = Math.Round(initAmount, 2),
                        NegQuote = Math.Round(amount, 2),
                        Quantity = (int)qty,
                        IniUnitPrice = Math.Round(qty != 0 ? initAmount / qty : 0, 2),
                        NegUnitPrice = Math.Round(negUnitPrice, 2),
                        NegUnitPriceFee = Math.Round(negUnitPrice * (mfeeByUnit[x.ManagementFeeUnit ?? 0] / 100), 2),
                        PmgSellPrice = x.ItemSellPrice,
                        IsCalculateId = x.EdSupplierId,
                    };
                })];

                var calculationDetails = await (from h in instancedb.TblSupplierQuotes
                                                join ed in instancedb.TblEnquiryDetails on h.EnquiryDetailsId equals ed.Id
                                                where h.EnquiryId == enquiryId && h.QuoteStatus == 2 && ed.SupplierId == h.SupplierId
                                                group new { h, ed } by 1 into g
                                                select new
                                                {
                                                    Cost = g.Sum(x => (decimal?)x.h.Amount) ?? 0,
                                                    Sell = g.Sum(x => (decimal?)x.ed.ItemSellPrice) ?? 0
                                                }).FirstOrDefaultAsync(cancellationToken);

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

                rslt.calculationSupplierlogs = await (from ed in instancedb.TblEnquiryDetails
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
                                                      }).ToListAsync(cancellationToken);

                var count = await instancedb.TblEnquiryDetails.Where(x => x.EnquiryId == enquiryId).GroupBy(x => new { x.Savingstype, x.Savingsreason }).CountAsync(cancellationToken);

                rslt.savingReasons = await instancedb.TblEnquiryDetails.Where(x => x.EnquiryId == enquiryId).Select(x => new SavingReasons
                {
                    ItemName = count == 1 ? "All items" : x.ItemName,
                    Id = count == 1 ? 0 : x.Id,
                    SavingsType = x.Savingstype,
                    SavingsReason = x.Savingsreason
                }).Distinct().ToListAsync(cancellationToken);

                rslt.SavingsResponseDto = await GetSavings(enquiryId, cancellationToken);

                rslt.DeliveryOrder = await instancedb.TblClintDeliveryAddresses.Where(x => x.Enqid == enquiryId).Select(x => new DeliveryOrder
                {
                    Enqid = x.Enqid,
                    AddressLineOne = x.AdressLine1,
                    AddressLineTwo = x.AdressLine2,
                    AddressLineThree = x.AdressLine3,
                    CompanyName = x.CompanyName,
                    ContactNo = x.ContactNo,
                    NameorDept = x.NameorDept,
                    Id = x.Id,
                    Remarks = x.Remarks
                }).ToListAsync(cancellationToken);

                rslt.ProjectStatus = await GetProjectStatus(enquiryId, cancellationToken);

                rslt.PreviewQuotes = await instancedb.TblEnquiryDetails.Where(x => x.EnquiryId == enquiryId).Select(x => new PreviewQuotes
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
                }).ToListAsync(cancellationToken);

            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "GetProjectDetails failed for enquiry {EnquiryId}", req.Enquiryid);
            }
            return rslt;
        }

        #region Getsavings
        [HttpGet("GetSavings")]
        public async Task<SavingsResponseDto> GetSavings(int enquiryId, CancellationToken cancellationToken)
        {
            var response = new SavingsResponseDto();
            // var savings = instancedb.TblSavingsDetails
            //   .Where(x => x.EnquiryId == enquiryId)
            // .ToList();

            var savings = await (from s in instancedb.TblSavingsDetails.AsNoTracking()
                                 join d in instancedb.TblEnquiryDetails on s.EnquiryDetailsId equals d.Id
                                 where s.EnquiryId == enquiryId
                                 select new
                                 {
                                     ItemName = d.ItemName,
                                     s
                                 }).ToListAsync(cancellationToken);


            if (!savings.Any()) return response;

            var enquiryDetails = await instancedb.TblEnquiryDetails
                .Where(x => x.EnquiryId == enquiryId)
                .Select(x => new { x.ItemNumber, x.ItemName, x.QuoteQtyOrSize, x.Savingstype, x.ManagementFeeUnit })
                .ToListAsync(cancellationToken);

            var result = await (from m in instancedb.TblEnquiryMasters
                                join d in instancedb.TblEnquiryDetails on m.Id equals d.EnquiryId
                                join c in instancedb.TblCountries on m.PmgEntity equals c.Id
                                where m.Id == enquiryId && (d.ManagementFeeUnit ?? 0) != 0
                                select new
                                {
                                    TaxPercentage = c.TaxPercentage,
                                    ManagementFeeUnit = d.ManagementFeeUnit,
                                    ManagementFeeMethod = d.ManagementFeeMethod
                                }).FirstOrDefaultAsync(cancellationToken);


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
                    ItemName = enquiry?.ItemName ?? x.ItemName ?? "",
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
        public async Task<List<ProjectStatus>> GetProjectStatus(int enqId, CancellationToken cancellationToken)
        {
            int? status = await instancedb.TblEnquiryDetails.Where(x => x.EnquiryId == enqId).OrderByDescending(x => x.Id).Select(x => x.Status)
            .FirstOrDefaultAsync(cancellationToken);
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
            return await instancedb.TblJobStatuses.Where(x => statusIds.Contains(x.StatusId)).Select(x => new ProjectStatus
            {
                Value = x.StatusId,
                Label = x.StatusName
            }).ToListAsync(cancellationToken);

        }

        #endregion

        #endregion

        #region DeliveryAddress
        [HttpPost("CheckDeliveryAddress")]
        public async Task<Result> CheckDeliveryAddress(DeliveryOrder req, CancellationToken cancellationToken)
        {
            Result rslt = new Result();

            var data = await instancedb.TblClintDeliveryAddresses.AsNoTracking()
                .FirstOrDefaultAsync(x => x.CompanyName == req.CompanyName, cancellationToken);

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
        public async Task<Result> AddUpdateDeliveryAddress(DeliveryOrder req)
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
                    var existing = await instancedb.TblClintDeliveryAddresses.FirstOrDefaultAsync(x => x.Id == req.Id);

                    if (existing == null)
                    {
                        rslt.Status = false;
                        rslt.Data = "Delivery address not found.";
                        return rslt;
                    }
                    address = existing;
                }
                address.Enqid = req.Enqid;
                address.CompanyName = req.CompanyName;
                address.AdressLine1 = req.AddressLineOne;
                address.AdressLine2 = req.AddressLineTwo;
                address.AdressLine3 = req.AddressLineThree;
                address.NameorDept = req.NameorDept;
                address.ContactNo = req.ContactNo;
                address.Remarks = req.Remarks;
                await instancedb.SaveChangesAsync();
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
            if (quotes == null || quotes.Count == 0)
            {
                rslt.Status = false;
                rslt.Data = "No quotes to update";
                return rslt;
            }
            try
            {
                static decimal? ParseAmount(string? value) =>
                    decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount) ? amount : null;

                foreach (var item in quotes)
                {
                    // Values are per quote. They used to live outside the loop, so a quote with no price (or no
                    // price increase) silently reused the previous quote's price / price difference.
                    decimal? initial;
                    decimal? negotiated;
                    if (item.SupplierQuoteAmountPriceOrQuantity == 1)
                    {
                        initial = ParseAmount(item.Intialprice);
                        negotiated = ParseAmount(item.Negoprice ?? item.Intialprice);
                    }
                    else
                    {
                        // Quote by unit price: total = unit price x quantity, rounded to 2 decimals
                        initial = ParseAmount(item.InitialUnitprice) * item.Qty;
                        negotiated = ParseAmount(item.NegoUnitprice ?? item.InitialUnitprice) * item.Qty;
                        if (initial.HasValue) initial = Math.Round(initial.Value, 2, MidpointRounding.AwayFromZero);
                        if (negotiated.HasValue) negotiated = Math.Round(negotiated.Value, 2, MidpointRounding.AwayFromZero);
                    }

                    decimal finalPrice = initial ?? negotiated ?? 0;
                    decimal finalPrice1 = negotiated ?? initial ?? 0;
                    decimal priceDiff = finalPrice1 > finalPrice ? finalPrice1 - finalPrice : 0;

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
        public async Task<decimal> GetMfeeAsync(int enqid, decimal Mfee, CancellationToken cancellationToken = default)
        {
            if (Mfee != 0)
                return Mfee;

            var tblenqmaster = await instancedb.TblEnquiryMasters.AsNoTracking()
                .Where(x => x.Id == enqid)
                .Select(x => new { x.Managementfeetypeid, x.ClientId })
                .FirstOrDefaultAsync(cancellationToken);
            if (tblenqmaster == null)
                return 0;

            return await ResolveManagementFeeAsync(tblenqmaster.Managementfeetypeid, tblenqmaster.ClientId, null, cancellationToken) ?? 0;
        }

        /// <summary>
        /// Management fee %: the item's own non-zero value, otherwise the enquiry's management fee type,
        /// otherwise the client's default sales percentage.
        /// </summary>
        private async Task<decimal?> ResolveManagementFeeAsync(int? managementFeeTypeId, int? clientId, decimal? itemFeeUnit, CancellationToken cancellationToken)
        {
            if (itemFeeUnit != null && itemFeeUnit != 0)
                return itemFeeUnit;

            if (managementFeeTypeId != null)
                return await instancedb.Tblmanagmenttypes
                    .Where(x => x.Id == managementFeeTypeId).Select(x => x.Managementtypevalue).FirstOrDefaultAsync(cancellationToken);

            return Convert.ToDecimal(await instancedb.TblClientDetails
                .Where(x => x.Id == clientId).Select(x => x.DefaultSalesPercentage).FirstOrDefaultAsync(cancellationToken));
        }

        #region CalculateSavings
        [HttpPost("CalculateSavings")]
        public async Task<Result> CalculateSavings(Calculate_supplierQuotes req)
        {
            decimal ErouRate = 0;
            string? ManagementFeeMethod = "";
            decimal? ManagementFeeUnit = 0;
            decimal TotalManagementFee = 0;
            decimal? TaxPercentage = 0;
            try
            {
                ErouRate = Convert.ToDecimal(await instancedb.TblEnquiryMasters.Where(e => e.Id == req.Enqid)
                    .Join(instancedb.TblClientDetails, e => e.ClientId, c => c.Id, (e, c) => c.EuroRate).FirstOrDefaultAsync());

                var result = await (from m in instancedb.TblEnquiryMasters
                                    join d in instancedb.TblEnquiryDetails on m.Id equals d.EnquiryId
                                    join c in instancedb.TblCountries on m.PmgEntity equals c.Id
                                    where m.Id == req.Enqid
                                    select new
                                    {
                                        TaxPercentage = c.TaxPercentage,
                                        ManagementFeeUnit = d.ManagementFeeUnit,
                                        ManagementFeeMethod = d.ManagementFeeMethod
                                    }).FirstOrDefaultAsync();
                //&& (d.ManagementFeeUnit ?? 0) != 0

                if (result != null)
                {
                    TaxPercentage = Convert.ToDecimal(result.TaxPercentage);
                    ManagementFeeUnit = await GetMfeeAsync(req.Enqid, result.ManagementFeeUnit ?? 0);
                    ManagementFeeMethod = result.ManagementFeeMethod;
                }

                // Both procedures run in one transaction on the EF connection (retried as a unit on deadlock).
                // Parameters are passed as SqlParameter objects so their precision is taken from the values,
                // exactly as before (EF's default decimal mapping would round to 2 decimals).
                return await InTransactionAsync(async () =>
                {
                    decimal? Quoteprice = 0;
                    foreach (var item in req.Ls_getsupplierQuotes)
                    {
                        // Accumulate the enquiry total (was "= +", which kept only the last item's price)
                        Quoteprice += Convert.ToDecimal(item.NegotiatePrice);
                        var initialprice = item.InitialPrice + ((item.InitialPrice * ManagementFeeUnit) / 100);
                        await ExecuteProcedureAsync("UpdateSupplierForItem",
                            ("@ItemPrice", initialprice),
                            ("@itempriceNEG", item.NegotiatePrice),
                            ("@EnquiryId", req.Enqid),
                            ("@ItemNumber", item.Itemnumber),
                            ("@SupplierId", item.SupplierID),
                            ("@totalerouiteamprice", ErouRate));
                    }
                    Quoteprice = Quoteprice + (Quoteprice * (ManagementFeeUnit / 100));
                    decimal? TotalTaxAmount = Quoteprice * (TaxPercentage / 100);
                    decimal? TotalQuoteAmount = Quoteprice + TotalTaxAmount;

                    decimal? eurototalManagmentFee = TotalManagementFee * ErouRate;
                    decimal? EurototalQuotePrice = Quoteprice * ErouRate;
                    decimal? EurototalTaxamount = TotalTaxAmount * ErouRate;
                    decimal? EuroTotalQuoteAmount = TotalQuoteAmount * ErouRate;
                    await ExecuteProcedureAsync("UpdateClientQuoteToEnqDetails",
                        ("@EnquiryId", req.Enqid),
                        ("@QuotePrice", Quoteprice),
                        ("@ManagementFeeMethod", ManagementFeeMethod),
                        ("@ManagementFeeUnit", ManagementFeeUnit),
                        ("@TotalManagementFee", TotalManagementFee),
                        ("@TaxPercentage", TaxPercentage),
                        ("@TotalTaxAmount", TotalTaxAmount),
                        ("@TotalQuoteAmount", TotalQuoteAmount),
                        ("@Statuscul", "cul"),
                        ("@Status", 2),
                        ("@eurototalManagmentFee", eurototalManagmentFee),
                        ("@EurototalQuotePrice", EurototalQuotePrice),
                        ("@EurototalTaxamount", EurototalTaxamount),
                        ("@EuroTotalQuoteAmount", EuroTotalQuoteAmount));

                    return Result.Ok("Savings calculated successfully.");
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CalculateSavings failed for enquiry {EnquiryId}", req.Enqid);
                return Result.Fail(ex.Message);
            }
        }

        /// <summary>
        /// Executes a stored procedure on the context's connection and current transaction. Parameter types are
        /// inferred from the values (same as SqlParameterCollection.AddWithValue); null is sent as DBNull.
        /// </summary>
        private Task<int> ExecuteProcedureAsync(string procedure, params (string Name, object? Value)[] parameters)
        {
            var sql = $"EXEC {procedure} " + string.Join(", ", parameters.Select(p => $"{p.Name} = {p.Name}"));
            var sqlParameters = parameters.Select(p => (object)new SqlParameter(p.Name, p.Value ?? DBNull.Value)).ToArray();
            return instancedb.Database.ExecuteSqlRawAsync(sql, sqlParameters);
        }

        /// <summary>
        /// Runs a report procedure and loads every result set, fully asynchronously (replaces the blocking
        /// SqlDataAdapter.Fill). Column naming follows Fill: unnamed columns become "ColumnN" and duplicate
        /// names get a numeric suffix.
        /// </summary>
        private async Task<DataSet> ExecuteReportAsync(string procedureName, IEnumerable<(string Name, object? Value)> parameters, CancellationToken cancellationToken)
        {
            var ds = new DataSet();
            await using var con = new SqlConnection(_configuration.GetConnectionString("VADbConnectionString"));
            await using var cmd = new SqlCommand(procedureName, con)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = _database.ReportCommandTimeoutSeconds
            };
            foreach (var (name, value) in parameters)
                cmd.Parameters.AddWithValue(name, value ?? DBNull.Value);

            await con.OpenAsync(cancellationToken);
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            do
            {
                if (reader.FieldCount == 0)
                    continue;

                var table = new DataTable();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var baseName = string.IsNullOrEmpty(reader.GetName(i)) ? $"Column{i + 1}" : reader.GetName(i);
                    var name = baseName;
                    for (int suffix = 1; table.Columns.Contains(name); suffix++)
                        name = baseName + suffix;
                    table.Columns.Add(name, reader.GetFieldType(i));
                }

                var values = new object[reader.FieldCount];
                while (await reader.ReadAsync(cancellationToken))
                {
                    reader.GetValues(values);
                    table.Rows.Add(values);
                }
                ds.Tables.Add(table);
            }
            while (await reader.NextResultAsync(cancellationToken));

            return ds;
        }
        #endregion

        #region Post Reference price
        [HttpPost("PostRefprice")]
        public async Task<Result> PostRefprice(List<Ls_Store_Refprice> quotes)
        {
            Result rslt = new Result();
            try
            {
                // One query for all rows instead of one per row
                var ids = quotes.Select(q => q.Id).Distinct().ToList();
                var savingsById = await instancedb.TblSavingsDetails
                    .Where(x => ids.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id);

                foreach (var item in quotes)
                {
                    if (savingsById.TryGetValue(item.Id, out var saving))
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
                var enq = await instancedb.TblEnquiryMasters.FirstOrDefaultAsync(x => x.Id == req.EnqId);
                if (enq == null)
                {
                    rslt.Status = false;
                    rslt.Data = "Enquiry not found";
                    return rslt;
                }
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
                await instancedb.SaveChangesAsync();
                var statusResult = await UpdateJobStatus(new Jobstatus { EnqId = req.EnqId, ModifiedBy = req.ModifiedBy, Status = req.StatusId });
                if (!statusResult.Status)
                    return statusResult;
                rslt.Status = true;
                data = req.StatusId == 7 ? "Po number created successfully" : "Job Summary updated successfully";
            }
            else if (req.Action == "fee")
            {
                var enq = await instancedb.TblEnquiryDetails.FirstOrDefaultAsync(x => x.EnquiryId == req.EnqId);
                if (enq == null)
                {
                    rslt.Status = false;
                    rslt.Data = "Enquiry not found";
                    return rslt;
                }
                enq.ManagementFeeUnit = req.ManagementFee;
                await instancedb.SaveChangesAsync();
                var statusResult = await UpdateJobStatus(new Jobstatus { EnqId = req.EnqId, ModifiedBy = req.ModifiedBy, Status = req.StatusId });
                if (!statusResult.Status)
                    return statusResult;
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
            var enq = await instancedb.TblEnquiryDetails.FirstOrDefaultAsync(x => x.Id == req.Id);
            if (enq == null)
            {
                rslt.Status = false;
                rslt.Data = "Enquiry item not found";
                return rslt;
            }
            enq.Supplier2Name = req.SupplierB;
            enq.Supplier2amount = req.SupplierBNegAmount;
            enq.Supplier2Intilamount = req.SupplierBInitAmount;
            enq.Supplier3Name = req.SupplierC;
            enq.Supplier3amount = req.SupplierCNegAmount;
            enq.Supplier3Intilamount = req.SupplierCInitAmount;
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
                // 1. Enquiry (master) id. The old code loaded a TblEnquiryDetails row by this id and then compared
                //    its detail Id against master ids, so it only worked when both ids happened to coincide.
                int enquiryId = req.Enquiryid;

                // 2. Get Company Details
                var companyDetails = await (from em in instancedb.TblEnquiryMasters
                                            join division in instancedb.TblDivisions on em.DivisionId equals division.Id
                                            join clientDetails in instancedb.TblClientDetails on em.ClientId equals clientDetails.Id
                                            join client in instancedb.TblClients on em.ClientContactId equals client.Id
                                            join pmgCountry in instancedb.TblCountries on em.PmgEntity equals pmgCountry.Id
                                            join user in instancedb.TblAgencyUsers on em.CreatedBy equals user.Id
                                            where em.Id == enquiryId && em.JobType == GetMenuName(1)
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

                if (companyDetails == null)
                {
                    return NotFound(new
                    {
                        Status = false,
                        Message = "Enquiry not found"
                    });
                }

                // 3. Get quotation items
                var items = await instancedb.TblEnquiryDetails.Where(x => x.EnquiryId == enquiryId && x.SupplierId != 0).OrderBy(x => x.Id).ToListAsync();

                // 4. Build items HTML
                StringBuilder itemHtml = new StringBuilder();
                itemHtml.Append(@" <table width='100%' border='1' cellspacing='0' cellpadding='0'
                           style='font-family:Franklin Gothic Book,Arial;font-size:12px;color:black;'>
                    <tbody>");

                foreach (var item in items)
                {
                    decimal.TryParse(item.QuoteQtyOrSize, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal qty);
                    decimal price = item.ItemSellPrice ?? 0;
                    decimal unitPrice = qty > 0 ? Math.Round(price / qty, 4) : 0;

                    // User-entered text is HTML-encoded: wkhtmltopdf renders raw HTML (incl. iframes/scripts)
                    itemHtml.Append($@"
                    <tr>
                        <td width='65%' align='left' style='border:1px solid black; padding:3px;'>
                            {Html(item.ItemDescription)}
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

                // 7. Read HTML (Thailand/Vietnam templates are referenced but not shipped: fall back to the default)
                string templatePath = Path.Combine(_hostingEnvironment.ContentRootPath, "PrintTemplates", template);
                if (!System.IO.File.Exists(templatePath))
                {
                    _logger.LogWarning("Quotation template {Template} not found, using default", template);
                    templatePath = Path.Combine(_hostingEnvironment.ContentRootPath, "PrintTemplates", "Quatation.html");
                }
                string html = await System.IO.File.ReadAllTextAsync(templatePath);

                // 8. Replace quotation values
                html = html.Replace("[CompanyEntityName]", Html(companyDetails.CompanyEntityName))
                    .Replace("[ClientContact]", Html(companyDetails.ClientContactName))
                    .Replace("[Address1]", Html(companyDetails.AddressLine1))
                    .Replace("[Address2]", Html(companyDetails.AddressLine2))
                    .Replace("[Address3]", Html(companyDetails.AddressLine3))
                    .Replace("[Address4]", Html(companyDetails.AddressLine4))
                    .Replace("[EntityCity]", Html(companyDetails.City))
                    .Replace("[EntityCountry]", Html(country))
                    .Replace("[ProjectName]", Html(companyDetails.ProjectDesc))
                    .Replace("[ProjectNo]", Html(companyDetails.ProjectNo))
                    .Replace("[PaymentTerms]", Html(companyDetails.PaymentTerm))
                    .Replace("[PMGENTITYNAME]", Html(companyDetails.PmgCountry))
                    .Replace("[PMGCREATORUSER]", Html(companyDetails.AgencyUserName))
                    .Replace("[PMGCREATORUSEREMAIL]", Html(companyDetails.AgencyUserEmail))
                    .Replace("[PMGCREATORUSERPHONECONTACT]", Html(companyDetails.AgencyUserPhone))
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
                var fileRef = string.IsNullOrWhiteSpace(companyDetails.ProjectNo) ? enquiryId.ToString() : companyDetails.ProjectNo;
                var safeFileRef = string.Concat(fileRef.Split(Path.GetInvalidFileNameChars()));
                return File(pdf, "application/pdf", $"Quotation_{safeFileRef}.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetPreviewQuotes failed for enquiry {EnquiryId}", req.Enquiryid);
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
        public async Task<IActionResult> GetReport(GetReport request, CancellationToken cancellationToken)
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

                var parameters = new List<(string Name, object? Value)>
                {
                    ("@StartDate", request.StartDate),
                    ("@EndDate", request.EndDate),
                    ("@Role", request.Role),
                    ("@Type", request.ReportType),
                };
                // EB_NewGenerateReport ("All" clients) has no @clientname parameter; sending it made that report always fail
                if (procedureName != "EB_NewGenerateReport")
                {
                    parameters.Add(("@clientname", request.ClientName));
                }
                if ((request.Role == "Administration" || request.Role == "ClientServices") && request.UserClientId > 0)
                {
                    parameters.Add(("@UserClientId", request.UserClientId));
                }

                if (request.UserType == "Client")
                {
                    parameters.Add(("@countryId", request.CountryId));
                    parameters.Add(("@Businessunit", request.BusinessUnit));
                }

                DataSet ds = await ExecuteReportAsync(procedureName, parameters, cancellationToken);

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
                    DataTable exportTable = source;
                    bool noData = source.Rows.Count == 0;

                    if (exportTable.Rows.Count == 0 && exportTable.Columns.Count > 0)
                    {
                        // The placeholder text needs a text column (a numeric first column used to throw here)
                        exportTable.Columns[0].DataType = typeof(string);
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
                        if (noData)
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
                _logger.LogError(ex, "GetReport failed ({ReportType}, {ClientName})", request.ReportType, request.ClientName);
                return StatusCode(500, new
                {
                    Status = false,
                    Message = ex.Message
                });
            }
        }
        #endregion

        #region UpdateSurvey

        [HttpPost("UpdateSurvey")]
        public async Task<Result> UpdateSurvey([FromBody] SendSurveyMailRequest request)
        {
            Result rslt = new Result();
            try
            {

                string connectionString = _configuration["ConnectionStrings:VADbConnectionString"];
                using var con = new SqlConnection(connectionString);
                using var cmd = new SqlCommand("UPDATE tblEnquiryMaster SET Surveynotification = @SurveyNotification WHERE Id = @EnquiryId", con);
                cmd.Parameters.Add("@SurveyNotification", SqlDbType.VarChar, 3).Value = "YES";
                cmd.Parameters.Add("@EnquiryId", SqlDbType.Int).Value = request.EnqId;
                await con.OpenAsync();
                int rows = await cmd.ExecuteNonQueryAsync();
                if (rows == 0)
                {
                    rslt.Status = false;
                    rslt.Data = "Enquiry not found.";
                    return rslt;
                }
                SendMailSurvey(request);
                rslt.Status = true;
                rslt.Data = "Survey notification updated successfully.";
            }
            catch (Exception ex)
            {
                rslt.Status = false;
                rslt.Data = ex.InnerException?.Message ?? ex.Message;
            }
            return rslt;

        }

        [HttpPost("SendMailSurvey")]
        public async Task<Result> SendMailSurvey([FromBody] SendSurveyMailRequest request)
        {
            Result rslt = new Result();
            try
            {
                string? cdate = await GetCompletateDate(request.EnqId);
                string country = request.Country?.Trim() ?? "";

                string template = country == "Japan" ? "SurveyMonkey_JP.html" : "SurveyMonkey.html";
                string templatePath = Path.Combine(_hostingEnvironment.ContentRootPath, "EmailTemplates", template);
                string emailTemplate = await System.IO.File.ReadAllTextAsync(templatePath);
                //string surveyPage = country == "Japan" ? "Survey_JP.aspx" : "Survey.aspx";
                //string surveyUrl = $"{request.RootPath.TrimEnd('/')}/PerformanceSurvey/Survey/{surveyPage}?Id={request.EnqId}";
                string clientName = string.IsNullOrWhiteSpace(request.ClientName) ? "Client" : request.ClientName;
                string emailAddress = request.EmailAddress ?? "";

                string countryEmail = country switch
                {
                    "Thailand" => "Piyanuch.Jeerapornbundit@th.nestle.com",
                    "Indonesia" => "Yurieca.Garnis@id.nestle.com",
                    "Philippines" => "Maritess.Deguzman@ph.nestle.com",
                    "Singapore" => "NurJannatulfirdaus.Mdsaid@my.nestle.com",
                    "Japan" => "Kaori.ishikawa@jp.nestle.com",
                    "Australia" or "New Zealand" => "Matthew.Rolfe@au.nestle.com",
                    _ => ""
                };
                if (!string.IsNullOrWhiteSpace(countryEmail))
                {
                    emailAddress = string.IsNullOrWhiteSpace(emailAddress) ? countryEmail : emailAddress.TrimEnd(',', ';') + "," + countryEmail;
                }

                emailTemplate = emailTemplate
                    //.Replace("$$ClientUrl$$", surveyUrl)
                    .Replace("$$PROJECTNO$$", request.ProjectNumber ?? "")
                    .Replace("$$PROJECTDESCRIPTION$$", request.ProjectDescription ?? "")
                    .Replace("$$ClientName$$", clientName)
                    .Replace("$$enqid$$", request.EnquiryNumber ?? "")
                    .Replace("$$cdate$$", cdate ?? "");

                bool sent = await _emailService.SendNotificationEmail(_httpContextAccessor.HttpContext, "PMG Performance (1 minute survey) | Your Valuable Feedback Required", emailTemplate, emailAddress);
                rslt.Status = sent;
                rslt.Data = sent ? "Survey email sent successfully." : "Unable to send survey email.";
            }
            catch (Exception ex)
            {
                rslt.Status = false;
                rslt.Data = ex.InnerException?.Message ?? ex.Message;
            }
            return rslt;
        }

        [NonAction]
        public async Task<string?> GetCompletateDate(int enqId)
        {
            try
            {
                var connectionString = _configuration["ConnectionStrings:VADbConnectionString"];

                using var con = new SqlConnection(connectionString);
                using var cmd = new SqlCommand(@"SELECT TOP 1 ModifiedDate FROM tblJobHistory WHERE EnqId = @Id AND NewValue = '24' ORDER BY ModifiedDate DESC", con);
                cmd.Parameters.Add("@Id", SqlDbType.Int).Value = enqId;
                await con.OpenAsync();
                object result = await cmd.ExecuteScalarAsync();
                if (result == null || result == DBNull.Value) return null;
                DateTime modifiedDate = Convert.ToDateTime(result);
                DateTime nextMonth = modifiedDate.AddMonths(1);
                return new DateTime(nextMonth.Year, nextMonth.Month, 1).ToString("dd/MM/yyyy");
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        #endregion
    }

}
