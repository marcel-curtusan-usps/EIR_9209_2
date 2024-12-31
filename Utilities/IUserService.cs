using System.Security.Claims;

public interface IUserService
{
    ClaimsPrincipal GetUser();
}