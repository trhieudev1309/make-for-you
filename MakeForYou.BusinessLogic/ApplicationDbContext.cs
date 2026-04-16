using MakeForYou.BusinessLogic.Entities;
using Microsoft.EntityFrameworkCore;

namespace MakeForYou.BusinessLogic
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Buyer> Buyers => Set<Buyer>();
        public DbSet<Seller> Sellers => Set<Seller>();
        public DbSet<PortfolioItem> PortfolioItems => Set<PortfolioItem>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<Review> Reviews => Set<Review>();
        public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
        public DbSet<Quotation> Quotations => Set<Quotation>();

        public DbSet<Product> Products => Set<Product>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1:1 User - Buyer
            modelBuilder.Entity<Buyer>()
                .HasOne(b => b.User)
                .WithOne(u => u.Buyer)
                .HasForeignKey<Buyer>(b => b.BuyerId)
                .OnDelete(DeleteBehavior.Cascade);

            // 1:1 User - Seller
            modelBuilder.Entity<Seller>()
                .HasOne(s => s.User)
                .WithOne(u => u.Seller)
                .HasForeignKey<Seller>(s => s.SellerId)
                .OnDelete(DeleteBehavior.Cascade);

            // Order -> Buyer (User) - do NOT cascade delete
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Buyer)
                .WithMany(u => u.OrdersAsBuyer)
                .HasForeignKey(o => o.BuyerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Order -> Seller - do NOT cascade delete
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Seller)
                .WithMany(s => s.Orders)
                .HasForeignKey(o => o.SellerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Portfolio -> Seller
            modelBuilder.Entity<PortfolioItem>()
                .HasOne(p => p.Seller)
                .WithMany(s => s.PortfolioItems)
                .HasForeignKey(p => p.SellerId)
                .OnDelete(DeleteBehavior.Cascade);

            // Review relationships
            modelBuilder.Entity<Review>()
                .HasOne(r => r.Order)
                .WithMany(o => o.Reviews)
                .HasForeignKey(r => r.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Buyer)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.BuyerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Seller)
                .WithMany(s => s.Reviews)
                .HasForeignKey(r => r.SellerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Chat messages
            modelBuilder.Entity<ChatMessage>()
                .HasOne(cm => cm.Order)
                .WithMany(o => o.ChatMessages)
                .HasForeignKey(cm => cm.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ChatMessage>()
                .HasOne(cm => cm.Sender)
                .WithMany(u => u.SentMessages)
                .HasForeignKey(cm => cm.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            // Quotation -> Order
            modelBuilder.Entity<Quotation>()
                .HasOne(q => q.Order)
                .WithMany(o => o.Quotations)
                .HasForeignKey(q => q.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}