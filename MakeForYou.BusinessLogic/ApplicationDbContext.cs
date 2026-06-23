using System.Data;
using MakeForYou.BusinessLogic.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;

namespace MakeForYou.BusinessLogic
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        { }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await AssignMissingIdsAsync(cancellationToken);
            return await base.SaveChangesAsync(cancellationToken);
        }

        private async Task AssignMissingIdsAsync(CancellationToken cancellationToken)
        {
            // EF Core assigns a temporary negative placeholder to ValueGeneratedOnAdd long PKs,
            // so entry.Property(...).CurrentValue is NOT 0 even when the entity holds 0.
            // Read and write via reflection directly on entry.Entity instead.
            var pendingByTable = new Dictionary<string,
                (string Column, List<(EntityEntry Entry, System.Reflection.PropertyInfo EntityProp)> Entries)>(
                StringComparer.OrdinalIgnoreCase);

            foreach (var entry in ChangeTracker.Entries().Where(e => e.State == EntityState.Added))
            {
                var pk = entry.Metadata.FindPrimaryKey();
                if (pk is null || pk.Properties.Count != 1) continue;

                var pkProp = pk.Properties[0];
                if (pkProp.ClrType != typeof(long)) continue;

                var entityProp = entry.Entity.GetType().GetProperty(pkProp.Name);
                if (entityProp == null) continue;

                // Check the real value on the entity object (always 0 when unset)
                if ((long)(entityProp.GetValue(entry.Entity) ?? 0L) != 0) continue;

                var tableName = entry.Metadata.GetTableName()!;
                var storeId = StoreObjectIdentifier.Table(tableName, entry.Metadata.GetSchema());
                var column = pkProp.GetColumnName(storeId);

                if (!pendingByTable.ContainsKey(tableName))
                    pendingByTable[tableName] = (column, new());

                pendingByTable[tableName].Entries.Add((entry, entityProp));
            }

            if (pendingByTable.Count == 0) return;

            var conn = Database.GetDbConnection();
            var wasOpen = conn.State == ConnectionState.Open;
            if (!wasOpen) await conn.OpenAsync(cancellationToken);

            try
            {
                foreach (var (tableName, (column, entries)) in pendingByTable)
                {
                    using var cmd = conn.CreateCommand();
                    cmd.Transaction = Database.CurrentTransaction?.GetDbTransaction();
                    cmd.CommandText = $"SELECT COALESCE(MAX(\"{column}\"), 0) FROM \"{tableName}\"";
                    var maxId = Convert.ToInt64(await cmd.ExecuteScalarAsync(cancellationToken));

                    for (int i = 0; i < entries.Count; i++)
                    {
                        var newId = maxId + 1 + i;
                        // Set directly on the entity so EF picks up the correct value
                        entries[i].EntityProp.SetValue(entries[i].Entry.Entity, newId);
                    }
                }
            }
            finally
            {
                if (!wasOpen) await conn.CloseAsync();
            }
        }

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
        public DbSet<ProductImage> ProductImages => Set<ProductImage>();

        public DbSet<CartItem> CartItems { get; set; }

        public DbSet<OrderItem> OrderItems { get; set; }

        public DbSet<OrderProgress> OrderProgresses => Set<OrderProgress>();

        public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();

        public DbSet<Notification> Notifications => Set<Notification>();

        public DbSet<CustomizationGroup> CustomizationGroups => Set<CustomizationGroup>();
        public DbSet<CustomizationOption> CustomizationOptions => Set<CustomizationOption>();

        public DbSet<SellerPost> SellerPosts => Set<SellerPost>();

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

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Product)
                .WithMany()
                .HasForeignKey(r => r.ProductId)
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
                entity.Property(m => m.CreatedAt).HasDefaultValueSql("now()");
            });

            // CustomizationGroup -> Product relationship
            modelBuilder.Entity<CustomizationGroup>()
                .HasOne(cg => cg.Product)
                .WithMany(p => p.CustomizationGroups)
                .HasForeignKey(cg => cg.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Product -> ProductImage relationship (one-to-many)
            modelBuilder.Entity<ProductImage>()
                .HasOne(pi => pi.Product)
                .WithMany(p => p.Images)
                .HasForeignKey(pi => pi.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // CustomizationOption -> CustomizationGroup relationship
            modelBuilder.Entity<CustomizationOption>()
                .HasOne(co => co.CustomizationGroup)
                .WithMany(cg => cg.Options)
                .HasForeignKey(co => co.CustomizationGroupId)
                .OnDelete(DeleteBehavior.Cascade);

            // SellerPost -> Seller relationship
            modelBuilder.Entity<SellerPost>()
                .HasOne(p => p.Seller)
                .WithMany(s => s.Posts)
                .HasForeignKey(p => p.SellerId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}