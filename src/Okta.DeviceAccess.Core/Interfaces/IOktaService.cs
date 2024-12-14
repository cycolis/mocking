namespace Okta.DeviceAccess.Core.Interfaces;

public interface IOktaService
{
    Task<string> AuthenticateAsync(string username, string password);
}