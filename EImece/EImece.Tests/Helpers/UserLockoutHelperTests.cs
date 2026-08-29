using EImece.Domain.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace EImece.Tests.Helpers
{
    [TestClass]
    public class UserLockoutHelperTests
    {
        [TestMethod]
        public void IsLockedOut_WhenEndIsInTheFuture_ReturnsTrue()
        {
            var now = new DateTime(2026, 8, 29, 18, 0, 0, DateTimeKind.Utc);

            Assert.IsTrue(UserLockoutHelper.IsLockedOut(now.AddMinutes(5), now));
        }

        [TestMethod]
        public void IsLockedOut_WhenEndIsNullOrPast_ReturnsFalse()
        {
            var now = new DateTime(2026, 8, 29, 18, 0, 0, DateTimeKind.Utc);

            Assert.IsFalse(UserLockoutHelper.IsLockedOut(null, now));
            Assert.IsFalse(UserLockoutHelper.IsLockedOut(now, now));
            Assert.IsFalse(UserLockoutHelper.IsLockedOut(now.AddMinutes(-1), now));
        }

        [TestMethod]
        public void RemainingMinutes_RoundsUpUntilUnlock()
        {
            var now = new DateTimeOffset(2026, 8, 29, 18, 0, 0, TimeSpan.Zero);

            Assert.AreEqual(5, UserLockoutHelper.RemainingMinutes(now.AddMinutes(5), now));
            Assert.AreEqual(1, UserLockoutHelper.RemainingMinutes(now.AddSeconds(10), now));
            Assert.AreEqual(1, UserLockoutHelper.RemainingMinutes(now.AddMinutes(-1), now));
        }

        [TestMethod]
        public void RemainingSeconds_ReturnsZeroWhenExpired()
        {
            var now = new DateTimeOffset(2026, 8, 29, 18, 0, 0, TimeSpan.Zero);

            Assert.AreEqual(300, UserLockoutHelper.RemainingSeconds(now.AddMinutes(5), now));
            Assert.AreEqual(10, UserLockoutHelper.RemainingSeconds(now.AddSeconds(10), now));
            Assert.AreEqual(0, UserLockoutHelper.RemainingSeconds(now.AddMinutes(-1), now));
        }
    }
}
