using Microsoft.EntityFrameworkCore;
using Trabajo_Final.models;

namespace Trabajo_Final.data
{
    public class CryptoDbContext : DbContext
    {
        public CryptoDbContext(DbContextOptions<CryptoDbContext> options) : base(options)
        {
        }

        // DbSets
        public DbSet<User> Users { get; set; }
        public DbSet<UserSession> UserSessions { get; set; }
        public DbSet<Cryptocurrency> Cryptocurrencies { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Exchange> Exchanges { get; set; }
        public DbSet<PriceCache> PriceCaches { get; set; }
        public DbSet<UserPreference> UserPreferences { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuraciones de User
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // Configuraciones de UserSession
            modelBuilder.Entity<UserSession>(entity =>
            {
                entity.HasIndex(e => e.SessionToken).IsUnique();
                entity.HasIndex(e => e.ExpiresAt);
                entity.HasIndex(e => e.UserId);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // Configuraciones de Cryptocurrency
            modelBuilder.Entity<Cryptocurrency>(entity =>
            {
                entity.HasIndex(e => e.Symbol).IsUnique();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // Configuraciones de Transaction
            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasIndex(e => e.TransactionType);
                entity.HasIndex(e => e.TransactionDate);
                entity.HasIndex(e => e.CryptocurrencyId);
                entity.HasIndex(e => new { e.UserId, e.TransactionDate });
                entity.HasIndex(e => new { e.UserId, e.CryptocurrencyId });

                entity.Property(e => e.TransactionType)
                    .HasConversion<string>();

                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasCheckConstraint("CK_Transaction_Quantity", "Quantity > 0");
            });

            // Configuraciones de Exchange
            modelBuilder.Entity<Exchange>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // Configuraciones de PriceCache
            modelBuilder.Entity<PriceCache>(entity =>
            {
                entity.HasIndex(e => new { e.CryptocurrencyId, e.ExchangeId });
                entity.HasIndex(e => e.ExpiresAt);
                entity.Property(e => e.CachedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // Configuraciones de UserPreference
            modelBuilder.Entity<UserPreference>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // Datos iniciales
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Cryptocurrencies
            modelBuilder.Entity<Cryptocurrency>().HasData(
                new Cryptocurrency { Id = 1, Symbol = "BTC", Name = "Bitcoin", CriptoyaCode = "btc" },
                new Cryptocurrency { Id = 2, Symbol = "ETH", Name = "Ethereum", CriptoyaCode = "eth" },
                new Cryptocurrency { Id = 3, Symbol = "USDT", Name = "Tether", CriptoyaCode = "usdt" },
                new Cryptocurrency { Id = 4, Symbol = "DOGE", Name = "Dogecoin", CriptoyaCode = "doge" },
                new Cryptocurrency { Id = 5, Symbol = "SOL", Name = "Solana", CriptoyaCode = "sol" }
            );

            // Seed Exchanges
            modelBuilder.Entity<Exchange>().HasData(
                new Exchange { Id = 1, Name = "SatoshiTango", CriptoyaCode = "satoshitango", IsDefault = true },
                new Exchange { Id = 2, Name = "Buenbit", CriptoyaCode = "buenbit", IsDefault = false },
                new Exchange { Id = 3, Name = "Binance P2P", CriptoyaCode = "binancep2p", IsDefault = false }
            );
        }
    }
}

