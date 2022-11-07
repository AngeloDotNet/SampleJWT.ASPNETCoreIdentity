using System.Security.Claims;

namespace Identity.BusinessLayer.Services;

public interface IUserService
{
    string GetUserName();
    public ClaimsIdentity GetIdentity();
}