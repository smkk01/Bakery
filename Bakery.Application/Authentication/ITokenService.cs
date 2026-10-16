using Bakery.Domain.Entities;

namespace Bakery.Application.Authentication;

public interface ITokenService
{
    (string Token, DateTime ExpiresAtUtc) CreateToken(Customer customer);
}
