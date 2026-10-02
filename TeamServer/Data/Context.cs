//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ BEGINNING OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
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
        public DbSet<Payload> Payloads { get; set; }
        public DbSet<Protocol> Protocols { get; set; }
        public DbSet<Listener> Listeners { get; set; }
        public DbSet<Campaign> Campaigns { get; set; }
        public DbSet<Operator> Operators { get; set; }
        public DbSet<CampaignBridge> CampaignBridges { get; set; }
        public DbSet<Admin> Admins { get; set; }
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
            modelBuilder.Entity<Protocol>().HasData(
                new Protocol
                {
                    ID = 1,
                    Name = "HTTP"
                },
                new Protocol
                {
                    ID = 2,
                    Name = "SMB"
                },
                new Protocol
                {
                    ID = 3,
                    Name = "TCP"
                }
            );

            modelBuilder.Entity<Trinity.Shared.Models.TaskStatus>().HasData(
                new Trinity.Shared.Models.TaskStatus
                {
                    ID = 1,
                    Name = "Queued",
                },
                new Trinity.Shared.Models.TaskStatus
                {
                    ID = 2,
                    Name = "Pending",
                },
                new Trinity.Shared.Models.TaskStatus
                {
                    ID = 3,
                    Name = "Running",
                },
                new Trinity.Shared.Models.TaskStatus
                {
                    ID = 4,
                    Name = "Successful",
                },
                new Trinity.Shared.Models.TaskStatus
                {
                    ID = 5,
                    Name = "Failure",
                }
            );
            modelBuilder.Entity<CampaignBridge>().HasOne<Campaign>().WithMany().HasForeignKey(cb => cb.CampaignID);

            modelBuilder.Entity<CampaignBridge>().HasOne<Operator>().WithMany().HasForeignKey(cb => cb.OperatorID);

        }
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//