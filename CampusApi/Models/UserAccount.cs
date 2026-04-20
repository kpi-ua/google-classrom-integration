using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampusApi.Models
{
    [Table("useraccount")]
    public class UserAccount
    {
        [Key]
        public int UserAccountId { get; set; }

        public string Gmail { get; set; } = string.Empty;
    }
}