using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampusApi.Models
{
    // Таблиця студентів (спрощено)
    [Table("spersonality1")]
    public class SPersonality
    {
        [Key] public int Id { get; set; }
        public string STUDENT_ID { get; set; }
        public string FIRST_NAME { get; set; }
        public string LAST_NAME { get; set; }
    }
}