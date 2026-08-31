using EImece.Tests.Infrastructure;
using EImece.Domain.Entities;
using EImece.Domain.Repositories.IRepositories;
using EImece.Domain.Services;
using EImece.Domain.Services.IServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Threading;
using System.Threading.Tasks;

namespace EImece.Tests.Helpers
{
    [TestClass]
    public class OrderDeleteAddressTests
    {
        private class DynamicFake<T> : RealProxy where T : class
        {
            private readonly Dictionary<string, Func<object[], object>> _handlers = new Dictionary<string, Func<object[], object>>();

            public DynamicFake() : base(typeof(T)) { }

            public void Setup(string methodName, Func<object[], object> handler)
            {
                _handlers[methodName] = handler;
            }

            public override IMessage Invoke(IMessage msg)
            {
                var call = (IMethodCallMessage)msg;
                if (_handlers.TryGetValue(call.MethodName, out var handler))
                {
                    var result = handler(call.Args);
                    return new ReturnMessage(result, null, 0, call.LogicalCallContext, call);
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

            public T Object => (T)GetTransparentProxy();
        }

        private class TestDbAsyncQueryProvider<TEntity> : IDbAsyncQueryProvider
        {
            private readonly IQueryProvider _inner;

            internal TestDbAsyncQueryProvider(IQueryProvider inner)
            {
                _inner = inner;
            }

            public IQueryable CreateQuery(Expression expression)
            {
                return new TestDbAsyncEnumerable<TEntity>(expression);
            }

            public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
            {
                return new TestDbAsyncEnumerable<TElement>(expression);
            }

            public object Execute(Expression expression)
            {
                return _inner.Execute(expression);
            }

            public TResult Execute<TResult>(Expression expression)
            {
                return _inner.Execute<TResult>(expression);
            }

            public Task<object> ExecuteAsync(Expression expression, CancellationToken cancellationToken)
            {
                return Task.FromResult(Execute(expression));
            }

            public Task<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
            {
                return Task.FromResult(Execute<TResult>(expression));
            }
        }

        private class TestDbAsyncEnumerable<T> : EnumerableQuery<T>, IDbAsyncEnumerable<T>, IQueryable<T>
        {
            public TestDbAsyncEnumerable(IEnumerable<T> enumerable)
                : base(enumerable)
            { }

            public TestDbAsyncEnumerable(Expression expression)
                : base(expression)
            { }

            public IDbAsyncEnumerator<T> GetAsyncEnumerator()
            {
                return new TestDbAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
            }

            IDbAsyncEnumerator IDbAsyncEnumerable.GetAsyncEnumerator()
            {
                return GetAsyncEnumerator();
            }

            IQueryProvider IQueryable.Provider => new TestDbAsyncQueryProvider<T>(this);
        }

        private class TestDbAsyncEnumerator<T> : IDbAsyncEnumerator<T>
        {
            private readonly IEnumerator<T> _inner;

            public TestDbAsyncEnumerator(IEnumerator<T> inner)
            {
                _inner = inner;
            }

            public void Dispose()
            {
                _inner?.Dispose();
            }

            public Task<bool> MoveNextAsync(CancellationToken cancellationToken)
            {
                return Task.FromResult(_inner.MoveNext());
            }

            public T Current => _inner.Current;

            object IDbAsyncEnumerator.Current => Current;
        }

        private class TestableOrderService : OrderService
        {
            public List<Order> Orders = new List<Order>();
            public List<int> DeletedOrderIds = new List<int>();

            public TestableOrderService(IOrderRepository repo, ICustomerService customerService, IOrderProductService orderProductService, IAddressService addressService = null)
                : base(repo, TestNullLoggers.Create<OrderService>(), customerService, orderProductService, addressService)
            {
            }

            public override Order GetSingle(int id)
            {
                return Orders.FirstOrDefault(o => o.Id == id);
            }

            public override Task<Order> GetSingleAsync(int id)
            {
                return Task.FromResult(Orders.FirstOrDefault(o => o.Id == id));
            }

            public override bool DeleteById(int id)
            {
                DeletedOrderIds.Add(id);
                Orders.RemoveAll(o => o.Id == id);
                return true;
            }

            public override Task<bool> DeleteByIdAsync(int id)
            {
                DeletedOrderIds.Add(id);
                Orders.RemoveAll(o => o.Id == id);
                return Task.FromResult(true);
            }
        }

        private static IQueryable<T> AsAsyncQueryable<T>(IEnumerable<T> source)
        {
            return new TestDbAsyncEnumerable<T>(source);
        }

        [TestMethod]
        public void DeleteOrderById_DeletesAssociatedShippingAndBillingAddresses()
        {
            var fakeRepo = new DynamicFake<IOrderRepository>();
            var fakeProductService = new DynamicFake<IOrderProductService>();
            var fakeAddressService = new DynamicFake<IAddressService>();

            var deletedProductOrderIds = new List<int>();
            fakeProductService.Setup("DeleteOrderProductsByOrderId", args =>
            {
                deletedProductOrderIds.Add((int)args[0]);
                return true;
            });

            var deletedAddressIds = new List<int>();
            fakeAddressService.Setup("DeleteById", args =>
            {
                deletedAddressIds.Add((int)args[0]);
                return true;
            });

            var orderService = new TestableOrderService(fakeRepo.Object, null, fakeProductService.Object, fakeAddressService.Object);
            orderService.Orders.Add(new Order { Id = 1, ShippingAddressId = 10, BillingAddressId = 20 });

            fakeRepo.Setup("FindBy", args =>
            {
                var expr = (Expression<Func<Order, bool>>)args[0];
                return AsAsyncQueryable(orderService.Orders.AsQueryable().Where(expr));
            });

            orderService.DeleteOrderById(1);

            Assert.AreEqual(0, orderService.Orders.Count, "Order should be removed.");
            Assert.AreEqual(1, deletedProductOrderIds.Count);
            Assert.AreEqual(1, deletedProductOrderIds[0]);
            CollectionAssert.AreEquivalent(new[] { 10, 20 }, deletedAddressIds, "Both shipping and billing addresses should be deleted.");
        }

        [TestMethod]
        public void DeleteOrderById_SameShippingAndBillingAddress_DeletesAddressOnlyOnce()
        {
            var fakeRepo = new DynamicFake<IOrderRepository>();
            var fakeProductService = new DynamicFake<IOrderProductService>();
            var fakeAddressService = new DynamicFake<IAddressService>();

            var deletedAddressIds = new List<int>();
            fakeAddressService.Setup("DeleteById", args =>
            {
                deletedAddressIds.Add((int)args[0]);
                return true;
            });

            var orderService = new TestableOrderService(fakeRepo.Object, null, fakeProductService.Object, fakeAddressService.Object);
            orderService.Orders.Add(new Order { Id = 1, ShippingAddressId = 10, BillingAddressId = 10 });

            fakeRepo.Setup("FindBy", args =>
            {
                var expr = (Expression<Func<Order, bool>>)args[0];
                return AsAsyncQueryable(orderService.Orders.AsQueryable().Where(expr));
            });

            orderService.DeleteOrderById(1);

            Assert.AreEqual(0, orderService.Orders.Count);
            Assert.AreEqual(1, deletedAddressIds.Count);
            Assert.AreEqual(10, deletedAddressIds[0]);
        }

        [TestMethod]
        public void DeleteOrderById_AddressSharedWithAnotherOrder_DoesNotDeleteSharedAddress()
        {
            var fakeRepo = new DynamicFake<IOrderRepository>();
            var fakeProductService = new DynamicFake<IOrderProductService>();
            var fakeAddressService = new DynamicFake<IAddressService>();

            var deletedAddressIds = new List<int>();
            fakeAddressService.Setup("DeleteById", args =>
            {
                deletedAddressIds.Add((int)args[0]);
                return true;
            });

            var orderService = new TestableOrderService(fakeRepo.Object, null, fakeProductService.Object, fakeAddressService.Object);
            orderService.Orders.Add(new Order { Id = 1, ShippingAddressId = 10, BillingAddressId = 10 });
            orderService.Orders.Add(new Order { Id = 2, ShippingAddressId = 10, BillingAddressId = 20 });

            fakeRepo.Setup("FindBy", args =>
            {
                var expr = (Expression<Func<Order, bool>>)args[0];
                return AsAsyncQueryable(orderService.Orders.AsQueryable().Where(expr));
            });

            orderService.DeleteOrderById(1);

            Assert.AreEqual(1, orderService.Orders.Count);
            Assert.AreEqual(2, orderService.Orders[0].Id);
            Assert.AreEqual(0, deletedAddressIds.Count, "Shared address (10) should NOT be deleted because order 2 uses it.");
        }

        [TestMethod]
        public async Task DeleteOrderByIdAsync_DeletesAssociatedShippingAndBillingAddresses()
        {
            var fakeRepo = new DynamicFake<IOrderRepository>();
            var fakeProductService = new DynamicFake<IOrderProductService>();
            var fakeAddressService = new DynamicFake<IAddressService>();

            var deletedProductOrderIds = new List<int>();
            fakeProductService.Setup("DeleteOrderProductsByOrderIdAsync", args =>
            {
                deletedProductOrderIds.Add((int)args[0]);
                return Task.FromResult(true);
            });

            var deletedAddressIds = new List<int>();
            fakeAddressService.Setup("DeleteByIdAsync", args =>
            {
                deletedAddressIds.Add((int)args[0]);
                return Task.FromResult(true);
            });

            var orderService = new TestableOrderService(fakeRepo.Object, null, fakeProductService.Object, fakeAddressService.Object);
            orderService.Orders.Add(new Order { Id = 1, ShippingAddressId = 30, BillingAddressId = 40 });

            fakeRepo.Setup("FindBy", args =>
            {
                var expr = (Expression<Func<Order, bool>>)args[0];
                return AsAsyncQueryable(orderService.Orders.AsQueryable().Where(expr));
            });

            await orderService.DeleteOrderByIdAsync(1);

            Assert.AreEqual(0, orderService.Orders.Count, "Order should be removed.");
            Assert.AreEqual(1, deletedProductOrderIds.Count);
            CollectionAssert.AreEquivalent(new[] { 30, 40 }, deletedAddressIds, "Both shipping and billing addresses should be deleted.");
        }

        [TestMethod]
        public void DeleteOrderById_NullAddressService_DoesNotThrow()
        {
            var fakeRepo = new DynamicFake<IOrderRepository>();
            var fakeProductService = new DynamicFake<IOrderProductService>();

            var orderService = new TestableOrderService(fakeRepo.Object, null, fakeProductService.Object);
            orderService.Orders.Add(new Order { Id = 1, ShippingAddressId = 10, BillingAddressId = 20 });

            orderService.DeleteOrderById(1);

            Assert.AreEqual(0, orderService.Orders.Count);
            Assert.AreEqual(1, orderService.DeletedOrderIds.Count);
        }
    }
}
