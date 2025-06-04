using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Trabajo_Final.data;
using Trabajo_Final.models;
using Trabajo_Final.Services.Interfaces;

namespace Trabajo_Final.Services
{
    public class CriptoyaService : ICriptoyaService
    {
        private readonly HttpClient _httpClient;
        private readonly CryptoDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly string _baseUrl;

        public CriptoyaService(HttpClient httpClient, CryptoDbContext context, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _context = context;
            _configuration = configuration;
            _baseUrl = _configuration["CriptoyaAPI:BaseUrl"] ?? "https://criptoya.com/api";
        }

        public async Task<decimal?> GetPriceAsync(string coinCode, string exchangeCode = "satoshitango")
        {
            try
            {
                // First try to get from cache
                var cachedPrice = await GetPriceFromCacheAsync(coinCode, exchangeCode);
                if (cachedPrice.HasValue)
                    return cachedPrice;

                // If not in cache or expired, fetch from API
                var url = $"{_baseUrl}/{exchangeCode}/{coinCode}/ars";
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    return null;

                var jsonContent = await response.Content.ReadAsStringAsync();
                var priceData = JsonSerializer.Deserialize<JsonElement>(jsonContent);

                // Criptoya returns different structures depending on the exchange
                // For most exchanges: { "totalAsk": price, "totalBid": price }
                // We'll use totalAsk (sell price) as the reference price
                if (priceData.TryGetProperty("totalAsk", out var askElement) && askElement.TryGetDecimal(out var askPrice))
                {
                    // Cache the price
                    await CachePriceAsync(coinCode, exchangeCode, askPrice);
                    return askPrice;
                }

                return null;
            }
            catch (Exception ex)
            {
                // Log error (you might want to use a proper logging framework)
                Console.WriteLine($"Error fetching price for {coinCode}: {ex.Message}");
                return null;
            }
        }

        public async Task<Dictionary<string, decimal>> GetMultiplePricesAsync(List<string> coinCodes, string exchangeCode = "satoshitango")
        {
            var prices = new Dictionary<string, decimal>();
            var tasks = coinCodes.Select(async coinCode =>
            {
                var price = await GetPriceAsync(coinCode, exchangeCode);
                return new { CoinCode = coinCode, Price = price };
            });

            var results = await Task.WhenAll(tasks);

            foreach (var result in results)
            {
                if (result.Price.HasValue)
                {
                    prices[result.CoinCode] = result.Price.Value;
                }
            }

            return prices;
        }

        public async Task UpdatePriceCacheAsync()
        {
            var cryptocurrencies = await _context.Cryptocurrencies
                .Where(c => c.IsActive)
                .ToListAsync();

            var defaultExchange = await _context.Exchanges
                .FirstOrDefaultAsync(e => e.IsDefault && e.IsActive);

            if (defaultExchange == null) return;

            foreach (var crypto in cryptocurrencies)
            {
                var price = await GetPriceAsync(crypto.CriptoyaCode, defaultExchange.CriptoyaCode);
                if (price.HasValue)
                {
                    await CachePriceAsync(crypto.CriptoyaCode, defaultExchange.CriptoyaCode, price.Value);
                }

                // Add delay to avoid hitting API limits
                await Task.Delay(100);
            }
        }

        private async Task<decimal?> GetPriceFromCacheAsync(string coinCode, string exchangeCode)
        {
            var cryptocurrency = await _context.Cryptocurrencies
                .FirstOrDefaultAsync(c => c.CriptoyaCode == coinCode);

            if (cryptocurrency == null) return null;

            var exchange = await _context.Exchanges
                .FirstOrDefaultAsync(e => e.CriptoyaCode == exchangeCode);

            if (exchange == null) return null;

            var cachedPrice = await _context.PriceCaches
                .Where(pc => pc.CryptocurrencyId == cryptocurrency.Id
                           && pc.ExchangeId == exchange.Id
                           && pc.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(pc => pc.CachedAt)
                .FirstOrDefaultAsync();

            return cachedPrice?.PriceArs;
        }

        private async Task CachePriceAsync(string coinCode, string exchangeCode, decimal price)
        {
            var cryptocurrency = await _context.Cryptocurrencies
                .FirstOrDefaultAsync(c => c.CriptoyaCode == coinCode);

            if (cryptocurrency == null) return;

            var exchange = await _context.Exchanges
                .FirstOrDefaultAsync(e => e.CriptoyaCode == exchangeCode);

            if (exchange == null) return;

            // Remove old cache entries for this crypto/exchange pair
            var oldEntries = await _context.PriceCaches
                .Where(pc => pc.CryptocurrencyId == cryptocurrency.Id && pc.ExchangeId == exchange.Id)
                .ToListAsync();

            _context.PriceCaches.RemoveRange(oldEntries);

            // Add new cache entry (expires in 5 minutes)
            var cacheEntry = new PriceCache
            {
                CryptocurrencyId = cryptocurrency.Id,
                ExchangeId = exchange.Id,
                PriceArs = price,
                ExpiresAt = DateTime.UtcNow.AddMinutes(5)
            };

            _context.PriceCaches.Add(cacheEntry);
            await _context.SaveChangesAsync();
        }
    }
}
