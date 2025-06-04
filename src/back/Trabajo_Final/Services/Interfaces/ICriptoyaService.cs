namespace Trabajo_Final.Services.Interfaces
{
    public interface ICriptoyaService
    {
        Task<decimal?> GetPriceAsync(string coinCode, string exchangeCode = "satoshitango");
        Task<Dictionary<string, decimal>> GetMultiplePricesAsync(List<string> coinCodes, string exchangeCode = "satoshitango");
        Task UpdatePriceCacheAsync();
    }
}
