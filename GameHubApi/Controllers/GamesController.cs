﻿using Microsoft.AspNetCore.Mvc;

namespace GameHubApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GamesController : ControllerBase
{
    // GET: api/<GamesController>
    [HttpGet]
    public IEnumerable<string> Get()
    {
        return new string[] { "value1", "value2" };
    }

    // GET api/<GamesController>/5
    [HttpGet("{id}")]
    public string Get(int id)
    {
        return "value";
    }

    // POST api/<GamesController>
    [HttpPost]
    public void Post([FromBody] string value)
    {
    }

    // PUT api/<GamesController>/5
    [HttpPut("{id}")]
    public void Put(int id, [FromBody] string value)
    {
    }

    // DELETE api/<GamesController>/5
    [HttpDelete("{id}")]
    public void Delete(int id)
    {
    }
}
