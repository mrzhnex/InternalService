using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InternalService.Administrative
{
    public class Employee
    {
        [Key]
        [Required]
        public string DiscordId { get; set; }
        [Required]
        public string SteamId { get; set; }
        [Required]
        public string Name { get; set; }
        [ForeignKey("Post")]
        public string PostId { get; set; }
        public virtual Post Post { get; set; }
    }
}