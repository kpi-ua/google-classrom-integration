using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampusApi.Models
{
    // Зв'язок акаунта та контактів (тут зберігається GoogleId / Email)
    [Table("usercontact")]
    public class UserContact
    {
        [Key] public int UserContactId { get; set; }
        public int UserAccountId { get; set; }
        public int DcContactTypeId { get; set; } // 29 - це GoogleId (Email) за ТЗ
        public string UserContactValue { get; set; }
        public int SPersonalityId { get; set; } // Зв'язок зі студентом
    }
}