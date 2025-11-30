IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250908043349_InitialBaseline', N'6.0.13');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [CloseBatchs] ADD [OrderDetailsId] bigint NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250908043545_AddOrderDetailsIdInCloseBatch', N'6.0.13');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CloseBatchs]') AND [c].[name] = N'OrderDetailsId');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [CloseBatchs] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [CloseBatchs] DROP COLUMN [OrderDetailsId];
GO

ALTER TABLE [ProductionOrder_Receiving] ADD [IsAddedInSap] bit NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250923201324_Add isAddedInSap column', N'6.0.13');
GO

COMMIT;
GO

