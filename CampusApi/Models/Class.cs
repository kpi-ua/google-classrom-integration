using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampusApi.Models
{
    // Таблица студентов (упрощенно)
    [Table("spersonality1")]
    public class SPersonality
    {
        [Key] public int Id { get; set; }
        public string STUDENT_ID { get; set; }
        public string FIRST_NAME { get; set; }
        public string LAST_NAME { get; set; }
    }

    // Связь аккаунта и контактов (здесь хранится GoogleId / Email)
    [Table("usercontact")]
    public class UserContact
    {
        [Key] public int UserContactId { get; set; }
        public int UserAccountId { get; set; }
        public int DcContactTypeId { get; set; } // 29 - это GoogleId по ТЗ (стр. 12)
        public string UserContactValue { get; set; } // Тут будет email или google id студента
        public int SPersonalityId { get; set; } // Связь со студентом (упрощение для примера)
    }

    // Студент в рамках ведомости
    [Table("cmonitoringstudent")]
    public class CMonitoringStudent
    {
        [Key] public int CMonitoringStudentId { get; set; }
        public int CMonitoringEmployeeId { get; set; }
        public int SPersonalityId { get; set; }
    }

    // Колонка в ведомости (Задание / Итоговая оценка)
    [Table("cmonitoringcolumn")]
    public class CMonitoringColumn
    {
        [Key] public int CMonitoringColumnId { get; set; }
        public int CMonitoringEmployeeId { get; set; }
        public int DcLoadId { get; set; } // 26 - Завдання Google класу
        public DateTime DateMark { get; set; }
        public string Description { get; set; }
    }

    // Сама оценка (Ячейка на пересечении студента и колонки)
    [Table("cmonitoringrow")]
    public class CMonitoringRow
    {
        [Key] public int CMonitoringRowId { get; set; }
        public int CMonitoringColumnId { get; set; }
        public int SPersonalityId { get; set; }
        public int CMonitoringStudentId { get; set; }
        public float? Mark { get; set; } // Итоговая сумма балов
        public char VcActuality { get; set; } = '1';
        public DateTime VcChangeDate { get; set; } = DateTime.Now;
    }
}