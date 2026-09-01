using EImece.Domain.Caching;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EImece.Tests.Caching
{
    [TestClass]
    public class CacheHealthTests
    {
        [TestMethod]
        public void Evaluate_Disabled_IsNotEffective()
        {
            Assert.AreEqual(CacheEffectivenessLevel.NotEffective, CacheHealth.Evaluate(false, 100, 1, 90d));
        }

        [TestMethod]
        public void Evaluate_TooFewReads_IsLimited()
        {
            Assert.AreEqual(CacheEffectivenessLevel.Limited, CacheHealth.Evaluate(true, 3, 2, 90d));
        }

        [TestMethod]
        public void Evaluate_HighHitRate_IsEffective()
        {
            Assert.AreEqual(CacheEffectivenessLevel.Effective, CacheHealth.Evaluate(true, 80, 20, 50d));
        }

        [TestMethod]
        public void Evaluate_HighHitRateButSmallGain_IsLimited()
        {
            Assert.AreEqual(CacheEffectivenessLevel.Limited, CacheHealth.Evaluate(true, 80, 20, 5d));
        }

        [TestMethod]
        public void Evaluate_LowHitRate_IsNotEffective()
        {
            Assert.AreEqual(CacheEffectivenessLevel.NotEffective, CacheHealth.Evaluate(true, 5, 95, 90d));
        }

        [TestMethod]
        public void ImprovementPercent_RequiresBothAverages()
        {
            Assert.IsNull(CacheHealth.ImprovementPercent(null, 10d));
            Assert.IsNull(CacheHealth.ImprovementPercent(100d, null));
            Assert.AreEqual(90d, CacheHealth.ImprovementPercent(100d, 10d));
        }

        [TestMethod]
        public void SavedMs_IsHitsTimesDelta_WhenMeasured()
        {
            Assert.IsNull(CacheHealth.SavedMs(10, null, 2d));
            Assert.AreEqual(1800d, CacheHealth.SavedMs(10, 200d, 20d));
        }
    }
}
