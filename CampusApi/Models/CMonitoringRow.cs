using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampusApi.Models
{
    // Сама оцінка (Комірка на перетині студента та колонки-завдання)
    [Table("cmonitoringrow")]
    public class CMonitoringRow
    {
        [Key] public int CMonitoringRowId { get; set; }
        public int CMonitoringColumnId { get; set; }
        public int SPersonalityId { get; set; }
        public int CMonitoringStudentId { get; set; }
        public float? Mark { get; set; } // Виставлений бал
        public char VcActuality { get; set; } = '1';
        public DateTime VcChangeDate { get; set; } = DateTime.Now;
    }
}