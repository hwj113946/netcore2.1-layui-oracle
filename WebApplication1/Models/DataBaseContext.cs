using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Models
{
    public class DataBaseContext : DbContext
    {
        public DataBaseContext(DbContextOptions<DataBaseContext> options)
            : base(options)
        {
        }
        public DbSet<TXRY_PERSON> TXRY_PERSON { get; set; }
        public DbSet<ANOTHERPLACEAFR> AFR { get; set; }
        public DbSet<APP_WORKFLOW_APPR> APP_WORKFLOW_APPR { get; set; }
        public DbSet<APP_WORKFLOW_APPR_FLOW> APP_WORKFLOW_APPR_FLOW { get; set; }
        public DbSet<APP_WORKFLOW_APPR_NODES> APP_WORKFLOW_APPR_NODES { get; set; }
        public DbSet<APP_WORKFLOW_APPR_LINES> APP_WORKFLOW_APPR_LINES { get; set; }
        public DbSet<APP_WORKFLOW_LINE_PROPERTY> APP_WORKFLOW_LINE_PROPERTY { get; set; }
        public DbSet<APP_WORKFLOW_NODE_PROPERTY> APP_WORKFLOW_NODE_PROPERTY { get; set; }
        public DbSet<NOTICE> NOTICE { get; set; }
        public DbSet<POLICY> POLICY { get; set; }
        public DbSet<MEDICAL_FUND> MEDICAL_FUND { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //判断当前数据库是Oracle 需要手动添加Schema(DBA提供的数据库账号名称)
            if (this.Database.IsOracle())
            {
                modelBuilder.HasDefaultSchema("TXRYHD");
            }
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<TXRY_PERSON>(entity =>
            {
                entity.ToTable("TXRY_PERSON");
                entity.HasKey(i => i.PERSON_ID);
                entity.Property(o => o.PERSON_ID).ForOracleUseSequenceHiLo("TXRY_PERSON_S");

            });
            modelBuilder.Entity<ANOTHERPLACEAFR>(entity =>
            {
                entity.ToTable("ANOTHERPLACEAFR");
                entity.HasKey(i => i.AFR_ID);
                entity.Property(o => o.AFR_ID).ForOracleUseSequenceHiLo("ANOTHERPLACEAFR_S");

            });
            modelBuilder.Entity<APP_WORKFLOW_APPR>(entity =>
            {
                entity.ToTable("APP_WORKFLOW_APPR");
                entity.HasKey(i => i.APPR_ID);
                entity.Property(o => o.APPR_ID).ForOracleUseSequenceHiLo("APP_WORKFLOW_APPR_S");

            });
            modelBuilder.Entity<APP_WORKFLOW_APPR_FLOW>(entity =>
            {
                entity.ToTable("APP_WORKFLOW_APPR_FLOW");
                entity.HasKey(i => i.APPR_FLOW_ID);
                entity.Property(o => o.APPR_FLOW_ID).ForOracleUseSequenceHiLo("APP_WORKFLOW_APPR_FLOW_S");

            });
            modelBuilder.Entity<APP_WORKFLOW_APPR_NODES>(entity =>
            {
                entity.ToTable("APP_WORKFLOW_APPR_NODES");
                entity.HasKey(i => i.ID);
                entity.Property(o => o.ID).ForOracleUseSequenceHiLo("APP_WORKFLOW_APPR_NODES_S");

            });
            modelBuilder.Entity<APP_WORKFLOW_APPR_LINES>(entity =>
            {
                entity.ToTable("APP_WORKFLOW_APPR_LINES");
                entity.HasKey(i => i.ID);
                entity.Property(o => o.ID).ForOracleUseSequenceHiLo("APP_WORKFLOW_APPR_LINES_S");

            });
            modelBuilder.Entity<APP_WORKFLOW_LINE_PROPERTY>(entity =>
            {
                entity.ToTable("APP_WORKFLOW_LINE_PROPERTY");
                entity.HasKey(i => i.PRO_ID);
                entity.Property(o => o.PRO_ID).ForOracleUseSequenceHiLo("APP_WORKFLOW_LINE_PROPERTY_S");

            });
            modelBuilder.Entity<APP_WORKFLOW_NODE_PROPERTY>(entity =>
            {
                entity.ToTable("APP_WORKFLOW_NODE_PROPERTY");
                entity.HasKey(i => i.PRO_ID);
                entity.Property(o => o.PRO_ID).ForOracleUseSequenceHiLo("APP_WORKFLOW_NODE_PROPERTY_S");

            });
            modelBuilder.Entity<NOTICE>(entity =>
            {
                entity.ToTable("NOTICE");
                entity.HasKey(i => i.NOTICE_ID);
                entity.Property(o => o.NOTICE_ID).ForOracleUseSequenceHiLo("NOTICE_S");

            });
            modelBuilder.Entity<POLICY>(entity =>
            {
                entity.ToTable("POLICY");
                entity.HasKey(i => i.POLICY_ID);
                entity.Property(o => o.POLICY_ID).ForOracleUseSequenceHiLo("POLICY_S");

            });
            modelBuilder.Entity<MEDICAL_FUND>(entity =>
            {
                entity.ToTable("MEDICAL_FUND");
                entity.HasKey(i => i.FUND_ID);
                entity.Property(o => o.FUND_ID).ForOracleUseSequenceHiLo("MEDICAL_FUND_S");

            });
        }
    }
}
