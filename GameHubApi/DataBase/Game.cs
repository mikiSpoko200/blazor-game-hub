using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameHubApi.DataBase;

[Table("Game")]
public class Game
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Name { get; set; }
}

