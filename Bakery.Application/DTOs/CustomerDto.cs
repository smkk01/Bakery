namespace Bakery.Application.DTOs;

public sealed record CustomerDto(Guid Id, string FullName, string Email, string Phone);
