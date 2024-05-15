using GameHubShared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GameHubApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class LobbiesController : ControllerBase
{
    private List<LobbyModel> lobbies;
    
    public LobbiesController(List<LobbyModel> lobbies)
    {
        this.lobbies = lobbies;
    }

    [HttpGet]
    public IEnumerable<LobbyModel> Get()
    {
        return lobbies;
    }

    [HttpGet("{id}")]
    public LobbyModel? Get(Guid id)
    {
        return lobbies.FirstOrDefault(lobby => lobby.Id == id);
    }

    [HttpPost]
    public void Post([FromBody] WaitingLobbyModel lobby)
    {
        lobbies.Add(lobby);
    }
}
