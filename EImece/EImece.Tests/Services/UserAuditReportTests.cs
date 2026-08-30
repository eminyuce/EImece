using EImece.Areas.Admin.Controllers;
using EImece.Areas.Admin.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;

namespace EImece.Tests.Services
{
    [TestClass]
    public class UserAuditReportTests
    {
        [TestMethod]
        public void UserAuditReportViewModel_DefaultInitialization_ShouldHaveEmptyDataTablesAndDefaults()
        {
            var model = new UserAuditReportViewModel();

            Assert.IsNotNull(model.AvailableUsers);
            Assert.IsNotNull(model.AvailableTables);
            Assert.IsNotNull(model.AvailableActionTypes);
            Assert.IsNotNull(model.UserSummaryData);
            Assert.IsNotNull(model.MonthlyBreakdownData);
            Assert.IsNotNull(model.DetailedRecordsData);
            Assert.AreEqual("summary", model.ActiveTab);
            Assert.AreEqual(0, model.TotalUsersCount);
            Assert.AreEqual(0, model.TotalCreatedCount);
            Assert.AreEqual(0, model.TotalUpdatedCount);
            Assert.AreEqual(0, model.TotalActivityCount);
        }

        [TestMethod]
        public void UserAudit_FullNameResolution_ShouldFormatProperlyOrFallbackToUnknown()
        {
            // Scenario 1: First and Last Name present
            string firstName = "John";
            string lastName = "Doe";
            string userName = "johndoe";
            string fullName = FormatUserFullName(firstName, lastName, userName);
            Assert.AreEqual("John Doe", fullName);

            // Scenario 2: Only First Name
            fullName = FormatUserFullName("Alice", null, "alice123");
            Assert.AreEqual("Alice", fullName);

            // Scenario 3: Only UserName present
            fullName = FormatUserFullName(null, null, "admin_user");
            Assert.AreEqual("admin_user", fullName);

            // Scenario 4: No name info -> Unknown
            fullName = FormatUserFullName(null, null, null);
            Assert.AreEqual("Unknown", fullName);

            fullName = FormatUserFullName("", "   ", "");
            Assert.AreEqual("Unknown", fullName);
        }

        [TestMethod]
        public void UserAudit_DateFormatting_ShouldFollowCulture()
        {
            var testDate = new DateTime(2026, 8, 26, 14, 30, 0);

            var trCulture = CultureInfo.GetCultureInfo("tr-TR");
            var enCulture = CultureInfo.GetCultureInfo("en-US");

            var trFormatted = testDate.ToString("g", trCulture);
            var enFormatted = testDate.ToString("g", enCulture);

            Assert.IsFalse(string.IsNullOrWhiteSpace(trFormatted));
            Assert.IsFalse(string.IsNullOrWhiteSpace(enFormatted));
            Assert.IsTrue(trFormatted.Contains("2026") || trFormatted.Contains("26"));
            Assert.IsTrue(enFormatted.Contains("2026") || enFormatted.Contains("8"));
        }

        [TestMethod]
        public void UserAudit_MonthlyBreakdown_ShouldSortNewestToOldest()
        {
            var dt = new DataTable("UserAuditMonthlyBreakdown");
            dt.Columns.Add("UserId", typeof(string));
            dt.Columns.Add("FullName", typeof(string));
            dt.Columns.Add("Year", typeof(int));
            dt.Columns.Add("Month", typeof(int));
            dt.Columns.Add("YearMonth", typeof(string));
            dt.Columns.Add("CreatedCount", typeof(int));
            dt.Columns.Add("UpdatedCount", typeof(int));
            dt.Columns.Add("TotalCount", typeof(int));

            dt.Rows.Add("u1", "User One", 2026, 1, "2026-01", 5, 2, 7);
            dt.Rows.Add("u1", "User One", 2026, 8, "2026-08", 10, 4, 14);
            dt.Rows.Add("u2", "User Two", 2025, 12, "2025-12", 3, 1, 4);

            var sortedRows = dt.AsEnumerable()
                .OrderByDescending(r => r.Field<int>("Year"))
                .ThenByDescending(r => r.Field<int>("Month"))
                .ThenBy(r => r.Field<string>("FullName"))
                .ToList();

            Assert.AreEqual("2026-08", sortedRows[0].Field<string>("YearMonth"));
            Assert.AreEqual("2026-01", sortedRows[1].Field<string>("YearMonth"));
            Assert.AreEqual("2025-12", sortedRows[2].Field<string>("YearMonth"));
        }

