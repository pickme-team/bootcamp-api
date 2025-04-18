using System.Security.Claims;

namespace Bootcamp.Api;

public static class ClaimsPrincipalExtensions
{
    public static Guid Id(this ClaimsPrincipal user) => new(user.FindFirst(ClaimTypes.NameIdentifier)!.Value!);
}