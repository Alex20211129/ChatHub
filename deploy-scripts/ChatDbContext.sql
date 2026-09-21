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
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920223923_InitialCreate'
)
BEGIN
    CREATE TABLE [LoginAudits] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [UserName] nvarchar(256) NOT NULL,
        [EventType] int NOT NULL,
        [IpAddress] nvarchar(45) NULL,
        [OccurredAt] datetime2 NOT NULL,
        CONSTRAINT [PK_LoginAudits] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920223923_InitialCreate'
)
BEGIN
    CREATE TABLE [SignalRChatMessages] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NULL,
        [DisplayName] nvarchar(256) NOT NULL,
        [IsGuest] bit NOT NULL,
        [Text] nvarchar(2000) NOT NULL,
        [SentAt] datetime2 NOT NULL,
        CONSTRAINT [PK_SignalRChatMessages] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920223923_InitialCreate'
)
BEGIN
    CREATE TABLE [WebSocketChatMessages] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NULL,
        [DisplayName] nvarchar(256) NOT NULL,
        [IsGuest] bit NOT NULL,
        [Text] nvarchar(2000) NOT NULL,
        [SentAt] datetime2 NOT NULL,
        CONSTRAINT [PK_WebSocketChatMessages] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920223923_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_LoginAudits_UserId] ON [LoginAudits] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920223923_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_SignalRChatMessages_SentAt] ON [SignalRChatMessages] ([SentAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920223923_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_WebSocketChatMessages_SentAt] ON [WebSocketChatMessages] ([SentAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920223923_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260920223923_InitialCreate', N'10.0.11');
END;

COMMIT;
GO

