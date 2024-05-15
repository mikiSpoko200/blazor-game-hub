using System.Security.Cryptography;

namespace GameHubShared.Models;

public interface IGame
{
    public static int MaxPlayers { get; set; }
}

public class EnumReference
{
    public Mark Mark { get; set; }
}

public enum Mark
{
    Empty,
    Cross,
    Circle,
}

public enum Status {
    Over,
    InProgress,
    Interrupted
}

public class GameModel : IGame
{
    public static int MaxPlayers { get; set; } = 2;

    public string Id { get; set; }
    public string Name { get; set; }
    public int CurrentPlayerIndex { get; set; }
    public List<UserModel> Players { get; set; }
    public List<Mark> PlayerMarks { get; set; } = new();
    public List<Mark> Board { get; set; } = new();
    public Status Status { get; set; }
    public GameModel(string id, string name, List<UserModel> players) {
        Id = id;
        Name = name;
        Players = players;

        for (var i = 0; i < 9; i++) {
            Board.Add(Mark.Empty);
        }
        Status = Status.InProgress;
        var rng = new Random();
        CurrentPlayerIndex = rng.Next(0, MaxPlayers - 1);

        var mark = rng.Next(0, 1) == 0 ? Mark.Cross : Mark.Circle;
        PlayerMarks.Add(mark);
        PlayerMarks.Add(mark == Mark.Cross ? Mark.Circle : Mark.Cross);
    }

    public void ToggleCurrentPlayer() {
        CurrentPlayerIndex = (CurrentPlayerIndex + 1) % MaxPlayers;
    }

    /// <summary>
    ///  Place a mark on the board if it's the player's turn and the cell is empty.
    /// </summary>
    /// <param name="user">User that performs the action</param>
    /// <param name="cell">Cell to place the mark</param>
    /// <returns>true if the mark was placed, false otherwise.</returns>
    public bool Place(UserModel user, int cell) {
        if (cell >= 9) { return false; }
        if (Board[cell] != Mark.Empty) { return false; }
        if (Players[CurrentPlayerIndex].UserName != user.UserName) { return false; }

        Board[cell] = PlayerMarks[CurrentPlayerIndex];

        return true;
    }

    public bool WasWinningMove(int cell) {
        var row = cell / 3;
        var col = cell % 3;

        // Check row
        if (Board[row * 3] == Board[row * 3 + 1] && Board[row * 3] == Board[row * 3 + 2]) { return true; }
        // Check column
        if (Board[col] == Board[col + 3] && Board[col] == Board[col + 6]) { return true; }
        // Check diagonals
        if (cell % 2 == 0) {
            if (Board[0] != Mark.Empty && Board[0] == Board[4] && Board[0] == Board[8]) { return true; }
            if (Board[2] != Mark.Empty && Board[2] == Board[4] && Board[2] == Board[6]) { return true; }
        }
        return false;
    }

    public bool IsDraw() {
        return Board.All(cell => cell != Mark.Empty);
    }

    public void Reset() {
        Board.Clear();
        for (var i = 0; i < 9; i++) {
            Board.Add(Mark.Empty);
        }

        Status = Status.InProgress;
    }

    public void ProcessMove(UserModel user, int cell) {
        if (Status != Status.InProgress) { return; }
        if (Place(user, cell)) {
            if (WasWinningMove(cell)) {
                Status = Status.Over;
            } else if (IsDraw()) {
                Status = Status.Over;
            } else {
                ToggleCurrentPlayer();
            }
        }
    }
}
