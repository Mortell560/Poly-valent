using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace PolyDatabase
{
    public class PolyContext : DbContext
    {
        public DbSet<Server> Servers { get; set; }
        public DbSet<Newsletter> Newsletters { get; set; }
        public DbSet<Grade> Grades { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Grade>().HasKey(g => new { g.Subject_id, g.Name });
        }
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            ServerVersion version = ServerVersion.Create(1, 0, 0, Pomelo.EntityFrameworkCore.MySql.Infrastructure.ServerType.MySql);
            optionsBuilder.UseMySql("server=localhost;user=root;Database=test;port=3306;Connect Timeout=5;", version, option => option.EnableRetryOnFailure());
        }
    }

    public class Server
    {
        [Key]
        public ulong Id { get; set; }
        public ulong NewsChannel { get; set; }
    }
    public class Newsletter
    {
        [Key]
        public ulong UserId { get; set; }
        public ulong Id { get; set; }
        public string? Calendar { get; set; }
    }
    
    //Owner only
    public class Grade
    {
        [Key]
        public string? Name { get; set; }
        public string? Subject { get; set; }
        [Key]
        public string? Subject_id { get; set; }

        public float? grade { get; set; }
        public string? Date_str { get; set; }
        public string? Appr { get; set; }
        public float? Class_avg { get; set; }
        public string? Rank { get; set; }
        public DateTime? Date { get; set; }
    }
}