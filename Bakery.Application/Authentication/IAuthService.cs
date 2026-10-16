namespace Bakery.Application.Authentication;

public interface IAuthService
{
    Task<AuthResultDto> RegisterAsync(
        RegisterRequestDto request,
        CancellationToken cancellationToken = default);

    Task<AuthResultDto?> LoginAsync(
        LoginRequestDto request,
        CancellationToken cancellationToken = default);
}
