# EF Core migrations (EImeceDbContext)

## Important — existing SQL Server databases

The legacy application already has a production schema (no EF6 Code First migrations history).

**Do not** run `dotnet ef database update` against an existing EImece database without baselining first.
That would attempt to `CREATE TABLE` objects that already exist.

### Baseline an existing database

After deploying code that includes `InitialEImeceModel`:

1. Ensure the model matches the live schema (compare columns/FKs; adjust fluent config as needed).
2. Insert the migration row without applying DDL:

```sql
IF OBJECT_ID(N'__EFMigrationsHistory') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260804215355_InitialEImeceModel', N'8.0.14');
```

(Use the actual `MigrationId` filename prefix if it differs.)

### Greenfield database

For a new empty database only:

```bash
dotnet ef database update \
  --project EImece.Domain.Core/EImece.Domain.Core.csproj \
  --startup-project EImece.Web/EImece.Web.csproj \
  --context EImece.Domain.Core.Data.EImeceDbContext
```

The host does **not** call `Database.Migrate()` on startup.
