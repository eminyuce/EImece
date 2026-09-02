# Full seed: SQL dummy data and images

- **Captured:** 2026-08-12 8:34:53 PM
- **Source:** WhatsApp chat export (coding prompt only)
- **Use:** paste this file into an AI coding session as the task brief

---

# Full seed: SQL dummy data + images (+ sync to IIS if present)
.\RunSeedDummyData.ps1 -SeedDatabase -ConnectionString "Data Source=<SQL_SERVER>;Initial Catalog=<DATABASE>;User ID=<USER>;Password=<PASSWORD>;Encrypt=True;TrustServerCertificate=True;

# Images only (existing DB records; no SQL seed)
.\RunSeedDummyData.ps1 -ImagesOnly -ConnectionString "Data Source=<SQL_SERVER>;Initial Catalog=<DATABASE>;User ID=<USER>;Password=<PASSWORD>;Encrypt=True;TrustServerCertificate=True;

# Data only (no image files)
.\RunSeedDummyData.ps1 -SeedDatabase -SkipImages -ConnectionString "Data Source=<SQL_SERVER>;Initial Catalog=<DATABASE>;User ID=<USER>;Password=<PASSWORD>;Encrypt=True;TrustServerCertificate=True;

# Cleanup dummy seed data
.\RunSeedDummyData.ps1 -CleanupDatabase -ConnectionString "Data Source=<SQL_SERVER>;Initial Catalog=<DATABASE>;User ID=<USER>;Password=<PASSWORD>;Encrypt=True;TrustServerCertificate=True;
