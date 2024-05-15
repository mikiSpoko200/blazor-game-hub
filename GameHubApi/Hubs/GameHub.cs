using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using GameHubApi.DataBase;
using GameHubShared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace GameHubApi.Hubs;

[Authorize]
public class GameHub : Hub<IGameHubClient>
{
    private List<LobbyModel> lobbies;
    private Context context;
    private bool storeStats = true;

    public GameHub(List<LobbyModel> lobbies, Context context)
    {
        this.context = context;
        this.lobbies = lobbies;
    }

    private UserModel GetUser() {
        var jwtHandler = new JwtSecurityTokenHandler();
        var auth = Context.GetHttpContext()!
            .Request
            .Headers["Authorization"];
        var jwtInput = auth.ToString().Replace("Bearer ", string.Empty);
        var token = jwtHandler.ReadJwtToken(jwtInput);
        var Name = token.Claims.First(claim => claim.Type == ClaimTypes.Name).Value;
        var Authority = token.Claims.First(claim => claim.Type == ClaimTypes.Role).Value;

        return new UserModel(Name, Authority);
    }

    public async Task PlayAgain(Guid lobbyId) {
        var lobby = (InGameLobbyModel)lobbies.Find(lobby => lobby.Id == lobbyId)!;
        lobby.CurrentGame.Reset();
        await Clients.Group(lobbyId.ToString()).UpdateInGameLobbyState(lobby);
        storeStats = true;
    }

    public async Task Place(Guid lobbyId, int tileIndex) {
        var lobby = (InGameLobbyModel)lobbies.Find(lobby => lobby.Id == lobbyId)!;
        if (lobby is null) return;
        var user = GetUser();
        lobby.CurrentGame.ProcessMove(user, tileIndex);
        if (lobby.CurrentGame.Status == Status.Over && storeStats) {
            storeStats = false;
            var game = context.Games!.SingleOrDefault(game => game.Name == lobby.GameName)!;
            var oponent = lobby.Players.Find(player => player.UserName != user.UserName)!;

            if (user.Authority == "LoggedInUser") {
                var dbUser = context.Users!.SingleOrDefault(u => u.UserName == user.UserName)!;
                var newStatistic = new Statistic
                {
                    Date = DateTime.Now,
                    Game = game,
                    Won = true,
                    Opponent = oponent.UserName
                };
                dbUser.Statistics ??= new List<Statistic>();
                dbUser.Statistics.Add(newStatistic);
            }
            
            if (oponent.Authority == "LoggedInUser") {
                var dbUser = context.Users!.SingleOrDefault(user => user.UserName == oponent.UserName)!;
                oponent = user;
                var newStatistic = new Statistic
                {
                    Date = DateTime.Now,
                    Game = game,
                    Won = false,
                    Opponent = oponent.UserName
                };
                dbUser.Statistics ??= new List<Statistic>();
                dbUser.Statistics.Add(newStatistic);
            }
            context.SaveChanges();
        }

        await Clients.Group(lobbyId.ToString()).UpdateInGameLobbyState(lobby);
    }

    public async Task JoinLobby(Guid lobbyId) {
        var user = GetUser();
        var lobby = lobbies.Find(lobby => lobby.Id == lobbyId);

        if (lobby is WaitingLobbyModel waitingLobby && !waitingLobby.IsFull()) {
            WaitingLobbyModel waitingLobbyModel = (WaitingLobbyModel)lobby;
            waitingLobbyModel.AddPlayer(user);
            waitingLobbyModel.NCurrentPlayers++;

            await Groups.AddToGroupAsync(Context.ConnectionId, lobbyId.ToString());

            if (waitingLobby.IsFull()) {
                lobbies.Remove(lobby);
                var gameLobby = new InGameLobbyModel(lobby.Id, lobby.MaxPlayers, lobby.GameName, lobby.Description, lobby.Players);
                lobbies.Add(gameLobby);
                await Clients.Group(lobbyId.ToString()).UpdateInGameLobbyState(gameLobby);
            } else {
                await Clients.Group(lobbyId.ToString()).UpdateWaitingLobbyState(waitingLobbyModel);
            }
            
        }
    }

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var user = GetUser();
        var lobby = lobbies.Find(lobby => lobby.Players.Exists(player => player.UserName == user.UserName));

        var player = lobby!.Players.Find(player => player.UserName == user.UserName);

        // TODO: update statistics for the player


        lobby!.RemovePlayer(player!);
        lobby!.NCurrentPlayers--;
        
        // check if player leaves lobby normally or if the game is interrupted
        if (lobby is InGameLobbyModel) {
            ((InGameLobbyModel)lobby).CurrentGame.Status = Status.Interrupted;
            await Clients.Group(lobby.Id.ToString()).UpdateInGameLobbyState((InGameLobbyModel)lobby);
        } else {
            await Clients.Group(lobby.Id.ToString()).UpdateWaitingLobbyState((WaitingLobbyModel)lobby);
        }

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, lobby.Id.ToString());

        // Delete the lobby is all players left
        if (lobby.NCurrentPlayers == 0) {
            lobbies.Remove(lobby);
        }

        await base.OnDisconnectedAsync(exception);
    }
}

public interface IGameHubClient {
    Task UpdateWaitingLobbyState(WaitingLobbyModel lobby);
    Task UpdateInGameLobbyState(InGameLobbyModel lobby);
    Task EndGame(InGameLobbyModel lobby);
}