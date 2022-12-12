using EmBrito.Dataverse.DataExport.Sql.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmBrito.Dataverse.DataExport.Sql.Sql
{
    public class DataContext : DbContext
    {

        string connString;
        int commandTimeout;

        public DataContext(string connectionString, int commandTimeoutSeconds)
        {
            connString = connectionString;
            commandTimeout = commandTimeoutSeconds;
        }

        public DbSet<ActivityLog> ActivityLogs { get; set; }
        public DbSet<SynchronizationJob> SynchronizationJobs { get; set; }
        public DbSet<SynchronizationLog> SynchronizationLogs { get; set; }
        public DbSet<SynchronizedTable> SynchronizedTables { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options
                .UseSqlServer(connString, o => 
                {
                    o.CommandTimeout(commandTimeout);
                    o.EnableRetryOnFailure();
                }); ;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<ActivityLog>()
                .HasKey(t => t.Id)
                .HasName("PK_ActivityLogs");

            modelBuilder.Entity<SynchronizationJob>()
                .HasKey(t => t.Id)
                .HasName("PK_SynchronizationJobs");

            modelBuilder.Entity<SynchronizationLog>()
                .HasKey(t => t.Id)
                .HasName("PK_SynchronizationLogs");

            modelBuilder.Entity<SynchronizedTable>()
                .HasKey(t => t.Id)
                .HasName("PK_SynchronizedTables");
        }

    }
}
