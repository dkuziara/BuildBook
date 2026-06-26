# Backup And Restore Guide

This guide describes how to back up and restore BuildBook's SQL Server database as part of normal internal operations.

BuildBook stores:

- normal Build Record data in SQL Server
- encrypted sensitive values in SQL Server
- application role assignments in SQL Server

Because BuildBook replaces an important spreadsheet, database backup and restore should be treated as a routine operational requirement.

## Scope

This guide covers:

- SQL Server backups
- restore verification
- recovery preparation
- operational precautions for sensitive data

It should be used alongside the deployment guide in [internal-deployment-guide.md](C:/Users/DavidKuziara/Documents/source/BuildBook/docs/internal-deployment-guide.md).

## What must be backed up

At minimum, back up:

- the BuildBook SQL Server database
- the ASP.NET Core Data Protection key directory used by BuildBook

The SQL Server backup protects Build Records, secrets, imports, audit history, and role assignments.

The Data Protection key backup is also important because BuildBook uses Data Protection for encrypted secret handling.

## Backup frequency

Use a backup schedule that matches internal recovery expectations.

Recommended minimum:

- full SQL Server backup on a regular scheduled basis
- transaction log backups if the SQL Server recovery model and internal policy require them
- regular backup of the Data Protection key directory

Agree the exact schedule with the internal IT or infrastructure team.

## SQL Server backup process

Example SQL Server backup command:

```sql
BACKUP DATABASE [BuildBook]
TO DISK = N'D:\SQLBackups\BuildBook\BuildBook-full.bak'
WITH INIT, COMPRESSION, CHECKSUM, STATS = 10;
```

Operational expectations:

- store backups on an internal managed backup location
- retain backups according to the organisation's retention policy
- protect backup files with normal internal access controls
- treat backup files as sensitive because secret data remains present in encrypted form

## Data Protection key backup

Back up the configured `BuildBook:DataProtectionKeyDirectory` folder as part of the same operational process.

Example deployment location:

```text
C:\BuildBook\Keys
```

Without the corresponding key material, secret-related features may not be recoverable in the expected way after a restore or server rebuild.

## Restore process

Example SQL Server restore command:

```sql
RESTORE DATABASE [BuildBook]
FROM DISK = N'D:\SQLBackups\BuildBook\BuildBook-full.bak'
WITH REPLACE, RECOVERY, STATS = 10;
```

After restoring the database:

1. Restore or reconnect the correct Data Protection key directory.
2. Confirm the deployed application still points to the restored SQL Server database.
3. Start BuildBook and allow normal startup checks to complete.
4. Confirm the application can load the Build Register and Build Record pages.

## Restore verification checklist

After a restore, verify:

1. Windows Authentication still signs users in correctly.
2. A bootstrap administrator can access `Admin > Users & Roles`.
3. Build Records load correctly.
4. Search still returns expected results.
5. Exports still work without exposing sensitive values.
6. Authorized users can reveal secrets normally.
7. Audit history is still present.

## Recovery test expectation

The restore process should be tested periodically, not only documented.

Recommended practice:

- perform a test restore to a non-production SQL Server environment
- confirm the application starts against the restored database
- confirm secrets remain protected and usable
- confirm user and role assignments are present after restore

## Security notes

- Do not copy backup files to uncontrolled locations.
- Do not email backup files.
- Do not log decrypted secrets during backup or restore work.
- Keep backup access limited to trusted internal administrators.

## Failure handling

If a restore is needed because of corruption, failed deployment, or server loss:

1. Restore the SQL Server database from the latest appropriate backup.
2. Restore the Data Protection keys.
3. Verify BuildBook startup and user access.
4. Run the restore verification checklist before returning the system to normal use.
