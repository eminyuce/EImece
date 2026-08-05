using System;
using System.Data;
using EImece.Domain.Helpers;
using EImece.Domain.Services;
using EImece.Integration.Tests.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EImece.Integration.Tests.Reports
{
    [TestClass]
    public class ReportExportIntegrationTests
    {
        [TestMethod]
        public void ConvertDataTableToList_ExportShape_IsStable()
        {
            Environment.SetEnvironmentVariable(
                ConnectionStringProvider.EnvironmentVariableName,
                LegacyTestDbFixture.ConnectionString);

            var table = new DataTable("Sales");
            table.Columns.Add("OrderNumber", typeof(string));
            table.Columns.Add("PaidPrice", typeof(decimal));
            table.Rows.Add("ORD-1", 120.5m);

            using (var svc = new ReportService())
            {
                var rows = svc.ConvertDataTableToList(table);
                Assert.AreEqual(1, rows.Count);
                Assert.AreEqual("ORD-1", rows[0]["OrderNumber"]);
                Assert.AreEqual(120.5m, rows[0]["PaidPrice"]);
            }
        }
    }
}
