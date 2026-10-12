/*
    07_database_settings.sql   (DBA - run in a maintenance window)

    READ_COMMITTED_SNAPSHOT (RCSI)
      Currently OFF: every read waits for any write on the same rows and vice versa. That is the most common cause
      of blocking ("the screen hangs") and reader/writer deadlocks in SQL Server. With RCSI ON, readers see the last
      committed version of a row instead of waiting. No application change is needed (EF Core and the procedures
      use READ COMMITTED by default). Cost: row versions are kept in tempdb.
      Switching it on needs exclusive access for a moment: WITH ROLLBACK IMMEDIATE disconnects active sessions.

    RECOVERY MODEL
      Currently SIMPLE: a failure can only be restored to the last full backup (no point-in-time restore).
      FULL requires scheduled log backups - only switch together with a backup job.
*/
IF (SELECT is_read_committed_snapshot_on FROM sys.databases WHERE name = DB_NAME()) = 0
BEGIN
    DECLARE @sql nvarchar(400) = N'ALTER DATABASE ' + QUOTENAME(DB_NAME()) + N' SET READ_COMMITTED_SNAPSHOT ON WITH ROLLBACK IMMEDIATE;';
    EXEC (@sql);
    PRINT 'READ_COMMITTED_SNAPSHOT is now ON';
END
ELSE
    PRINT 'READ_COMMITTED_SNAPSHOT already ON';

-- Only together with a log-backup job:
-- ALTER DATABASE [dbVANestle] SET RECOVERY FULL;
