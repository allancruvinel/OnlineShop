using Microsoft.EntityFrameworkCore;
using Stock;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Stock
{
    public class ItemData : DbContext
    {
        public ItemData(DbContextOptions<ItemData> options)
            : base(options)
        {
        }

        // Defina suas DbSets (tabelas) aqui
        public DbSet<Item> Item { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        public void SeedData()
        {
            // Seed manual para bancos em memória
            if (!Item.Any())
            {
                Item.Add(new Item { ItemId = 1, Quantity = 5 });
                SaveChanges();
            }
        }


    }
}
