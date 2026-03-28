using Microsoft.EntityFrameworkCore;
using CampusApi.Models;

namespace CampusApi.Data
{
    public class CampusDbContext : DbContext
    {
        public CampusDbContext(DbContextOptions<CampusDbContext> options) : base(options) { }

        public DbSet<SPersonality> Personalities { get; set; }
        public DbSet<UserContact> UserContacts { get; set; }
        public DbSet<CMonitoringStudent> MonitoringStudents { get; set; }
        public DbSet<CMonitoringColumn> MonitoringColumns { get; set; }
        public DbSet<CMonitoringRow> MonitoringRows { get; set; }
    }
}