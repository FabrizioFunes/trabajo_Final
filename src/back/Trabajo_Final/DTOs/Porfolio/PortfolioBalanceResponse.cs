namespace Trabajo_Final.DTOs.Porfolio
{
    public class PortfolioBalanceResponse
    {
        public int CryptocurrencyId { get; set; }
        public string Symbol { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal CurrentBalance { get; set; }
        public decimal? CurrentPriceArs { get; set; }
        public decimal? TotalValueArs { get; set; }
        public string ExchangeName { get; set; } = string.Empty;
    }
}
