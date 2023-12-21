namespace MongoAuction.Models;

public class Lot
{
    public string Id { get; set; }
    public string Title { get; set; }
    public TimeOnly StartTime { get; set; }
    public DateOnly StartDate { get; set; }
    public Periods Period { get; set; }
    public string? LotOwnerName { get; set; }
    public string LastBidderName { get; set; }
    public double CurrentCost { get; set; }
    public double StartCost { get; set; }
    public double EndCost { get; set; }
    public double CostStep { get; set; }
    public string ItemCategory { get; set; }
    public bool IsActive { get; set; }
    public Dictionary<string, string> ItemProperties { get; set; }
}