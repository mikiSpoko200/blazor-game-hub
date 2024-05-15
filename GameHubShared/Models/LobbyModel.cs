using System.Text.Json.Serialization;

namespace GameHubShared.Models;

public class LobbyModel
{
    public Guid Id { get; set; }
    public string? Description { get; set; }
    public string GameName { get; set; }
    public int MaxPlayers { get; set; }
    public int NCurrentPlayers { get; set; }
    public List<UserModel> Players { get; set; } = new();

    [JsonConstructor]
    public LobbyModel(Guid id, int nCurrentPlayers, int maxPlayers, string gameName, string? description, List<UserModel> players)
    {
        Id = id;
        NCurrentPlayers = nCurrentPlayers;
        MaxPlayers = maxPlayers;
        GameName = gameName;
        Description = description;
        Players = players;
    }
    
    public void RemovePlayer(UserModel player)
    {
        Players.Remove(player);
    }
}

public class LobbyFullException : Exception
{
    public LobbyFullException(string message) : base(message) { }
}

public class WaitingLobbyModel : LobbyModel
{
    
    [JsonConstructor]
    public WaitingLobbyModel(Guid id, int maxPlayers, string gameName, string? description) : base(id, 0, maxPlayers, gameName, description, new())
    {
    }
    
    public bool IsFull() => Players.Count >= MaxPlayers;
    
    public void AddPlayer(UserModel player)
    {
        if (Players.Count < MaxPlayers)
        {
            Players.Add(player);
        } else {
            throw new LobbyFullException("The lobby is full.");
        }
    }
}

public class InGameLobbyModel : LobbyModel
{
    public GameModel CurrentGame { get; set; }

    [JsonConstructor]
    public InGameLobbyModel(
        Guid id, 
        int maxPlayers, 
        string gameName, 
        string? description, 
        List<UserModel> players
    ) : base(id, maxPlayers, maxPlayers, gameName, description, players)
    {
        Id = id;
        string gameId = Guid.NewGuid().ToString();
        Players = players;
        CurrentGame = new GameModel(gameId, gameName, players);
    }
}
