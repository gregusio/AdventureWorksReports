using System.Data.Entity;
using AdventureWorksReports.Legacy.NetFramework.Models;

namespace AdventureWorksReports.Legacy.NetFramework.Data
{
    public class AdventureWorksContext : DbContext
    {
        public AdventureWorksContext() : base("name=AdventureWorksConnection")
        {
            Configuration.LazyLoadingEnabled = false;

            Database.SetInitializer<AdventureWorksContext>(null);
        }

        public DbSet<SalesOrderHeader> SalesOrderHeaders { get; set; }
        public DbSet<SalesOrderDetail> SalesOrderDetails { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductSubcategory> ProductSubcategories { get; set; }
        public DbSet<ProductCategory> ProductCategories { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Person> Persons { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<ProductInventory> ProductInventories { get; set; }
        public DbSet<ProductReview> ProductReviews { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SalesOrderHeader>().HasKey(e => e.SalesOrderID);
            modelBuilder.Entity<SalesOrderDetail>().HasKey(e => e.SalesOrderDetailID);
            modelBuilder.Entity<SalesOrderDetail>()
                .Property(e => e.LineTotal)
                .HasPrecision(38, 6);
            modelBuilder.Entity<Product>().HasKey(e => e.ProductID);
            modelBuilder.Entity<ProductSubcategory>().HasKey(e => e.ProductSubcategoryID);
            modelBuilder.Entity<ProductCategory>().HasKey(e => e.ProductCategoryID);
            modelBuilder.Entity<Customer>().HasKey(e => e.CustomerID);
            modelBuilder.Entity<Person>().HasKey(e => e.BusinessEntityID);
            modelBuilder.Entity<Location>().HasKey(e => e.LocationId);
            modelBuilder.Entity<ProductInventory>().HasKey(e => new { e.ProductId, e.LocationId });
            modelBuilder.Entity<ProductReview>().HasKey(e => e.ProductReviewID);
        }
    }
}