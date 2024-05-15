using System.ComponentModel.DataAnnotations;

namespace GameHubApi.DataBase;
public class Statistic
{
    [Key]
    public int Id { get; set; }

    [Required]
    public DateTime Date { get; set; }

    [Required]
    public virtual Game Game { get; set; }

    [Required]
    public bool Won { get; set; }

    public string Opponent { get; set; }
}
