namespace MongoAuction.Models
{
    public class UserStatDto
    {
        public string Username { get; set; }
        public int LotsAmount { get; set; }
        public double TotalCost { get; set; }
        public string[] LotTitles { get; set; }
    }
}
