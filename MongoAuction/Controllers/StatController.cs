using Microsoft.AspNetCore.Mvc;
using MongoAuction.Models;
using MongoAuction.Services;

namespace MongoAuction.Controllers;

[ApiController]
[Route("[controller]")]
public class StatController : ControllerBase
{
    private readonly MongoDBService _mongoDBService;

    public StatController(MongoDBService mongoDBService)
    {
        _mongoDBService = mongoDBService;
    }

    [HttpGet("BestSeller")]
    public async Task<UserStatDto> GetBestSeller()
    {
        return await _mongoDBService.GetBestSellerAsync()
            .ConfigureAwait(false);
    }

    [HttpGet("BestCustomer")]
    public async Task<UserStatDto> GetBestCustomer()
    {
        return await _mongoDBService.GetBestCustomerAsync()
            .ConfigureAwait(false);
    }

    [HttpGet("MostExpensiveLot")]
    public async Task<Lot> GetMostExpensiveLot()
    {
        return await _mongoDBService.GetMostExpensiveLotAsync()
            .ConfigureAwait(false);
    }
}