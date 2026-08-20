using System;
using System.Collections.Generic;
using BMW_Special_Tools.Models;
using Microsoft.EntityFrameworkCore;

namespace BMW_Special_Tools.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Tool> Tools { get; set; }

//     protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
// // #warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
//         => optionsBuilder.UseSqlServer("Server=localhost,1433;Database=BmwSpecialToolsDb;User Id=sa;Password=Belal2007?;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tool>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Tools__3214EC07DD776A7A");

            entity.HasIndex(e => new { e.Number, e.Bmwnumber }, "UQ_Tools_Number_BMWNumber").IsUnique();

            entity.HasIndex(e => e.QrTag, "UQ__Tools__16F562D8C488AF31").IsUnique();

            entity.Property(e => e.ArabicName).HasMaxLength(500);
            entity.Property(e => e.BmwkitNumber)
                .HasMaxLength(150)
                .HasColumnName("BMWKitNumber");
            entity.Property(e => e.Bmwnumber)
                .HasMaxLength(150)
                .HasColumnName("BMWNumber");
            entity.Property(e => e.CostPrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.EnglishName).HasMaxLength(500);
            entity.Property(e => e.KitNumber).HasMaxLength(150);
            entity.Property(e => e.MainImagePath).HasMaxLength(150);
            entity.Property(e => e.Number).HasMaxLength(150);
            entity.Property(e => e.QrTag).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
