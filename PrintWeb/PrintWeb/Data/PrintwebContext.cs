using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace PrintWeb.Data;

public partial class PrintwebContext : DbContext
{
    public PrintwebContext()
    {
    }

    public PrintwebContext(DbContextOptions<PrintwebContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<BuyPaperLog> BuyPaperLogs { get; set; }

    public virtual DbSet<DetailBuyPaperLog> DetailBuyPaperLogs { get; set; }

    public virtual DbSet<DetailPaperPrinter> DetailPaperPrinters { get; set; }

    public virtual DbSet<DetailPaperStudent> DetailPaperStudents { get; set; }

    public virtual DbSet<Document> Documents { get; set; }

    public virtual DbSet<PaperType> PaperTypes { get; set; }

    public virtual DbSet<PaymentMethod> PaymentMethods { get; set; }

    public virtual DbSet<PaymentRecord> PaymentRecords { get; set; }

    public virtual DbSet<Printer> Printers { get; set; }

    public virtual DbSet<PrintingLog> PrintingLogs { get; set; }

    public virtual DbSet<Student> Students { get; set; }

   

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.AccountId).HasName("PK__Account__349DA5862D93361B");

            entity.ToTable("Account");

            entity.Property(e => e.AccountId)
                .HasMaxLength(250)
                .HasColumnName("AccountID");
            entity.Property(e => e.Password).HasMaxLength(250);
            entity.Property(e => e.Role).HasMaxLength(250);

