using BuildBook.Domain.Rmas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildBook.Infrastructure.Persistence.Configurations;

public sealed class RmaRecordConfiguration : IEntityTypeConfiguration<RmaRecord>
{
    public void Configure(EntityTypeBuilder<RmaRecord> builder)
    {
        builder.ToTable("RmaRecords");

        builder.HasKey(rmaRecord => rmaRecord.Id);

        builder.Property(rmaRecord => rmaRecord.RmaNumber)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(rmaRecord => rmaRecord.CreatedBy)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(rmaRecord => rmaRecord.LastUpdatedBy)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(rmaRecord => rmaRecord.ClosedBy)
            .HasMaxLength(256);

        builder.Property(rmaRecord => rmaRecord.ProductCode)
            .HasMaxLength(128);

        builder.Property(rmaRecord => rmaRecord.ProductName)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(rmaRecord => rmaRecord.SerialNumber)
            .HasMaxLength(128);

        builder.Property(rmaRecord => rmaRecord.ContactName)
            .HasMaxLength(256);

        builder.Property(rmaRecord => rmaRecord.ContactEmail)
            .HasMaxLength(256);

        builder.Property(rmaRecord => rmaRecord.ContactPhone)
            .HasMaxLength(64);

        builder.Property(rmaRecord => rmaRecord.CustomerReference)
            .HasMaxLength(128);

        builder.Property(rmaRecord => rmaRecord.SupportTicketNumber)
            .HasMaxLength(128);

        builder.Property(rmaRecord => rmaRecord.SupportTicketUrl)
            .HasMaxLength(1024);

        builder.Property(rmaRecord => rmaRecord.OriginalOrderNumber)
            .HasMaxLength(128);

        builder.Property(rmaRecord => rmaRecord.OriginalInvoiceNumber)
            .HasMaxLength(128);

        builder.Property(rmaRecord => rmaRecord.FaultSummary)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(rmaRecord => rmaRecord.FaultSubcategory)
            .HasMaxLength(128);

        builder.Property(rmaRecord => rmaRecord.DiagnosisNotes)
            .HasMaxLength(4000);

        builder.Property(rmaRecord => rmaRecord.QuoteNumber)
            .HasMaxLength(128);

        builder.Property(rmaRecord => rmaRecord.PurchaseOrderNumber)
            .HasMaxLength(128);

        builder.Property(rmaRecord => rmaRecord.RepairInvoiceNumber)
            .HasMaxLength(128);

        builder.Property(rmaRecord => rmaRecord.EstimatedRepairCost)
            .HasColumnType("decimal(18,2)");

        builder.Property(rmaRecord => rmaRecord.ActualRepairCost)
            .HasColumnType("decimal(18,2)");

        builder.Property(rmaRecord => rmaRecord.ReceivedBy)
            .HasMaxLength(256);

        builder.Property(rmaRecord => rmaRecord.AssignedTo)
            .HasMaxLength(256);

        builder.Property(rmaRecord => rmaRecord.OnHoldReason)
            .HasMaxLength(256);

        builder.Property(rmaRecord => rmaRecord.EscalatedTo)
            .HasMaxLength(256);

        builder.Property(rmaRecord => rmaRecord.RepairCompletedBy)
            .HasMaxLength(256);

        builder.Property(rmaRecord => rmaRecord.TestPlanUsed)
            .HasMaxLength(256);

        builder.Property(rmaRecord => rmaRecord.TestedBy)
            .HasMaxLength(256);

        builder.Property(rmaRecord => rmaRecord.TestNotes)
            .HasMaxLength(4000);

        builder.Property(rmaRecord => rmaRecord.QaCheckedBy)
            .HasMaxLength(256);

        builder.Property(rmaRecord => rmaRecord.ReleaseApprovedBy)
            .HasMaxLength(256);

        builder.Property(rmaRecord => rmaRecord.ReturnMethod)
            .HasMaxLength(128);

        builder.Property(rmaRecord => rmaRecord.Courier)
            .HasMaxLength(128);

        builder.Property(rmaRecord => rmaRecord.TrackingNumber)
            .HasMaxLength(256);

        builder.Property(rmaRecord => rmaRecord.ShippedBy)
            .HasMaxLength(256);

        builder.HasIndex(rmaRecord => rmaRecord.RmaNumber)
            .IsUnique();

        builder.HasIndex(rmaRecord => rmaRecord.BuildRecordId);
        builder.HasIndex(rmaRecord => rmaRecord.CustomerId);
        builder.HasIndex(rmaRecord => rmaRecord.Status);
        builder.HasIndex(rmaRecord => rmaRecord.AssignedTo);
        builder.HasIndex(rmaRecord => rmaRecord.Priority);
        builder.HasIndex(rmaRecord => rmaRecord.DueDate);
        builder.HasIndex(rmaRecord => rmaRecord.SerialNumber);
        builder.HasIndex(rmaRecord => rmaRecord.ProductCode);
        builder.HasIndex(rmaRecord => rmaRecord.ProductName);
        builder.HasIndex(rmaRecord => rmaRecord.LastUpdatedAt);

        builder.HasOne(rmaRecord => rmaRecord.BuildRecord)
            .WithMany()
            .HasForeignKey(rmaRecord => rmaRecord.BuildRecordId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(rmaRecord => rmaRecord.Customer)
            .WithMany()
            .HasForeignKey(rmaRecord => rmaRecord.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(rmaRecord => rmaRecord.ChecklistItems)
            .WithOne(checklistItem => checklistItem.RmaRecord)
            .HasForeignKey(checklistItem => checklistItem.RmaRecordId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(rmaRecord => rmaRecord.Notes)
            .WithOne(note => note.RmaRecord)
            .HasForeignKey(note => note.RmaRecordId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(rmaRecord => rmaRecord.Communications)
            .WithOne(communication => communication.RmaRecord)
            .HasForeignKey(communication => communication.RmaRecordId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(rmaRecord => rmaRecord.Attachments)
            .WithOne(attachment => attachment.RmaRecord)
            .HasForeignKey(attachment => attachment.RmaRecordId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(rmaRecord => rmaRecord.Parts)
            .WithOne(part => part.RmaRecord)
            .HasForeignKey(part => part.RmaRecordId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(rmaRecord => rmaRecord.StatusHistoryEntries)
            .WithOne(statusHistory => statusHistory.RmaRecord)
            .HasForeignKey(statusHistory => statusHistory.RmaRecordId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(rmaRecord => rmaRecord.AuditEntries)
            .WithOne(auditEntry => auditEntry.RmaRecord)
            .HasForeignKey(auditEntry => auditEntry.RmaRecordId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
