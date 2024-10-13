using Microsoft.EntityFrameworkCore;

namespace Order
{
    public class OrderData : DbContext
    {
        public OrderData(DbContextOptions<OrderData> options)
            : base(options)
        {
        }

        // Defina suas DbSets (tabelas) aqui
        public DbSet<Order> Orders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
           
        }
    }
}
