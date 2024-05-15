
namespace GameHubShared.Models;

public class StatisticModel {
    public DateTime Date { get; set; }
    public string Game { get; set; }
    public bool Won { get; set; }
    public string Opponent { get; set; }

    public StatisticModel(DateTime date, string game, bool won, string opponent) {
        Date = date;
        Game = game;
        Won = won;
        Opponent = opponent;
    }
}