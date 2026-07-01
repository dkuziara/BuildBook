using System.Globalization;
using System.IO.Compression;
using System.Xml.Linq;
using BuildBook.Application.Orders;
using BuildBook.Domain.Orders;
using BuildBook.Domain.Security;
using Microsoft.EntityFrameworkCore;

namespace BuildBook.Infrastructure.Persistence.Orders;

public sealed class OrderPlannerImportService : IOrderPlannerImportService
{
    private const int MaximumPreviewRows = 10;
    private const string ImportedUnmappedStatus = "Imported - Unmapped";
    private const string SummarySuffix = "...";
    private readonly IDbContextFactory<BuildBookDbContext>? dbContextFactory;

    private static readonly IReadOnlyList<OrderPlannerImportPreviewColumn> PreviewColumns =
    [
        CreatePreviewColumn("PlannerTaskId", "Task ID"),
        CreatePreviewColumn("OrderTitle", "Task name"),
        CreatePreviewColumn("PlannerBucketName", "Bucket"),
        CreatePreviewColumn("MappedStatus", "BuildBook status"),
        CreatePreviewColumn("Priority", "Priority"),
        CreatePreviewColumn("AssignedTo", "Assigned to"),
        CreatePreviewColumn("ChecklistCount", "Checklist items"),
        CreatePreviewColumn("CompletedChecklistCount", "Completed checklist"),
        CreatePreviewColumn("Notes", "Notes")
    ];

