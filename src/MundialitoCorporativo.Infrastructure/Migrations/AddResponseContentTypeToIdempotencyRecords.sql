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

DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[IdempotencyRecords]') AND [c].[name] = N'ResponseBody');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [IdempotencyRecords] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [IdempotencyRecords] ALTER COLUMN [ResponseBody] nvarchar(max) NULL;
GO

CREATE TABLE [Referees] (
    [Id] uniqueidentifier NOT NULL,
    [FirstName] nvarchar(100) NOT NULL,
    [LastName] nvarchar(100) NOT NULL,
    [LicenseNumber] nvarchar(50) NULL,
    [CreatedAtUtc] datetime2 NOT NULL,
    [UpdatedAtUtc] datetime2 NULL,
    CONSTRAINT [PK_Referees] PRIMARY KEY ([Id])
);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260220135007_AddReferees', N'8.0.11');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[IdempotencyRecords]') AND [c].[name] = N'ResponseBody');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [IdempotencyRecords] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [IdempotencyRecords] ALTER COLUMN [ResponseBody] nvarchar(max) NULL;
GO

CREATE TABLE [Users] (
    [Id] uniqueidentifier NOT NULL,
    [UserName] nvarchar(100) NOT NULL,
    [Email] nvarchar(256) NULL,
    [PasswordHash] nvarchar(500) NOT NULL,
    [Salt] nvarchar(100) NOT NULL,
    [CreatedAtUtc] datetime2 NOT NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
);
GO

CREATE UNIQUE INDEX [IX_Users_UserName] ON [Users] ([UserName]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260220143438_AddUsers', N'8.0.11');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DECLARE @var2 sysname;
SELECT @var2 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Users]') AND [c].[name] = N'Salt');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Users] DROP CONSTRAINT [' + @var2 + '];');
ALTER TABLE [Users] ALTER COLUMN [Salt] nvarchar(2000) NOT NULL;
GO

DECLARE @var3 sysname;
SELECT @var3 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Users]') AND [c].[name] = N'PasswordHash');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [Users] DROP CONSTRAINT [' + @var3 + '];');
ALTER TABLE [Users] ALTER COLUMN [PasswordHash] nvarchar(2000) NOT NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260220160859_IncreaseUserPasswordFieldSizes', N'8.0.11');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DECLARE @var4 sysname;
SELECT @var4 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Users]') AND [c].[name] = N'Salt');
IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [Users] DROP CONSTRAINT [' + @var4 + '];');
ALTER TABLE [Users] ALTER COLUMN [Salt] nvarchar(max) NOT NULL;
GO

DECLARE @var5 sysname;
SELECT @var5 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Users]') AND [c].[name] = N'PasswordHash');
IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [Users] DROP CONSTRAINT [' + @var5 + '];');
ALTER TABLE [Users] ALTER COLUMN [PasswordHash] nvarchar(max) NOT NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260220163150_UserPasswordFieldsMaxLength', N'8.0.11');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [Matches] ADD [RefereeId] uniqueidentifier NULL;
GO

CREATE TABLE [MatchCards] (
    [Id] uniqueidentifier NOT NULL,
    [MatchId] uniqueidentifier NOT NULL,
    [PlayerId] uniqueidentifier NOT NULL,
    [CardType] int NOT NULL,
    [Minute] int NOT NULL,
    [CreatedAtUtc] datetime2 NOT NULL,
    CONSTRAINT [PK_MatchCards] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_MatchCards_Matches_MatchId] FOREIGN KEY ([MatchId]) REFERENCES [Matches] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_MatchCards_Players_PlayerId] FOREIGN KEY ([PlayerId]) REFERENCES [Players] ([Id]) ON DELETE NO ACTION
);
GO

CREATE INDEX [IX_Matches_RefereeId] ON [Matches] ([RefereeId]);
GO

CREATE INDEX [IX_MatchCards_MatchId] ON [MatchCards] ([MatchId]);
GO

CREATE INDEX [IX_MatchCards_PlayerId] ON [MatchCards] ([PlayerId]);
GO

ALTER TABLE [Matches] ADD CONSTRAINT [FK_Matches_Referees_RefereeId] FOREIGN KEY ([RefereeId]) REFERENCES [Referees] ([Id]) ON DELETE SET NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260220174837_AddRefereeToMatchAndMatchCards', N'8.0.11');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DECLARE @var6 sysname;
SELECT @var6 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Users]') AND [c].[name] = N'Salt');
IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [Users] DROP CONSTRAINT [' + @var6 + '];');
ALTER TABLE [Users] ALTER COLUMN [Salt] nvarchar(max) NOT NULL;
GO

DECLARE @var7 sysname;
SELECT @var7 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Users]') AND [c].[name] = N'PasswordHash');
IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [Users] DROP CONSTRAINT [' + @var7 + '];');
ALTER TABLE [Users] ALTER COLUMN [PasswordHash] nvarchar(max) NOT NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260220182744_EnsureUserPasswordMaxLength', N'8.0.11');
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [IdempotencyRecords] ADD [ResponseContentType] nvarchar(200) NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260223151551_AddResponseContentTypeToIdempotencyRecords', N'8.0.11');
GO

COMMIT;
GO

