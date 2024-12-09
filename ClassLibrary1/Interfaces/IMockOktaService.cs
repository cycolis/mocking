namespace MockOktaClientLibrary.Services;

public interface IMockOktaService
{
    Task<string> AuthenticateAsync(string username, string password);
}