using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InternalService.Administrative
{
    public class Post
    {
        [Key]
        [Required]
        public string DiscordId { get; set; }
        [Required]
        public int Position { get; set; }
        [Required]
        public string GameRole { get; set; }
        [Required]
        public string Name { get; set; }
        public virtual ICollection<Employee> Employees { get; set; }
        public Post()
        {
            Employees = new List<Employee>();
        }
    }
}