using System.ComponentModel.DataAnnotations;

namespace MongoAuction.Models;

public class LotInputDto
{
    public string Title { get; set; }
    [DataType(DataType.Time)]
    public string StartTime { get; set; }
    [DataType(DataType.Date)]
    public string StartDate { get; set; }
    public Periods Period { get; set; }
    public double StartCost { get; set; }
    public double EndCost { get; set; }
    public double CostStep { get; set; }
    public string ItemCategory { get; set; }
    public string AdditionalItemProperties { get; set; }
}