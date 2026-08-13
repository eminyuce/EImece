using EImece.Domain.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace EImece.Tests.Helpers
{
    [TestClass]
    public class GeneralHelperQaFixTests
    {
        [TestMethod]
        public void ToTelHref_StripsLocationAndSpaces()
        {
            Assert.AreEqual("tel:+902165550123", GeneralHelper.ToTelHref("+90 216 555 01 23 | İstanbul"));
        }

        [TestMethod]
        public void ToTelHref_Empty_ReturnsEmpty()
        {
            Assert.AreEqual(string.Empty, GeneralHelper.ToTelHref(null));
            Assert.AreEqual(string.Empty, GeneralHelper.ToTelHref("   "));
        }

        [TestMethod]
        public void TryParseFlexibleDate_AcceptsIsoAndTurkish()
        {
            Assert.AreEqual(new DateTime(2026, 1, 1), GeneralHelper.TryParseFlexibleDate("2026-01-01"));
            Assert.AreEqual(new DateTime(2026, 12, 31), GeneralHelper.TryParseFlexibleDate("31.12.2026"));
            Assert.IsNull(GeneralHelper.TryParseFlexibleDate("not-a-date"));
        }
    }
}
