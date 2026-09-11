using TradicioCarnica.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace TradicioCarnica.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        // PRODUCTS
        public DbSet<TProduct> TProducts { get; set; }
        public DbSet<TProductTranslations> TProductTranslations { get; set; }
        public DbSet<TProductPrice> TProductPrices { get; set; }

        // CATEGORIES
        public DbSet<TCategories> TCategories { get; set; }
        public DbSet<TCategoriesTranslations> TCategoriesTranslations { get; set; }

        // UNITS
        public DbSet<TPriceUnit> TPriceUnits { get; set; }

        // STOCK
        public DbSet<TStock> Stocks { get; set; }
        public DbSet<TStockMovements> TStockMovements { get; set; }

        // WAREHOUSES
        public DbSet<TWarehouse> TWarehouses { get; set; }

        // OFFERS
        public DbSet<TOffers> TOffers { get; set; }
        public DbSet<TOffersProducts> TOffersProducts { get; set; }

        // INVENTORY
        public DbSet<TInventory> TInventories { get; set; }
        public DbSet<TInventoryItems> TInventoryItems { get; set; }

        // LANGUAGES
        public DbSet<TLanguages> TLanguages { get; set; }

        // ENUMS
        public DbSet<TEnums> TEnums { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // PRODUCT -> CATEGORY
            modelBuilder.Entity<TProduct>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // PRODUCT -> PRICE UNIT
            modelBuilder.Entity<TProduct>()
                .HasOne(p => p.PriceUnit)
                .WithMany(pu => pu.Products)
                .HasForeignKey(p => p.PriceUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            // PRODUCT TRANSLATIONS
            modelBuilder.Entity<TProductTranslations>()
                .HasOne(pt => pt.Product)
                .WithMany(p => p.ProductTranslations)
                .HasForeignKey(pt => pt.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TProductTranslations>()
                .HasOne(pt => pt.Language)
                .WithMany(l => l.ProductTranslations)
                .HasForeignKey(pt => pt.LanguageId)
                .OnDelete(DeleteBehavior.Restrict);

            // CATEGORY TRANSLATIONS
            modelBuilder.Entity<TCategoriesTranslations>()
                .HasOne(ct => ct.Category)
                .WithMany(c => c.CategoriesTranslations)
                .HasForeignKey(ct => ct.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TCategoriesTranslations>()
                .HasOne(ct => ct.Language)
                .WithMany(l => l.CategoriesTranslations)
                .HasForeignKey(ct => ct.LanguageId)
                .OnDelete(DeleteBehavior.Restrict);

            // PRODUCT PRICE HISTORY
            modelBuilder.Entity<TProductPrice>()
                .HasOne(pp => pp.Product)
                .WithMany(p => p.Prices)
                .HasForeignKey(pp => pp.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // STOCK
            modelBuilder.Entity<TStock>()
                .HasOne(s => s.Product)
                .WithMany(p => p.Stocks)
                .HasForeignKey(s => s.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TStock>()
                .HasOne(s => s.Warehouse)
                .WithMany(w => w.Stocks)
                .HasForeignKey(s => s.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TStock>()
                .HasIndex(s => new { s.ProductId, s.WarehouseId })
                .IsUnique();

            // STOCK MOVEMENTS
            modelBuilder.Entity<TStockMovements>()
                .HasOne(sm => sm.Product)
                .WithMany()
                .HasForeignKey(sm => sm.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TStockMovements>()
                .HasOne(sm => sm.Warehouse)
                .WithMany()
                .HasForeignKey(sm => sm.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);

            // OFFERS PRODUCTS (MANY TO MANY)
            modelBuilder.Entity<TOffersProducts>()
                .HasOne(op => op.Offer)
                .WithMany(o => o.OfferProducts)
                .HasForeignKey(op => op.OfferId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TOffersProducts>()
                .HasOne(op => op.Product)
                .WithMany(p => p.OfferProducts)
                .HasForeignKey(op => op.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TOffersProducts>()
                .HasIndex(op => new { op.OfferId, op.ProductId })
                .IsUnique();

            // INVENTORY
            modelBuilder.Entity<TInventory>()
                .HasOne(i => i.Warehouse)
                .WithMany()
                .HasForeignKey(i => i.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);

            // INVENTORY ITEMS
            modelBuilder.Entity<TInventoryItems>()
                .HasOne(ii => ii.Inventory)
                .WithMany(i => i.Items)
                .HasForeignKey(ii => ii.InventoryId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TInventoryItems>()
                .HasOne(ii => ii.Product)
                .WithMany()
                .HasForeignKey(ii => ii.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.UseCollation("SQL_Latin1_General_CP1_CI_AS");
        }

    }
}
