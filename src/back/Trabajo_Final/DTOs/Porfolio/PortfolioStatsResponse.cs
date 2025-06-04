namespace Trabajo_Final.DTOs.Porfolio
{
    public class PortfolioStatsResponse
    {
        public int TotalCryptocurrencies { get; set; }
        public decimal TotalValueArs { get; set; }
        public List<PortfolioBalanceResponse> Holdings { get; set; } = new List<PortfolioBalanceResponse>();
    }
}
