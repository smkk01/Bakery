namespace Bakery.Application.Cart;

public sealed record CartItemInput(Guid CustomerId, Guid ProductId, int Quantity);
