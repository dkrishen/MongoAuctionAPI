using Microsoft.AspNetCore.Mvc;
using MongoAuction.Models;
using MongoAuction.Services;

namespace MongoAuction.Controllers;

[ApiController]
[Route("[controller]")]
public class LotController : ControllerBase
{
    private readonly MongoDBService _mongoDBService;

    public LotController(MongoDBService mongoDBService)
    {
        _mongoDBService = mongoDBService;
    }

    [HttpGet("All")]
    public async Task<IEnumerable<string>> GetLotsAsync()
    {
        return await _mongoDBService.GetLotsAsync()
            .ConfigureAwait(false);
    }

    [HttpGet("ByCategory/{category}")]
    public async Task<IEnumerable<string>> LotsByCategoryAsync(string category)
    {
        return await _mongoDBService.GetLotsByCategoryAsync(category)
            .ConfigureAwait(false);
    }

    [HttpGet("ByUser/{username}")]
    public async Task<IEnumerable<string>> LotsByUserAsync(string username)
    {
        return await _mongoDBService.GetLotsByUserAsync(username)
            .ConfigureAwait(false);
    }

    [HttpGet("My/{token}")]
    public async Task<IEnumerable<string>> MyLotsAsync(string token)
    {
        return await _mongoDBService.GetLotsByTokenAsync(token)
            .ConfigureAwait(false);
    }

    [HttpGet("MyBids/{token}")]
    public async Task<IEnumerable<string>> MyBidsAsync(string token)
    {
        return await _mongoDBService.GetLotsByBidderTokenAsync(token)
            .ConfigureAwait(false);
    }

    [HttpGet("ByTitle/{title}")]
    public async Task<Lot> LotByIdAsync(string title)
    {
        return await _mongoDBService.GetLotByTitleAsync(title)
            .ConfigureAwait(false);
    }

    [HttpPut("Create/{token}")]
    public async Task<IActionResult> CreateLotAsync([FromBody] LotInputDto lot, string token)
    {
        await _mongoDBService.CreateLotAsync(lot, token)
             .ConfigureAwait(false);
        return Ok();
    }

    [HttpPut("Remove/{token}")]
    public async Task<IActionResult> RemoveLotAsync([FromBody] string id, string token)
    {
        await _mongoDBService.RemoveLotAsync(id, token)
            .ConfigureAwait(false);
        return Ok();
    }

    [HttpPut("Bid/{token}")]
    public async Task<Lot> BidLotAsync([FromBody] BidParams bidParams, string token)
    {
        return await _mongoDBService.BidLotAsync(bidParams, token)
            .ConfigureAwait(false);
    }
}