using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampusApi.Models
{
    [Table("cmonitoring")]
    public class CMonitoring
    {
        [Key]
        public int CMonitoringId { get; set; }

        public int CRNPRowId { get; set; }
        public int RtStudyGroupId { get; set; }
        public char VcActuality { get; set; } = '1';
        public DateTime VcChangeDate { get; set; } = DateTime.Now;
        public int? SemesterYear { get; set; }
        public string? GoogleCourseId { get; set; }
    }
}