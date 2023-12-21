using Microsoft.AspNetCore.Mvc;
using MongoAuction.Services;

namespace MongoAuction.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    private readonly MongoDBService _mongoDBService;

    public UserController(MongoDBService mongoDBService)
    {
        _mongoDBService = mongoDBService;
    }

    [HttpGet("All")]
    public async Task<IEnumerable<string>> GetUsersAsync()
    {
        return await _mongoDBService.GetUsersAsync()
            .ConfigureAwait(false);
    }

    [HttpGet("SignIn/{username}")]
    public async Task<string> SignInAsync(string username)
    {
        return await _mongoDBService.GetFakeJWTAsync(username)
            .ConfigureAwait(false);
    }

    [HttpPost("SignUp")]
    public async Task<string> SignUpAsync([FromBody] string username)
    {
        return await _mongoDBService.CreateUserAsync(username)
            .ConfigureAwait(false);
    }

    [HttpDelete]
    public async Task<IActionResult> RemoveAsync([FromBody] string username)
    {
        await _mongoDBService.RemoveUserAsync(username)
            .ConfigureAwait(false);
        return Ok();
    }
}