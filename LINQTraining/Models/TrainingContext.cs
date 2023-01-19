using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

#nullable disable

namespace LINQTraining.Models
{
    public class TrainingContext : DbContext
    {
        public DbSet<Mapping> Mappings { get; set; }
        public DbSet<DataCategory> DataCategory { get; set; }
        public DbSet<Metadata> Metadata { get; set; }
        public DbSet<MetadataDataCategory> MetadataDataCategory { get; set; }
        public DbSet<DataValue> DataValues { get; set; }
        public DbSet<CandidateListA> CandidateListA { get; set; }
        public DbSet<CandidateListB> CandidateListB { get; set; }
        public DbSet<CandidateListC> CandidateListC { get; set; }

        private static readonly ILoggerFactory SqlLoggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DataValue>()
                .HasOne(x => x.Metadata)
                .WithMany(x => x.DataValues)
                .HasForeignKey(x => x.MetadataId);

            modelBuilder.Entity<Mapping>()
                .HasIndex(x => new { x.CodeA, x.CodeB });

            modelBuilder.Entity<MetadataDataCategory>()
                .HasKey(x => new { x.MetadataId, x.DataCategoryId });
            
            modelBuilder.Entity<MetadataDataCategory>()
                .HasOne(x => x.Metadata)
                .WithMany(x => x.MetadataDataCategories)
                .HasForeignKey(x => x.MetadataId);
            
            modelBuilder.Entity<MetadataDataCategory>()
                .HasOne(x => x.DataCategory)
                .WithMany(x => x.MetadataDataCategory)
                .HasForeignKey(x => x.DataCategoryId);

            modelBuilder.Entity<Metadata>()
                .HasMany(x => x.DataValues)
                .WithOne(x => x.Metadata)
                .HasForeignKey(x => x.MetadataId);
            
            modelBuilder.Entity<Metadata>()
                .HasMany(x => x.MetadataDataCategories)
                .WithOne(x => x.Metadata)
                .HasForeignKey(x => x.MetadataId);
        }
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .EnableSensitiveDataLogging()
                // LocalDB (SQL Server Express)を使用する
                .UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=Training")
                // EFが実行するSQLをログに出力する
                // https://learn.microsoft.com/en-us/ef/core/logging-events-diagnostics/extensions-logging?tabs=v3
                .UseLoggerFactory(SqlLoggerFactory);
        }
    }
}