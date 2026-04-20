using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampusApi.Models
{
    [Table("cmonitoringemployee")]
    public class CMonitoringEmployee
    {
        [Key]
        public int CMonitoringEmployeeId { get; set; }

        public int CMonitoringId { get; set; }
        public int EEmployees1Id { get; set; }
        public string? Name { get; set; }
        public char VcActuality { get; set; } = '1';
        public DateTime VcChangeDate { get; set; } = DateTime.Now;
    }
}