using BookStore.Models;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace BookStore.Data
{
    public class BookStoreContext : DbContext
    {
        public BookStoreContext() : base("BookStoreContext")
        {
        }
        public DbSet<Role> Roles { get; set; }

        public DbSet<Author> Authors { get; set; }

        public DbSet<Category> Categories { get; set; }

        public DbSet<Order> Orders { get; set; }

        public DbSet<OrderDetail> OrderDetails { get; set; }

        public DbSet<Product> Products { get; set; }

        public DbSet<ProductDetail> ProductDetails { get; set; }

        public DbSet<ProductImport> ProductImports { get; set; }

        public DbSet<ImportDetail> ImportDetails { get; set; }

        public DbSet<Publisher> Publishers { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<AnonymousUser> AnonymousUsers { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }
    }
}