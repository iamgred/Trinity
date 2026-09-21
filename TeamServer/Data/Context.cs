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
        public DbSet<Campaign> Campaigns { get; set; }
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


            modelBuilder.Entity<Campaign>().HasData(
                new Campaign
                {
                    ID = 1,
                    Name = "Emeris Campaign",
                    Status = Trinity.Shared.Enums.CampaignStatuses.Active,
                },
                new Campaign
                {
                    ID = 2,
                    Name = "Robocorp Campaign",
                    Status = Trinity.Shared.Enums.CampaignStatuses.Active,
                }
            );

           modelBuilder.Entity<Listener>().HasData(
                new Listener 
                { 
                    ID = 1,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    Name = "HTTP Listener",
                    Type = Trinity.Shared.Enums.ListenerTypes.HTTP
                },
                new Listener 
                { 
                    ID = 2,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    Name = "TCP Listener",
                    Type = Trinity.Shared.Enums.ListenerTypes.TCP
                }
            );

            modelBuilder.Entity<HttpListener>().HasData(
                new HttpListener
                {
                    ID = 1,
                    ListenerID = 1,
                    BindPort = 8888,
                    HostRotationStrategy = Trinity.Shared.Enums.RotationStrategies.RoundRobin,
                    MaxRetryStrategy = "HHH/MMM/SSSS",
                    C2Port = 8888,
                    Header = "X-Agent",
                    Name = "HTTP Listener",
                    UserAgent = "User Agent",
                }
            );

            modelBuilder.Entity<ListenerHost>().HasData(
                new ListenerHost
                {
                    ID = 1,
                    AddedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    Host = "www.test.com",
                    ListenerID = 1
                },
                new ListenerHost
                {
                    ID = 2,
                    AddedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    Host = "www.hello.com",
                    ListenerID = 1
                }
            );

            modelBuilder.Entity<TcpListener>().HasData(
                new TcpListener
                {
                    ID = 2,
                    ListenerID = 2,
                    LocalHostOnly = true,
                    Name = "TCP Listener",
                    Port = 2000
                }
            );

            modelBuilder.Entity<Agent>().HasData(
                new Agent
                {
                    ID = 1,
                    UUID = "123e4567-e89b-12d3-a456-426655440000",
                    Type = Trinity.Shared.Enums.AgentTypes.Peer,
                    CampaignID = 1,
                    ListenerID = 2,
                    PayloadID = 1,
                    Username = "Lucus",
                    ProcesseName = "bitwarden.exe",
                    Integrity = 3,
                    Status = Trinity.Shared.Enums.AgentStatuses.Active,
                    FirstSeen = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    LastSeen = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Agent
                {
                    ID = 2,
                    UUID = "fc32619d-446b-4989-9fcf-2854aae816ac",
                    Type = Trinity.Shared.Enums.AgentTypes.Peer,
                    CampaignID = 1,
                    ListenerID = 2,
                    PayloadID = 1,
                    Username = "mark",
                    ProcesseName = "notepad.exe",
                    Integrity = 2,
                    Status = Trinity.Shared.Enums.AgentStatuses.Active,
                    FirstSeen = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    LastSeen = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Agent
                {
                    ID = 3,
                    UUID = "123e4567-e89b-12d3-a456-426655440000",
                    Type = Trinity.Shared.Enums.AgentTypes.Egress,
                    CampaignID = 1,
                    ListenerID = 1,
                    PayloadID = 1,
                    Username = "adminEmeris",
                    ProcesseName = "explorer.exe",
                    Integrity = 3,
                    Status = Trinity.Shared.Enums.AgentStatuses.Active,
                    FirstSeen = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    LastSeen = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Agent
                {
                    ID = 4,
                    UUID = "fc32619d-446b-4989-9fcf-2854aae816ac",
                    Type = Trinity.Shared.Enums.AgentTypes.Peer,
                    CampaignID = 1,
                    ListenerID = 2,
                    PayloadID = 1,
                    Username = "lucus",
                    ProcesseName = "notepad.exe",
                    Integrity = 2,
                    Status = Trinity.Shared.Enums.AgentStatuses.Active,
                    FirstSeen = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    LastSeen = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Agent
                {
                    ID = 5,
                    UUID = "123e4567-e89b-12d3-a456-426655440000",
                    Type = Trinity.Shared.Enums.AgentTypes.Egress,
                    CampaignID = 2,
                    ListenerID = 1,
                    PayloadID = 1,
                    Username = "admin",
                    ProcesseName = "explorer.exe",
                    Integrity = 3,
                    Status = Trinity.Shared.Enums.AgentStatuses.Active,
                    FirstSeen = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    LastSeen = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Agent
                {
                    ID = 6,
                    UUID = "fc32619d-446b-4989-9fcf-2854aae816ac",
                    Type = Trinity.Shared.Enums.AgentTypes.Peer,
                    CampaignID = 2,
                    ListenerID = 2,
                    PayloadID = 1,
                    Username = "mark",
                    ProcesseName = "notepad.exe",
                    Integrity = 2,
                    Status = Trinity.Shared.Enums.AgentStatuses.Active,
                    FirstSeen = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    LastSeen = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );

        }
    }
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^{ END OF FILE }^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^//