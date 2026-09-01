using EImece.Domain.DbContext;
using EImece.Domain.Entities;
using EImece.Domain.Repositories;
using EImece.Tests.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;

namespace EImece.Tests.Helpers
{
    [TestClass]
    public class RepositoryLifetimeTests
    {
        private class FakeContextProxy : RealProxy
        {
            public bool IsDisposed { get; private set; }

            public FakeContextProxy() : base(typeof(IEImeceContext))
            {
            }

            public override IMessage Invoke(IMessage msg)
            {
                var call = (IMethodCallMessage)msg;
                if (call.MethodName == "Dispose")
                {
                    IsDisposed = true;
                    return new ReturnMessage(null, null, 0, call.LogicalCallContext, call);
                }

                object defaultResult = null;
                if (call.MethodBase is MethodInfo mi && mi.ReturnType != typeof(void))
                {
                    if (mi.ReturnType.IsValueType)
                    {
                        defaultResult = Activator.CreateInstance(mi.ReturnType);
                    }
                }
                return new ReturnMessage(defaultResult, null, 0, call.LogicalCallContext, call);
            }

            public IEImeceContext Context => (IEImeceContext)GetTransparentProxy();
        }

        private class TestBaseRepository : BaseRepository<Setting>
        {
            public int InMemDeletedCount { get; set; } = 3;
            public IsolationLevel? LastBeginTransactionIsolationLevel { get; set; }

            public TestBaseRepository(IEImeceContext dbContext) : base(dbContext)
            {
            }
        }

        [TestMethod]
        public void BaseRepository_Dispose_DoesNotDisposeInjectedDbContext()
        {
            // Arrange
            var proxy = new FakeContextProxy();
            var fakeContext = proxy.Context;
            var repo = new TestBaseRepository(fakeContext);

            // Act
            repo.Dispose();

            // Assert: DI container owns the lifecycle of DbContext, so repo.Dispose() must be a no-op
            Assert.IsFalse(proxy.IsDisposed, "Repository Dispose() must not dispose the injected scoped DbContext.");
        }

        [TestMethod]
        public void ConcreteRepository_Dispose_DoesNotDisposeInjectedDbContext()
        {
            // Arrange
            var proxy = new FakeContextProxy();
            var fakeContext = proxy.Context;
            var addressRepo = new AddressRepository(fakeContext, TestNullLoggers.Create<AddressRepository>());

            // Act
            addressRepo.Dispose();

            // Assert
            Assert.IsFalse(proxy.IsDisposed, "AddressRepository Dispose() must not dispose the injected scoped DbContext.");
        }
    }
}
