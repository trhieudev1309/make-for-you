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

        public DbSet<CartItem> CartItems { get; set; }

        public DbSet<OrderItem> OrderItems { get; set; }

        public DbSet<OrderProgress> OrderProgresses => Set<OrderProgress>();

        public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();

        public DbSet<Notification> Notifications => Set<Notification>();

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

            // Quotation -> Order
            modelBuilder.Entity<Quotation>()
                .HasOne(q => q.Order)
                .WithMany(o => o.Quotations)
                .HasForeignKey(q => q.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Quan hệ Order -> OrderItem (1 đơn hàng có nhiều món)
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Quan hệ Product -> OrderItem
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany()
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // ChatMessage relationships: explicit configuration for FromUser and ToUser
            modelBuilder.Entity<ChatMessage>(entity =>
            {
                entity.HasKey(m => m.MessageId);

                entity.HasOne(m => m.FromUser)
                      .WithMany(u => u.SentMessages)
                      .HasForeignKey(m => m.FromUserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(m => m.ToUser)
                      .WithMany(u => u.ReceivedMessages)
                      .HasForeignKey(m => m.ToUserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.Property(m => m.Message).IsRequired();
                entity.Property(m => m.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            });
        }
    }
}