    private static readonly Dictionary<string, string> PlanHeaderAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Plan ID"] = "PlanId",
        ["Plan Name"] = "PlanName",
        ["Export Date"] = "ExportDate"
    };

    private static readonly Dictionary<string, string> TaskHeaderAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Task ID"] = "TaskId",
        ["Task Name"] = "TaskName",
        ["Bucket"] = "Bucket",
        ["Bucket ID"] = "BucketId",
        ["Goal"] = "Goal",
        ["Status"] = "Status",
        ["Priority"] = "Priority",
        ["Assigned To"] = "AssignedTo",
        ["Created By"] = "CreatedBy",
        ["Created Date"] = "CreatedDate",
        ["Due date"] = "DueDate",
        ["Due Date"] = "DueDate",
        ["Start date"] = "StartDate",
        ["Start Date"] = "StartDate",
        ["Is Recurring"] = "IsRecurring",
        ["Late"] = "Late",
        ["Completed Date"] = "CompletedDate",
        ["Completed By"] = "CompletedBy",
        ["Completed Checklist Items"] = "CompletedChecklistItems",
        ["Checklist Items"] = "ChecklistItems",
        ["Labels"] = "Labels",
        ["Notes"] = "Notes"
    };

    private static readonly Dictionary<string, string> BucketHeaderAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Bucket ID"] = "BucketId",
        ["Bucket"] = "BucketId",
        ["Bucket Name"] = "BucketName",
        ["Name"] = "BucketName"
    };

    private static readonly Dictionary<string, string> UserHeaderAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["User ID"] = "UserId",
        ["Assigned To"] = "UserId",
        ["Name"] = "DisplayName",
        ["User Name"] = "DisplayName",
        ["Display Name"] = "DisplayName",
        ["Email"] = "EmailAddress",
        ["Email Address"] = "EmailAddress"
    };

    public OrderPlannerImportService()
    {
    }

    public OrderPlannerImportService(IDbContextFactory<BuildBookDbContext> dbContextFactory)
    {
        this.dbContextFactory = dbContextFactory;
    }

    public async Task<OrderPlannerImportReview> BuildReviewAsync(
        string fileName,
        Stream fileStream,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentNullException.ThrowIfNull(fileStream);

        var workbookData = await ReadWorkbookDataAsync(fileName, fileStream, cancellationToken);
        var plannerWorkbook = BuildPlannerWorkbook(workbookData);
        var previewRows = plannerWorkbook.Rows
            .Take(MaximumPreviewRows)
            .Select(row => new OrderPlannerImportPreviewRow
            {
                SourceRowNumber = row.SourceRowNumber,
                Values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["PlannerTaskId"] = row.TaskId ?? string.Empty,
                    ["OrderTitle"] = row.TaskName ?? string.Empty,
                    ["PlannerBucketName"] = row.ResolvedBucketName ?? string.Empty,
                    ["MappedStatus"] = MapPlannerBucketToStatus(row.ResolvedBucketName).Status,
                    ["Priority"] = row.Priority ?? string.Empty,
                    ["AssignedTo"] = string.Join("; ", row.AssignedUsers),
                    ["ChecklistCount"] = row.ChecklistItems.Count.ToString(CultureInfo.InvariantCulture),
                    ["CompletedChecklistCount"] = row.CompletedChecklistItems.Count.ToString(CultureInfo.InvariantCulture),
                    ["Notes"] = SummarizePreviewText(row.Notes)
                }
            })
            .ToArray();

        var notices = plannerWorkbook.Notices.ToList();
        if (plannerWorkbook.Rows.Count > MaximumPreviewRows)
        {
            notices.Add($"Showing the first {MaximumPreviewRows} rows of {plannerWorkbook.Rows.Count} imported task rows.");
        }

        return new OrderPlannerImportReview
        {
            PreferredWorksheetName = plannerWorkbook.TaskWorksheetName,
            PlanId = plannerWorkbook.PlanMetadata.PlanId,
            PlanName = plannerWorkbook.PlanMetadata.PlanName,
            ExportDate = plannerWorkbook.PlanMetadata.ExportDate,
            RowsRead = plannerWorkbook.Rows.Count,
            RowsShown = previewRows.Length,
            WorksheetNames = workbookData.Worksheets.Select(worksheet => worksheet.Name).ToArray(),
            Notices = notices,
            Columns = PreviewColumns,
            Rows = previewRows
        };
    }

    public async Task<OrderPlannerImportValidationResult> BuildValidationAsync(
        string fileName,
        Stream fileStream,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentNullException.ThrowIfNull(fileStream);

        var workbookData = await ReadWorkbookDataAsync(fileName, fileStream, cancellationToken);
        var plannerWorkbook = BuildPlannerWorkbook(workbookData);
        var issues = new List<OrderPlannerImportValidationIssue>();

        var knownUsers = await LoadKnownUsersAsync(cancellationToken);
        var existingPlannerTaskIds = await LoadExistingPlannerTaskIdsAsync(cancellationToken);

        foreach (var notice in plannerWorkbook.Notices)
        {
            issues.Add(new OrderPlannerImportValidationIssue
            {
                Severity = OrderImportWarningSeverity.Info,
                Message = notice
            });
        }

        foreach (var row in plannerWorkbook.Rows)
        {
            if (string.IsNullOrWhiteSpace(row.TaskName))
            {
                issues.Add(CreateIssue(row, OrderImportWarningSeverity.Error, "Task name is required."));
            }

            ValidateParsedDate(row, row.CreatedDateRaw, row.CreatedAt, "Created date", issues);
            ValidateParsedDate(row, row.StartDateRaw, row.StartDate, "Start date", issues);
            ValidateParsedDate(row, row.DueDateRaw, row.DueDate, "Due date", issues);
            ValidateParsedDate(row, row.CompletedDateRaw, row.CompletedAt, "Completed date", issues);

            if (!string.IsNullOrWhiteSpace(row.TaskId) && existingPlannerTaskIds.Contains(row.TaskId))
            {
                issues.Add(CreateIssue(
                    row,
                    OrderImportWarningSeverity.Warning,
                    $"Planner task ID '{row.TaskId}' already exists in BuildBook and will be skipped by default."));
            }

            var statusMapping = MapPlannerBucketToStatus(row.ResolvedBucketName);
            if (statusMapping.IsUnmapped)
            {
                issues.Add(CreateIssue(
                    row,
                    OrderImportWarningSeverity.Warning,
                    $"Bucket '{row.ResolvedBucketName ?? "(blank)"}' does not match a seeded BuildBook order status."));
            }

            foreach (var assignedUser in row.AssignedUsers)
            {
                if (!TryFindApplicationUser(knownUsers, assignedUser, out _))
                {
                    issues.Add(CreateIssue(
                        row,
                        OrderImportWarningSeverity.Warning,
                        $"Assigned user '{assignedUser}' could not be matched to a BuildBook user and will be kept as imported text."));
                }
            }

            if (row.AssignedUsers.Count == 0)
            {
                issues.Add(CreateIssue(
                    row,
                    OrderImportWarningSeverity.Warning,
                    "No assigned users were found in the imported Planner row."));
            }

            if (string.IsNullOrWhiteSpace(row.TaskId))
            {
                issues.Add(CreateIssue(
                    row,
                    OrderImportWarningSeverity.Warning,
                    "Task ID is missing. BuildBook will generate an internal import order number."));
            }
        }

        var duplicateTaskIds = plannerWorkbook.Rows
            .Where(row => !string.IsNullOrWhiteSpace(row.TaskId))
            .GroupBy(row => row.TaskId, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1);

        foreach (var duplicateGroup in duplicateTaskIds)
        {
            foreach (var row in duplicateGroup)
            {
                issues.Add(CreateIssue(
                    row,
                    OrderImportWarningSeverity.Error,
                    $"Duplicate Planner task ID '{row.TaskId}' was found in the import file."));
            }
        }

        var orderedIssues = issues
            .OrderByDescending(issue => issue.Severity)
            .ThenBy(issue => issue.SourceRowNumber)
            .ThenBy(issue => issue.Message, StringComparer.Ordinal)
            .ToArray();

        return new OrderPlannerImportValidationResult
        {
            RowsRead = plannerWorkbook.Rows.Count,
            ErrorCount = orderedIssues.Count(issue => issue.Severity == OrderImportWarningSeverity.Error),
            WarningCount = orderedIssues.Count(issue => issue.Severity == OrderImportWarningSeverity.Warning),
            Issues = orderedIssues
        };
    }

    public async Task<OrderPlannerImportExecutionResult> BuildImportAsync(
        string fileName,
        Stream fileStream,
        string importedBy,
        CancellationToken cancellationToken = default)
    {
        if (dbContextFactory is null)
        {
            throw new InvalidOperationException("Order import execution requires database services.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentNullException.ThrowIfNull(fileStream);

        var workbookData = await ReadWorkbookDataAsync(fileName, fileStream, cancellationToken);
        var plannerWorkbook = BuildPlannerWorkbook(workbookData);
        var normalizedImportedBy = string.IsNullOrWhiteSpace(importedBy) ? "Unknown" : importedBy.Trim();

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var applicationUsers = await dbContext.ApplicationUsers
            .Where(user => user.IsActive)
            .ToListAsync(cancellationToken);
        var existingPlannerTaskIds = await dbContext.OrderRecords
            .Where(orderRecord => orderRecord.IsActive && orderRecord.PlannerTaskId != null)
            .Select(orderRecord => orderRecord.PlannerTaskId!)
            .ToListAsync(cancellationToken);
        var knownPlannerTaskIds = new HashSet<string>(existingPlannerTaskIds, StringComparer.OrdinalIgnoreCase);
        var importedPlannerTaskIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var importedByUserId = applicationUsers
            .Where(user => MatchesApplicationUser(user, normalizedImportedBy))
            .Select(user => (int?)user.Id)
            .FirstOrDefault();
        var importedAt = DateTimeOffset.UtcNow;

        var importBatch = new OrderImportBatch
        {
            FileName = fileName,
            PlanId = plannerWorkbook.PlanMetadata.PlanId,
            PlanName = plannerWorkbook.PlanMetadata.PlanName,
            ExportDate = plannerWorkbook.PlanMetadata.ExportDate,
            ImportedAt = importedAt,
            ImportedByUserId = importedByUserId,
            RowsRead = plannerWorkbook.Rows.Count
        };
        dbContext.OrderImportBatches.Add(importBatch);

        var createdCount = 0;
        var skippedCount = 0;
        var warningCount = 0;
        var errorCount = 0;

        foreach (var row in plannerWorkbook.Rows)
        {
            if (string.IsNullOrWhiteSpace(row.TaskName))
            {
                errorCount++;
                skippedCount++;
                AddImportWarning(importBatch, row, "MissingTaskName", "Task name is required.", OrderImportWarningSeverity.Error);
                continue;
            }

            if (!string.IsNullOrWhiteSpace(row.TaskId) && !importedPlannerTaskIds.Add(row.TaskId))
            {
                errorCount++;
                skippedCount++;
                AddImportWarning(importBatch, row, "DuplicateTaskId", $"Duplicate Planner task ID '{row.TaskId}' was found in the import file.", OrderImportWarningSeverity.Error);
                continue;
            }

            if (!string.IsNullOrWhiteSpace(row.TaskId) && !knownPlannerTaskIds.Add(row.TaskId))
            {
                warningCount++;
                skippedCount++;
                AddImportWarning(importBatch, row, "ExistingTaskId", $"Planner task ID '{row.TaskId}' already exists in BuildBook and was skipped.", OrderImportWarningSeverity.Warning);
                continue;
            }

            var statusMapping = MapPlannerBucketToStatus(row.ResolvedBucketName);
            if (statusMapping.IsUnmapped)
            {
                warningCount++;
                AddImportWarning(importBatch, row, "UnknownBucket", $"Bucket '{row.ResolvedBucketName ?? "(blank)"}' did not match a seeded BuildBook order status.", OrderImportWarningSeverity.Warning);
            }

            var createdByUser = FindApplicationUser(applicationUsers, row.CreatedBy);
            var completedByUser = FindApplicationUser(applicationUsers, row.CompletedBy);

            var orderRecord = new OrderRecord
            {
                OrderNumber = BuildOrderNumber(row),
                OrderTitle = row.TaskName!.Trim(),
                Status = statusMapping.Status,
                Priority = ParsePriority(row.Priority),
                ImportedPriorityText = NormalizeOptionalValue(row.Priority),
                StartDate = ParseDateOnly(row.StartDateRaw),
                DueDate = ParseDateOnly(row.DueDateRaw),
                CompletedAt = ParseDateTimeOffset(row.CompletedDateRaw),
                CompletedByUserId = completedByUser?.Id,
                ImportedCompletedByText = NormalizeOptionalValue(row.CompletedBy),
                CreatedAt = ParseDateTimeOffset(row.CreatedDateRaw) ?? importedAt,
                CreatedByUserId = createdByUser?.Id,
                ImportedCreatedByText = NormalizeOptionalValue(row.CreatedBy),
                LastUpdatedAt = importedAt,
                LastUpdatedByUserId = importedByUserId,
                IsRecurring = ParseBoolean(row.IsRecurring),
                PlannerTaskId = NormalizeOptionalValue(row.TaskId),
                PlannerPlanId = plannerWorkbook.PlanMetadata.PlanId,
                PlannerBucketId = NormalizeOptionalValue(row.BucketId),
                PlannerBucketName = NormalizeOptionalValue(row.ResolvedBucketName),
                PlannerSource = plannerWorkbook.TaskWorksheetName,
                PlannerStatus = NormalizeOptionalValue(row.Status),
                PlannerGoal = NormalizeOptionalValue(row.Goal),
                ImportedLateFlag = ParseNullableBoolean(row.Late),
                NotesSummary = SummarizeText(row.Notes, 1024)
            };

            foreach (var assignedUser in row.AssignedUsers)
            {
                var matchedUser = FindApplicationUser(applicationUsers, assignedUser);
                if (matchedUser is null)
                {
                    warningCount++;
                    AddImportWarning(importBatch, row, "UnknownUser", $"Assigned user '{assignedUser}' could not be matched to a BuildBook user and was stored as imported text.", OrderImportWarningSeverity.Warning);
                }

                orderRecord.Assignments.Add(new OrderAssignment
                {
                    ApplicationUserId = matchedUser?.Id,
                    ImportedUserText = matchedUser is null ? assignedUser : null,
                    AssignedAt = importedAt
                });
            }

            var completedChecklistLookup = new HashSet<string>(
                row.CompletedChecklistItems.Select(NormalizeForComparison),
                StringComparer.OrdinalIgnoreCase);

            for (var index = 0; index < row.ChecklistItems.Count; index++)
            {
                var checklistItem = row.ChecklistItems[index];
                var isCompleted = completedChecklistLookup.Contains(NormalizeForComparison(checklistItem));
                orderRecord.ChecklistItems.Add(new OrderChecklistItem
                {
                    DisplayOrder = index + 1,
                    Text = checklistItem,
                    IsCompleted = isCompleted,
                    CompletedByUserId = isCompleted ? completedByUser?.Id : null,
                    CompletedAt = isCompleted ? ParseDateTimeOffset(row.CompletedDateRaw) : null,
                    Source = "Planner import",
                    ImportedCompletedText = isCompleted ? NormalizeOptionalValue(row.CompletedBy) : null,
                    ShowInBoardView = true
                });
            }

            if (!string.IsNullOrWhiteSpace(row.Notes))
            {
                orderRecord.Notes.Add(new OrderNote
                {
                    NoteType = OrderNoteType.PlannerImportedNote,
                    NoteText = row.Notes.Trim(),
                    CreatedByUserId = createdByUser?.Id,
                    CreatedAt = ParseDateTimeOffset(row.CreatedDateRaw) ?? importedAt,
                    LastUpdatedByUserId = importedByUserId,
                    LastUpdatedAt = importedAt
                });
            }

            foreach (var label in row.Labels)
            {
                orderRecord.Labels.Add(new OrderLabel
                {
                    LabelText = label,
                    Source = "Planner import"
                });
            }

            orderRecord.StatusHistoryEntries.Add(new OrderStatusHistory
            {
                OldStatus = null,
                NewStatus = orderRecord.Status,
                ChangedByUserId = importedByUserId,
                ChangedAt = importedAt,
                Reason = "Imported from Planner spreadsheet."
            });

            dbContext.OrderRecords.Add(orderRecord);
            createdCount++;
        }

        importBatch.OrdersCreated = createdCount;
        importBatch.OrdersUpdated = 0;
        importBatch.OrdersSkipped = skippedCount;
        importBatch.Warnings = warningCount;
        importBatch.Errors = errorCount;

        await dbContext.SaveChangesAsync(cancellationToken);

        var summary = createdCount == 0
            ? "No orders were imported."
            : warningCount > 0 || errorCount > 0
                ? $"Imported {createdCount} orders with {warningCount} warnings and {errorCount} errors."
                : $"Imported {createdCount} orders successfully.";

        return new OrderPlannerImportExecutionResult
        {
            ImportBatchId = importBatch.Id,
            RowsRead = importBatch.RowsRead,
            OrdersCreated = importBatch.OrdersCreated,
            OrdersUpdated = importBatch.OrdersUpdated,
            OrdersSkipped = importBatch.OrdersSkipped,
            WarningCount = importBatch.Warnings,
            ErrorCount = importBatch.Errors,
            Summary = summary
        };
    }

    private static void AddImportWarning(
        OrderImportBatch importBatch,
        PlannerTaskRow row,
        string warningType,
        string message,
        OrderImportWarningSeverity severity)
    {
        importBatch.WarningEntries.Add(new OrderImportWarning
        {
            RowNumber = row.SourceRowNumber,
            PlannerTaskId = NormalizeOptionalValue(row.TaskId),
            WarningType = warningType,
            Message = message,
            Severity = severity
        });
    }

    private static void ValidateParsedDate(
        PlannerTaskRow row,
        string? rawValue,
        DateTimeOffset? parsedValue,
        string fieldLabel,
        ICollection<OrderPlannerImportValidationIssue> issues)
    {
        if (!string.IsNullOrWhiteSpace(rawValue) && parsedValue is null)
        {
            issues.Add(CreateIssue(row, OrderImportWarningSeverity.Error, $"{fieldLabel} is not a valid date."));
        }
    }

    private static OrderPlannerImportValidationIssue CreateIssue(
        PlannerTaskRow row,
        OrderImportWarningSeverity severity,
        string message)
    {
        return new OrderPlannerImportValidationIssue
        {
            SourceRowNumber = row.SourceRowNumber,
            PlannerTaskId = NormalizeOptionalValue(row.TaskId),
            Severity = severity,
            Message = message
        };
    }

    private static async Task<WorkbookData> ReadWorkbookDataAsync(
        string fileName,
        Stream fileStream,
        CancellationToken cancellationToken)
    {
        var seekableStream = await CreateSeekableStreamAsync(fileStream, cancellationToken);
        var extension = Path.GetExtension(fileName);
        try
        {
            if (extension.Equals(".csv", StringComparison.OrdinalIgnoreCase))
            {
                var worksheet = await ReadCsvWorksheetAsync("Consolidated Data", seekableStream, cancellationToken);
                return new WorkbookData([worksheet], ["CSV import assumes the file contains Planner task rows equivalent to the Consolidated Data worksheet."]);
            }

            if (extension.Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                return ReadXlsxWorkbook(seekableStream);
            }

            if (extension.Equals(".xls", StringComparison.OrdinalIgnoreCase))
            {
                return new WorkbookData([], ["Planner import supports .xlsx workbooks. Save legacy .xls exports as .xlsx first."]);
            }

            return new WorkbookData([], ["Planner import supports .xlsx workbooks or .csv task exports."]);
        }
        finally
        {
            if (!ReferenceEquals(seekableStream, fileStream))
            {
                await seekableStream.DisposeAsync();
            }
        }
    }

    private static async Task<Stream> CreateSeekableStreamAsync(Stream sourceStream, CancellationToken cancellationToken)
    {
        if (sourceStream.CanSeek)
        {
            sourceStream.Position = 0;
            return sourceStream;
        }

        var bufferedStream = new MemoryStream();
        await sourceStream.CopyToAsync(bufferedStream, cancellationToken);
        bufferedStream.Position = 0;
        return bufferedStream;
    }

    private static async Task<WorksheetData> ReadCsvWorksheetAsync(
        string worksheetName,
        Stream stream,
        CancellationToken cancellationToken)
    {
        stream.Position = 0;

        using var reader = new StreamReader(stream, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
        string[] headers = [];
        var rows = new List<WorksheetRow>();
        var sourceRowNumber = 0;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var line = await reader.ReadLineAsync(cancellationToken);
            if (line is null)
            {
                break;
            }

            sourceRowNumber++;
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var cells = ParseCsvLine(line).Select(value => value.Trim()).ToArray();
            if (headers.Length == 0)
            {
                headers = cells.Where(value => !string.IsNullOrWhiteSpace(value)).ToArray();
                continue;
            }

            rows.Add(new WorksheetRow(sourceRowNumber, NormalizeRowCells(cells, headers.Length)));
        }

        return new WorksheetData(worksheetName, headers, rows.Where(row => row.Cells.Any(cell => !string.IsNullOrWhiteSpace(cell))).ToArray());
    }

    private static WorkbookData ReadXlsxWorkbook(Stream stream)
    {
        stream.Position = 0;

        using var archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: true);
        XNamespace spreadsheetNs = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        XNamespace relationshipsNs = "http://schemas.openxmlformats.org/package/2006/relationships";

        var workbookEntry = archive.GetEntry("xl/workbook.xml");
        var workbookRelationshipsEntry = archive.GetEntry("xl/_rels/workbook.xml.rels");
        if (workbookEntry is null || workbookRelationshipsEntry is null)
        {
            return new WorkbookData([], ["The workbook structure could not be read."]);
        }

        using var workbookStream = workbookEntry.Open();
        using var workbookRelationshipsStream = workbookRelationshipsEntry.Open();
        var workbookDocument = XDocument.Load(workbookStream);
        var relationshipsDocument = XDocument.Load(workbookRelationshipsStream);
        var sharedStrings = ReadSharedStrings(archive);

        var worksheets = new List<WorksheetData>();

        foreach (var sheet in workbookDocument.Root?
                     .Element(spreadsheetNs + "sheets")?
                     .Elements(spreadsheetNs + "sheet")
                     ?? [])
        {
            var relationshipId = (string?)sheet.Attribute(XName.Get("id", "http://schemas.openxmlformats.org/officeDocument/2006/relationships"));
            var sheetName = (string?)sheet.Attribute("name") ?? "Worksheet";
            var target = relationshipsDocument.Root?
                .Elements(relationshipsNs + "Relationship")
                .FirstOrDefault(relationship => string.Equals((string?)relationship.Attribute("Id"), relationshipId, StringComparison.Ordinal))?
                .Attribute("Target")?
                .Value;

            if (string.IsNullOrWhiteSpace(target))
            {
                continue;
            }

            var normalizedTarget = target.Replace('\\', '/');
            if (!normalizedTarget.StartsWith("/", StringComparison.Ordinal))
            {
                normalizedTarget = $"xl/{normalizedTarget.TrimStart('/')}";
            }

            var worksheetEntry = archive.GetEntry(normalizedTarget.TrimStart('/'));
            if (worksheetEntry is null)
            {
                continue;
            }

            using var worksheetStream = worksheetEntry.Open();
            var worksheetDocument = XDocument.Load(worksheetStream);
            var worksheetRows = worksheetDocument.Root?
                .Element(spreadsheetNs + "sheetData")?
                .Elements(spreadsheetNs + "row")
                .ToArray()
                ?? [];

            if (worksheetRows.Length == 0)
            {
                worksheets.Add(new WorksheetData(sheetName, [], []));
                continue;
            }

            var headers = ReadRowCells(worksheetRows[0], sharedStrings, spreadsheetNs)
                .Select(value => value.Trim())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .ToArray();

            var rows = worksheetRows
                .Skip(1)
                .Select(row =>
                {
                    var rowNumber = int.TryParse((string?)row.Attribute("r"), NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedRowNumber)
                        ? parsedRowNumber
                        : 0;
                    return new WorksheetRow(rowNumber, NormalizeRowCells(ReadRowCells(row, sharedStrings, spreadsheetNs), headers.Length));
                })
                .Where(row => row.Cells.Any(cell => !string.IsNullOrWhiteSpace(cell)))
                .ToArray();

            worksheets.Add(new WorksheetData(sheetName, headers, rows));
        }

        return new WorkbookData(worksheets, []);
    }

    private static PlannerWorkbook BuildPlannerWorkbook(WorkbookData workbookData)
    {
        var notices = workbookData.Notices.ToList();
        if (workbookData.Worksheets.Count == 0)
        {
            return new PlannerWorkbook([], "Consolidated Data", new PlanMetadata(), notices);
        }

        var taskWorksheet = workbookData.Worksheets.FirstOrDefault(worksheet => string.Equals(worksheet.Name, "Consolidated Data", StringComparison.OrdinalIgnoreCase));
        if (taskWorksheet is null)
        {
            taskWorksheet = workbookData.Worksheets.FirstOrDefault(worksheet => string.Equals(worksheet.Name, "Tasks", StringComparison.OrdinalIgnoreCase));
            if (taskWorksheet is not null)
            {
                notices.Add("Using the raw Tasks worksheet because Consolidated Data was not found.");
            }
        }

        taskWorksheet ??= workbookData.Worksheets.First();
        if (!string.Equals(taskWorksheet.Name, "Consolidated Data", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(taskWorksheet.Name, "Tasks", StringComparison.OrdinalIgnoreCase))
        {
            notices.Add($"Using worksheet '{taskWorksheet.Name}' because a standard Planner task sheet was not found.");
        }

        var bucketLookup = BuildLookup(
            workbookData.Worksheets.FirstOrDefault(worksheet => string.Equals(worksheet.Name, "Buckets", StringComparison.OrdinalIgnoreCase)),
            BucketHeaderAliases,
            "BucketId",
            "BucketName");
        var userLookup = BuildUserLookup(workbookData.Worksheets.FirstOrDefault(worksheet => string.Equals(worksheet.Name, "Users", StringComparison.OrdinalIgnoreCase)));
        var planMetadata = BuildPlanMetadata(workbookData.Worksheets.FirstOrDefault(worksheet => string.Equals(worksheet.Name, "Plan", StringComparison.OrdinalIgnoreCase)));
        var taskHeaderMap = BuildHeaderMap(taskWorksheet.Headers, TaskHeaderAliases);

        var rows = taskWorksheet.Rows
            .Select(row => ParseTaskRow(row, taskWorksheet.Headers, taskHeaderMap, bucketLookup, userLookup))
            .ToArray();

        return new PlannerWorkbook(rows, taskWorksheet.Name, planMetadata, notices);
    }

    private static Dictionary<string, string> BuildLookup(
        WorksheetData? worksheet,
        IReadOnlyDictionary<string, string> headerAliases,
        string keyField,
        string valueField)
    {
        if (worksheet is null || worksheet.Headers.Count == 0)
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        var headerMap = BuildHeaderMap(worksheet.Headers, headerAliases);
        var keyIndex = FindHeaderIndex(worksheet.Headers, headerMap, keyField);
        var valueIndex = FindHeaderIndex(worksheet.Headers, headerMap, valueField);

        if (keyIndex < 0 || valueIndex < 0)
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        return worksheet.Rows
            .Where(row => !string.IsNullOrWhiteSpace(GetCellValue(row.Cells, keyIndex)) && !string.IsNullOrWhiteSpace(GetCellValue(row.Cells, valueIndex)))
            .ToDictionary(
                row => GetCellValue(row.Cells, keyIndex).Trim(),
                row => GetCellValue(row.Cells, valueIndex).Trim(),
                StringComparer.OrdinalIgnoreCase);
    }

    private static Dictionary<string, PlannerUserReference> BuildUserLookup(WorksheetData? worksheet)
    {
        if (worksheet is null || worksheet.Headers.Count == 0)
        {
            return new Dictionary<string, PlannerUserReference>(StringComparer.OrdinalIgnoreCase);
        }

        var headerMap = BuildHeaderMap(worksheet.Headers, UserHeaderAliases);
        var userIdIndex = FindHeaderIndex(worksheet.Headers, headerMap, "UserId");
        var displayNameIndex = FindHeaderIndex(worksheet.Headers, headerMap, "DisplayName");
        var emailIndex = FindHeaderIndex(worksheet.Headers, headerMap, "EmailAddress");

        var lookup = new Dictionary<string, PlannerUserReference>(StringComparer.OrdinalIgnoreCase);
        foreach (var row in worksheet.Rows)
        {
            var userId = GetCellValue(row.Cells, userIdIndex).Trim();
            if (string.IsNullOrWhiteSpace(userId))
            {
                continue;
            }

            lookup[userId] = new PlannerUserReference(
                GetCellValue(row.Cells, displayNameIndex).Trim(),
                GetCellValue(row.Cells, emailIndex).Trim());
        }

        return lookup;
    }

    private static PlanMetadata BuildPlanMetadata(WorksheetData? worksheet)
    {
        if (worksheet is null || worksheet.Headers.Count == 0 || worksheet.Rows.Count == 0)
        {
            return new PlanMetadata();
        }

        var headerMap = BuildHeaderMap(worksheet.Headers, PlanHeaderAliases);
        var firstRow = worksheet.Rows[0];

        return new PlanMetadata
        {
            PlanId = NormalizeOptionalValue(GetMappedCellValue(firstRow.Cells, worksheet.Headers, headerMap, "PlanId")),
            PlanName = NormalizeOptionalValue(GetMappedCellValue(firstRow.Cells, worksheet.Headers, headerMap, "PlanName")),
            ExportDate = ParseDateTimeOffset(GetMappedCellValue(firstRow.Cells, worksheet.Headers, headerMap, "ExportDate"))
        };
    }

    private static PlannerTaskRow ParseTaskRow(
        WorksheetRow row,
        IReadOnlyList<string> headers,
        IReadOnlyDictionary<string, string> headerMap,
        IReadOnlyDictionary<string, string> bucketLookup,
        IReadOnlyDictionary<string, PlannerUserReference> userLookup)
    {
        var taskId = GetMappedCellValue(row.Cells, headers, headerMap, "TaskId");
        var bucket = GetMappedCellValue(row.Cells, headers, headerMap, "Bucket");
        var bucketId = GetMappedCellValue(row.Cells, headers, headerMap, "BucketId");
        var resolvedBucketName = ResolveBucketName(bucket, bucketId, bucketLookup);

        return new PlannerTaskRow(
            row.RowNumber,
            NormalizeOptionalValue(taskId),
            NormalizeOptionalValue(GetMappedCellValue(row.Cells, headers, headerMap, "TaskName")),
            NormalizeOptionalValue(bucketId),
            NormalizeOptionalValue(resolvedBucketName),
            NormalizeOptionalValue(GetMappedCellValue(row.Cells, headers, headerMap, "Goal")),
            NormalizeOptionalValue(GetMappedCellValue(row.Cells, headers, headerMap, "Status")),
            NormalizeOptionalValue(GetMappedCellValue(row.Cells, headers, headerMap, "Priority")),
            ResolveAssignedUsers(GetMappedCellValue(row.Cells, headers, headerMap, "AssignedTo"), userLookup),
            NormalizeOptionalValue(GetMappedCellValue(row.Cells, headers, headerMap, "CreatedBy")),
            NormalizeOptionalValue(GetMappedCellValue(row.Cells, headers, headerMap, "CreatedDate")),
            ParseDateTimeOffset(GetMappedCellValue(row.Cells, headers, headerMap, "CreatedDate")),
            NormalizeOptionalValue(GetMappedCellValue(row.Cells, headers, headerMap, "DueDate")),
            ParseDateTimeOffset(GetMappedCellValue(row.Cells, headers, headerMap, "DueDate")),
            NormalizeOptionalValue(GetMappedCellValue(row.Cells, headers, headerMap, "StartDate")),
            ParseDateTimeOffset(GetMappedCellValue(row.Cells, headers, headerMap, "StartDate")),
            NormalizeOptionalValue(GetMappedCellValue(row.Cells, headers, headerMap, "IsRecurring")),
            NormalizeOptionalValue(GetMappedCellValue(row.Cells, headers, headerMap, "Late")),
            NormalizeOptionalValue(GetMappedCellValue(row.Cells, headers, headerMap, "CompletedDate")),
            ParseDateTimeOffset(GetMappedCellValue(row.Cells, headers, headerMap, "CompletedDate")),
            NormalizeOptionalValue(GetMappedCellValue(row.Cells, headers, headerMap, "CompletedBy")),
            ParseDelimitedValues(GetMappedCellValue(row.Cells, headers, headerMap, "ChecklistItems")),
            ParseDelimitedValues(GetMappedCellValue(row.Cells, headers, headerMap, "CompletedChecklistItems")),
            ParseDelimitedValues(GetMappedCellValue(row.Cells, headers, headerMap, "Labels")),
            NormalizeOptionalValue(GetMappedCellValue(row.Cells, headers, headerMap, "Notes")));
    }

    private static IReadOnlyList<string> ResolveAssignedUsers(
        string rawAssignedTo,
        IReadOnlyDictionary<string, PlannerUserReference> userLookup)
    {
        var values = ParseDelimitedValues(rawAssignedTo);
        return values
            .Select(value =>
            {
                if (userLookup.TryGetValue(value, out var userReference))
                {
                    return !string.IsNullOrWhiteSpace(userReference.DisplayName)
                        ? userReference.DisplayName
                        : !string.IsNullOrWhiteSpace(userReference.EmailAddress)
                            ? userReference.EmailAddress
                            : value;
                }

                return value;
            })
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static string ResolveBucketName(
        string? bucket,
        string? bucketId,
        IReadOnlyDictionary<string, string> bucketLookup)
    {
        if (!string.IsNullOrWhiteSpace(bucket))
        {
            return bucket.Trim();
        }

        if (!string.IsNullOrWhiteSpace(bucketId) && bucketLookup.TryGetValue(bucketId.Trim(), out var resolvedBucket))
        {
            return resolvedBucket;
        }

        return bucketId?.Trim() ?? string.Empty;
    }

    private static (string Status, bool IsUnmapped) MapPlannerBucketToStatus(string? bucketName)
    {
        if (string.IsNullOrWhiteSpace(bucketName))
        {
            return (ImportedUnmappedStatus, true);
        }

        var normalizedBucketName = NormalizeForComparison(bucketName);
        var matchedStatus = BuildBookOrderStatuses.DefaultWorkflow
            .FirstOrDefault(status => NormalizeForComparison(status) == normalizedBucketName);

        return matchedStatus is null
            ? (ImportedUnmappedStatus, true)
            : (matchedStatus, false);
    }

    private static IReadOnlyList<string> ParseDelimitedValues(string? rawValue)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return [];
        }

        return rawValue
            .Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static string BuildOrderNumber(PlannerTaskRow row)
    {
        if (!string.IsNullOrWhiteSpace(row.TaskId))
        {
            return $"PLN-{row.TaskId.Trim()}";
        }

        return $"ORD-IMP-{row.SourceRowNumber:D4}";
    }

    private static OrderPriority? ParsePriority(string? rawPriority)
    {
        if (string.IsNullOrWhiteSpace(rawPriority))
        {
            return null;
        }

        var normalized = NormalizeForComparison(rawPriority);
        return normalized switch
        {
            "LOW" => OrderPriority.Low,
            "MEDIUM" => OrderPriority.Medium,
            "HIGH" => OrderPriority.High,
            "URGENT" => OrderPriority.Urgent,
            "IMPORTANT" => OrderPriority.High,
            "1" => OrderPriority.Urgent,
            "3" => OrderPriority.High,
            "5" => OrderPriority.Medium,
            "9" => OrderPriority.Low,
            _ => null
        };
    }

    private static bool ParseBoolean(string? rawValue)
    {
        return ParseNullableBoolean(rawValue) ?? false;
    }

    private static bool? ParseNullableBoolean(string? rawValue)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return null;
        }

        var normalized = NormalizeForComparison(rawValue);
        return normalized switch
        {
            "TRUE" or "YES" or "Y" or "1" => true,
            "FALSE" or "NO" or "N" or "0" => false,
            _ => null
        };
    }

    private static DateOnly? ParseDateOnly(string? rawValue)
    {
        var parsed = ParseDateTimeOffset(rawValue);
        return parsed is null ? null : DateOnly.FromDateTime(parsed.Value.UtcDateTime);
    }

    private static DateTimeOffset? ParseDateTimeOffset(string? rawValue)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return null;
        }

        var trimmed = rawValue.Trim();
        if (DateTimeOffset.TryParse(trimmed, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dateTimeOffset)
            || DateTimeOffset.TryParse(trimmed, CultureInfo.GetCultureInfo("en-GB"), DateTimeStyles.AssumeLocal, out dateTimeOffset)
            || DateTimeOffset.TryParse(trimmed, CultureInfo.GetCultureInfo("en-US"), DateTimeStyles.AssumeLocal, out dateTimeOffset))
        {
            return dateTimeOffset;
        }

        if (DateOnly.TryParse(trimmed, CultureInfo.InvariantCulture, out var dateOnly)
            || DateOnly.TryParse(trimmed, CultureInfo.GetCultureInfo("en-GB"), out dateOnly)
            || DateOnly.TryParse(trimmed, CultureInfo.GetCultureInfo("en-US"), out dateOnly))
        {
            return dateOnly.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        }

        if (double.TryParse(trimmed, NumberStyles.Float, CultureInfo.InvariantCulture, out var excelDate))
        {
            try
            {
                return new DateTimeOffset(DateTime.SpecifyKind(DateTime.FromOADate(excelDate), DateTimeKind.Utc));
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        return null;
    }

    private static string? NormalizeOptionalValue(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string SummarizePreviewText(string? value)
    {
        var summary = SummarizeText(value, 90);
        return string.IsNullOrWhiteSpace(summary) ? "—" : summary;
    }

    private static string? SummarizeText(string? value, int maximumLength)
    {
        var normalized = NormalizeOptionalValue(value);
        if (normalized is null)
        {
            return null;
        }

        return normalized.Length <= maximumLength
            ? normalized
            : TruncateWithSuffix(normalized, maximumLength);
    }

    private static string TruncateWithSuffix(string value, int maximumLength)
    {
        if (maximumLength <= 0)
        {
            return string.Empty;
        }

        if (value.Length <= maximumLength)
        {
            return value;
        }

        if (maximumLength <= SummarySuffix.Length)
        {
            return SummarySuffix[..maximumLength];
        }

        return $"{value[..(maximumLength - SummarySuffix.Length)].TrimEnd()}{SummarySuffix}";
    }

    private static IReadOnlyDictionary<string, string> BuildHeaderMap(
        IReadOnlyList<string> headers,
        IReadOnlyDictionary<string, string> aliases)
    {
        return headers.ToDictionary(
            header => header,
            header => aliases.TryGetValue(header.Trim(), out var mappedValue)
                ? mappedValue
                : NormalizeForComparison(header),
            StringComparer.OrdinalIgnoreCase);
    }

    private static int FindHeaderIndex(
        IReadOnlyList<string> headers,
        IReadOnlyDictionary<string, string> headerMap,
        string canonicalFieldName)
    {
        for (var index = 0; index < headers.Count; index++)
        {
            if (headerMap.TryGetValue(headers[index], out var mappedName)
                && string.Equals(mappedName, canonicalFieldName, StringComparison.OrdinalIgnoreCase))
            {
                return index;
            }
        }

        return -1;
    }

    private static string GetMappedCellValue(
        IReadOnlyList<string> cells,
        IReadOnlyList<string> headers,
        IReadOnlyDictionary<string, string> headerMap,
        string canonicalFieldName)
    {
        var index = FindHeaderIndex(headers, headerMap, canonicalFieldName);
        return GetCellValue(cells, index);
    }

    private static string GetCellValue(IReadOnlyList<string> cells, int index)
    {
        return index >= 0 && index < cells.Count ? cells[index] : string.Empty;
    }

    private static OrderPlannerImportPreviewColumn CreatePreviewColumn(string key, string label)
    {
        return new OrderPlannerImportPreviewColumn
        {
            Key = key,
            Label = label
        };
    }

    private static ApplicationUser? FindApplicationUser(
        IReadOnlyList<ApplicationUser> knownUsers,
        string? importedUserValue)
    {
        return TryFindApplicationUser(knownUsers, importedUserValue, out var matchedUser)
            ? matchedUser
            : null;
    }

    private static bool TryFindApplicationUser(
        IReadOnlyList<ApplicationUser> knownUsers,
        string? importedUserValue,
        out ApplicationUser? matchedUser)
    {
        matchedUser = null;
        if (string.IsNullOrWhiteSpace(importedUserValue))
        {
            return false;
        }

        matchedUser = knownUsers.FirstOrDefault(user => MatchesApplicationUser(user, importedUserValue));
        return matchedUser is not null;
    }

    private static bool MatchesApplicationUser(ApplicationUser user, string importedUserValue)
    {
        var normalizedImportedValue = NormalizeForComparison(importedUserValue);

        return NormalizeForComparison(user.WindowsUserName) == normalizedImportedValue
               || NormalizeForComparison(user.DisplayName) == normalizedImportedValue
               || NormalizeForComparison(user.EmailAddress) == normalizedImportedValue;
    }

    private static string NormalizeForComparison(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return new string(value
            .Where(char.IsLetterOrDigit)
            .Select(char.ToUpperInvariant)
            .ToArray());
    }

    private static string[] NormalizeRowCells(IReadOnlyList<string> cells, int headerCount)
    {
        var normalized = new string[headerCount];
        for (var index = 0; index < headerCount; index++)
        {
            normalized[index] = index < cells.Count ? cells[index].Trim() : string.Empty;
        }

        return normalized;
    }

    private static string[] ReadRowCells(
        XElement row,
        IReadOnlyList<string> sharedStrings,
        XNamespace worksheetNs)
    {
        var cells = row.Elements(worksheetNs + "c")
            .Select(cell => new
            {
                ColumnIndex = GetColumnIndex((string?)cell.Attribute("r")),
                Value = ReadCellValue(cell, sharedStrings, worksheetNs)
            })
            .ToArray();

        if (cells.Length == 0)
        {
            return [];
        }

        var values = new string[cells.Max(cell => cell.ColumnIndex)];
        foreach (var cell in cells)
        {
            values[cell.ColumnIndex - 1] = cell.Value;
        }

        return values.Select(value => value ?? string.Empty).ToArray();
    }

    private static IReadOnlyList<string> ReadSharedStrings(ZipArchive archive)
    {
        XNamespace spreadsheetNs = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        var sharedStringsEntry = archive.GetEntry("xl/sharedStrings.xml");
        if (sharedStringsEntry is null)
        {
            return [];
        }

        using var sharedStringsStream = sharedStringsEntry.Open();
        var sharedStringsDocument = XDocument.Load(sharedStringsStream);
        return sharedStringsDocument.Root?
            .Elements(spreadsheetNs + "si")
            .Select(item => string.Concat(item.Descendants(spreadsheetNs + "t").Select(text => text.Value)))
            .ToArray()
            ?? [];
    }

    private static string ReadCellValue(
        XElement cell,
        IReadOnlyList<string> sharedStrings,
        XNamespace worksheetNs)
    {
        var value = cell.Element(worksheetNs + "v")?.Value ?? string.Empty;
        var type = (string?)cell.Attribute("t");

        return type switch
        {
            "s" when int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var index)
                && index >= 0
                && index < sharedStrings.Count => sharedStrings[index],
            "inlineStr" => string.Concat(cell.Descendants(worksheetNs + "t").Select(text => text.Value)),
            _ => value
        };
    }

    private static int GetColumnIndex(string? cellReference)
    {
        if (string.IsNullOrWhiteSpace(cellReference))
        {
            return int.MaxValue;
        }

        var columnLetters = new string(cellReference.TakeWhile(char.IsLetter).ToArray());
        var index = 0;

        foreach (var character in columnLetters.ToUpperInvariant())
        {
            index = (index * 26) + (character - 'A' + 1);
        }

        return index;
    }

    private static IReadOnlyList<string> ParseCsvLine(string line)
    {
        var values = new List<string>();
        var buffer = new System.Text.StringBuilder();
        var inQuotes = false;

        for (var index = 0; index < line.Length; index++)
        {
            var character = line[index];
            if (character == '"')
            {
                if (inQuotes && index + 1 < line.Length && line[index + 1] == '"')
                {
                    buffer.Append('"');
                    index++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (character == ',' && !inQuotes)
            {
                values.Add(buffer.ToString());
                buffer.Clear();
            }
            else
            {
                buffer.Append(character);
            }
        }

        values.Add(buffer.ToString());
        return values;
    }

    private async Task<IReadOnlyList<ApplicationUser>> LoadKnownUsersAsync(CancellationToken cancellationToken)
    {
        if (dbContextFactory is null)
        {
            return [];
        }

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        return await dbContext.ApplicationUsers
            .Where(user => user.IsActive)
            .ToListAsync(cancellationToken);
    }

    private async Task<HashSet<string>> LoadExistingPlannerTaskIdsAsync(CancellationToken cancellationToken)
    {
        if (dbContextFactory is null)
        {
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var existingTaskIds = await dbContext.OrderRecords
            .Where(orderRecord => orderRecord.IsActive && orderRecord.PlannerTaskId != null)
            .Select(orderRecord => orderRecord.PlannerTaskId!)
            .ToListAsync(cancellationToken);

        return new HashSet<string>(existingTaskIds, StringComparer.OrdinalIgnoreCase);
    }

    private sealed record WorkbookData(
        IReadOnlyList<WorksheetData> Worksheets,
        IReadOnlyList<string> Notices);

    private sealed record WorksheetData(
        string Name,
        IReadOnlyList<string> Headers,
        IReadOnlyList<WorksheetRow> Rows);

    private sealed record WorksheetRow(
        int RowNumber,
        IReadOnlyList<string> Cells);

    private sealed record PlanMetadata
    {
        public string? PlanId { get; init; }

        public string? PlanName { get; init; }

        public DateTimeOffset? ExportDate { get; init; }
    }

    private sealed record PlannerWorkbook(
        IReadOnlyList<PlannerTaskRow> Rows,
        string TaskWorksheetName,
        PlanMetadata PlanMetadata,
        IReadOnlyList<string> Notices);

    private sealed record PlannerTaskRow(
        int SourceRowNumber,
        string? TaskId,
        string? TaskName,
        string? BucketId,
        string? ResolvedBucketName,
        string? Goal,
        string? Status,
        string? Priority,
        IReadOnlyList<string> AssignedUsers,
        string? CreatedBy,
        string? CreatedDateRaw,
        DateTimeOffset? CreatedAt,
        string? DueDateRaw,
        DateTimeOffset? DueDate,
        string? StartDateRaw,
        DateTimeOffset? StartDate,
        string? IsRecurring,
        string? Late,
        string? CompletedDateRaw,
        DateTimeOffset? CompletedAt,
        string? CompletedBy,
        IReadOnlyList<string> ChecklistItems,
        IReadOnlyList<string> CompletedChecklistItems,
        IReadOnlyList<string> Labels,
        string? Notes);

    private sealed record PlannerUserReference(
        string DisplayName,
        string EmailAddress);
}
