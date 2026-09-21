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
        public DbSet<Payload> Payloads { get; set; }
        public DbSet<Listener> Listeners { get; set; }
        public DbSet<HttpListener> HttpListeners { get; set; }
        public DbSet<TcpListener> TcpListeners { get; set; }
        public DbSet<ListenerHost> ListenerHosts { get; set; }
        public DbSet<Header> Headers { get; set; }
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

            modelBuilder.Entity<Listener>().ToTable("Listeners");
            modelBuilder.Entity<HttpListener>().ToTable("HttpListeners");
            modelBuilder.Entity<TcpListener>().ToTable("TcpListeners");

            modelBuilder.Entity<Listener>()
                .HasOne(l => l.Listeners)
                .WithOne(d => d.Listener)
                .HasForeignKey<ListenerBase>(d => d.ListenerID)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//