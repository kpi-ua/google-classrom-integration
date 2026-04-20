using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampusApi.Models
{
    [Table("eemployees1")]
    public class EEmployee
    {
        [Key]
        public int EEmployees1Id { get; set; }

        public int? UserAccountId { get; set; }
    }
}