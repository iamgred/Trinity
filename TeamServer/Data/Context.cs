//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using Microsoft.EntityFrameworkCore;
using Trinity.Shared.Models;

namespace TeamServer.Data
{
    public class Context : DbContext
    {
        public DbSet<Trinity.Shared.Models.Task> Tasks { get; set; }
        public DbSet<TaskResult> TaskResult { get; set; }
        public DbSet<Agent> Agents { get; set; }
        public DbSet<Trinity.Shared.Models.Host> Hosts { get; set; }
        public DbSet<Payload> Payloads { get; set; }
        public DbSet<Listener> Listeners { get; set; }
        public DbSet<Campaign> Campaigns { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Operator> Operators { get; set; }
        public DbSet<CampaignBridge> CampaignBridges { get; set; }
        public DbSet<Trinity.Shared.Models.Error> Errors { get; set; }
        public DbSet<Tool> Tools { get; set; }
        public Context()
        {
        }
        public Context(DbContextOptions<Context> options) : base(options)
        {
        }

        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
        /// <summary>
        /// Configures the EF Core model and seeds initial Protocol entities.
        /// </summary>
        /// <remarks>Adds three Protocol seed records via modelBuilder.Entity<Protocol>().HasData. Seed
        /// data is applied by migrations and intended for static reference data rather than mutable runtime
        /// state.</remarks>
        /// <param name="modelBuilder">ModelBuilder used to configure entity mappings and seed the Protocol entities.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CampaignBridge>()
                .HasIndex(cb => new { cb.CampaignID, cb.OperatorID })
                .IsUnique();
        }
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//