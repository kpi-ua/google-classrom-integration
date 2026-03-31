using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampusApi.Models
{
    // Студент у рамках конкретної відомості викладача
    [Table("cmonitoringstudent")]
    public class CMonitoringStudent
    {
        [Key] public int CMonitoringStudentId { get; set; }
        public int CMonitoringEmployeeId { get; set; } // ID відомості (наприклад, 999)
        public int SPersonalityId { get; set; }
    }
}