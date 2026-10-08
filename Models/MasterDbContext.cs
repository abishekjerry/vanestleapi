using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace VAapiNestle.Models
{
    public class MasterDbContext : DbContext
    {
        public MasterDbContext(DbContextOptions<MasterDbContext> options)
            : base(options)
        {
        }

        // Example table
        public virtual DbSet<Tblinstancedetails> Tblinstancedetailss { get; set; }
        public virtual DbSet<tblsapdivision> Tblsapdivisions { get; set; }
        public virtual DbSet<tblCountry> TblCountries { get; set; }
        public virtual DbSet<ApiEventQueue> ApiEventQueues { get; set; }
        public virtual DbSet<tblLanguage> TblLanguages { get; set; }
        [Table("ApiEventQueue")]
        public class ApiEventQueue
        {
            public int Id { get; set; }
            public string DbName { get; set; }
            public string TableName { get; set; }
            public int RecordId { get; set; }
            public int Status { get; set; }
            public DateTime CreatedAt { get; set; }
            public bool IsProcessed { get; set; }
            public bool? API_status { get; set; }
            public bool? Initiated { get; set; }
            public string API_response { get; set; }
            public bool? API_1 { get; set; }
            public bool? API_2 { get; set; }
            public bool? API_3 { get; set; }
            public bool? API_4 { get; set; }
        }
        public class Tblinstancedetails
        {
            public int Id { get; set; }
            public string Instancename { get; set; }
            public string VAInstance { get; set; }
            public string Dbname { get; set; }
            public bool Status { get; set; }
        }
        [Table("tblsapdivision")]
        public class tblsapdivision
        {
            public int Id { get; set; }
            public int Divisioncode { get; set; }
            public string division { get; set; }
            public bool status { get; set; }
        }
        [Table("tblCountry")]
        public class tblCountry
        {
            public int Id { get; set; }
            public string Country { get; set; }
            public string code { get; set; }
            public string Currency { get; set; }
            public string Language { get; set; }
            public string Regionalsetting { get; set; }
            public string OrganizationCode { get; set; }
            public string Distributioncode { get; set; }
            public string Companycode { get; set; }
            public string Profitcenter { get; set; }
            public string Costcenter { get; set; }
            public bool status { get; set; }
        }
        [Table("tblLanguage")]
        public class tblLanguage
        {
            public int Id { get; set; }
            public string ResourceKey { get; set; }
            public string English { get; set; }
            public string Japanese { get; set; }
            public string Korea { get; set; }
            public string Chinese { get; set; }
            public string Vietnamese { get; set; }
            public string Taiwan { get; set; }
            public string Thai { get; set; }
            public string Indonesia { get; set; }
            public bool Status { get; set; }
        }
    }
}
