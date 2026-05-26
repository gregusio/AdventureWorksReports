using AdventureWorksReports.Modern.Net8.Mvc.Models;
using Microsoft.EntityFrameworkCore;

namespace AdventureWorksReports.Modern.Net8.Mvc.Data;

public class AdventureWorksContext : DbContext
{
    public AdventureWorksContext(DbContextOptions<AdventureWorksContext> options) 
        : base(options) { }

    public DbSet<SalesOrderHeader> SalesOrderHeaders { get; set; }
    public DbSet<SalesOrderDetail> SalesOrderDetails { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductSubcategory> ProductSubcategories { get; set; }
    public DbSet<ProductCategory> ProductCategories { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Person> Persons { get; set; }
    public DbSet<Location> Locations { get; set; }
    public DbSet<ProductInventory> ProductInventories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
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
    }
}