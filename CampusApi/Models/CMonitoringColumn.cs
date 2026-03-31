using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampusApi.Models
{
    // Колонка у відомості (конкретне завдання з Google Classroom)
    [Table("cmonitoringcolumn")]
    public class CMonitoringColumn
    {
        [Key] public int CMonitoringColumnId { get; set; }
        public int CMonitoringEmployeeId { get; set; }
        public int DcLoadId { get; set; } // 26 - Завдання Google класу за ТЗ
        public DateTime DateMark { get; set; }
        public string Description { get; set; } // Назва завдання
    }
}