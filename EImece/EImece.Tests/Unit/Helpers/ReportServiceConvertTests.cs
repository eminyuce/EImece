using System;
using System.Data;
using EImece.Domain.Helpers;
using EImece.Domain.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EImece.Tests.Unit.Helpers
{
    [TestClass]
    public class ReportServiceConvertTests
    {
        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            Environment.SetEnvironmentVariable(
                ConnectionStringProvider.EnvironmentVariableName,
                @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=EImece_Legacy_Test;Integrated Security=True;MultipleActiveResultSets=True;");
        }

        [TestMethod]
        public void ConvertDataTableToList_MapsRowsAndColumns()
        {
            var dt = new DataTable();
            dt.Columns.Add("Name", typeof(string));
            dt.Columns.Add("Amount", typeof(decimal));
            dt.Rows.Add("A", 10.5m);
            dt.Rows.Add("B", 20m);

            var sut = new ReportService();
            var list = sut.ConvertDataTableToList(dt);

            Assert.AreEqual(2, list.Count);
            Assert.AreEqual("A", list[0]["Name"]);
            Assert.AreEqual(10.5m, list[0]["Amount"]);
        }

        [TestMethod]
        public void ExportReportCsv_EmitsUtf8BomAndSemicolonPreamble()
        {
            var dt = new DataTable();
            dt.Columns.Add("Col1", typeof(string));
            dt.Rows.Add("value");

            var bytes = ExcelHelper.ExportReportCsv(dt);
            Assert.IsTrue(bytes.Length > 3);
            Assert.AreEqual(0xEF, bytes[0]);
            Assert.AreEqual(0xBB, bytes[1]);
            Assert.AreEqual(0xBF, bytes[2]);

            var text = System.Text.Encoding.UTF8.GetString(bytes);
            StringAssert.Contains(text, "sep=");
        }

        [TestMethod]
        public void ExportReportCsv_WhenNull_ReturnsNonEmptyBytes()
        {
            var bytes = ExcelHelper.ExportReportCsv(null);
            Assert.IsNotNull(bytes);
            Assert.IsTrue(bytes.Length > 0);
        }
    }
}
