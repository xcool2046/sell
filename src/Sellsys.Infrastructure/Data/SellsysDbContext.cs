using Microsoft.EntityFrameworkCore;
using Sellsys.Domain.Entities;

namespace Sellsys.Infrastructure.Data
{
    public class SellsysDbContext : DbContext
    {
        public SellsysDbContext(DbContextOptions<SellsysDbContext> options) : base(options)
        {
        }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<DepartmentGroup> DepartmentGroups { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<SalesFollowUpLog> SalesFollowUpLogs { get; set; }
        public DbSet<AfterSalesRecord> AfterSalesRecords { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Customer entity
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasIndex(e => e.Name);
                entity.HasIndex(e => e.SalesPersonId);
                entity.HasIndex(e => e.SupportPersonId);

                // Configure relationships
                entity.HasOne(c => c.SalesPerson)
                    .WithMany(e => e.SalesCustomers)
                    .HasForeignKey(c => c.SalesPersonId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(c => c.SupportPerson)
                    .WithMany(e => e.SupportCustomers)
                    .HasForeignKey(c => c.SupportPersonId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure Contact entity
            modelBuilder.Entity<Contact>(entity =>
            {
                entity.HasIndex(e => e.CustomerId);

                entity.HasOne(c => c.Customer)
                    .WithMany(cu => cu.Contacts)
                    .HasForeignKey(c => c.CustomerId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Order entity
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasIndex(e => e.CustomerId);
                entity.HasIndex(e => e.SalesPersonId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.OrderNumber).IsUnique();

                entity.HasOne(o => o.Customer)
                    .WithMany(c => c.Orders)
                    .HasForeignKey(o => o.CustomerId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(o => o.SalesPerson)
                    .WithMany(e => e.Orders)
                    .HasForeignKey(o => o.SalesPersonId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure OrderItem entity
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasIndex(e => e.OrderId);
                entity.HasIndex(e => e.ProductId);

                entity.HasOne(oi => oi.Order)
                    .WithMany(o => o.OrderItems)
                    .HasForeignKey(oi => oi.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(oi => oi.Product)
                    .WithMany(p => p.OrderItems)
                    .HasForeignKey(oi => oi.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure SalesFollowUpLog entity
            modelBuilder.Entity<SalesFollowUpLog>(entity =>
            {
                entity.HasIndex(e => e.CustomerId);

                entity.HasOne(s => s.Customer)
                    .WithMany(c => c.SalesFollowUpLogs)
                    .HasForeignKey(s => s.CustomerId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(s => s.Contact)
                    .WithMany(c => c.SalesFollowUpLogs)
                    .HasForeignKey(s => s.ContactId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(s => s.SalesPerson)
                    .WithMany(e => e.SalesFollowUpLogs)
                    .HasForeignKey(s => s.SalesPersonId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure AfterSalesRecord entity
            modelBuilder.Entity<AfterSalesRecord>(entity =>
            {
                entity.HasIndex(e => e.CustomerId);
                entity.HasIndex(e => e.SupportPersonId);

                entity.HasOne(a => a.Customer)
                    .WithMany(c => c.AfterSalesRecords)
                    .HasForeignKey(a => a.CustomerId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(a => a.Contact)
                    .WithMany(c => c.AfterSalesRecords)
                    .HasForeignKey(a => a.ContactId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(a => a.SupportPerson)
                    .WithMany(e => e.AfterSalesRecords)
                    .HasForeignKey(a => a.SupportPersonId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure Employee entity
            modelBuilder.Entity<Employee>(entity =>
            {
                entity.HasIndex(e => e.LoginUsername).IsUnique();
                entity.HasIndex(e => e.GroupId);
                entity.HasIndex(e => e.RoleId);

                entity.HasOne(e => e.Group)
                    .WithMany(g => g.Employees)
                    .HasForeignKey(e => e.GroupId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Role)
                    .WithMany(r => r.Employees)
                    .HasForeignKey(e => e.RoleId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure DepartmentGroup entity
            modelBuilder.Entity<DepartmentGroup>(entity =>
            {
                entity.HasIndex(e => e.DepartmentId);

                entity.HasOne(g => g.Department)
                    .WithMany(d => d.Groups)
                    .HasForeignKey(g => g.DepartmentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}