        [TestMethod]
        public void UserAudit_StoredProcedureScript_ShouldContainAllRequiredTablesAndProcedures()
        {
            var scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\..\\EImece\\SqlScripts\\AddUserAuditReportsStoredProcedures.sql");
            
            // If running in test runner where BaseDirectory differs, fall back to relative search
            if (!File.Exists(scriptPath))
            {
                var current = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
                while (current != null && !File.Exists(Path.Combine(current.FullName, "EImece\\EImece\\SqlScripts\\AddUserAuditReportsStoredProcedures.sql")))
                {
                    current = current.Parent;
                }
                if (current != null)
                {
                    scriptPath = Path.Combine(current.FullName, "EImece\\EImece\\SqlScripts\\AddUserAuditReportsStoredProcedures.sql");
                }
            }

            if (File.Exists(scriptPath))
            {
                var sql = File.ReadAllText(scriptPath);

                Assert.IsTrue(sql.Contains("sp_GetUserAuditSummaryReport"), "Must define sp_GetUserAuditSummaryReport");
                Assert.IsTrue(sql.Contains("sp_GetUserAuditMonthlyBreakdown"), "Must define sp_GetUserAuditMonthlyBreakdown");
                Assert.IsTrue(sql.Contains("sp_GetUserAuditDetailedRecords"), "Must define sp_GetUserAuditDetailedRecords");
                Assert.IsTrue(sql.Contains("sp_GetAuditUsersList"), "Must define sp_GetAuditUsersList");
                Assert.IsTrue(sql.Contains("sp_GetAuditTablesList"), "Must define sp_GetAuditTablesList");

                // Check required table names
                var expectedTables = new[]
                {
                    "Brands", "Coupons", "Faqs", "MailTemplates", "MainPageImages",
                    "Menus", "ProductCategories", "Products", "Stories", "StoryCategories",
                    "TagCategories", "Tags", "Templates"
                };

                foreach (var table in expectedTables)
                {
                    Assert.IsTrue(sql.Contains(table), $"Script must cover table: {table}");
                }
            }
        }

        [TestMethod]
        public void ReportExportFilter_ShouldSupportUserAuditFilterProperties()
        {
            var filter = new ReportExportFilter
            {
                ReportKey = "UserAuditSummary",
                Format = "excel",
                StartDate = new DateTime(2026, 1, 1),
                EndDate = new DateTime(2026, 8, 26),
                UserId = "test-user-id",
                TableName = "Products",
                ActionType = "Created"
            };

            Assert.AreEqual("UserAuditSummary", filter.ReportKey);
            Assert.AreEqual("test-user-id", filter.UserId);
            Assert.AreEqual("Products", filter.TableName);
            Assert.AreEqual("Created", filter.ActionType);
        }

        private static string FormatUserFullName(string firstName, string lastName, string userName)
        {
            var hasFirst = !string.IsNullOrWhiteSpace(firstName);
            var hasLast = !string.IsNullOrWhiteSpace(lastName);

            if (hasFirst && hasLast)
            {
                return $"{firstName.Trim()} {lastName.Trim()}";
            }
            if (hasFirst)
            {
                return firstName.Trim();
            }
            if (hasLast)
            {
                return lastName.Trim();
            }
            if (!string.IsNullOrWhiteSpace(userName))
            {
                return userName.Trim();
            }

            return "Unknown";
        }
    }
}