            entity.HasOne(d => d.AccountNavigation).WithOne(p => p.Account)
                .HasForeignKey<Account>(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Account_Student");
        });

        modelBuilder.Entity<BuyPaperLog>(entity =>
        {
            entity.HasKey(e => e.BuyPaperLogId).HasName("PK__BuyPaper__D287C589FCA3D254");

            entity.ToTable("BuyPaperLog");

            entity.Property(e => e.BuyPaperLogId)
                .ValueGeneratedNever()
                .HasColumnName("BuyPaperLogID");
            entity.Property(e => e.DateBuy).HasColumnType("datetime");
            entity.Property(e => e.StudentId)
                .HasMaxLength(250)
                .HasColumnName("StudentID");
            entity.Property(e => e.TotalBuy).HasColumnType("money");

            entity.HasOne(d => d.Student).WithMany(p => p.BuyPaperLogs)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BuyPaperLog_Student");
        });

        modelBuilder.Entity<DetailBuyPaperLog>(entity =>
        {
            entity.HasKey(e => new { e.BuyPaperLogId, e.PaperTypeId }).HasName("PK__DetailBu__D287C589BA47E158");

            entity.ToTable("DetailBuyPaperLog");

            entity.Property(e => e.BuyPaperLogId).HasColumnName("BuyPaperLogID");
            entity.Property(e => e.PaperTypeId).HasColumnName("PaperTypeID");

            entity.HasOne(d => d.BuyPaperLog).WithMany(p => p.DetailBuyPaperLogs)
                .HasForeignKey(d => d.BuyPaperLogId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetailBuyPaperLog_BuyPaperLog");

            entity.HasOne(d => d.PaperType).WithMany(p => p.DetailBuyPaperLogs)
                .HasForeignKey(d => d.PaperTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetailBuyPaperLog_PaperType");
        });

        modelBuilder.Entity<DetailPaperPrinter>(entity =>
        {
            entity.HasKey(e => new { e.PaperTypeId, e.PrinterId }).HasName("PK__DetailPa__F95F653AC1B453B6");

            entity.ToTable("DetailPaperPrinter");

            entity.Property(e => e.PaperTypeId).HasColumnName("PaperTypeID");
            entity.Property(e => e.PrinterId)
                .HasMaxLength(250)
                .HasColumnName("PrinterID");

            entity.HasOne(d => d.PaperType).WithMany(p => p.DetailPaperPrinters)
                .HasForeignKey(d => d.PaperTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetailPaperPrinter_PaperType");

            entity.HasOne(d => d.Printer).WithMany(p => p.DetailPaperPrinters)
                .HasForeignKey(d => d.PrinterId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetailPaperPrinter_Printer");
        });

        modelBuilder.Entity<DetailPaperStudent>(entity =>
        {
            entity.HasKey(e => new { e.PaperTypeId, e.StudentId });

            entity.ToTable("DetailPaperStudent");

            entity.Property(e => e.PaperTypeId).HasColumnName("PaperTypeID");
            entity.Property(e => e.StudentId)
                .HasMaxLength(250)
                .HasColumnName("StudentID");

            entity.HasOne(d => d.PaperType).WithMany(p => p.DetailPaperStudents)
                .HasForeignKey(d => d.PaperTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetailPaperStudent_PaperType");

            entity.HasOne(d => d.Student).WithMany(p => p.DetailPaperStudents)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetailPaperStudent_Student");
        });

        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasKey(e => e.DocumentId).HasName("PK__Document__1ABEEF6F493AC0BA");

            entity.ToTable("Document");

            entity.Property(e => e.DocumentId).HasColumnName("DocumentID");
            entity.Property(e => e.FileName).HasMaxLength(250);
            entity.Property(e => e.FileType).HasMaxLength(250);
            entity.Property(e => e.StudentId)
                .HasMaxLength(250)
                .HasColumnName("StudentID");
            entity.Property(e => e.UploadedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Student).WithMany(p => p.Documents)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Document_Student");
        });

        modelBuilder.Entity<PaperType>(entity =>
        {
            entity.HasKey(e => e.PaperTypeId).HasName("PK__PaperTyp__F95F653ACB65F6F3");

            entity.ToTable("PaperType");

            entity.Property(e => e.PaperTypeId).HasColumnName("PaperTypeID");
            entity.Property(e => e.PaperTypeName).HasMaxLength(250);
            entity.Property(e => e.Price).HasColumnType("money");
        });

        modelBuilder.Entity<PaymentMethod>(entity =>
        {
            entity.HasKey(e => e.PaymentMethodId).HasName("PK__PaymentM__DC31C1F36A0E6C98");

            entity.ToTable("PaymentMethod");

            entity.Property(e => e.PaymentMethodId).HasColumnName("PaymentMethodID");
            entity.Property(e => e.PaymentName).HasMaxLength(250);
        });

        modelBuilder.Entity<PaymentRecord>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__PaymentR__9B556A58743C0A14");

            entity.Property(e => e.PaymentId)
                .HasMaxLength(250)
                .HasColumnName("PaymentID");
            entity.Property(e => e.Amount).HasColumnType("money");
            entity.Property(e => e.PaymentDate).HasColumnType("datetime");
            entity.Property(e => e.StudentId)
                .HasMaxLength(250)
                .HasColumnName("StudentID");

            entity.HasOne(d => d.PaymentMethodNavigation).WithMany(p => p.PaymentRecords)
                .HasForeignKey(d => d.PaymentMethod)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PaymentRecords_PaymentMethod");

            entity.HasOne(d => d.Student).WithMany(p => p.PaymentRecords)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PaymentRecords_Student");
        });

        modelBuilder.Entity<Printer>(entity =>
        {
            entity.HasKey(e => e.PrinterId).HasName("PK__Printer__D452AB2135360DD1");

            entity.ToTable("Printer");

            entity.Property(e => e.PrinterId)
                .HasMaxLength(250)
                .HasColumnName("PrinterID");
            entity.Property(e => e.Brand).HasMaxLength(250);
            entity.Property(e => e.BuildingName).HasMaxLength(250);
            entity.Property(e => e.CampusName).HasMaxLength(250);
            entity.Property(e => e.Model).HasMaxLength(250);
            entity.Property(e => e.Status).HasMaxLength(250);
        });

        modelBuilder.Entity<PrintingLog>(entity =>
        {
            entity.HasKey(e => e.PrintingLogId).HasName("PK__Printing__005BFECE3AB14CB1");

            entity.ToTable("PrintingLog");

            entity.Property(e => e.PrintingLogId).HasColumnName("PrintingLogID");
            entity.Property(e => e.Colored).HasMaxLength(250);
            entity.Property(e => e.DocumentId).HasColumnName("DocumentID");
            entity.Property(e => e.EndTime).HasColumnType("datetime");
            entity.Property(e => e.IsTwoSide).HasColumnName("isTwoSide");
            entity.Property(e => e.PaperTypeId).HasColumnName("PaperTypeID");
            entity.Property(e => e.PrinterId)
                .HasMaxLength(250)
                .HasColumnName("PrinterID");
            entity.Property(e => e.StartTime).HasColumnType("datetime");
            entity.Property(e => e.Status).HasMaxLength(250);
            entity.Property(e => e.StudentId)
                .HasMaxLength(250)
                .HasColumnName("StudentID");

            entity.HasOne(d => d.Document).WithMany(p => p.PrintingLogs)
                .HasForeignKey(d => d.DocumentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PrintingLog_Document");

            entity.HasOne(d => d.PaperType).WithMany(p => p.PrintingLogs)
                .HasForeignKey(d => d.PaperTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PrintingLog_PaperType");

            entity.HasOne(d => d.Printer).WithMany(p => p.PrintingLogs)
                .HasForeignKey(d => d.PrinterId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PrintingLog_Printer");

            entity.HasOne(d => d.Student).WithMany(p => p.PrintingLogs)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PrintingLog_Student");
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.StudentId).HasName("PK__Student__32C52A796CEEFEBC");

            entity.ToTable("Student");

            entity.Property(e => e.StudentId)
                .HasMaxLength(250)
                .HasColumnName("StudentID");
            entity.Property(e => e.AccountBalance).HasColumnType("money");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.StudentName).HasMaxLength(100);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
