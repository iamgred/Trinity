//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Trinity.Shared.Interfaces;
using Trinity.Shared.Models;

namespace TeamServer.Data
{
    public class Context : DbContext
    {
        public DbSet<Trinity.Shared.Models.Task> Tasks { get; set; }
        public DbSet<TaskResult> TaskResult { get; set; }
        public DbSet<Agent> Agents { get; set; }
        public DbSet<Operator> Operators { get; set; }
        public Context()
        {
        }
        public Context(DbContextOptions<Context> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Trinity.Shared.Models.Task>()
                .Property(e => e.ID)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<TaskResult>()
                .Property(e => e.ID)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Agent>()
                .Property(e => e.ID)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Operator>()
                .Property(e => e.ID)
                .ValueGeneratedOnAdd();
        }
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//