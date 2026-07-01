using BuildBook.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildBook.Infrastructure.Persistence.Configurations;

public sealed class OrderRecordConfiguration : IEntityTypeConfiguration<OrderRecord>
{
    public void Configure(EntityTypeBuilder<OrderRecord> builder)
    {
        builder.ToTable("OrderRecords");

        builder.HasKey(orderRecord => orderRecord.Id);

        builder.Property(orderRecord => orderRecord.OrderNumber)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(orderRecord => orderRecord.OrderTitle)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(orderRecord => orderRecord.OrderDescription)
            .HasMaxLength(4000);

        builder.Property(orderRecord => orderRecord.Status)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(orderRecord => orderRecord.ImportedPriorityText)
            .HasMaxLength(64);

        builder.Property(orderRecord => orderRecord.ImportedCompletedByText)
            .HasMaxLength(256);

        builder.Property(orderRecord => orderRecord.ImportedCreatedByText)
            .HasMaxLength(256);

        builder.Property(orderRecord => orderRecord.PlannerTaskId)
            .HasMaxLength(128);

        builder.Property(orderRecord => orderRecord.PlannerPlanId)
            .HasMaxLength(128);

        builder.Property(orderRecord => orderRecord.PlannerBucketId)
            .HasMaxLength(128);

        builder.Property(orderRecord => orderRecord.PlannerBucketName)
            .HasMaxLength(128);

        builder.Property(orderRecord => orderRecord.PlannerSource)
            .HasMaxLength(128);

        builder.Property(orderRecord => orderRecord.PlannerStatus)
            .HasMaxLength(128);

        builder.Property(orderRecord => orderRecord.PlannerGoal)
            .HasMaxLength(256);

        builder.Property(orderRecord => orderRecord.CustomerReference)
            .HasMaxLength(128);

        builder.Property(orderRecord => orderRecord.CustomerPurchaseOrderNumber)
            .HasMaxLength(128);

        builder.Property(orderRecord => orderRecord.InternalOrderReference)
            .HasMaxLength(128);

        builder.Property(orderRecord => orderRecord.QuoteNumber)
            .HasMaxLength(128);

        builder.Property(orderRecord => orderRecord.NotesSummary)
            .HasMaxLength(1024);

        builder.Property(orderRecord => orderRecord.SupportTicketNo)
            .HasMaxLength(128);

        builder.Property(orderRecord => orderRecord.ShippingMethod)
            .HasMaxLength(128);

        builder.Property(orderRecord => orderRecord.Courier)
            .HasMaxLength(128);

        builder.Property(orderRecord => orderRecord.TrackingNumber)
            .HasMaxLength(256);

        builder.Property(orderRecord => orderRecord.InvoiceNumber)
            .HasMaxLength(128);

        builder.HasIndex(orderRecord => orderRecord.OrderNumber)
            .IsUnique();

        builder.HasIndex(orderRecord => orderRecord.CustomerId);
        builder.HasIndex(orderRecord => orderRecord.Status);
        builder.HasIndex(orderRecord => orderRecord.Priority);
        builder.HasIndex(orderRecord => orderRecord.StartDate);
        builder.HasIndex(orderRecord => orderRecord.DueDate);
        builder.HasIndex(orderRecord => orderRecord.InvoiceNumber);
        builder.HasIndex(orderRecord => orderRecord.SupportTicketNo);
        builder.HasIndex(orderRecord => orderRecord.LastUpdatedAt);

        builder.HasIndex(orderRecord => orderRecord.PlannerTaskId)
            .IsUnique()
            .HasFilter("[PlannerTaskId] IS NOT NULL");

        builder.HasOne(orderRecord => orderRecord.Customer)
            .WithMany(customer => customer.OrderRecords)
            .HasForeignKey(orderRecord => orderRecord.CustomerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(orderRecord => orderRecord.CompletedByUser)
            .WithMany()
            .HasForeignKey(orderRecord => orderRecord.CompletedByUserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(orderRecord => orderRecord.CreatedByUser)
            .WithMany()
            .HasForeignKey(orderRecord => orderRecord.CreatedByUserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(orderRecord => orderRecord.LastUpdatedByUser)
            .WithMany()
            .HasForeignKey(orderRecord => orderRecord.LastUpdatedByUserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(orderRecord => orderRecord.SalesAdminOwnerUser)
            .WithMany()
            .HasForeignKey(orderRecord => orderRecord.SalesAdminOwnerUserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(orderRecord => orderRecord.ProductionOwnerUser)
            .WithMany()
            .HasForeignKey(orderRecord => orderRecord.ProductionOwnerUserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(orderRecord => orderRecord.ShippedByUser)
            .WithMany()
            .HasForeignKey(orderRecord => orderRecord.ShippedByUserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(orderRecord => orderRecord.InvoicedByUser)
            .WithMany()
            .HasForeignKey(orderRecord => orderRecord.InvoicedByUserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(orderRecord => orderRecord.Assignments)
            .WithOne(orderAssignment => orderAssignment.OrderRecord)
            .HasForeignKey(orderAssignment => orderAssignment.OrderRecordId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(orderRecord => orderRecord.ChecklistItems)
            .WithOne(orderChecklistItem => orderChecklistItem.OrderRecord)
            .HasForeignKey(orderChecklistItem => orderChecklistItem.OrderRecordId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(orderRecord => orderRecord.Notes)
            .WithOne(orderNote => orderNote.OrderRecord)
            .HasForeignKey(orderNote => orderNote.OrderRecordId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(orderRecord => orderRecord.Labels)
            .WithOne(orderLabel => orderLabel.OrderRecord)
            .HasForeignKey(orderLabel => orderLabel.OrderRecordId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(orderRecord => orderRecord.BuildRecordLinks)
            .WithOne(orderBuildRecordLink => orderBuildRecordLink.OrderRecord)
            .HasForeignKey(orderBuildRecordLink => orderBuildRecordLink.OrderRecordId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(orderRecord => orderRecord.StatusHistoryEntries)
            .WithOne(orderStatusHistory => orderStatusHistory.OrderRecord)
            .HasForeignKey(orderStatusHistory => orderStatusHistory.OrderRecordId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
