using EImece.Domain.DbContext;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;
using System.Data.SqlClient;

namespace EImece.Tests.Helpers
{
    [TestClass]
    public class DatabaseUtilityTests
    {
        [TestMethod]
        public void GetSqlParameter_NullValue_UsesDbNull()
        {
            SqlParameter p = DatabaseUtility.GetSqlParameter("@StartDate", null, SqlDbType.DateTime);
            Assert.AreEqual(DBNull.Value, p.Value);
            Assert.AreEqual("@StartDate", p.ParameterName);
        }

        [TestMethod]
        public void GetSqlParameter_PrefixesAtSignWhenMissing()
        {
            SqlParameter p = DatabaseUtility.GetSqlParameter("State", "ProductInStock", SqlDbType.NVarChar);
            Assert.AreEqual("@State", p.ParameterName);
            Assert.AreEqual("ProductInStock", p.Value);
        }

        [TestMethod]
        public void GetSqlParameter_DoesNotDoublePrefixAtSign()
        {
            SqlParameter p = DatabaseUtility.GetSqlParameter("@IsActive", true, SqlDbType.Bit);
            Assert.AreEqual("@IsActive", p.ParameterName);
        }
    }
}
