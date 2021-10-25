using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using NodaTime.Extensions;
using NpgsqlTypes;

namespace npgsql_nodatime_efcore_poc
{
    class Program
    {
        public const string ConnectionString = "Host=localhost;Port=5432;Database=yolo;Username=postgres;Password=postgres";

        static async Task Main(string[] args)
        {
            await using var context = new MyDbContext();
            await context.Database.EnsureDeletedAsync();
            await context.Database.EnsureCreatedAsync();
            var now = DateTimeOffset.UtcNow;
            var sebiet = now.AddMinutes(10);
            var session = new Session
            {
                Validity = new NpgsqlRange<OffsetDateTime>(now.ToOffsetDateTime(), sebiet.ToOffsetDateTime())
            };
            context.Set<Session>().Add(session);
            await context.SaveChangesAsync();
        }
    }

    public class Session
    {
        public Guid Id { get; set; }
        public NpgsqlRange<OffsetDateTime> Validity { get; set; }
    }

    public class MyDbContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(Program.ConnectionString, builder =>
            {
                builder.UseNodaTime();
            });
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Session>().HasKey(_ => _.Id);
        }
    }
}
