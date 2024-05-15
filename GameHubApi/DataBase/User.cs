using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace GameHubApi.DataBase;

[Table("User")]
public class User
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Password { get; set; }

    [Required]
    public string UserName { get; set; }
    public virtual ICollection<Statistic>? Statistics { get; set; }
}
