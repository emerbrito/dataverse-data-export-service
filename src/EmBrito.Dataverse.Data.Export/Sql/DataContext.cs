using EmBrito.Dataverse.Data.Export.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmBrito.Dataverse.Data.Export.Sql
{
    internal class DataContext : DbContext
    {

        string connString;
        int commandTimeout;

        public DataContext(string connectionString, int commandTimeoutSeconds)
        {
            connString = connectionString;
            commandTimeout = commandTimeoutSeconds;
        }

        public DbSet<SynchronizationJob> SynchronizationJobs { get; set; }
        public DbSet<SynchronizationLog> SynchronizationLogs { get; set; }
        public DbSet<SynchronizedTable> SynchronizedTables { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options
                .UseSqlServer(connString, o => o.CommandTimeout(commandTimeout)); ;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
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
