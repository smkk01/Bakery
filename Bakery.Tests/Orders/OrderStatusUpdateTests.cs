using Bakery.Application.Orders;
using Bakery.Application.Repositories;
using Bakery.Domain.Entities;
using Bakery.Domain.Enums;

namespace Bakery.Tests.Orders;

public sealed class OrderStatusUpdateTests
{
    [Fact]
    public async Task UpdateStatusAsync_UpdatesAndPersistsOrderStatus()
    {
        var order = CreateOrder();
        var orders = new InMemoryOrderRepository(order);
        var service = new OrderService(orders, new EmptyProductRepository());

        var updated = await service.UpdateStatusAsync(order.Id, OrderStatus.Preparing);

        Assert.NotNull(updated);
        Assert.Equal(OrderStatus.Preparing, updated.Status);
        Assert.Equal(OrderStatus.Preparing, order.Status);
        Assert.Equal(1, orders.UpdateCount);
    }

    [Fact]
    public async Task UpdateStatusAsync_ReturnsNullWhenOrderDoesNotExist()
    {
        var orders = new InMemoryOrderRepository();
        var service = new OrderService(orders, new EmptyProductRepository());

        var updated = await service.UpdateStatusAsync(Guid.NewGuid(), OrderStatus.Confirmed);

        Assert.Null(updated);
        Assert.Equal(0, orders.UpdateCount);
    }

    [Fact]
    public async Task UpdateStatusAsync_RejectsUnknownStatus()
    {
        var order = CreateOrder();
        var service = new OrderService(
            new InMemoryOrderRepository(order),
            new EmptyProductRepository());

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => service.UpdateStatusAsync(order.Id, (OrderStatus)int.MaxValue));
    }

    private static Order CreateOrder() => new()
    {
        OrderNumber = "BK-TEST-0001",
        CustomerId = Guid.NewGuid(),
        Status = OrderStatus.Pending,
        Items =
        [
            new OrderItem
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Sourdough",
                Quantity = 1,
                UnitPrice = 5m
            }
        ],
        TotalAmount = 5m
    };

    private sealed class InMemoryOrderRepository(params Order[] initialOrders) : IOrderRepository
    {
        private readonly List<Order> _orders = [.. initialOrders];

        public int UpdateCount { get; private set; }

        public Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Order>>(_orders);

        public Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(_orders.FirstOrDefault(order => order.Id == id));

        public Task CreateAsync(Order entity, CancellationToken cancellationToken = default)
        {
            _orders.Add(entity);
            return Task.CompletedTask;
        }

        public Task<bool> UpdateAsync(Order entity, CancellationToken cancellationToken = default)
        {
            UpdateCount++;
            return Task.FromResult(_orders.Any(order => order.Id == entity.Id));
        }

        public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(_orders.RemoveAll(order => order.Id == id) > 0);
    }

    private sealed class EmptyProductRepository : IProductRepository
    {
        public Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Product>>([]);

        public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult<Product?>(null);

        public Task CreateAsync(Product entity, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task<bool> UpdateAsync(Product entity, CancellationToken cancellationToken = default) =>
            Task.FromResult(false);

        public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(false);
    }
}
